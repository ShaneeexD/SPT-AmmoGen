using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Mod;
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
        DatabaseService databaseService,
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
                if (!RegisterFlare(def, customItemService, databaseService, logger, out var patched))
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
                logger.LogWithColor($"[AmmoGen] Failed to register flare '{def.Name}': {ex.Message}", LogTextColor.Red);
            }
        }

        logger.LogWithColor(
            $"[AmmoGen] Registered {registeredCartridges} flare cartridge(s) and {registeredHandheld} handheld flare(s).",
            LogTextColor.Green);
        if (patchedChambers + patchedSignalPistols > 0)
            logger.LogWithColor(
                $"[AmmoGen] Patched {patchedChambers} handheld flare chamber(s) and {patchedSignalPistols} signal pistol chamber(s).",
                LogTextColor.Green);
        if (failed > 0)
            logger.LogWithColor($"[AmmoGen] {failed} flare registration(s) failed.", LogTextColor.Red);

        PatchSpecialSlotFilters(databaseService, definitions, logger);
        WriteColorConfig(flareColors, "flare_colors.json", logger);
    }

    private static void PatchSpecialSlotFilters(
        DatabaseService databaseService,
        IReadOnlyList<FlareDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var handheldIds = definitions
            .Where(d => d.Kind != "cartridge")
            .Select(d => d.Id)
            .ToList();

        if (handheldIds.Count == 0)
            return;

        var items = databaseService.GetItems();
        var patchedCount = 0;

        foreach (var item in items.Values)
        {
            var slots = GetPropertyOrField(item, "Slots") as IEnumerable<object>;
            if (slots == null)
                continue;

            foreach (var slot in slots)
            {
                var slotName = GetPropertyOrField(slot, "Name") as string ?? string.Empty;
                if (!slotName.StartsWith("SpecialSlot", StringComparison.OrdinalIgnoreCase))
                    continue;

                var slotProps = GetPropertyOrField(slot, "Properties");
                if (slotProps == null)
                    continue;

                var filters = GetPropertyOrField(slotProps, "Filters") as IEnumerable<object>;
                if (filters == null)
                    continue;

                foreach (var filter in filters)
                {
                    var filterList = GetPropertyOrField(filter, "Filter") as IList;
                    if (filterList == null)
                        continue;

                    foreach (var id in handheldIds)
                    {
                        if (AddToFilterList(filterList, id))
                            patchedCount++;
                    }
                }
            }
        }

        if (patchedCount > 0)
            logger.LogWithColor($"[AmmoGen] Patched {patchedCount} special slot filter(s) for {handheldIds.Count} handheld flare(s).", LogTextColor.Green);
    }

    private static bool RegisterFlare(
        FlareDefinition def,
        CustomItemService customItemService,
        DatabaseService databaseService,
        ISptLogger<AmmoGenPlugin> logger,
        out int patchedCount)
    {
        if (def.Kind == "cartridge")
        {
            return RegisterCartridge(def, customItemService, databaseService, logger, out patchedCount);
        }

        return RegisterHandheldFlare(def, customItemService, databaseService, logger, out patchedCount);
    }

    private static bool RegisterCartridge(
        FlareDefinition def,
        CustomItemService customItemService,
        DatabaseService databaseService,
        ISptLogger<AmmoGenPlugin> logger,
        out int patchedSignalPistols)
    {
        patchedSignalPistols = 0;
        var items = databaseService.GetItems();
        if (!items.TryGetValue(def.BaseTpl, out var baseCartridge) || baseCartridge.Properties == null)
        {
            logger.LogWithColor($"[AmmoGen] Base flare cartridge '{def.BaseTpl}' not found for '{def.Name}'. Skipping.", LogTextColor.Yellow);
            return false;
        }

        var ammoHandbookParentId = !string.IsNullOrWhiteSpace(def.HandbookParentId)
            ? def.HandbookParentId
            : ResolveAmmoHandbookParent(databaseService, def.BaseTpl);

        var ammoOverrides = BuildAmmoOverrides(def);
        var ammoDetails = new NewItemFromCloneDetails
        {
            NewId = def.Id,
            ItemTplToClone = def.BaseTpl,
            ParentId = AmmoCategoryParentId,
            HandbookParentId = ammoHandbookParentId,
            HandbookPriceRoubles = 0,
            FleaPriceRoubles = 0,
            OverrideProperties = ammoOverrides,
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

        var ammoResult = customItemService.CreateItemFromClone(ammoDetails);
        if (ammoResult.Success != true)
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for flare cartridge '{def.Name}': {string.Join(", ", ammoResult.Errors ?? [])}",
                LogTextColor.Yellow);
            return false;
        }

        ApplyCartridgeOverrides(items, def.Id, def, logger);
        patchedSignalPistols = PatchSignalPistols(databaseService, def.Id, def.Name, logger);

        return true;
    }

    private static bool RegisterHandheldFlare(
        FlareDefinition def,
        CustomItemService customItemService,
        DatabaseService databaseService,
        ISptLogger<AmmoGenPlugin> logger,
        out int patchedChambers)
    {
        patchedChambers = 0;
        var items = databaseService.GetItems();
        if (!items.TryGetValue(def.BaseTpl, out var baseWeapon) || baseWeapon.Properties == null)
        {
            logger.LogWithColor($"[AmmoGen] Base flare weapon '{def.BaseTpl}' not found for '{def.Name}'. Skipping.", LogTextColor.Yellow);
            return false;
        }

        var baseAmmoTpl = !string.IsNullOrWhiteSpace(def.AmmoBaseTpl)
            ? def.AmmoBaseTpl
            : GetPropertyOrField(baseWeapon.Properties, "defAmmo") as string ?? string.Empty;

        if (string.IsNullOrWhiteSpace(baseAmmoTpl) || !items.TryGetValue(baseAmmoTpl, out var baseAmmo))
        {
            logger.LogWithColor($"[AmmoGen] Base flare cartridge '{baseAmmoTpl}' not found for '{def.Name}'. Skipping.", LogTextColor.Yellow);
            return false;
        }

        var ammoHandbookParentId = !string.IsNullOrWhiteSpace(def.HandbookParentId)
            ? def.HandbookParentId
            : ResolveAmmoHandbookParent(databaseService, baseAmmoTpl);

        var weaponHandbookParentId = !string.IsNullOrWhiteSpace(def.HandbookParentId)
            ? def.HandbookParentId
            : ResolveWeaponHandbookParent(databaseService, def.BaseTpl);

        var ammoOverrides = BuildAmmoOverrides(def);
        var ammoDetails = new NewItemFromCloneDetails
        {
            NewId = def.AmmoId,
            ItemTplToClone = baseAmmoTpl,
            ParentId = AmmoCategoryParentId,
            HandbookParentId = ammoHandbookParentId,
            HandbookPriceRoubles = 0,
            FleaPriceRoubles = 0,
            OverrideProperties = ammoOverrides,
            Locales = new Dictionary<string, LocaleDetails>
            {
                ["en"] = new LocaleDetails
                {
                    Name = $"{def.Name} Cartridge",
                    ShortName = $"{def.ShortName} Cartridge",
                    Description = def.Description,
                }
            },
        };

        var ammoResult = customItemService.CreateItemFromClone(ammoDetails);
        if (ammoResult.Success != true)
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for flare cartridge '{def.Name}': {string.Join(", ", ammoResult.Errors ?? [])}",
                LogTextColor.Yellow);
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
            ItemTplToClone = def.BaseTpl,
            ParentId = weaponParentId,
            HandbookParentId = weaponHandbookParentId,
            HandbookPriceRoubles = def.Economy.HandbookPriceRoubles,
            FleaPriceRoubles = def.Economy.FleaPriceRoubles,
            OverrideProperties = weaponOverrides,
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

        var weaponResult = customItemService.CreateItemFromClone(weaponDetails);
        if (weaponResult.Success != true)
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for flare '{def.Name}': {string.Join(", ", weaponResult.Errors ?? [])}",
                LogTextColor.Yellow);
            return false;
        }

        if (items.TryGetValue(def.Id, out var weaponTpl) && weaponTpl.Properties != null)
        {
            weaponTpl.Properties.RarityPvE = def.Economy.RarityPvE;
            SetPropertyOrField(weaponTpl.Properties, "CanSellOnRagfair", !def.Economy.FleaBanned);
            SetPropertyOrField(weaponTpl.Properties, "defAmmo", def.AmmoId);

            if (!string.IsNullOrWhiteSpace(def.Stats.BackgroundColor) && def.Stats.BackgroundColor != "default")
                SetPropertyOrField(weaponTpl.Properties, "BackgroundColor", FormatBackgroundColor(def.Stats.BackgroundColor, def.Stats.BackgroundAlpha));

            var weapClass = string.IsNullOrWhiteSpace(def.Stats.WeapClass) ? "specialWeapon" : def.Stats.WeapClass;
            SetPropertyOrField(weaponTpl.Properties, "weapClass", weapClass);
            SetPropertyOrField(weaponTpl.Properties, "WeapClass", weapClass);

            SetPropertyOrField(weaponTpl.Properties, "IsSpecialSlotOnly", def.Stats.IsSpecialSlotOnly);
            SetPropertyOrField(weaponTpl.Properties, "isSpecialSlotOnly", def.Stats.IsSpecialSlotOnly);

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

        ammoTpl.Properties.RarityPvE = def.Economy.RarityPvE;
        SetPropertyOrField(ammoTpl.Properties, "CanSellOnRagfair", !def.Economy.FleaBanned);

        if (!string.IsNullOrWhiteSpace(def.Stats.BackgroundColor) && def.Stats.BackgroundColor != "default")
            SetPropertyOrField(ammoTpl.Properties, "BackgroundColor", FormatBackgroundColor(def.Stats.BackgroundColor, def.Stats.BackgroundAlpha));

        if (def.Stats.FlareTypes.Count > 0)
            SetPropertyOrField(ammoTpl.Properties, "FlareTypes", def.Stats.FlareTypes.ToList());

        if (!string.IsNullOrWhiteSpace(def.Stats.AirDropTemplateId))
            SetPropertyOrField(ammoTpl.Properties, "AirDropTemplateId", def.Stats.AirDropTemplateId);

        if (!string.IsNullOrWhiteSpace(def.Stats.AmmoType))
            SetPropertyOrField(ammoTpl.Properties, "ammoType", def.Stats.AmmoType);
    }

    // Known signal pistols that should accept custom flare cartridges.
    private static readonly string[] SignalPistolIds =
    [
        "620109578d82e67e7911abf2", // ZiD SP-81 26x75 signal pistol
    ];

    private static int PatchSignalPistols(DatabaseService databaseService, string ammoId, string ammoName, ISptLogger<AmmoGenPlugin> logger)
    {
        var items = databaseService.GetItems();
        var ammoMongoId = new MongoId(ammoId);
        var patchedCount = 0;

        foreach (var pistolId in SignalPistolIds)
        {
            if (!items.TryGetValue(new MongoId(pistolId), out var pistol) || pistol.Properties == null)
            {
                logger.LogWithColor($"[AmmoGen] Signal pistol '{pistolId}' not found; cannot patch cartridge '{ammoName}'.", LogTextColor.Yellow);
                continue;
            }

            var chambers = pistol.Properties.Chambers;
            if (chambers == null || !chambers.Any())
            {
                logger.LogWithColor($"[AmmoGen] Signal pistol '{pistolId}' has no chambers; cannot patch cartridge '{ammoName}'.", LogTextColor.Yellow);
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
            logger.LogWithColor($"[AmmoGen] Wrote {fileName} for {colors.Count} flare(s) to {configPath}", LogTextColor.Green);
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[AmmoGen] Failed to write {fileName}: {ex.Message}", LogTextColor.Red);
        }
    }

    private static void SetPropertyOrField(object target, string name, object value)
    {
        var type = target.GetType();
        var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(target, ConvertValue(value, prop.PropertyType));
            return;
        }
        var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (field != null)
            field.SetValue(target, ConvertValue(value, field.FieldType));
    }

    private static object? ConvertValue(object value, Type targetType)
    {
        if (value == null)
            return value;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (underlyingType == typeof(MongoId) && value is string str)
            return new MongoId(str);

        if (targetType.IsEnum && value is string enumStr)
            return Enum.Parse(targetType, enumStr, true);

        if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(underlyingType))
            return Convert.ChangeType(value, underlyingType, System.Globalization.CultureInfo.InvariantCulture);

        return value;
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

    private static string ResolveAmmoHandbookParent(DatabaseService databaseService, string baseTpl)
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
        return AmmoCategoryParentId;
    }

    private static string ResolveWeaponHandbookParent(DatabaseService databaseService, string baseTpl)
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
        return WeaponCategoryParentId;
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
