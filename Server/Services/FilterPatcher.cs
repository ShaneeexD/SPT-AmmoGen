using System.Collections;
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

        if (patched)
            logger.LogWithColor($"[AmmoGen] Patched {slotType} on '{itemTpl}' to accept '{ammoName}'.", LogTextColor.Green);
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
