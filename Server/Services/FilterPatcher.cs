using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using AmmoGen.Models;

namespace AmmoGen.Services;

// Patches magazine and weapon chamber filters so they accept custom ammo.
public static class FilterPatcher
{
    public static void PatchAll(
        DatabaseService databaseService,
        IReadOnlyList<AmmoDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var items = databaseService.GetItems();

        foreach (var def in definitions)
        {
            try
            {
                var enabledIds = new List<MongoId> { new MongoId(def.Id) };

                foreach (var magId in def.Filters.PatchMagazines)
                    PatchItem(magId, items, enabledIds, "Cartridges", def.Name, logger);

                foreach (var weaponId in def.Filters.PatchWeapons)
                    PatchItem(weaponId, items, enabledIds, "Chambers", def.Name, logger);
            }
            catch (Exception ex)
            {
                logger.LogWithColor($"[AmmoGen] Failed to patch filters for '{def.Name}': {ex.Message}", LogTextColor.Red);
            }
        }
    }

    private static void PatchItem(
        string itemTpl,
        Dictionary<MongoId, TemplateItem> items,
        List<MongoId> ammoIds,
        string slotType,
        string ammoName,
        ISptLogger<AmmoGenPlugin> logger)
    {
        MongoId id = new MongoId(itemTpl);
        if (!items.TryGetValue(id, out var item))
        {
            logger.LogWithColor($"[AmmoGen] Could not patch filters on '{itemTpl}' (not found) for '{ammoName}'.", LogTextColor.Yellow);
            return;
        }

        var slots = slotType == "Cartridges"
            ? item.Properties?.Cartridges
            : item.Properties?.Chambers;

        if (slots == null)
            return;

        foreach (var slot in slots)
        {
            if (slot.Properties?.Filters == null)
                continue;

            foreach (var slotFilter in slot.Properties.Filters)
            {
                slotFilter.Filter ??= new HashSet<MongoId>();
                foreach (var ammoId in ammoIds)
                    slotFilter.Filter.Add(ammoId);
            }
        }

        logger.LogWithColor($"[AmmoGen] Patched {slotType} on '{itemTpl}' to accept '{ammoName}'.", LogTextColor.Green);
    }
}
