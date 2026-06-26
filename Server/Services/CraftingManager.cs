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
                AddRecipe(def, productions, logger);
            }
            catch (Exception ex)
            {
                logger.LogWithColor($"[AmmoGen] Failed to add crafting recipe for '{def.Name}': {ex.Message}", LogTextColor.Red);
            }
        }
    }

    private static void AddRecipe(
        AmmoDefinition def,
        List<HideoutProduction> productions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var requirements = new List<Requirement>
        {
            new Requirement
            {
                Type = "Area",
                AreaType = (int)HideoutAreas.Workbench,
                RequiredLevel = def.Crafting.WorkbenchLevel,
            }
        };

        foreach (var req in def.Crafting.Requirements)
        {
            requirements.Add(new Requirement
            {
                Type = "Item",
                TemplateId = new MongoId(req.Tpl),
                Count = req.Count,
                IsEncoded = false,
            });
        }

        var recipe = new HideoutProduction
        {
            Id = new MongoId(GenerateProductionId(def.Id)),
            AreaType = HideoutAreas.Workbench,
            Requirements = requirements,
            ProductionTime = def.Crafting.CraftTimeSeconds,
            EndProduct = new MongoId(def.Id),
            Count = def.Crafting.OutputCount,
            ProductionLimitCount = 0,
            NeedFuelForAllProductionTime = false,
            Locked = false,
            IsEncoded = false,
            Continuous = false,
        };

        productions.Add(recipe);
        logger.LogWithColor($"[AmmoGen] Added crafting recipe for {def.Name} (Workbench L{def.Crafting.WorkbenchLevel}, {def.Crafting.OutputCount} rounds)", LogTextColor.Green);
    }

    private static string GenerateProductionId(string ammoId)
    {
        if (ammoId.Length >= 24)
        {
            var chars = ammoId[..24].ToCharArray();
            chars[0] = chars[0] == 'f' ? '0' : (char)(chars[0] + 1);
            return new string(chars);
        }
        return ("p" + ammoId).PadRight(24, '0')[..24];
    }
}
