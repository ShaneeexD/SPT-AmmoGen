using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
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

    public static void InjectAll(
        DatabaseService databaseService,
        IReadOnlyList<AmmoDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        dynamic? locations = databaseService.GetLocations();
        if (locations == null)
        {
            logger.LogWithColor("[AmmoGen] No locations found in database, skipping loot injection.", LogTextColor.Yellow);
            return;
        }

        foreach (var def in definitions)
        {
            if (!def.Loot.Enabled || def.Loot.ContainerIds.Count == 0) continue;

            try
            {
                var probability = RarityProbabilities.GetValueOrDefault(def.Loot.Rarity, 5000);
                if (probability <= 0) continue;

                int injected = 0;
                foreach (dynamic location in locations)
                {
                    var loc = (dynamic)location;
                    var staticLoot = loc.Value?.StaticLoot;
                    if (staticLoot == null) continue;

                    foreach (var containerId in def.Loot.ContainerIds)
                    {
                        var containerLoot = staticLoot[containerId];
                        if (containerLoot == null) continue;

                        var distribution = containerLoot.ItemDistribution;
                        if (distribution == null) continue;

                        var existing = ((IEnumerable<dynamic>)distribution).FirstOrDefault(d => d.Tpl == def.Id);
                        if (existing != null)
                        {
                            existing.RelativeProbability = probability;
                        }
                        else
                        {
                            distribution.Add(new { Tpl = def.Id, RelativeProbability = probability });
                        }
                        injected++;
                    }
                }

                logger.LogWithColor($"[AmmoGen] Injected {def.Name} into {injected} container loot entries.", LogTextColor.Green);
            }
            catch (Exception ex)
            {
                logger.LogWithColor($"[AmmoGen] Failed to inject loot for '{def.Name}': {ex.Message}", LogTextColor.Red);
            }
        }
    }
}
