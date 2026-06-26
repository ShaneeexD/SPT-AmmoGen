using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Mod;
using AmmoGen.Services;
using AmmoGen.Validation;

namespace AmmoGen;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.serenity.ammogen";
    public override string Name { get; init; } = "AmmoGen";
    public override string Author { get; init; } = "Serenity";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.1.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("4.0.13");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; } = false;
    public override string? License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.TraderRegistration - 1)]
public class AmmoGenPlugin(
    ISptLogger<AmmoGenPlugin> logger,
    AmmoLoader ammoLoader,
    CustomItemService customItemService,
    DatabaseService databaseService)
    : IOnLoad
{
    public Task OnLoad()
    {
        logger.LogWithColor("[AmmoGen] ====================================", LogTextColor.Cyan);
        logger.LogWithColor("[AmmoGen] AmmoGen Framework v1.1.0 loading...", LogTextColor.Cyan);
        logger.LogWithColor("[AmmoGen] ====================================", LogTextColor.Cyan);

        try
        {
            var packs = ammoLoader.LoadAllPacks();
            if (packs.Count == 0)
            {
                logger.LogWithColor(
                    "[AmmoGen] No ammo packs found. Place ammo pack JSON files in: user/mods/AmmoGen/ammo/",
                    LogTextColor.Yellow);
                return Task.CompletedTask;
            }

            logger.LogWithColor($"[AmmoGen] Found {packs.Count} ammo pack(s). Processing...", LogTextColor.Cyan);

            var definitions = packs.SelectMany(p => p.Definition.Ammo).ToList();
            var enabledDefinitions = definitions.Where(d => d.Enabled).ToList();

            logger.LogWithColor($"[AmmoGen] Loaded {definitions.Count} ammo definition(s), {enabledDefinitions.Count} enabled.", LogTextColor.Cyan);

            // Register ammo items into the database via cloning
            AmmoManager.RegisterAll(customItemService, databaseService, enabledDefinitions, logger);

            // Patch magazine and weapon filters so the new ammo can be loaded
            FilterPatcher.PatchAll(databaseService, enabledDefinitions, logger);

            // Add enabled ammo to vanilla traders
            TraderManager.RegisterAll(databaseService, enabledDefinitions, logger);

            // Add workbench crafting recipes
            CraftingManager.RegisterAll(databaseService, enabledDefinitions, logger);

            // Inject ammo into container loot tables
            LootInjector.InjectAll(databaseService, enabledDefinitions, logger);

            logger.LogWithColor("[AmmoGen] ====================================", LogTextColor.Cyan);
            logger.LogWithColor($"[AmmoGen] Done! Registered {enabledDefinitions.Count} custom ammo type(s).", LogTextColor.Green);
            logger.LogWithColor("[AmmoGen] ====================================", LogTextColor.Cyan);
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[AmmoGen] Fatal error during load: {ex}", LogTextColor.Red);
        }

        return Task.CompletedTask;
    }
}
