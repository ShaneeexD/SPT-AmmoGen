using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using EFT;
using EFT.InventoryLogic;
using EFT.PrefabSettings;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Systems.Effects;
using UnityEngine;

namespace AmmoGenClient;

public static class SmokeColorManager
{
    internal static ManualLogSource Log = null!;
    internal static Dictionary<string, Color> SmokeColors = new();
    internal static Dictionary<string, Color> BodyColors = new();
    internal static Dictionary<string, Color> FlareColors = new();
    internal static Dictionary<string, SmokeSettingsData> SmokeSettingsById = new();
    internal static Dictionary<GrenadeEmission, Color> EmissionColors = new();

    public static void Initialize(ManualLogSource logger)
    {
        Log = logger;
        LoadColors();
        LoadFlareColors();
        LoadSmokeSettings();

        LogInfo($"[AmmoGen Client] Smoke colors: {SmokeColors.Count}, body colors: {BodyColors.Count}, flare colors: {FlareColors.Count}, smoke settings: {SmokeSettingsById.Count}.");
    }

    [Conditional("DEBUG")]
    private static void LogInfo(string message)
    {
        Log?.LogInfo(message);
    }

    private static void LoadColors()
    {
        SmokeColors.Clear();
        BodyColors.Clear();

        var gameRoot = Path.GetDirectoryName(Application.dataPath);
        if (string.IsNullOrEmpty(gameRoot))
        {
            Log.LogError("[AmmoGen Client] Could not determine game root from Application.dataPath.");
            return;
        }

        // 1) Try to find server-generated color configs.
        var smokeConfigPath = FindExistingPath(gameRoot, Path.Combine("user", "mods", "AmmoGen", "config", "smoke_colors.json"));
        var bodyConfigPath = FindExistingPath(gameRoot, Path.Combine("user", "mods", "AmmoGen", "config", "body_colors.json"));

        if (!string.IsNullOrWhiteSpace(smokeConfigPath))
            LoadFromConfig(smokeConfigPath!, SmokeColors, "smoke");

        if (!string.IsNullOrWhiteSpace(bodyConfigPath))
            LoadFromConfig(bodyConfigPath!, BodyColors, "body");

        if (SmokeColors.Count > 0 || BodyColors.Count > 0)
            return;

        // 2) Fall back to reading the AmmoGen pack files directly.
        var packFolder = FindExistingPath(gameRoot, Path.Combine("user", "mods", "AmmoGen", "ammo"), true);
        if (!string.IsNullOrWhiteSpace(packFolder))
        {
            LoadFromPacks(packFolder!);
            if (SmokeColors.Count > 0 || BodyColors.Count > 0)
                return;
        }

        Log.LogWarning("[AmmoGen Client] Could not find AmmoGen color configs or pack folder. No custom colors will be applied.");
    }

    private static void LoadFlareColors()
    {
        FlareColors.Clear();

        var gameRoot = Path.GetDirectoryName(Application.dataPath);
        if (string.IsNullOrEmpty(gameRoot))
        {
            Log.LogError("[AmmoGen Client] Could not determine game root from Application.dataPath.");
            return;
        }

        // 1) Try to find server-generated flare color config.
        var flareConfigPath = FindExistingPath(gameRoot, Path.Combine("user", "mods", "AmmoGen", "config", "flare_colors.json"));
        if (!string.IsNullOrWhiteSpace(flareConfigPath))
        {
            LoadFromConfig(flareConfigPath!, FlareColors, "flare");
            if (FlareColors.Count > 0)
                return;
        }

        // 2) Fall back to reading the AmmoGen pack files directly.
        var packFolder = FindExistingPath(gameRoot, Path.Combine("user", "mods", "AmmoGen", "ammo"), true);
        if (string.IsNullOrWhiteSpace(packFolder))
        {
            Log.LogWarning("[AmmoGen Client] Could not find AmmoGen pack folder. No custom flare colors will be applied.");
            return;
        }

        try
        {
            LogInfo($"[AmmoGen Client] Loading flare colors from packs in {packFolder}");

            foreach (var file in Directory.GetFiles(packFolder, "*.json", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var raw = File.ReadAllText(file);
                    var pack = JsonConvert.DeserializeObject<JObject>(raw);
                    if (pack == null)
                        continue;

                    var flares = pack["flares"] as JArray;
                    if (flares == null)
                        continue;

                    foreach (var flare in flares.OfType<JObject>())
                    {
                        var kind = flare["kind"]?.Value<string>() ?? "handheld";
                        var colorId = kind == "cartridge"
                            ? flare["id"]?.Value<string>()
                            : flare["ammoId"]?.Value<string>();
                        if (string.IsNullOrWhiteSpace(colorId))
                            continue;

                        var flareId = colorId!.ToLowerInvariant();
                        var stats = flare["stats"] as JObject;
                        var flareColor = stats?["flareColor"]?.Value<string>();
                        if (!string.IsNullOrWhiteSpace(flareColor) && TryParseHexColor(flareColor!, out var color))
                        {
                            FlareColors[flareId] = color;
                            LogInfo($"[AmmoGen Client] Loaded flare color {flareColor} for {kind} {flareId} from pack {Path.GetFileName(file)}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"[AmmoGen Client] Failed to read pack {file}: {ex.Message}");
                }
            }

            LogInfo($"[AmmoGen Client] Loaded {FlareColors.Count} flare color(s) from packs.");
        }
        catch (Exception ex)
        {
            Log.LogError($"[AmmoGen Client] Failed to load flare colors from packs: {ex.Message}");
        }
    }

    private static void LoadSmokeSettings()
    {
        SmokeSettingsById.Clear();

        var gameRoot = Path.GetDirectoryName(Application.dataPath);
        if (string.IsNullOrEmpty(gameRoot))
            return;

        var settingsPath = FindExistingPath(gameRoot, Path.Combine("user", "mods", "AmmoGen", "config", "smoke_settings.json"));
        if (string.IsNullOrWhiteSpace(settingsPath))
            return;

        try
        {
            var raw = File.ReadAllText(settingsPath!);
            var settings = JsonConvert.DeserializeObject<Dictionary<string, SmokeSettingsData>>(raw);
            if (settings == null)
                return;

            foreach (var kvp in settings)
                SmokeSettingsById[kvp.Key.ToLowerInvariant()] = kvp.Value;

            LogInfo($"[AmmoGen Client] Loaded smoke settings for {SmokeSettingsById.Count} grenade(s).");
        }
        catch (Exception ex)
        {
            Log.LogError($"[AmmoGen Client] Failed to load smoke settings: {ex.Message}");
        }
    }

    private static string? FindExistingPath(string root, string relativePath, bool requireDirectory = false)
    {
        var candidates = new List<string>();

        void AddCandidate(string basePath)
        {
            if (string.IsNullOrEmpty(basePath))
                return;
            var full = Path.GetFullPath(Path.Combine(basePath, relativePath));
            if (!candidates.Contains(full))
                candidates.Add(full);
        }

        AddCandidate(root);

        var parent = Directory.GetParent(root);
        if (parent != null)
            AddCandidate(parent.FullName);

        foreach (var child in new[] { "SPT", "Server", "spt" })
            AddCandidate(Path.Combine(root, child));

        LogInfo($"[AmmoGen Client] Searching for {relativePath} in:");
        foreach (var candidate in candidates)
            LogInfo($"  - {candidate}");

        if (requireDirectory)
            return candidates.FirstOrDefault(Directory.Exists);

        return candidates.FirstOrDefault(File.Exists);
    }

    private static void LoadFromConfig(string configPath, Dictionary<string, Color> target, string colorType)
    {
        try
        {
            LogInfo($"[AmmoGen Client] Loading {colorType} config from {configPath}");
            var raw = File.ReadAllText(configPath);
            var hexColors = JsonConvert.DeserializeObject<Dictionary<string, string>>(raw);
            if (hexColors == null)
            {
                Log.LogWarning($"[AmmoGen Client] {colorType} config deserialized to null.");
                return;
            }

            foreach (var kvp in hexColors)
            {
                if (TryParseHexColor(kvp.Value, out var color))
                {
                    target[kvp.Key.ToLowerInvariant()] = color;
                    LogInfo($"[AmmoGen Client] Loaded {colorType} color {kvp.Value} for template {kvp.Key}.");
                }
                else
                {
                    Log.LogWarning($"[AmmoGen Client] Invalid {colorType} hex color '{kvp.Value}' for template {kvp.Key}.");
                }
            }

            LogInfo($"[AmmoGen Client] Loaded {target.Count} custom {colorType} color(s) from config.");
        }
        catch (Exception ex)
        {
            Log.LogError($"[AmmoGen Client] Failed to load {colorType} colors config: {ex.Message}");
        }
    }

    private static void LoadFromPacks(string packFolder)
    {
        try
        {
            LogInfo($"[AmmoGen Client] Loading colors from packs in {packFolder}");

            foreach (var file in Directory.GetFiles(packFolder, "*.json", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var raw = File.ReadAllText(file);
                    var pack = JsonConvert.DeserializeObject<JObject>(raw);
                    if (pack == null)
                        continue;

                    var grenades = pack["grenades"] as JArray;
                    if (grenades == null)
                        continue;

                    foreach (var grenade in grenades.OfType<JObject>())
                    {
                        var id = grenade["id"]?.Value<string>();
                        if (string.IsNullOrWhiteSpace(id))
                            continue;

                        var grenadeId = id!.ToLowerInvariant();
                        var stats = grenade["stats"] as JObject;

                        var smokeColor = stats?["smokeColor"]?.Value<string>();
                        if (!string.IsNullOrWhiteSpace(smokeColor) && TryParseHexColor(smokeColor!, out var smoke))
                        {
                            SmokeColors[grenadeId] = smoke;
                            LogInfo($"[AmmoGen Client] Loaded smoke color {smokeColor} for template {grenadeId} from pack {Path.GetFileName(file)}.");
                        }

                        var bodyColor = stats?["bodyColor"]?.Value<string>();
                        if (!string.IsNullOrWhiteSpace(bodyColor) && TryParseHexColor(bodyColor!, out var body))
                        {
                            BodyColors[grenadeId] = body;
                            LogInfo($"[AmmoGen Client] Loaded body color {bodyColor} for template {grenadeId} from pack {Path.GetFileName(file)}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"[AmmoGen Client] Failed to read pack {file}: {ex.Message}");
                }
            }

            LogInfo($"[AmmoGen Client] Loaded {SmokeColors.Count} smoke color(s) and {BodyColors.Count} body color(s) from packs.");
        }
        catch (Exception ex)
        {
            Log.LogError($"[AmmoGen Client] Failed to load colors from packs: {ex.Message}");
        }
    }

    private static bool TryParseHexColor(string hex, out Color color)
    {
        color = Color.white;
        if (string.IsNullOrWhiteSpace(hex))
            return false;

        hex = hex.TrimStart('#');
        if (hex.Length == 6)
        {
            if (byte.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                byte.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                byte.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
            {
                color = new Color32(r, g, b, 255);
                return true;
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(GrenadeEmission), nameof(GrenadeEmission.AttachTo))]
    public static class GrenadeEmissionAttachPatch
    {
        public static void Postfix(GrenadeEmission __instance, Transform t)
        {
            if (t == null)
                return;

            var grenade = t.GetComponent<SmokeGrenade>();
            if (grenade == null)
                return;

            var templateId = grenade.WeaponSource?.TemplateId.ToString().ToLowerInvariant();
            LogInfo($"[AmmoGen Client] GrenadeEmission attached to grenade {templateId ?? "null"}.");

            if (templateId != null && SmokeSettingsById.TryGetValue(templateId, out var settings))
                ApplySmokeSettings(grenade, __instance, settings);

            if (templateId == null || !SmokeColors.TryGetValue(templateId, out var color))
                return;

            EmissionColors[__instance] = color;
            ApplySmokeColor(__instance.gameObject, color);
        }
    }

    [HarmonyPatch(typeof(GrenadeEmission), nameof(GrenadeEmission.StartEmission))]
    public static class GrenadeEmissionStartPatch
    {
        public static void Postfix(GrenadeEmission __instance)
        {
            if (!EmissionColors.TryGetValue(__instance, out var color))
                return;

            ApplySmokeColor(__instance.gameObject, color);
            LogInfo($"[AmmoGen Client] Applied smoke color {ColorUtility.ToHtmlStringRGB(color)} to GrenadeEmission {__instance.name}.");
        }
    }

    [HarmonyPatch(typeof(SmokeGrenade), nameof(SmokeGrenade.Init))]
    public static class SmokeGrenadeInitPatch
    {
        public static void Postfix(SmokeGrenade __instance, ThrowWeapItemClass throwWeap)
        {
            if (throwWeap == null)
            {
                Log.LogWarning("[AmmoGen Client] Init patch called with null throwWeap.");
                return;
            }

            var templateId = throwWeap.TemplateId.ToString().ToLowerInvariant();
            LogInfo($"[AmmoGen Client] Init patch for template {templateId}. Body colors: {BodyColors.Count}.");

            if (!string.IsNullOrWhiteSpace(templateId) && BodyColors.TryGetValue(templateId, out var color))
                ApplyBodyColor(__instance, color);
        }
    }

    [HarmonyPatch(typeof(SmokeGrenade), nameof(SmokeGrenade.OnExplosion))]
    public static class SmokeGrenadeExplosionPatch
    {
        public static void Postfix(SmokeGrenade __instance)
        {
            var templateId = __instance.WeaponSource?.TemplateId.ToString().ToLowerInvariant();
            LogInfo($"[AmmoGen Client] OnExplosion patch for template {templateId ?? "null"}.");

            if (templateId != null && !string.IsNullOrWhiteSpace(templateId) && SmokeSettingsById.TryGetValue(templateId, out var settings))
            {
                var emission = __instance.GetComponentInChildren<GrenadeEmission>();
                ApplySmokeSettings(__instance, emission, settings);
            }

            if (templateId != null && !string.IsNullOrWhiteSpace(templateId) && BodyColors.TryGetValue(templateId, out var color))
                __instance.StartCoroutine(ReapplyBodyColor(__instance, color));
        }

        private static System.Collections.IEnumerator ReapplyBodyColor(SmokeGrenade grenade, Color color)
        {
            yield return new WaitForSeconds(0.1f);
            ApplyBodyColor(grenade, color);
        }
    }

    private static void ApplyBodyColor(SmokeGrenade grenade, Color color)
    {
        try
        {
            ApplyColorToRenderers(grenade.gameObject, color);
            LogInfo($"[AmmoGen Client] Applied body color {ColorUtility.ToHtmlStringRGB(color)} to grenade {grenade.name}.");
        }
        catch (Exception ex)
        {
            Log.LogError($"[AmmoGen Client] Failed to apply body color: {ex.Message}");
        }
    }

    private static void ApplySmokeColor(GameObject go, Color color)
    {
        try
        {
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                main.startColor = color;

                var psRenderer = ps.GetComponent<Renderer>();
                if (psRenderer != null)
                    ApplyColorToMaterial(psRenderer.material, color);
            }

            foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                ApplyColorToMaterial(renderer.material, color);
            }
        }
        catch (Exception ex)
        {
            Log.LogError($"[AmmoGen Client] Failed to apply smoke color: {ex.Message}");
        }
    }

    private static void ApplyColorToRenderers(GameObject go, Color color)
    {
        if (go == null)
            return;

        foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
        {
            ApplyColorToMaterial(renderer.material, color);
        }
    }

    private static void ApplyColorToMaterial(Material material, Color color)
    {
        if (material == null)
            return;

        var properties = new[] { "_Color", "_TintColor", "_MainColor", "_EmissionColor", "_BaseColor", "_SmokeColor" };
        foreach (var prop in properties)
        {
            if (material.HasProperty(prop))
                material.SetColor(prop, color);
        }
    }

    private static void ApplySmokeSettings(SmokeGrenade grenade, GrenadeEmission emission, SmokeSettingsData settings)
    {
        try
        {
            var sgs = grenade.SmokeGrenadeSettings_0;
            if (sgs != null)
            {
                if (settings.SmokeRadius != 0)
                    sgs._radiusMultiplier = settings.SmokeRadius;

                if (settings.SmokeSizeOverTime.Count > 0)
                {
                    sgs._sizeOverTime = new AnimationCurve(settings.SmokeSizeOverTime
                        .Select(k => new Keyframe(k.Time, k.Value))
                        .ToArray());
                }
            }

            if (emission != null)
            {
                var emissionType = typeof(GrenadeEmission);
                if (settings.SmokeDuration != 0)
                    SetField(emissionType, "_removalDelay", emission, settings.SmokeDuration);

                if (settings.SmokeFillSize != 0)
                    SetField(emissionType, "_startFillSize", emission, settings.SmokeFillSize);

                if (settings.SmokeStartSpeed.Count > 0)
                {
                    var ranges = settings.SmokeStartSpeed
                        .Select(s => new Vector2(s.X, s.Y))
                        .ToArray();
                    SetField(emissionType, "_startSpeed", emission, ranges);
                }
            }

            LogInfo($"[AmmoGen Client] Applied smoke settings to grenade {grenade.name}.");
        }
        catch (Exception ex)
        {
            Log.LogError($"[AmmoGen Client] Failed to apply smoke settings: {ex.Message}");
        }
    }

    private static void SetField(Type type, string fieldName, object target, object value)
    {
        var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field == null)
        {
            Log.LogWarning($"[AmmoGen Client] Field {fieldName} not found on {type.Name}.");
            return;
        }
        field.SetValue(target, value);
    }

    public class SmokeSettingsData
    {
        [JsonProperty("smokeRadius")] public float SmokeRadius;
        [JsonProperty("smokeDuration")] public float SmokeDuration;
        [JsonProperty("smokeFillSize")] public float SmokeFillSize;
        [JsonProperty("smokeSizeOverTime")] public List<SmokeSizeKeyframe> SmokeSizeOverTime = new();
        [JsonProperty("smokeStartSpeed")] public List<SmokeSpeedRange> SmokeStartSpeed = new();
    }

    public class SmokeSizeKeyframe
    {
        [JsonProperty("time")] public float Time;
        [JsonProperty("value")] public float Value;
    }

    public class SmokeSpeedRange
    {
        [JsonProperty("x")] public float X;
        [JsonProperty("y")] public float Y;
    }

    // Maps live FlareCartridge instances to their source ammo item so the visual effect can be customized by template ID.
    public static class FlareItemTracker
    {
        internal static readonly ConditionalWeakTable<FlareCartridge, AmmoItemClass> FlareItems = new();
    }

    [HarmonyPatch(typeof(FlareCartridge), "Init")]
    public static class FlareCartridgeInitPatch
    {
        public static void Postfix(FlareCartridge __instance, AmmoItemClass flareCartridge)
        {
            if (__instance == null || flareCartridge == null)
                return;

            try
            {
                FlareItemTracker.FlareItems.Add(__instance, flareCartridge);
                LogInfo($"[AmmoGen Client] Tracked flare cartridge {flareCartridge.TemplateId}.");
            }
            catch (Exception ex)
            {
                Log.LogError($"[AmmoGen Client] Failed to track flare cartridge: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(FlareShotEffectSelector), nameof(FlareShotEffectSelector.SetFlareEffect))]
    public static class FlareShotEffectColorPatch
    {
        public static void Postfix(FlareShotEffectSelector __instance, FlareColorType flareColorType, float lifetime)
        {
            if (__instance == null)
                return;

            try
            {
                // Find the owning FlareCartridge to get the template ID.
                var flareCartridge = __instance.GetComponentInParent<FlareCartridge>();
                if (flareCartridge == null)
                {
                    Log.LogWarning("[AmmoGen Client] FlareShotEffectSelector has no parent FlareCartridge.");
                    return;
                }

                if (!FlareItemTracker.FlareItems.TryGetValue(flareCartridge, out var ammoItem) || ammoItem == null)
                {
                    Log.LogWarning("[AmmoGen Client] No tracked ammo item for FlareCartridge.");
                    return;
                }

                var templateId = ammoItem.TemplateId.ToString().ToLowerInvariant();
                if (!FlareColors.TryGetValue(templateId, out var color))
                    return;

                var flareLight = Traverse.Create(__instance).Field("_flareLight").GetValue<Light>();
                var flareParticles = Traverse.Create(__instance).Field("_flareParticleSystem").GetValue<ParticleSystem>();
                var smokeParticles = Traverse.Create(__instance).Field("_smokeParticleSystem").GetValue<ParticleSystem>();

                if (flareLight != null)
                {
                    flareLight.color = color;
                    LogInfo($"[AmmoGen Client] Applied flare light color {ColorUtility.ToHtmlStringRGB(color)} to {templateId}.");
                }

                if (flareParticles != null)
                {
                    var main = flareParticles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(color);
                }

                if (smokeParticles != null)
                {
                    var main = smokeParticles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(color);
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[AmmoGen Client] Failed to apply flare color: {ex.Message}");
            }
        }
    }
}
