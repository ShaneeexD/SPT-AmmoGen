using System.Collections;
using System.Linq;
using System.Reflection;
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
    private enum PatchResult
    {
        Patched,
        NotFound,
        NoChange,
    }

    public static void PatchAll(
        DatabaseService databaseService,
        IReadOnlyList<AmmoDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var items = databaseService.GetItems();
        var patchedMagazines = 0;
        var patchedWeapons = 0;
        var missing = 0;
        var failed = 0;

        foreach (var def in definitions)
        {
            try
            {
                var enabledIds = new List<MongoId> { new MongoId(def.Id) };

                foreach (var magId in def.Filters.PatchMagazines)
                {
                    var result = PatchItem(magId, items, enabledIds, "Cartridges", def.Name, logger);
                    if (result == PatchResult.Patched)
                        patchedMagazines++;
                    else if (result == PatchResult.NotFound)
                        missing++;
                }

                foreach (var weaponId in def.Filters.PatchWeapons)
                {
                    var result = PatchItem(weaponId, items, enabledIds, "Chambers", def.Name, logger);
                    if (result == PatchResult.Patched)
                        patchedWeapons++;
                    else if (result == PatchResult.NotFound)
                        missing++;
                }
            }
            catch (Exception ex)
            {
                failed++;
                logger.LogWithColor($"[AmmoGen] Failed to patch filters for '{def.Name}': {ex.Message}", LogTextColor.Red);
            }
        }

        logger.LogWithColor(
            $"[AmmoGen] Patched {patchedMagazines} magazine(s) and {patchedWeapons} weapon(s) to accept custom ammo.",
            LogTextColor.Green);
        if (missing > 0)
            logger.LogWithColor($"[AmmoGen] {missing} filter target(s) were not found in the database.", LogTextColor.Yellow);
        if (failed > 0)
            logger.LogWithColor($"[AmmoGen] {failed} filter patch attempt(s) failed.", LogTextColor.Red);
    }

    public static void PatchModdedItems(
        DatabaseService databaseService,
        IReadOnlyList<AmmoDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var vanillaItemsPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "SPT_Data", "database", "templates", "items.json");
        if (!File.Exists(vanillaItemsPath))
        {
            logger.LogWithColor("[AmmoGen] Modded filter patch skipped: vanilla items file not found.", LogTextColor.Yellow);
            return;
        }

        var vanillaIds = ModdedItemDumper.LoadVanillaItemIds(vanillaItemsPath);
        var items = databaseService.GetItems();
        var moddedItems = items
            .Where(kvp => !vanillaIds.Contains(kvp.Key.ToString()))
            .Select(kvp => kvp.Value)
            .ToList();

        var baseTplMap = definitions
            .GroupBy(d => d.BaseTpl, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        if (moddedItems.Count == 0 || baseTplMap.Count == 0)
        {
            return;
        }

        var patchedItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in moddedItems)
        {
            try
            {
                if (PatchModdedItem(item, baseTplMap, "Cartridges"))
                    patchedItems.Add(item.Id.ToString());
                if (PatchModdedItem(item, baseTplMap, "Chambers"))
                    patchedItems.Add(item.Id.ToString());
                if (PatchModdedCamoras(item, baseTplMap))
                    patchedItems.Add(item.Id.ToString());
            }
            catch (Exception ex)
            {
                logger.LogWithColor($"[AmmoGen] Failed to patch modded item '{item.Id}': {ex.Message}", LogTextColor.Red);
            }
        }

        if (patchedItems.Count > 0)
        {
            logger.LogWithColor($"[AmmoGen] Patched {patchedItems.Count} modded item(s) to accept custom ammo.", LogTextColor.Green);
        }
    }

    private static PatchResult PatchItem(
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
            return PatchResult.NotFound;
        }

        bool patched = false;

        var slots = slotType == "Cartridges"
            ? item.Properties?.Cartridges
            : item.Properties?.Chambers ?? item.Properties?.Cartridges;

        if (slots != null)
        {
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
            patched = true;
        }

        // Revolver cylinders store ammo in camora slots rather than Cartridges/Chambers.
        var camoraSlots = GetCamoraSlots(item.Properties);
        if (camoraSlots != null)
        {
            foreach (var slot in camoraSlots)
            {
                var slotProps = GetPropertyOrField(slot, "Properties");
                if (slotProps == null)
                    continue;

                var filters = GetPropertyOrField(slotProps, "Filters") as IEnumerable;
                if (filters == null)
                    continue;

                foreach (var slotFilter in filters)
                {
                    var filterList = GetPropertyOrField(slotFilter, "Filter");
                    if (filterList == null)
                        continue;

                    foreach (var ammoId in ammoIds)
                        AddToFilterList(filterList, ammoId.ToString());
                }
            }
            patched = true;
        }

        return patched ? PatchResult.Patched : PatchResult.NoChange;
    }

    private static IEnumerable? GetCamoraSlots(TemplateItemProperties? props)
    {
        if (props == null)
            return null;

        var camoras = GetPropertyOrField(props, "Camoras") as IEnumerable;
        if (camoras != null && camoras.Cast<object>().Any())
            return camoras;

        var slots = GetPropertyOrField(props, "Slots") as IEnumerable;
        if (slots == null)
            return null;

        var camoraSlots = slots.Cast<object>().Where(s =>
        {
            var name = GetPropertyOrField(s, "Name") as string;
            return !string.IsNullOrEmpty(name) && name.StartsWith("camora", StringComparison.OrdinalIgnoreCase);
        }).ToList();

        return camoraSlots.Count > 0 ? camoraSlots : null;
    }

    private static bool AddToFilterList(object filterList, string id)
    {
        if (filterList == null)
            return false;

        var enumerable = filterList as IEnumerable ?? (filterList as IEnumerable<object>);
        if (enumerable == null)
            return false;

        var existing = new HashSet<string>(enumerable.Cast<object>().Select(o => o?.ToString() ?? string.Empty));
        if (existing.Contains(id))
            return false;

        var type = filterList.GetType();
        var elementType = type.IsGenericType
            ? type.GetGenericArguments()[0]
            : typeof(object);
        var value = elementType == typeof(MongoId) || elementType.IsAssignableFrom(typeof(MongoId))
            ? (object)new MongoId(id)
            : id;

        var addMethod = type.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new[] { elementType }, null);
        if (addMethod == null)
            return false;

        addMethod.Invoke(filterList, new[] { value });
        return true;
    }

    private static bool PatchModdedItem(
        TemplateItem item,
        Dictionary<string, List<AmmoDefinition>> baseTplMap,
        string slotType)
    {
        var slots = slotType == "Cartridges"
            ? item.Properties?.Cartridges
            : item.Properties?.Chambers;

        if (slots == null)
            return false;

        var patched = false;
        var itemId = item.Id.ToString();
        foreach (var slot in slots)
        {
            if (slot.Properties?.Filters == null)
                continue;

            foreach (var slotFilter in slot.Properties.Filters)
            {
                var matchingAmmo = GetMatchingAmmo(slotFilter, baseTplMap);
                if (matchingAmmo.Count == 0)
                    continue;

                slotFilter.Filter ??= new HashSet<MongoId>();
                foreach (var ammo in matchingAmmo)
                {
                    if (IsExcluded(ammo, itemId, slotType))
                        continue;
                    if (slotFilter.Filter.Add(new MongoId(ammo.Id)))
                        patched = true;
                }
            }
        }

        return patched;
    }

    private static bool PatchModdedCamoras(
        TemplateItem item,
        Dictionary<string, List<AmmoDefinition>> baseTplMap)
    {
        var camoraSlots = GetCamoraSlots(item.Properties);
        if (camoraSlots == null)
            return false;

        var patched = false;
        var itemId = item.Id.ToString();
        foreach (var slot in camoraSlots)
        {
            var slotProps = GetPropertyOrField(slot, "Properties");
            if (slotProps == null)
                continue;

            var filters = GetPropertyOrField(slotProps, "Filters") as IEnumerable;
            if (filters == null)
                continue;

            foreach (var slotFilter in filters)
            {
                var filterList = GetPropertyOrField(slotFilter, "Filter");
                if (filterList == null)
                    continue;

                var matchingAmmo = GetMatchingAmmo(filterList, baseTplMap, true);
                if (matchingAmmo.Count == 0)
                    continue;

                foreach (var ammo in matchingAmmo)
                {
                    if (IsExcluded(ammo, itemId, "Chambers"))
                        continue;
                    if (AddToFilterList(filterList, ammo.Id))
                        patched = true;
                }
            }
        }

        return patched;
    }

    private static List<AmmoDefinition> GetMatchingAmmo(SlotFilter slotFilter, Dictionary<string, List<AmmoDefinition>> baseTplMap)
    {
        if (slotFilter.Filter == null)
            return [];

        var matching = new List<AmmoDefinition>();
        foreach (var kvp in baseTplMap)
        {
            if (slotFilter.Filter.Any(id => id.ToString().Equals(kvp.Key, StringComparison.OrdinalIgnoreCase)))
            {
                matching.AddRange(kvp.Value);
            }
        }

        return matching;
    }

    private static List<AmmoDefinition> GetMatchingAmmo(object filterList, Dictionary<string, List<AmmoDefinition>> baseTplMap, bool _)
    {
        var matching = new List<AmmoDefinition>();
        foreach (var kvp in baseTplMap)
        {
            if (FilterListContains(filterList, kvp.Key))
            {
                matching.AddRange(kvp.Value);
            }
        }

        return matching;
    }

    private static bool IsExcluded(AmmoDefinition ammo, string itemId, string slotType)
    {
        var exclusions = slotType == "Cartridges"
            ? ammo.ModdedFilterExclusions.ExcludeMagazines
            : ammo.ModdedFilterExclusions.ExcludeWeapons;

        return exclusions.Any(e => e.Equals(itemId, StringComparison.OrdinalIgnoreCase));
    }

    private static bool FilterListContains(object filterList, string id)
    {
        if (filterList == null)
            return false;

        var enumerable = filterList as IEnumerable ?? (filterList as IEnumerable<object>);
        if (enumerable == null)
            return false;

        return enumerable.Cast<object>().Any(o => (o?.ToString() ?? string.Empty).Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    private static object? GetPropertyOrField(object target, string name)
    {
        var type = target.GetType();
        var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop != null)
            return prop.GetValue(target);

        var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return field?.GetValue(target);
    }
}
