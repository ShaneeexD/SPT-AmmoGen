using System.IO;
using System.Text.Json;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Mod;
using AmmoGen.Models;

namespace AmmoGen.Services;

// Registers custom grenade items by cloning an existing grenade template and applying overrides.
public static class GrenadeManager
{
    private const string GrenadeCategoryParentId = "543be6564bdc2df4348b4568";

    public static void RegisterAll(
        CustomItemService customItemService,
        DatabaseService databaseService,
        IReadOnlyList<GrenadeDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var smokeColors = new Dictionary<string, string>();
        var bodyColors = new Dictionary<string, string>();
        var smokeSettings = new Dictionary<string, SmokeSettingsConfig>();
        foreach (var def in definitions)
        {
            try
            {
                RegisterGrenade(def, customItemService, databaseService, logger);
                if (!string.IsNullOrWhiteSpace(def.Stats.SmokeColor))
                    smokeColors[def.Id] = def.Stats.SmokeColor;
                if (!string.IsNullOrWhiteSpace(def.Stats.BodyColor))
                    bodyColors[def.Id] = def.Stats.BodyColor;

                var settings = new SmokeSettingsConfig();
                if (def.Stats.OverrideSmokeRadius)
                    settings.SmokeRadius = def.Stats.SmokeRadius;
                if (def.Stats.OverrideSmokeDuration)
                    settings.SmokeDuration = def.Stats.SmokeDuration;
                if (def.Stats.OverrideSmokeFillSize)
                    settings.SmokeFillSize = def.Stats.SmokeFillSize;
                if (def.Stats.OverrideSmokeSizeOverTime)
                    settings.SmokeSizeOverTime = def.Stats.SmokeSizeOverTime;
                if (def.Stats.OverrideSmokeStartSpeed)
                    settings.SmokeStartSpeed = def.Stats.SmokeStartSpeed;

                if (settings.SmokeRadius != 0 || settings.SmokeDuration != 0 || settings.SmokeFillSize != 0 ||
                    settings.SmokeSizeOverTime.Count > 0 || settings.SmokeStartSpeed.Count > 0)
                    smokeSettings[def.Id] = settings;
            }
            catch (Exception ex)
            {
                logger.LogWithColor($"[AmmoGen] Failed to register grenade '{def.Name}': {ex.Message}", LogTextColor.Red);
            }
        }

        WriteColorConfig(smokeColors, "smoke_colors.json", logger);
        WriteColorConfig(bodyColors, "body_colors.json", logger);
        WriteSmokeSettingsConfig(smokeSettings, logger);
    }

    private static void RegisterGrenade(
        GrenadeDefinition def,
        CustomItemService customItemService,
        DatabaseService databaseService,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var handbookParentId = !string.IsNullOrWhiteSpace(def.HandbookParentId)
            ? def.HandbookParentId
            : ResolveHandbookParent(databaseService, def.BaseTpl);

        var overrides = new TemplateItemProperties
        {
            Name = def.ShortName,
            ShortName = def.ShortName,
            Description = def.Description,
            MinExplosionDistance = def.Stats.MinExplosionDistance,
            MaxExplosionDistance = def.Stats.MaxExplosionDistance,
            FragmentsCount = def.Stats.FragmentsCount > 0 ? def.Stats.FragmentsCount : null,
            FragmentType = def.Stats.FragmentType,
            ExplosionEffectType = def.Stats.ExplosionEffectType,
            ArmorDistanceDistanceDamage = new XYZ
            {
                X = def.Stats.ArmorDistanceDistanceDamage.X,
                Y = def.Stats.ArmorDistanceDistanceDamage.Y,
                Z = def.Stats.ArmorDistanceDistanceDamage.Z,
            },
            Contusion = new XYZ
            {
                X = def.Stats.Contusion.X,
                Y = def.Stats.Contusion.Y,
                Z = def.Stats.Contusion.Z,
            },
            Blindness = new XYZ
            {
                X = def.Stats.Blindness.X,
                Y = def.Stats.Blindness.Y,
                Z = def.Stats.Blindness.Z,
            },
            ContusionDistance = def.Stats.ContusionDistance,
            // TemplateItemProperties has both capitalized and camel-case JSON aliases for some fields.
            // Set both to ensure the value is respected regardless of which serializer path the client reads.
            ExplDelay = def.Stats.ExplDelay,
            explDelay = def.Stats.ExplDelay,
            MinTimeToContactExplode = def.Stats.MinTimeToContactExplode,
            PlayFuzeSound = def.Stats.PlayFuzeSound,
            Strength = def.Stats.Strength,
            ThrowType = string.IsNullOrWhiteSpace(def.Stats.ThrowType)
                ? null
                : Enum.Parse<ThrowWeapType>(def.Stats.ThrowType, true),
            ThrowDamMax = def.Stats.ThrowDamMax,
            Weight = def.Stats.Weight,
        };

        var details = new NewItemFromCloneDetails
        {
            NewId = def.Id,
            ItemTplToClone = def.BaseTpl,
            ParentId = GrenadeCategoryParentId,
            HandbookParentId = handbookParentId,
            HandbookPriceRoubles = def.Economy.HandbookPriceRoubles,
            FleaPriceRoubles = def.Economy.FleaPriceRoubles,
            OverrideProperties = overrides,
            Locales = new Dictionary<string, LocaleDetails>
            {
                ["en"] = new LocaleDetails
                {
                    Name = def.Name,
                    ShortName = def.ShortName,
                    Description = def.Description,
                }
            },
        };

        var result = customItemService.CreateItemFromClone(details);

        if (result.Success == true)
        {
            logger.LogWithColor($"[AmmoGen] Registered grenade: {def.Name} ({def.Id})", LogTextColor.Green);

            var items = databaseService.GetItems();
            if (items.TryGetValue(def.Id, out var tpl) && tpl.Properties != null)
            {
                tpl.Properties.RarityPvE = def.Economy.RarityPvE;
                SetPropertyOrField(tpl.Properties, "CanSellOnRagfair", !def.Economy.FleaBanned);

                if (!string.IsNullOrWhiteSpace(def.Stats.BackgroundColor) && def.Stats.BackgroundColor != "default")
                    SetPropertyOrField(tpl.Properties, "BackgroundColor", FormatBackgroundColor(def.Stats.BackgroundColor, def.Stats.BackgroundAlpha));

                // SPT's TemplateItemProperties does not expose these fields directly, so set them via reflection
                // if the underlying cloned template has them.
                if (def.Stats.MinFragmentDamage > 0)
                    SetPropertyOrField(tpl.Properties, "MinFragmentDamage", (float)def.Stats.MinFragmentDamage);
                if (def.Stats.CanPlantOnGround)
                    SetPropertyOrField(tpl.Properties, "CanPlantOnGround", true);
            }
        }
        else
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for grenade '{def.Name}': {string.Join(", ", result.Errors ?? [])}",
                LogTextColor.Yellow);
        }
    }

    private static void SetPropertyOrField(object target, string name, object value)
    {
        var type = target.GetType();
        var prop = type.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(target, value);
            return;
        }
        var field = type.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (field != null)
            field.SetValue(target, value);
    }

    private static string ResolveHandbookParent(DatabaseService databaseService, string baseTpl)
    {
        var items = databaseService.GetItems();
        if (items.TryGetValue(baseTpl, out var baseItem))
        {
            var handbook = databaseService.GetHandbook().Items.FirstOrDefault(h => h.Id == baseTpl);
            if (handbook != null && !string.IsNullOrWhiteSpace(handbook.ParentId))
            {
                return handbook.ParentId;
            }
        }
        return GrenadeCategoryParentId;
    }

    private static void WriteColorConfig(Dictionary<string, string> colors, string fileName, ISptLogger<AmmoGenPlugin> logger)
    {
        if (colors.Count == 0)
            return;

        try
        {
            var configDir = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "user", "mods", "AmmoGen", "config");
            Directory.CreateDirectory(configDir);
            var configPath = System.IO.Path.Combine(configDir, fileName);
            File.WriteAllText(configPath, JsonSerializer.Serialize(colors, new JsonSerializerOptions { WriteIndented = true }));
            logger.LogWithColor($"[AmmoGen] Wrote {fileName} for {colors.Count} grenade(s) to {configPath}", LogTextColor.Green);
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[AmmoGen] Failed to write {fileName}: {ex.Message}", LogTextColor.Red);
        }
    }

    private static void WriteSmokeSettingsConfig(Dictionary<string, SmokeSettingsConfig> settings, ISptLogger<AmmoGenPlugin> logger)
    {
        if (settings.Count == 0)
            return;

        try
        {
            var configDir = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "user", "mods", "AmmoGen", "config");
            Directory.CreateDirectory(configDir);
            var configPath = System.IO.Path.Combine(configDir, "smoke_settings.json");
            File.WriteAllText(configPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            logger.LogWithColor($"[AmmoGen] Wrote smoke_settings.json for {settings.Count} grenade(s) to {configPath}", LogTextColor.Green);
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[AmmoGen] Failed to write smoke_settings.json: {ex.Message}", LogTextColor.Red);
        }
    }

    private static string FormatBackgroundColor(string color, double alpha)
    {
        if (string.IsNullOrWhiteSpace(color) || color == "default")
            return "default";
        if (alpha >= 1)
            return color;

        var namedMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["yellow"] = "#ffff00",
            ["blue"] = "#0000ff",
            ["green"] = "#00ff00",
            ["red"] = "#ff0000",
            ["violet"] = "#ee82ee",
            ["black"] = "#000000",
            ["grey"] = "#808080",
            ["white"] = "#ffffff",
            ["orange"] = "#ffa500",
        };

        var baseHex = namedMap.TryGetValue(color, out var hex) ? hex : color;
        if (!baseHex.StartsWith("#"))
            baseHex = "#ffffff";

        var alphaByte = (byte)Math.Round(Math.Max(0, Math.Min(1, alpha)) * 255);
        return $"{baseHex}{alphaByte:x2}";
    }
}
