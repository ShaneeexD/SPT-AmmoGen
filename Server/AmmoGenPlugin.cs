using Color = Spectre.Console.Color;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Modding.Custom;
using AmmoGen.Models;
using AmmoGen.Services;
using AmmoGen.Validation;

namespace AmmoGen;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.serenity.ammogen";
    public string Name { get; init; } = "AmmoGen";
    public string Author { get; init; } = "Serenity";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("2.4.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.1");
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public string? Url { get; init; }
    public string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.TraderRegistration + 1)]
public class AmmoGenPlugin(
    ISptLogger<AmmoGenPlugin> logger,
    AmmoLoader ammoLoader,
    CustomItemService customItemService,
    TemplateTable templateTable,
    TradersTable tradersTable,
    HideoutTable hideoutTable,
    LocationTable locationTable)
    : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        logger.LogWithColor("[AmmoGen] ====================================", Color.Cyan);
        logger.LogWithColor($"[AmmoGen] AmmoGen Framework v{new ModMetadata().Version} loading...", Color.Cyan);
        logger.LogWithColor("[AmmoGen] ====================================", Color.Cyan);

        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "user", "mods", "AmmoGen", "config", "config.json");
        var config = ModConfig.Load(configPath);
        if (config.Debug)
            logger.LogWithColor($"[AmmoGen] Debug logging enabled (config: {configPath}).", Color.Grey);

        try
        {
            var packs = ammoLoader.LoadAllPacks();
            if (packs.Count == 0)
            {
                logger.LogWithColor(
                    "[AmmoGen] No ammo packs found. Place ammo pack JSON files in: user/mods/AmmoGen/ammo/",
                    Color.Yellow);
                return Task.CompletedTask;
            }

            logger.LogWithColor($"[AmmoGen] Found {packs.Count} ammo pack(s). Processing...", Color.Cyan);

            var ammoDefinitions = packs.SelectMany(p => p.Definition.Ammo).ToList();
            var enabledAmmo = ammoDefinitions.Where(d => d.Enabled).ToList();
            var grenadeDefinitions = packs.SelectMany(p => p.Definition.Grenades).ToList();
            var enabledGrenades = grenadeDefinitions.Where(d => d.Enabled).ToList();
            var flareDefinitions = packs.SelectMany(p => p.Definition.Flares).ToList();
            var enabledFlares = flareDefinitions.Where(d => d.Enabled).ToList();

            logger.LogWithColor($"[AmmoGen] Loaded {ammoDefinitions.Count} ammo definition(s), {enabledAmmo.Count} enabled.", Color.Cyan);
            logger.LogWithColor($"[AmmoGen] Loaded {grenadeDefinitions.Count} grenade definition(s), {enabledGrenades.Count} enabled.", Color.Cyan);
            logger.LogWithColor($"[AmmoGen] Loaded {flareDefinitions.Count} flare definition(s), {enabledFlares.Count} enabled.", Color.Cyan);

            // Register ammo items into the database via cloning
            AmmoManager.RegisterAll(customItemService, templateTable, enabledAmmo, logger);

            // Register grenade items into the database via cloning
            GrenadeManager.RegisterAll(customItemService, templateTable, enabledGrenades, logger);

            // Register flare items into the database via cloning
            FlareManager.RegisterAll(customItemService, templateTable, enabledFlares, logger);

            // Patch magazine and weapon filters so the new ammo can be loaded
            FilterPatcher.PatchAll(templateTable, enabledAmmo, logger);

            // Scan modded weapons/magazines and patch their filters if they accept our ammo base templates
            if (config.PatchModdedItemFilters)
            {
                FilterPatcher.PatchModdedItems(templateTable, enabledAmmo, logger);
            }

            // Add enabled items to vanilla traders
            TraderManager.RegisterAll(tradersTable, enabledAmmo, enabledGrenades, enabledFlares, logger);

            // Add workbench crafting recipes
            CraftingManager.RegisterAll(hideoutTable, enabledAmmo, enabledGrenades, enabledFlares, logger);

            // Inject items into container loot tables
            LootInjector.InjectAll(locationTable, enabledAmmo, enabledGrenades, enabledFlares, logger, config.Debug);

            logger.LogWithColor("[AmmoGen] ====================================", Color.Cyan);
            logger.LogWithColor($"[AmmoGen] Done! Registered {enabledAmmo.Count} custom ammo type(s), {enabledGrenades.Count} custom grenade type(s), and {enabledFlares.Count} custom flare type(s).", Color.Green);
            logger.LogWithColor("[AmmoGen] ====================================", Color.Cyan);
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[AmmoGen] Fatal error during load: {ex}", Color.Red);
        }

        return Task.CompletedTask;
    }
}
