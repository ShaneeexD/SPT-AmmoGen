using System;
using System.Reflection;
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
        var registeredAmmo = 0;
        var registeredAmmoBoxes = 0;
        var failedAmmo = 0;
        var failedAmmoBoxes = 0;

        foreach (var def in definitions)
        {
            try
            {
                if (RegisterAmmo(def, customItemService, databaseService, logger))
                    registeredAmmo++;
                else
                    failedAmmo++;
            }
            catch (Exception ex)
            {
                failedAmmo++;
                logger.LogWithColor($"[AmmoGen] Failed to register ammo '{def.Name}': {ex.Message}", LogTextColor.Red);
            }
        }

        foreach (var def in definitions)
        {
            if (!def.AmmoBox.Enabled) continue;
            try
            {
                if (RegisterAmmoBox(def, customItemService, databaseService, logger))
                    registeredAmmoBoxes++;
                else
                    failedAmmoBoxes++;
            }
            catch (Exception ex)
            {
                failedAmmoBoxes++;
                logger.LogWithColor($"[AmmoGen] Failed to register ammo box for '{def.Name}': {ex.Message}", LogTextColor.Red);
            }
        }

        logger.LogWithColor(
            $"[AmmoGen] Registered {registeredAmmo} ammo type(s) and {registeredAmmoBoxes} ammo box(es).",
            LogTextColor.Green);
        if (failedAmmo + failedAmmoBoxes > 0)
            logger.LogWithColor($"[AmmoGen] {failedAmmo + failedAmmoBoxes} registration(s) failed.", LogTextColor.Red);
    }

    private static bool RegisterAmmo(
        AmmoDefinition def,
        CustomItemService customItemService,
        DatabaseService databaseService,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var handbookParentId = !string.IsNullOrWhiteSpace(def.HandbookParentId)
            ? def.HandbookParentId
            : ResolveHandbookParent(databaseService, def.BaseTpl);

        var overrides = PropertiesHelper.DeserializeProperties(def.Properties) ?? new TemplateItemProperties();
        overrides.Name = def.ShortName;
        overrides.ShortName = def.ShortName;
        overrides.Description = def.Description;
        overrides.Damage = def.Stats.Damage;
        overrides.PenetrationPower = def.Stats.PenetrationPower;
        overrides.ArmorDamage = def.Stats.ArmorDamage;
        overrides.InitialSpeed = def.Stats.InitialSpeed;
        overrides.AmmoAccr = def.Stats.AmmoAccr;
        overrides.AmmoRec = def.Stats.AmmoRec;
        overrides.StackMaxSize = def.Stats.StackMaxSize > 0 ? def.Stats.StackMaxSize : null;
        overrides.LightBleedingDelta = def.Stats.LightBleedingDelta != 0 ? def.Stats.LightBleedingDelta : null;
        overrides.HeavyBleedingDelta = def.Stats.HeavyBleedingDelta != 0 ? def.Stats.HeavyBleedingDelta : null;
        overrides.DurabilityBurnModificator = def.Stats.DurabilityBurnModificator;
        overrides.BallisticCoeficient = def.Stats.BallisticCoeficient;
        overrides.ProjectileCount = def.Stats.ProjectileCount > 0 ? def.Stats.ProjectileCount : null;
        overrides.RicochetChance = def.Stats.RicochetChance;
        overrides.FragmentationChance = def.Stats.FragmentationChance;
        overrides.PenetrationDamageMod = def.Stats.PenetrationDamageMod;
        overrides.PenetrationChanceObstacle = def.Stats.PenetrationChanceObstacle;
        overrides.AmmoLifeTimeSec = def.Stats.AmmoLifeTimeSec;
        overrides.BulletMassGram = def.Stats.BulletMassGram;
        overrides.BulletDiameterMilimeters = def.Stats.BulletDiameterMilimeters;
        overrides.Weight = def.Stats.Weight;
        overrides.MisfireChance = def.Stats.MisfireChance;
        overrides.MalfMisfireChance = def.Stats.MalfMisfireChance;
        overrides.MalfFeedChance = def.Stats.MalfFeedChance;
        overrides.HeatFactor = def.Stats.HeatFactor;
        overrides.StaminaBurnPerDamage = def.Stats.StaminaBurnPerDamage;
        overrides.Tracer = def.Stats.Tracer;
        overrides.TracerDistance = def.Stats.TracerDistance;
        overrides.AmmoSfx = def.Stats.AmmoSfx;
        overrides.CasingSounds = def.Stats.CasingSounds;
        overrides.FuzeArmTimeSec = def.Stats.FuzeArmTimeSec;
        overrides.MinExplosionDistance = def.Stats.MinExplosionDistance;
        overrides.MaxExplosionDistance = def.Stats.MaxExplosionDistance;
        overrides.FragmentsCount = def.Stats.FragmentsCount > 0 ? def.Stats.FragmentsCount : null;
        overrides.FragmentType = def.Stats.FragmentType;
        overrides.ExplosionType = def.Stats.ExplosionType;
        overrides.ExplosionStrength = def.Stats.ExplosionStrength;
        overrides.ShowHitEffectOnExplode = def.Stats.ShowHitEffectOnExplode;
        overrides.IsLightAndSoundShot = def.Stats.IsLightAndSoundShot;
        overrides.LightAndSoundShotAngle = def.Stats.LightAndSoundShotAngle;
        overrides.LightAndSoundShotSelfContusionTime = def.Stats.LightAndSoundShotSelfContusionTime;
        overrides.LightAndSoundShotSelfContusionStrength = def.Stats.LightAndSoundShotSelfContusionStrength;
        overrides.ArmorDistanceDistanceDamage = new XYZ
        {
            X = def.Stats.ArmorDistanceDistanceDamage.X,
            Y = def.Stats.ArmorDistanceDistanceDamage.Y,
            Z = def.Stats.ArmorDistanceDistanceDamage.Z,
        };
        overrides.Contusion = new XYZ
        {
            X = def.Stats.Contusion.X,
            Y = def.Stats.Contusion.Y,
            Z = def.Stats.Contusion.Z,
        };
        overrides.Blindness = new XYZ
        {
            X = def.Stats.Blindness.X,
            Y = def.Stats.Blindness.Y,
            Z = def.Stats.Blindness.Z,
        };

        if (!string.IsNullOrWhiteSpace(def.Stats.TracerColor))
            overrides.TracerColor = def.Stats.TracerColor;

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

        if (result.Success != true)
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for '{def.Name}': {string.Join(", ", result.Errors ?? [])}",
                LogTextColor.Yellow);
            return false;
        }

        // Apply rarity override if it differs from the cloned template
        var items = databaseService.GetItems();
        if (items.TryGetValue(def.Id, out var tpl) && tpl.Properties != null)
        {
            tpl.Properties.RarityPvE = def.Economy.RarityPvE;
            SetPropertyOrField(tpl.Properties, "CanSellOnRagfair", !def.Economy.FleaBanned);

            if (!string.IsNullOrWhiteSpace(def.Stats.BackgroundColor) && def.Stats.BackgroundColor != "default")
                SetPropertyOrField(tpl.Properties, "BackgroundColor", FormatBackgroundColor(def.Stats.BackgroundColor, def.Stats.BackgroundAlpha));

            // SPT's TemplateItemProperties does not expose these fields directly, so set them via reflection
            // if the underlying cloned template has them.
            if (def.Stats.BuckshotBullets > 0)
                SetPropertyOrField(tpl.Properties, "BuckshotBullets", def.Stats.BuckshotBullets);
            if (def.Stats.PenetrationPowerDiviation != 0)
                SetPropertyOrField(tpl.Properties, "PenetrationPowerDiviation", def.Stats.PenetrationPowerDiviation);
            if (def.Stats.HasGrenaderComponent)
                SetPropertyOrField(tpl.Properties, "HasGrenaderComponent", def.Stats.HasGrenaderComponent);

            ApplyCustomPrefabPaths(tpl.Properties, def.CustomModel, def.CustomUsePrefab);
        }

        return true;
    }

    private static bool RegisterAmmoBox(
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
            Prefab = null,
            UsePrefab = null,
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
            return false;
        }

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

                ApplyCustomPrefabPaths(boxItem.Properties, box.CustomModel, box.CustomUsePrefab);
            }
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[AmmoGen] Created ammo box '{box.Name}' but failed to patch StackSlots: {ex.Message}", LogTextColor.Yellow);
        }

        return true;
    }

    private static void ApplyCustomPrefabPaths(TemplateItemProperties properties, string customModel, string customUsePrefab)
    {
        if (!string.IsNullOrWhiteSpace(customModel) && properties.Prefab != null)
            properties.Prefab.Path = customModel;
        if (!string.IsNullOrWhiteSpace(customUsePrefab) && properties.UsePrefab != null)
            properties.UsePrefab.Path = customUsePrefab;
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
            prop.SetValue(target, ConvertValue(value, prop.PropertyType));
            return;
        }

        var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (field != null)
            field.SetValue(target, ConvertValue(value, field.FieldType));
    }

    private static object? ConvertValue(object value, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (value.GetType() == underlyingType || value.GetType().IsAssignableTo(underlyingType))
            return value;
        return Convert.ChangeType(value, underlyingType);
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
