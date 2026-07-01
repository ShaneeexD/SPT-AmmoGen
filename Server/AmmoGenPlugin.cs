using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Mod;
using AmmoGen.Models;
using AmmoGen.Services;
using AmmoGen.Validation;

namespace AmmoGen;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.serenity.ammogen";
    public override string Name { get; init; } = "AmmoGen";
    public override string Author { get; init; } = "Serenity";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("2.0.0");
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
        logger.LogWithColor("[AmmoGen] AmmoGen Framework v2.0.0 loading...", LogTextColor.Cyan);
        logger.LogWithColor("[AmmoGen] ====================================", LogTextColor.Cyan);

        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "user", "mods", "AmmoGen", "config", "config.json");
        var config = ModConfig.Load(configPath);
        if (config.Debug)
            logger.LogWithColor($"[AmmoGen] Debug logging enabled (config: {configPath}).", LogTextColor.Gray);

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

            var ammoDefinitions = packs.SelectMany(p => p.Definition.Ammo).ToList();
            var enabledAmmo = ammoDefinitions.Where(d => d.Enabled).ToList();
            var grenadeDefinitions = packs.SelectMany(p => p.Definition.Grenades).ToList();
            var enabledGrenades = grenadeDefinitions.Where(d => d.Enabled).ToList();

            logger.LogWithColor($"[AmmoGen] Loaded {ammoDefinitions.Count} ammo definition(s), {enabledAmmo.Count} enabled.", LogTextColor.Cyan);
            logger.LogWithColor($"[AmmoGen] Loaded {grenadeDefinitions.Count} grenade definition(s), {enabledGrenades.Count} enabled.", LogTextColor.Cyan);

            // Register ammo items into the database via cloning
            AmmoManager.RegisterAll(customItemService, databaseService, enabledAmmo, logger);

            // Register grenade items into the database via cloning
            GrenadeManager.RegisterAll(customItemService, databaseService, enabledGrenades, logger);

            // Patch magazine and weapon filters so the new ammo can be loaded
            FilterPatcher.PatchAll(databaseService, enabledAmmo, logger);

            // Add enabled ammo to vanilla traders
            TraderManager.RegisterAll(databaseService, enabledAmmo, enabledGrenades, logger);

            // Add workbench crafting recipes
            CraftingManager.RegisterAll(databaseService, enabledAmmo, enabledGrenades, logger);

            // Inject items into container loot tables
            LootInjector.InjectAll(databaseService, enabledAmmo, enabledGrenades, logger, config.Debug);

            logger.LogWithColor("[AmmoGen] ====================================", LogTextColor.Cyan);
            logger.LogWithColor($"[AmmoGen] Done! Registered {enabledAmmo.Count} custom ammo type(s) and {enabledGrenades.Count} custom grenade type(s).", LogTextColor.Green);
            logger.LogWithColor("[AmmoGen] ====================================", LogTextColor.Cyan);
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[AmmoGen] Fatal error during load: {ex}", LogTextColor.Red);
        }

        return Task.CompletedTask;
    }
}
