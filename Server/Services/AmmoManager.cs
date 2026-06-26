using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Mod;
using AmmoGen.Models;

namespace AmmoGen.Services;

// Registers custom ammo items by cloning an existing ammo template and applying overrides.
public static class AmmoManager
{
    // Default parent category for ammo items.
    private const string AmmoCategoryParentId = "5485a8684bdc2da71d8b4567";

    // Parent category for ammo boxes.
    private const string AmmoBoxParentId = "543be5cb4bdc2deb348b4568";

    public static void RegisterAll(
        CustomItemService customItemService,
        DatabaseService databaseService,
        IReadOnlyList<AmmoDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        foreach (var def in definitions)
        {
            try
            {
                RegisterAmmo(def, customItemService, databaseService, logger);
            }
            catch (Exception ex)
            {
                logger.LogWithColor($"[AmmoGen] Failed to register ammo '{def.Name}': {ex.Message}", LogTextColor.Red);
            }
        }

        foreach (var def in definitions)
        {
            if (!def.AmmoBox.Enabled) continue;
            try
            {
                RegisterAmmoBox(def, customItemService, databaseService, logger);
            }
            catch (Exception ex)
            {
                logger.LogWithColor($"[AmmoGen] Failed to register ammo box for '{def.Name}': {ex.Message}", LogTextColor.Red);
            }
        }
    }

    private static void RegisterAmmo(
        AmmoDefinition def,
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
            Damage = def.Stats.Damage,
            PenetrationPower = def.Stats.PenetrationPower,
            ArmorDamage = def.Stats.ArmorDamage,
            InitialSpeed = def.Stats.InitialSpeed,
            AmmoAccr = def.Stats.AmmoAccr,
            AmmoRec = def.Stats.AmmoRec,
            StackMaxSize = def.Stats.StackMaxSize > 0 ? def.Stats.StackMaxSize : null,
        };

        var details = new NewItemFromCloneDetails
        {
            NewId = def.Id,
            ItemTplToClone = def.BaseTpl,
            ParentId = AmmoCategoryParentId,
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
            logger.LogWithColor($"[AmmoGen] Registered: {def.Name} ({def.Id})", LogTextColor.Green);

            // Apply rarity override if it differs from the cloned template
            var items = databaseService.GetItems();
            if (items.TryGetValue(def.Id, out var tpl) && tpl.Properties != null)
            {
                tpl.Properties.RarityPvE = def.Economy.RarityPvE;
            }
        }
        else
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for '{def.Name}': {string.Join(", ", result.Errors ?? [])}",
                LogTextColor.Yellow);
        }
    }

    private static void RegisterAmmoBox(
        AmmoDefinition def,
        CustomItemService customItemService,
        DatabaseService databaseService,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var box = def.AmmoBox;
        var overrides = new TemplateItemProperties
        {
            Name = box.ShortName,
            ShortName = box.ShortName,
            Description = box.Description,
        };

        var details = new NewItemFromCloneDetails
        {
            NewId = box.Id,
            ItemTplToClone = box.BaseTpl,
            ParentId = AmmoBoxParentId,
            HandbookPriceRoubles = box.HandbookPriceRoubles,
            FleaPriceRoubles = 0,
            OverrideProperties = overrides,
            Locales = new Dictionary<string, LocaleDetails>
            {
                ["en"] = new LocaleDetails
                {
                    Name = box.Name,
                    ShortName = box.ShortName,
                    Description = box.Description,
                }
            },
        };

        var result = customItemService.CreateItemFromClone(details);

        if (result.Success != true)
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for ammo box '{box.Name}': {string.Join(", ", result.Errors ?? [])}",
                LogTextColor.Yellow);
            return;
        }

        logger.LogWithColor($"[AmmoGen] Registered ammo box: {box.Name} ({box.Id})", LogTextColor.Green);

        try
        {
            var items = databaseService.GetItems();
            if (items.TryGetValue(box.Id, out var boxItem) && boxItem.Properties != null)
            {
                if (boxItem.Properties.StackSlots is not null)
                {
                    foreach (var slot in boxItem.Properties.StackSlots)
                    {
                        if (slot is null)
                        {
                            continue;
                        }

                        slot.MaxCount = box.Count;
                        if (slot.Properties?.Filters is not null)
                        {
                            foreach (var filter in slot.Properties.Filters)
                            {
                                if (filter is null)
                                {
                                    continue;
                                }

                                filter.Filter = new HashSet<MongoId> { new MongoId(def.Id) };
                            }
                        }
                    }
                }

                boxItem.Properties.RarityPvE = box.RarityPvE;
            }
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[AmmoGen] Created ammo box '{box.Name}' but failed to patch StackSlots: {ex.Message}", LogTextColor.Yellow);
        }
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
        return AmmoCategoryParentId;
    }
}
