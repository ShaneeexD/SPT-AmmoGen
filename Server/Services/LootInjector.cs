using System.Linq;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Json;
using AmmoGen.Models;

namespace AmmoGen.Services;

// Injects custom ammo into container loot distributions across all locations.
public static class LootInjector
{
    private static readonly Dictionary<string, int> RarityProbabilities = new()
    {
        ["Common"] = 10000,
        ["Rare"] = 5000,
        ["SuperRare"] = 1000,
        ["NotExists"] = 0,
    };

    private record LootInjectionDefinition(
        string Name,
        IReadOnlyList<string> ContainerIds,
        IReadOnlyList<string> ItemsToInject,
        int Probability);

    public static void InjectAll(
        DatabaseService databaseService,
        IReadOnlyList<AmmoDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger,
        bool debug = false)
    {
        var locations = databaseService.GetLocations();
        if (locations == null)
        {
            logger.LogWithColor("[AmmoGen] No locations found in database, skipping loot injection.", LogTextColor.Yellow);
            return;
        }

        var locationDictionary = locations.GetDictionary();
        var processedDefinitions = BuildInjectionDefinitions(definitions, logger);
        if (processedDefinitions.Count == 0)
        {
            logger.LogWithColor("[AmmoGen] No ammo definitions have loot injection enabled, skipping.", LogTextColor.Gray);
            return;
        }

        int locationCount = 0;
        foreach (var location in locationDictionary.Values)
        {
            if (location.StaticLoot == null) continue;
            location.StaticLoot.AddTransformer(staticLoot =>
                TransformStaticLoot(staticLoot, processedDefinitions, logger, debug));
            locationCount++;
        }

        logger.LogWithColor(
            $"[AmmoGen] Registered loot injection transformer for {locationCount} location(s) covering {processedDefinitions.Count} ammo definition(s).",
            LogTextColor.Green);

        foreach (var def in processedDefinitions)
        {
            logger.LogWithColor(
                $"[AmmoGen] {def.Name}: items [{string.Join(", ", def.ItemsToInject)}] -> containers [{string.Join(", ", def.ContainerIds)}] at probability {def.Probability}.",
                LogTextColor.Gray);
        }
    }

    private static List<LootInjectionDefinition> BuildInjectionDefinitions(
        IReadOnlyList<AmmoDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var result = new List<LootInjectionDefinition>();
        foreach (var def in definitions)
        {
            if (!def.Loot.Enabled || def.Loot.ContainerIds.Count == 0) continue;

            var probability = RarityProbabilities.GetValueOrDefault(def.Loot.Rarity, 5000);
            if (probability <= 0) continue;

            var itemsToInject = new List<string>();
            var lootItem = def.Loot.LootItem?.ToLowerInvariant() ?? "ammo";
            if (lootItem == "ammo" || lootItem == "both")
            {
                itemsToInject.Add(def.Id);
            }
            if ((lootItem == "box" || lootItem == "both") && def.AmmoBox.Enabled)
            {
                itemsToInject.Add(def.AmmoBox.Id);
            }

            if (itemsToInject.Count == 0)
            {
                logger.LogWithColor(
                    $"[AmmoGen] Loot injection enabled for '{def.Name}' but no items selected or ammo box not enabled.",
                    LogTextColor.Yellow);
                continue;
            }

            result.Add(new LootInjectionDefinition(def.Name, def.Loot.ContainerIds, itemsToInject, probability));
        }

        return result;
    }

    private static Dictionary<MongoId, StaticLootDetails>? TransformStaticLoot(
        Dictionary<MongoId, StaticLootDetails>? staticLoot,
        IReadOnlyList<LootInjectionDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger,
        bool debug)
    {
        // Verbose per-call logging commented out; re-enable only when needed.
        // _transformCallCount++;
        // var containerCount = staticLoot?.Count ?? 0;
        // logger.LogWithColor(
        //     $"[AmmoGen][DEBUG] TransformStaticLoot called #{_transformCallCount} (containers: {containerCount}).",
        //     LogTextColor.Gray);

        if (staticLoot == null) return staticLoot;

        var injected = 0;
        var containersTouched = new HashSet<string>();
        var containersNotFound = new HashSet<string>();

        foreach (var def in definitions)
        {
            foreach (var containerId in def.ContainerIds)
            {
                if (!staticLoot.TryGetValue(containerId, out var containerLoot) || containerLoot == null)
                {
                    containersNotFound.Add(containerId);
                    continue;
                }

                var distribution = containerLoot.ItemDistribution?.ToList();
                if (distribution == null) continue;

                containersTouched.Add(containerId);

                foreach (var itemId in def.ItemsToInject)
                {
                    var mongoId = new MongoId(itemId);
                    var existing = distribution.FirstOrDefault(d => d.Tpl == mongoId);
                    if (existing != null)
                    {
                        existing.RelativeProbability = def.Probability;
                        if (debug)
                            logger.LogWithColor(
                                $"[AmmoGen][Debug] Updated '{itemId}' probability in '{containerId}' to {def.Probability}.",
                                LogTextColor.Gray);
                    }
                    else
                    {
                        distribution.Add(new ItemDistribution { Tpl = mongoId, RelativeProbability = def.Probability });
                        if (debug)
                            logger.LogWithColor(
                                $"[AmmoGen][Debug] Added '{itemId}' to '{containerId}' with probability {def.Probability}.",
                                LogTextColor.Gray);
                    }
                    injected++;
                }

                containerLoot.ItemDistribution = distribution;
            }
        }

        // Verbose per-call summary logging commented out; re-enable only when needed.
        // logger.LogWithColor(
        //     $"[AmmoGen][DEBUG] Transformer call #{_transformCallCount} finished: injected={injected}, touched={containersTouched.Count}, notFound={containersNotFound.Count}.",
        //     LogTextColor.Gray);

        if (containersNotFound.Count > 0)
        {
            logger.LogWithColor(
                $"[AmmoGen] Warning: {containersNotFound.Count} container ID(s) were not found in static loot: {string.Join(", ", containersNotFound)}.",
                LogTextColor.Yellow);
        }

        return staticLoot;
    }
}
