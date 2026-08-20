using System.Collections;
using System.Linq;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Color = Spectre.Console.Color;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Services.Modding.Custom;
using AmmoGen.Helpers;
using AmmoGen.Models;

namespace AmmoGen.Services;

// Registers custom flare weapons (RSP-30 style) by cloning an existing flare weapon and its cartridge.
public static class FlareManager
{
    // Parent category for ammo items (the flare cartridge).
    private const string AmmoCategoryParentId = "5485a8684bdc2da71d8b4567";

    // Parent category for weapon items (the flare pistol).
    private const string WeaponCategoryParentId = "5447bedf4bdc2d87278b4568";

    public static void RegisterAll(
        CustomItemService customItemService,
        TemplateTable templateTable,
        IReadOnlyList<FlareDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var flareColors = new Dictionary<string, string>();
        var registeredCartridges = 0;
        var registeredHandheld = 0;
        var failed = 0;
        var patchedChambers = 0;
        var patchedSignalPistols = 0;

        foreach (var def in definitions)
        {
            try
            {
                if (!RegisterFlare(def, customItemService, templateTable, logger, out var patched))
                {
                    failed++;
                    continue;
                }

                if (def.Kind == "cartridge")
                {
                    registeredCartridges++;
                    patchedSignalPistols += patched;
                }
                else
                {
                    registeredHandheld++;
                    patchedChambers += patched;
                }

                if (!string.IsNullOrWhiteSpace(def.Stats.FlareColor))
                {
                    var colorId = def.Kind == "cartridge" ? def.Id : def.AmmoId;
                    if (!string.IsNullOrWhiteSpace(colorId))
                        flareColors[colorId] = def.Stats.FlareColor;
                }
            }
            catch (Exception ex)
            {
                failed++;
                logger.LogWithColor($"[AmmoGen] Failed to register flare '{def.Name}': {ex.Message}", Color.Red);
            }
        }

        logger.LogWithColor(
            $"[AmmoGen] Registered {registeredCartridges} flare cartridge(s) and {registeredHandheld} handheld flare(s).",
            Color.Green);
        if (patchedChambers + patchedSignalPistols > 0)
            logger.LogWithColor(
                $"[AmmoGen] Patched {patchedChambers} handheld flare chamber(s) and {patchedSignalPistols} signal pistol chamber(s).",
                Color.Green);
        if (failed > 0)
            logger.LogWithColor($"[AmmoGen] {failed} flare registration(s) failed.", Color.Red);

        PatchSpecialSlotFilters(templateTable, definitions, logger);
        ConfigWriter.WriteJsonConfig(flareColors, "flare_colors.json", logger, "flare(s)");
    }

    private static void PatchSpecialSlotFilters(
        TemplateTable templateTable,
        IReadOnlyList<FlareDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var handheldIds = definitions
            .Where(d => d.Kind != "cartridge")
            .Select(d => d.Id)
            .ToList();

        if (handheldIds.Count == 0)
            return;

        var items = templateTable.Items;
        var patchedCount = 0;

        foreach (var item in items.Values)
        {
            var slots = ReflectionHelper.GetPropertyOrField(item, "Slots") as IEnumerable<object>;
            if (slots == null)
                continue;

            foreach (var slot in slots)
            {
                var slotName = ReflectionHelper.GetPropertyOrField(slot, "Name") as string ?? string.Empty;
                if (!slotName.StartsWith("SpecialSlot", StringComparison.OrdinalIgnoreCase))
                    continue;

                var slotProps = ReflectionHelper.GetPropertyOrField(slot, "Properties");
                if (slotProps == null)
                    continue;

                var filters = ReflectionHelper.GetPropertyOrField(slotProps, "Filters") as IEnumerable<object>;
                if (filters == null)
                    continue;

                foreach (var filter in filters)
                {
                    var filterList = ReflectionHelper.GetPropertyOrField(filter, "Filter") as IList;
                    if (filterList == null)
                        continue;

                    foreach (var id in handheldIds)
                    {
                        if (ReflectionHelper.AddToFilterList(filterList, id))
                            patchedCount++;
                    }
                }
            }
        }

        if (patchedCount > 0)
            logger.LogWithColor($"[AmmoGen] Patched {patchedCount} special slot filter(s) for {handheldIds.Count} handheld flare(s).", Color.Green);
    }

    private static bool RegisterFlare(
        FlareDefinition def,
        CustomItemService customItemService,
        TemplateTable templateTable,
        ISptLogger<AmmoGenPlugin> logger,
        out int patchedCount)
    {
        if (def.Kind == "cartridge")
        {
            return RegisterCartridge(def, customItemService, templateTable, logger, out patchedCount);
        }

        return RegisterHandheldFlare(def, customItemService, templateTable, logger, out patchedCount);
    }

    private static bool RegisterCartridge(
        FlareDefinition def,
        CustomItemService customItemService,
        TemplateTable templateTable,
        ISptLogger<AmmoGenPlugin> logger,
        out int patchedSignalPistols)
    {
        patchedSignalPistols = 0;
        var items = templateTable.Items;
        if (!items.TryGetValue(def.BaseTpl, out var baseCartridge) || baseCartridge.Properties == null)
        {
            logger.LogWithColor($"[AmmoGen] Base flare cartridge '{def.BaseTpl}' not found for '{def.Name}'. Skipping.", Color.Yellow);
            return false;
        }

        var ammoHandbookParentId = !string.IsNullOrWhiteSpace(def.HandbookParentId)
            ? def.HandbookParentId
            : ItemHelper.ResolveHandbookParent(templateTable, def.BaseTpl, AmmoCategoryParentId);

        var ammoOverrides = BuildAmmoOverrides(def);
        var ammoDetails = new NewItemFromCloneDetails
        {
            NewId = def.Id,
            NewItemName = def.Name,
            ItemTplToClone = def.BaseTpl,
            ParentId = AmmoCategoryParentId,
            HandbookParentId = ammoHandbookParentId,
            HandbookPriceRoubles = 0,
            FleaPriceRoubles = 0,
            OverrideProperties = ammoOverrides,
            Locales = ItemHelper.CreateEnLocale(def.Name, def.ShortName, def.Description),
        };

        var ammoResult = customItemService.CreateItemFromClone(ammoDetails);
        if (ammoResult.Success != true)
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for flare cartridge '{def.Name}': {string.Join(", ", ammoResult.Errors ?? [])}",
                Color.Yellow);
            return false;
        }

        ApplyCartridgeOverrides(items, def.Id, def, logger);
        patchedSignalPistols = PatchSignalPistols(templateTable, def.Id, def.Name, logger);

        return true;
    }

    private static bool RegisterHandheldFlare(
        FlareDefinition def,
        CustomItemService customItemService,
        TemplateTable templateTable,
        ISptLogger<AmmoGenPlugin> logger,
        out int patchedChambers)
    {
        patchedChambers = 0;
        var items = templateTable.Items;
        if (!items.TryGetValue(def.BaseTpl, out var baseWeapon) || baseWeapon.Properties == null)
        {
            logger.LogWithColor($"[AmmoGen] Base flare weapon '{def.BaseTpl}' not found for '{def.Name}'. Skipping.", Color.Yellow);
            return false;
        }

        var baseAmmoTpl = !string.IsNullOrWhiteSpace(def.AmmoBaseTpl)
            ? def.AmmoBaseTpl
            : ReflectionHelper.GetPropertyOrField(baseWeapon.Properties, "defAmmo") as string ?? string.Empty;

        if (string.IsNullOrWhiteSpace(baseAmmoTpl) || !items.TryGetValue(baseAmmoTpl, out var baseAmmo))
        {
            logger.LogWithColor($"[AmmoGen] Base flare cartridge '{baseAmmoTpl}' not found for '{def.Name}'. Skipping.", Color.Yellow);
            return false;
        }

        var ammoHandbookParentId = !string.IsNullOrWhiteSpace(def.HandbookParentId)
            ? def.HandbookParentId
            : ItemHelper.ResolveHandbookParent(templateTable, baseAmmoTpl, AmmoCategoryParentId);

        var weaponHandbookParentId = !string.IsNullOrWhiteSpace(def.HandbookParentId)
            ? def.HandbookParentId
            : ItemHelper.ResolveHandbookParent(templateTable, def.BaseTpl, WeaponCategoryParentId);

        var ammoOverrides = BuildAmmoOverrides(def);
        var ammoDetails = new NewItemFromCloneDetails
        {
            NewId = def.AmmoId,
            NewItemName = def.Name,
            ItemTplToClone = baseAmmoTpl,
            ParentId = AmmoCategoryParentId,
            HandbookParentId = ammoHandbookParentId,
            HandbookPriceRoubles = 0,
            FleaPriceRoubles = 0,
            OverrideProperties = ammoOverrides,
            Locales = ItemHelper.CreateEnLocale($"{def.Name} Cartridge", $"{def.ShortName} Cartridge", def.Description),
        };

        var ammoResult = customItemService.CreateItemFromClone(ammoDetails);
        if (ammoResult.Success != true)
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for flare cartridge '{def.Name}': {string.Join(", ", ammoResult.Errors ?? [])}",
                Color.Yellow);
            return false;
        }

        ApplyCartridgeOverrides(items, def.AmmoId, def, logger);

        var weaponOverrides = new TemplateItemProperties
        {
            Name = def.ShortName,
            ShortName = def.ShortName,
            Description = def.Description,
        };

        var weaponParentId = items.TryGetValue(def.BaseTpl, out var baseWeaponTpl) && !string.IsNullOrWhiteSpace(baseWeaponTpl.Parent.ToString())
            ? baseWeaponTpl.Parent.ToString()
            : WeaponCategoryParentId;

        var weaponDetails = new NewItemFromCloneDetails
        {
            NewId = def.Id,
            NewItemName = def.Name,
            ItemTplToClone = def.BaseTpl,
            ParentId = weaponParentId,
            HandbookParentId = weaponHandbookParentId,
            HandbookPriceRoubles = def.Economy.HandbookPriceRoubles,
            FleaPriceRoubles = def.Economy.FleaPriceRoubles,
            OverrideProperties = weaponOverrides,
            Locales = ItemHelper.CreateEnLocale(def.Name, def.ShortName, def.Description),
        };

        var weaponResult = customItemService.CreateItemFromClone(weaponDetails);
        if (weaponResult.Success != true)
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for flare '{def.Name}': {string.Join(", ", weaponResult.Errors ?? [])}",
                Color.Yellow);
            return false;
        }

        if (items.TryGetValue(def.Id, out var weaponTpl) && weaponTpl.Properties != null)
        {
            ItemHelper.ApplyCommonPostRegistration(
                weaponTpl.Properties, def.Economy.RarityPvE, def.Economy.FleaBanned,
                def.Stats.BackgroundColor, def.Stats.BackgroundAlpha);
            ReflectionHelper.SetPropertyOrField(weaponTpl.Properties, "defAmmo", def.AmmoId);

            var weapClass = string.IsNullOrWhiteSpace(def.Stats.WeapClass) ? "specialWeapon" : def.Stats.WeapClass;
            ReflectionHelper.SetPropertyOrField(weaponTpl.Properties, "weapClass", weapClass);
            ReflectionHelper.SetPropertyOrField(weaponTpl.Properties, "WeapClass", weapClass);

            ReflectionHelper.SetPropertyOrField(weaponTpl.Properties, "IsSpecialSlotOnly", def.Stats.IsSpecialSlotOnly);
            ReflectionHelper.SetPropertyOrField(weaponTpl.Properties, "isSpecialSlotOnly", def.Stats.IsSpecialSlotOnly);

            if (PatchChambers(weaponTpl, def.AmmoId, def.Name, logger))
                patchedChambers++;
        }

        return true;
    }

    private static TemplateItemProperties BuildAmmoOverrides(FlareDefinition def)
    {
        var overrides = PropertiesHelper.DeserializeProperties(def.Properties) ?? new TemplateItemProperties();
        overrides.Name = def.ShortName;
        overrides.ShortName = def.ShortName;
        overrides.Description = def.Description;
        overrides.Damage = def.Stats.Damage;
        overrides.InitialSpeed = def.Stats.InitialSpeed;
        overrides.StackMaxSize = def.Stats.StackMaxSize > 0 ? def.Stats.StackMaxSize : null;
        overrides.AmmoLifeTimeSec = def.Stats.AmmoLifeTimeSec;
        overrides.Tracer = def.Stats.Tracer;
        overrides.TracerDistance = def.Stats.TracerDistance;
        overrides.CasingSounds = def.Stats.CasingSounds;
        overrides.MisfireChance = def.Stats.MisfireChance;
        overrides.RicochetChance = def.Stats.RicochetChance;
        overrides.Weight = def.Stats.Weight;

        if (!string.IsNullOrWhiteSpace(def.Stats.TracerColor))
            overrides.TracerColor = def.Stats.TracerColor;

        return overrides;
    }

    private static void ApplyCartridgeOverrides(
        Dictionary<MongoId, TemplateItem> items,
        string ammoId,
        FlareDefinition def,
        ISptLogger<AmmoGenPlugin> logger)
    {
        if (!items.TryGetValue(new MongoId(ammoId), out var ammoTpl) || ammoTpl.Properties == null)
            return;

        ItemHelper.ApplyCommonPostRegistration(
            ammoTpl.Properties, def.Economy.RarityPvE, def.Economy.FleaBanned,
            def.Stats.BackgroundColor, def.Stats.BackgroundAlpha);

        if (def.Stats.FlareTypes.Count > 0)
            ReflectionHelper.SetPropertyOrField(ammoTpl.Properties, "FlareTypes", def.Stats.FlareTypes.ToList());

        if (!string.IsNullOrWhiteSpace(def.Stats.AirDropTemplateId))
            ReflectionHelper.SetPropertyOrField(ammoTpl.Properties, "AirDropTemplateId", def.Stats.AirDropTemplateId);

        if (!string.IsNullOrWhiteSpace(def.Stats.AmmoType))
            ReflectionHelper.SetPropertyOrField(ammoTpl.Properties, "ammoType", def.Stats.AmmoType);
    }

    // Known signal pistols that should accept custom flare cartridges.
    private static readonly string[] SignalPistolIds =
    [
        "620109578d82e67e7911abf2", // ZiD SP-81 26x75 signal pistol
    ];

    private static int PatchSignalPistols(TemplateTable templateTable, string ammoId, string ammoName, ISptLogger<AmmoGenPlugin> logger)
    {
        var items = templateTable.Items;
        var ammoMongoId = new MongoId(ammoId);
        var patchedCount = 0;

        foreach (var pistolId in SignalPistolIds)
        {
            if (!items.TryGetValue(new MongoId(pistolId), out var pistol) || pistol.Properties == null)
            {
                logger.LogWithColor($"[AmmoGen] Signal pistol '{pistolId}' not found; cannot patch cartridge '{ammoName}'.", Color.Yellow);
                continue;
            }

            var chambers = pistol.Properties.Chambers;
            if (chambers == null || !chambers.Any())
            {
                logger.LogWithColor($"[AmmoGen] Signal pistol '{pistolId}' has no chambers; cannot patch cartridge '{ammoName}'.", Color.Yellow);
                continue;
            }

            var added = false;
            foreach (var chamber in chambers)
            {
                if (chamber.Properties?.Filters == null)
                    continue;

                foreach (var filter in chamber.Properties.Filters)
                {
                    filter.Filter ??= new HashSet<MongoId>();
                    if (filter.Filter.Add(ammoMongoId))
                        added = true;
                }
            }

            if (added)
                patchedCount++;
        }

        return patchedCount;
    }

    private static bool PatchChambers(TemplateItem weapon, string ammoId, string ammoName, ISptLogger<AmmoGenPlugin> logger)
    {
        var chambers = weapon.Properties?.Chambers;
        if (chambers == null || !chambers.Any())
            return false;

        var added = false;
        var ammoMongoId = new MongoId(ammoId);
        foreach (var chamber in chambers)
        {
            if (chamber.Properties?.Filters == null)
                continue;

            foreach (var filter in chamber.Properties.Filters)
            {
                filter.Filter ??= new HashSet<MongoId>();
                if (filter.Filter.Add(ammoMongoId))
                    added = true;
            }
        }

        return added;
    }

}
