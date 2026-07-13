using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace AmmoGenClient;

[BepInPlugin("com.serenity.ammogen.client", "AmmoGen Client Debug", "2.3.0")]
public class AmmoGenClientPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log = null!;
    internal static HashSet<string> WatchedTemplateIds = new HashSet<string>();
    internal static ConfigEntry<bool> DebugEnabled = null!;

    private void Awake()
    {
        Log = Logger;
        DebugEnabled = Config.Bind("Debug", "Enabled", true, "Log when watched items spawn in raid");
        LoadWatchlist();
        SmokeColorManager.Initialize(Logger);
        try
        {
            BundleInjector.Init(Logger);
            new Harmony("com.serenity.ammogen.client").PatchAll();
            StartCoroutine(InjectWhenReady());
            Log.LogInfo($"[AmmoGenClient] Loaded. Watching {WatchedTemplateIds.Count} template(s). Debug enabled: {DebugEnabled.Value}.");
        }
        catch (System.Exception ex)
        {
            Log.LogError($"[AmmoGenClient] Failed to apply Harmony patches: {ex}");
        }
    }

    private IEnumerator InjectWhenReady()
    {
        yield return new WaitUntil(() => Singleton<IEasyAssets>.Instance != null);
        BundleInjector.InjectAll();
        Log.LogInfo("[AmmoGenClient] All AmmoGen bundles fully loaded.");
    }

    private void LoadWatchlist()
    {
        WatchedTemplateIds.Clear();
        var configDir = Path.GetDirectoryName(Config.ConfigFilePath);
        if (string.IsNullOrEmpty(configDir))
        {
            Log.LogError("[AmmoGenClient] Could not determine config directory.");
            return;
        }

        var watchlistPath = Path.Combine(configDir, "AmmoGenClient", "watchlist.json");

        if (!File.Exists(watchlistPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(watchlistPath));
            File.WriteAllText(watchlistPath, "[\"REPLACE_WITH_TEMPLATE_ID_1\", \"REPLACE_WITH_TEMPLATE_ID_2\"]");
            Log.LogWarning($"[AmmoGenClient] Created watchlist template at {watchlistPath}. Add your AmmoGen item IDs there.");
            return;
        }

        try
        {
            var raw = File.ReadAllText(watchlistPath);
            var ids = JsonConvert.DeserializeObject<List<string>>(raw);
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                        WatchedTemplateIds.Add(id.Trim().ToLowerInvariant());
                }
            }
        }
        catch (System.Exception ex)
        {
            Log.LogError($"[AmmoGenClient] Failed to read watchlist: {ex.Message}");
        }
    }

    // If a custom prefab path is requested before the bundle is injected, inject it on demand.
    [HarmonyPatch(typeof(DependencyGraphClass<IEasyBundle>), "GetNode")]
    internal static class GetNodePatch
    {
        static void Prefix(DependencyGraphClass<IEasyBundle> __instance, string key)
        {
            try
            {
                BundleInjector.InjectSingle(__instance, key);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AmmoGenClient] On-demand bundle injection failed: {ex}");
            }
        }
    }
}
