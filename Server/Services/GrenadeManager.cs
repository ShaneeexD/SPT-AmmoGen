using System.IO;
using System.Text.Json;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Mod;
using AmmoGen.Models;

namespace AmmoGen.Services;

// Registers custom grenade items by cloning an existing grenade template and applying overrides.
public static class GrenadeManager
{
    private const string GrenadeCategoryParentId = "543be6564bdc2df4348b4568";

    public static void RegisterAll(
        CustomItemService customItemService,
        DatabaseService databaseService,
        IReadOnlyList<GrenadeDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var smokeColors = new Dictionary<string, string>();
        var bodyColors = new Dictionary<string, string>();
        foreach (var def in definitions)
        {
            try
            {
                RegisterGrenade(def, customItemService, databaseService, logger);
                if (!string.IsNullOrWhiteSpace(def.Stats.SmokeColor))
                    smokeColors[def.Id] = def.Stats.SmokeColor;
                if (!string.IsNullOrWhiteSpace(def.Stats.BodyColor))
                    bodyColors[def.Id] = def.Stats.BodyColor;
            }
            catch (Exception ex)
            {
                logger.LogWithColor($"[AmmoGen] Failed to register grenade '{def.Name}': {ex.Message}", LogTextColor.Red);
            }
        }

        WriteColorConfig(smokeColors, "smoke_colors.json", logger);
        WriteColorConfig(bodyColors, "body_colors.json", logger);
    }

    private static void RegisterGrenade(
        GrenadeDefinition def,
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
            MinExplosionDistance = def.Stats.MinExplosionDistance,
            MaxExplosionDistance = def.Stats.MaxExplosionDistance,
            FragmentsCount = def.Stats.FragmentsCount > 0 ? def.Stats.FragmentsCount : null,
            FragmentType = def.Stats.FragmentType,
            ExplosionEffectType = def.Stats.ExplosionEffectType,
            ArmorDistanceDistanceDamage = new XYZ
            {
                X = def.Stats.ArmorDistanceDistanceDamage.X,
                Y = def.Stats.ArmorDistanceDistanceDamage.Y,
                Z = def.Stats.ArmorDistanceDistanceDamage.Z,
            },
            Contusion = new XYZ
            {
                X = def.Stats.Contusion.X,
                Y = def.Stats.Contusion.Y,
                Z = def.Stats.Contusion.Z,
            },
            Blindness = new XYZ
            {
                X = def.Stats.Blindness.X,
                Y = def.Stats.Blindness.Y,
                Z = def.Stats.Blindness.Z,
            },
            ContusionDistance = def.Stats.ContusionDistance,
            // TemplateItemProperties has both capitalized and camel-case JSON aliases for some fields.
            // Set both to ensure the value is respected regardless of which serializer path the client reads.
            ExplDelay = def.Stats.ExplDelay,
            explDelay = def.Stats.ExplDelay,
            MinTimeToContactExplode = def.Stats.MinTimeToContactExplode,
            PlayFuzeSound = def.Stats.PlayFuzeSound,
            Strength = def.Stats.Strength,
            ThrowType = string.IsNullOrWhiteSpace(def.Stats.ThrowType)
                ? null
                : Enum.Parse<ThrowWeapType>(def.Stats.ThrowType, true),
            ThrowDamMax = def.Stats.ThrowDamMax,
            Weight = def.Stats.Weight,
        };

        var details = new NewItemFromCloneDetails
        {
            NewId = def.Id,
            ItemTplToClone = def.BaseTpl,
            ParentId = GrenadeCategoryParentId,
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
            logger.LogWithColor($"[AmmoGen] Registered grenade: {def.Name} ({def.Id})", LogTextColor.Green);

            var items = databaseService.GetItems();
            if (items.TryGetValue(def.Id, out var tpl) && tpl.Properties != null)
            {
                tpl.Properties.RarityPvE = def.Economy.RarityPvE;
            }
        }
        else
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for grenade '{def.Name}': {string.Join(", ", result.Errors ?? [])}",
                LogTextColor.Yellow);
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
        return GrenadeCategoryParentId;
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
            logger.LogWithColor($"[AmmoGen] Wrote {fileName} for {colors.Count} grenade(s) to {configPath}", LogTextColor.Green);
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[AmmoGen] Failed to write {fileName}: {ex.Message}", LogTextColor.Red);
        }
    }
}
