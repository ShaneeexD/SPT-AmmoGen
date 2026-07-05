using System.Reflection;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
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
            LightBleedingDelta = def.Stats.LightBleedingDelta != 0 ? def.Stats.LightBleedingDelta : null,
            HeavyBleedingDelta = def.Stats.HeavyBleedingDelta != 0 ? def.Stats.HeavyBleedingDelta : null,
            DurabilityBurnModificator = def.Stats.DurabilityBurnModificator,
            BallisticCoeficient = def.Stats.BallisticCoeficient,
            ProjectileCount = def.Stats.ProjectileCount > 0 ? def.Stats.ProjectileCount : null,
            RicochetChance = def.Stats.RicochetChance,
            FragmentationChance = def.Stats.FragmentationChance,
            PenetrationDamageMod = def.Stats.PenetrationDamageMod,
            PenetrationChanceObstacle = def.Stats.PenetrationChanceObstacle,
            AmmoLifeTimeSec = def.Stats.AmmoLifeTimeSec,
            BulletMassGram = def.Stats.BulletMassGram,
            BulletDiameterMilimeters = def.Stats.BulletDiameterMilimeters,
            MisfireChance = def.Stats.MisfireChance,
            MalfMisfireChance = def.Stats.MalfMisfireChance,
            MalfFeedChance = def.Stats.MalfFeedChance,
            HeatFactor = def.Stats.HeatFactor,
            StaminaBurnPerDamage = def.Stats.StaminaBurnPerDamage,
            Tracer = def.Stats.Tracer,
            TracerColor = def.Stats.TracerColor,
            TracerDistance = def.Stats.TracerDistance,
            AmmoSfx = def.Stats.AmmoSfx,
            CasingSounds = def.Stats.CasingSounds,
            FuzeArmTimeSec = def.Stats.FuzeArmTimeSec,
            MinExplosionDistance = def.Stats.MinExplosionDistance,
            MaxExplosionDistance = def.Stats.MaxExplosionDistance,
            FragmentsCount = def.Stats.FragmentsCount > 0 ? def.Stats.FragmentsCount : null,
            FragmentType = def.Stats.FragmentType,
            ExplosionType = def.Stats.ExplosionType,
            ExplosionStrength = def.Stats.ExplosionStrength,
            ShowHitEffectOnExplode = def.Stats.ShowHitEffectOnExplode,
            IsLightAndSoundShot = def.Stats.IsLightAndSoundShot,
            LightAndSoundShotAngle = def.Stats.LightAndSoundShotAngle,
            LightAndSoundShotSelfContusionTime = def.Stats.LightAndSoundShotSelfContusionTime,
            LightAndSoundShotSelfContusionStrength = def.Stats.LightAndSoundShotSelfContusionStrength,
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
                SetPropertyOrField(tpl.Properties, "CanSellOnRagfair", !def.Economy.FleaBanned);

                if (!string.IsNullOrWhiteSpace(def.Stats.BackgroundColor) && def.Stats.BackgroundColor != "default")
                    SetPropertyOrField(tpl.Properties, "BackgroundColor", FormatBackgroundColor(def.Stats.BackgroundColor, def.Stats.BackgroundAlpha));
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

                if (!string.IsNullOrWhiteSpace(box.BackgroundColor) && box.BackgroundColor != "default")
                    SetPropertyOrField(boxItem.Properties, "BackgroundColor", FormatBackgroundColor(box.BackgroundColor, box.BackgroundAlpha));
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

    private static void SetPropertyOrField(object target, string name, object value)
    {
        var type = target.GetType();
        var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(target, value);
            return;
        }

        var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (field != null)
            field.SetValue(target, value);
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
