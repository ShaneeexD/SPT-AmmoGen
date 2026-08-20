using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Color = Spectre.Console.Color;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Services.Modding.Custom;
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
        TemplateTable templateTable,
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
                if (RegisterAmmo(def, customItemService, templateTable, logger))
                    registeredAmmo++;
                else
                    failedAmmo++;
            }
            catch (Exception ex)
            {
                failedAmmo++;
                logger.LogWithColor($"[AmmoGen] Failed to register ammo '{def.Name}': {ex.Message}", Color.Red);
            }
        }

        foreach (var def in definitions)
        {
            if (!def.AmmoBox.Enabled) continue;
            try
            {
                if (RegisterAmmoBox(def, customItemService, templateTable, logger))
                    registeredAmmoBoxes++;
                else
                    failedAmmoBoxes++;
            }
            catch (Exception ex)
            {
                failedAmmoBoxes++;
                logger.LogWithColor($"[AmmoGen] Failed to register ammo box for '{def.Name}': {ex.Message}", Color.Red);
            }
        }

        logger.LogWithColor(
            $"[AmmoGen] Registered {registeredAmmo} ammo type(s) and {registeredAmmoBoxes} ammo box(es).",
            Color.Green);
        if (failedAmmo + failedAmmoBoxes > 0)
            logger.LogWithColor($"[AmmoGen] {failedAmmo + failedAmmoBoxes} registration(s) failed.", Color.Red);
    }

    private static bool RegisterAmmo(
        AmmoDefinition def,
        CustomItemService customItemService,
        TemplateTable templateTable,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var handbookParentId = !string.IsNullOrWhiteSpace(def.HandbookParentId)
            ? def.HandbookParentId
            : ItemHelper.ResolveHandbookParent(templateTable, def.BaseTpl, AmmoCategoryParentId);

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
        overrides.ArmorDistanceDistanceDamage = ItemHelper.CreateXYZ(def.Stats.ArmorDistanceDistanceDamage);
        overrides.Contusion = ItemHelper.CreateXYZ(def.Stats.Contusion);
        overrides.Blindness = ItemHelper.CreateXYZ(def.Stats.Blindness);

        if (!string.IsNullOrWhiteSpace(def.Stats.TracerColor))
            overrides.TracerColor = def.Stats.TracerColor;

        var details = new NewItemFromCloneDetails
        {
            NewId = def.Id,
            NewItemName = def.Name,
            ItemTplToClone = def.BaseTpl,
            ParentId = AmmoCategoryParentId,
            HandbookParentId = handbookParentId,
            HandbookPriceRoubles = def.Economy.HandbookPriceRoubles,
            FleaPriceRoubles = def.Economy.FleaPriceRoubles,
            OverrideProperties = overrides,
            Locales = ItemHelper.CreateEnLocale(def.Name, def.ShortName, def.Description),
        };

        var result = customItemService.CreateItemFromClone(details);

        if (result.Success != true)
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for '{def.Name}': {string.Join(", ", result.Errors ?? [])}",
                Color.Yellow);
            return false;
        }

        // Apply rarity override if it differs from the cloned template
        var items = templateTable.Items;
        if (items.TryGetValue(def.Id, out var tpl) && tpl.Properties != null)
        {
            ItemHelper.ApplyCommonPostRegistration(
                tpl.Properties, def.Economy.RarityPvE, def.Economy.FleaBanned,
                def.Stats.BackgroundColor, def.Stats.BackgroundAlpha);

            // SPT's TemplateItemProperties does not expose these fields directly, so set them via reflection
            // if the underlying cloned template has them.
            if (def.Stats.BuckshotBullets > 0)
                ReflectionHelper.SetPropertyOrField(tpl.Properties, "BuckshotBullets", def.Stats.BuckshotBullets);
            if (def.Stats.PenetrationPowerDiviation != 0)
                ReflectionHelper.SetPropertyOrField(tpl.Properties, "PenetrationPowerDiviation", def.Stats.PenetrationPowerDiviation);
            if (def.Stats.HasGrenaderComponent)
                ReflectionHelper.SetPropertyOrField(tpl.Properties, "HasGrenaderComponent", def.Stats.HasGrenaderComponent);

            ItemHelper.ApplyCustomPrefabPaths(tpl.Properties, def.CustomModel, def.CustomUsePrefab);
        }

        return true;
    }

    private static bool RegisterAmmoBox(
        AmmoDefinition def,
        CustomItemService customItemService,
        TemplateTable templateTable,
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

        var boxHandbookParentId = ItemHelper.ResolveHandbookParent(templateTable, box.BaseTpl, AmmoBoxParentId);

        var details = new NewItemFromCloneDetails
        {
            NewId = box.Id,
            NewItemName = box.Name,
            ItemTplToClone = box.BaseTpl,
            ParentId = AmmoBoxParentId,
            HandbookParentId = boxHandbookParentId,
            HandbookPriceRoubles = box.HandbookPriceRoubles,
            FleaPriceRoubles = 0,
            AddToFleaPriceDb = false,
            OverrideProperties = overrides,
            Locales = ItemHelper.CreateEnLocale(box.Name, box.ShortName, box.Description),
        };

        var result = customItemService.CreateItemFromClone(details);

        if (result.Success != true)
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for ammo box '{box.Name}': {string.Join(", ", result.Errors ?? [])}",
                Color.Yellow);
            return false;
        }

        try
        {
            var items = templateTable.Items;
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

                                filter.Filter = [new MongoId(def.Id)];
                            }
                        }
                    }
                }

                boxItem.Properties.RarityPvE = box.RarityPvE;

                if (!string.IsNullOrWhiteSpace(box.BackgroundColor) && box.BackgroundColor != "default")
                    ReflectionHelper.SetPropertyOrField(boxItem.Properties, "BackgroundColor", ItemHelper.FormatBackgroundColor(box.BackgroundColor, box.BackgroundAlpha));

                ItemHelper.ApplyCustomPrefabPaths(boxItem.Properties, box.CustomModel, box.CustomUsePrefab);
            }
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[AmmoGen] Created ammo box '{box.Name}' but failed to patch StackSlots: {ex.Message}", Color.Yellow);
        }

        return true;
    }

}
