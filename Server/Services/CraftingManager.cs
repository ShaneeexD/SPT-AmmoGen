using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using AmmoGen.Models;

namespace AmmoGen.Services;

// Adds hideout Workbench production recipes for custom ammo types.
public static class CraftingManager
{
    public static void RegisterAll(
        DatabaseService databaseService,
        IReadOnlyList<AmmoDefinition> definitions,
        IReadOnlyList<GrenadeDefinition> grenades,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var hideout = databaseService.GetHideout();
        if (hideout?.Production?.Recipes == null)
        {
            logger.LogWithColor("[AmmoGen] Could not access hideout production recipes. Crafting will not be added.", LogTextColor.Red);
            return;
        }

        var productions = hideout.Production.Recipes;

        foreach (var def in definitions)
        {
            if (!def.Crafting.Enabled)
                continue;

            try
            {
                AddRecipe(def.Id, def.Name, def.Crafting, productions, logger);
            }
            catch (Exception ex)
            {
                logger.LogWithColor($"[AmmoGen] Failed to add crafting recipe for '{def.Name}': {ex.Message}", LogTextColor.Red);
            }
        }

        foreach (var def in grenades)
        {
            if (!def.Crafting.Enabled)
                continue;

            try
            {
                AddRecipe(def.Id, def.Name, def.Crafting, productions, logger);
            }
            catch (Exception ex)
            {
                logger.LogWithColor($"[AmmoGen] Failed to add crafting recipe for grenade '{def.Name}': {ex.Message}", LogTextColor.Red);
            }
        }
    }

    private static void AddRecipe(
        string itemId,
        string itemName,
        CraftingEntry crafting,
        List<HideoutProduction> productions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var requirements = new List<Requirement>
        {
            new Requirement
            {
                Type = "Area",
                AreaType = (int)HideoutAreas.Workbench,
                RequiredLevel = crafting.WorkbenchLevel,
            }
        };

        foreach (var req in crafting.Requirements)
        {
            requirements.Add(new Requirement
            {
                Type = "Item",
                TemplateId = new MongoId(req.Tpl),
                Count = req.Count,
                IsEncoded = false,
            });
        }

        if (productions.Any(p => p.Id == itemId))
        {
            logger.LogWithColor($"[AmmoGen] Skipping crafting recipe for {itemName}: a recipe with ID '{itemId}' already exists.", LogTextColor.Yellow);
            return;
        }

        var recipe = new HideoutProduction
        {
            Id = new MongoId(itemId),
            AreaType = HideoutAreas.Workbench,
            Requirements = requirements,
            ProductionTime = crafting.CraftTimeSeconds,
            EndProduct = new MongoId(itemId),
            Count = crafting.OutputCount,
            ProductionLimitCount = 0,
            NeedFuelForAllProductionTime = false,
            Locked = false,
            IsEncoded = false,
            Continuous = false,
        };

        productions.Add(recipe);
        logger.LogWithColor($"[AmmoGen] Added crafting recipe for {itemName} (Workbench L{crafting.WorkbenchLevel}, {crafting.OutputCount} items)", LogTextColor.Green);
    }
}
