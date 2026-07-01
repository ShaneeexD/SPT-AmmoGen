using System.Text.RegularExpressions;
using AmmoGen.Models;

namespace AmmoGen.Validation;

public static class AmmoValidator
{
    private static readonly Regex Hex24 = new("^[0-9a-fA-F]{24}$", RegexOptions.Compiled);

    public static List<string> ValidatePack(AmmoPackDefinition pack, string fileName)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(pack.Name))
            errors.Add($"Pack '{fileName}': 'name' is required.");

        if ((pack.Ammo == null || pack.Ammo.Count == 0) && (pack.Grenades == null || pack.Grenades.Count == 0))
        {
            errors.Add($"Pack '{fileName}': at least one ammo or grenade entry is required.");
            return errors;
        }

        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ammoList = pack.Ammo ?? [];
        var grenadeList = pack.Grenades ?? [];

        for (var i = 0; i < ammoList.Count; i++)
        {
            var ammo = ammoList[i];
            var prefix = $"Ammo[{i}]";

            if (string.IsNullOrWhiteSpace(ammo.Id))
            {
                errors.Add($"{prefix}: 'id' is required.");
            }
            else if (!Hex24.IsMatch(ammo.Id))
            {
                errors.Add($"{prefix}: 'id' must be a 24-character hex string.");
            }
            else if (!seenIds.Add(ammo.Id))
            {
                errors.Add($"{prefix}: duplicate ammo id '{ammo.Id}'.");
            }

            if (string.IsNullOrWhiteSpace(ammo.BaseTpl) || !Hex24.IsMatch(ammo.BaseTpl))
                errors.Add($"{prefix}: 'baseTpl' must be a 24-character hex string.");

            if (string.IsNullOrWhiteSpace(ammo.Name))
                errors.Add($"{prefix}: 'name' is required.");

            if (string.IsNullOrWhiteSpace(ammo.ShortName))
                errors.Add($"{prefix}: 'shortName' is required.");

            if (string.IsNullOrWhiteSpace(ammo.Description))
                errors.Add($"{prefix}: 'description' is required.");

            ValidateLootEntry(ammo.AmmoLoot, "ammoLoot", prefix, errors);
            ValidateLootEntry(ammo.AmmoBoxLoot, "ammoBoxLoot", prefix, errors);

            if (ammo.AmmoBoxLoot.Enabled && !ammo.AmmoBox.Enabled)
            {
                errors.Add($"{prefix}: 'ammoBoxLoot' is enabled but 'ammoBox.enabled' is false.");
            }

            if (ammo.AmmoBoxLoot.Enabled && (ammo.AmmoBoxLoot.ContainerIds == null || ammo.AmmoBoxLoot.ContainerIds.Count == 0) && string.IsNullOrWhiteSpace(ammo.AmmoBox.Id))
            {
                errors.Add($"{prefix}: 'ammoBox.id' is required when ammo box loot is enabled.");
            }

            if (ammo.Economy.HandbookPriceRoubles < 0)
                errors.Add($"{prefix}: 'economy.handbookPriceRoubles' cannot be negative.");

            if (ammo.Economy.FleaPriceRoubles < 0)
                errors.Add($"{prefix}: 'economy.fleaPriceRoubles' cannot be negative.");

            if (ammo.Stats.DurabilityBurnModificator < 0)
                errors.Add($"{prefix}: 'stats.durabilityBurnModificator' cannot be negative.");

            if (ammo.Stats.BallisticCoeficient < 0)
                errors.Add($"{prefix}: 'stats.ballisticCoeficient' cannot be negative.");

            if (ammo.Stats.RicochetChance < 0)
                errors.Add($"{prefix}: 'stats.ricochetChance' cannot be negative.");

            if (ammo.Stats.FragmentationChance < 0)
                errors.Add($"{prefix}: 'stats.fragmentationChance' cannot be negative.");

            if (ammo.Stats.PenetrationChanceObstacle < 0)
                errors.Add($"{prefix}: 'stats.penetrationChanceObstacle' cannot be negative.");

            if (ammo.Stats.MisfireChance < 0)
                errors.Add($"{prefix}: 'stats.misfireChance' cannot be negative.");

            if (ammo.Stats.MalfMisfireChance < 0)
                errors.Add($"{prefix}: 'stats.malfMisfireChance' cannot be negative.");

            if (ammo.Stats.MalfFeedChance < 0)
                errors.Add($"{prefix}: 'stats.malfFeedChance' cannot be negative.");

            if (ammo.Stats.HeatFactor < 0)
                errors.Add($"{prefix}: 'stats.heatFactor' cannot be negative.");

            if (ammo.Stats.StaminaBurnPerDamage < 0)
                errors.Add($"{prefix}: 'stats.staminaBurnPerDamage' cannot be negative.");

            if (ammo.Stats.BulletMassGram < 0)
                errors.Add($"{prefix}: 'stats.bulletMassGram' cannot be negative.");

            if (ammo.Stats.BulletDiameterMilimeters < 0)
                errors.Add($"{prefix}: 'stats.bulletDiameterMilimeters' cannot be negative.");

            if (ammo.Stats.TracerDistance < 0)
                errors.Add($"{prefix}: 'stats.tracerDistance' cannot be negative.");

            if (ammo.Stats.FuzeArmTimeSec < 0)
                errors.Add($"{prefix}: 'stats.fuzeArmTimeSec' cannot be negative.");

            if (ammo.Stats.MinExplosionDistance < 0)
                errors.Add($"{prefix}: 'stats.minExplosionDistance' cannot be negative.");

            if (ammo.Stats.MaxExplosionDistance < 0)
                errors.Add($"{prefix}: 'stats.maxExplosionDistance' cannot be negative.");

            if (ammo.Stats.ExplosionStrength < 0)
                errors.Add($"{prefix}: 'stats.explosionStrength' cannot be negative.");

            if (ammo.Stats.LightAndSoundShotAngle < 0)
                errors.Add($"{prefix}: 'stats.lightAndSoundShotAngle' cannot be negative.");

            if (ammo.Stats.LightAndSoundShotSelfContusionTime < 0)
                errors.Add($"{prefix}: 'stats.lightAndSoundShotSelfContusionTime' cannot be negative.");

            if (ammo.Stats.LightAndSoundShotSelfContusionStrength < 0)
                errors.Add($"{prefix}: 'stats.lightAndSoundShotSelfContusionStrength' cannot be negative.");

            if (ammo.Stats.ProjectileCount < 0)
                errors.Add($"{prefix}: 'stats.projectileCount' cannot be negative.");

            if (ammo.Stats.FragmentsCount < 0)
                errors.Add($"{prefix}: 'stats.fragmentsCount' cannot be negative.");

            if (ammo.Stats.AmmoLifeTimeSec < 0)
                errors.Add($"{prefix}: 'stats.ammoLifeTimeSec' cannot be negative.");

            if (ammo.Stats.ArmorDistanceDistanceDamage.X < 0 || ammo.Stats.ArmorDistanceDistanceDamage.Y < 0 || ammo.Stats.ArmorDistanceDistanceDamage.Z < 0)
                errors.Add($"{prefix}: 'stats.armorDistanceDistanceDamage' components cannot be negative.");

            if (ammo.Stats.Contusion.X < 0 || ammo.Stats.Contusion.Y < 0 || ammo.Stats.Contusion.Z < 0)
                errors.Add($"{prefix}: 'stats.contusion' components cannot be negative.");

            if (ammo.Stats.Blindness.X < 0 || ammo.Stats.Blindness.Y < 0 || ammo.Stats.Blindness.Z < 0)
                errors.Add($"{prefix}: 'stats.blindness' components cannot be negative.");

            for (var j = 0; j < ammo.Traders.Count; j++)
            {
                var trader = ammo.Traders[j];
                if (!trader.Enabled)
                    continue;

                var tPrefix = $"{prefix}.traders[{j}]";
                if (string.IsNullOrWhiteSpace(trader.TraderId) || !Hex24.IsMatch(trader.TraderId))
                    errors.Add($"{tPrefix}: 'traderId' must be a 24-character hex string.");

                if (trader.LoyaltyLevel < 1)
                    errors.Add($"{tPrefix}: 'loyaltyLevel' must be >= 1.");

                if (trader.PriceRoubles < 0)
                    errors.Add($"{tPrefix}: 'priceRoubles' cannot be negative.");

                if (trader.StockCount < 0)
                    errors.Add($"{tPrefix}: 'stockCount' cannot be negative.");
            }

            if (ammo.Crafting.Enabled)
            {
                if (ammo.Crafting.WorkbenchLevel < 1)
                    errors.Add($"{prefix}: 'crafting.workbenchLevel' must be >= 1.");

                if (ammo.Crafting.CraftTimeSeconds < 1)
                    errors.Add($"{prefix}: 'crafting.craftTimeSeconds' must be >= 1.");

                if (ammo.Crafting.OutputCount < 1)
                    errors.Add($"{prefix}: 'crafting.outputCount' must be >= 1.");

                for (var j = 0; j < ammo.Crafting.Requirements.Count; j++)
                {
                    var req = ammo.Crafting.Requirements[j];
                    if (string.IsNullOrWhiteSpace(req.Tpl) || !Hex24.IsMatch(req.Tpl))
                        errors.Add($"{prefix}.crafting.requirements[{j}]: 'tpl' must be a 24-character hex string.");
                    if (req.Count < 1)
                        errors.Add($"{prefix}.crafting.requirements[{j}]: 'count' must be >= 1.");
                }
            }

            foreach (var mag in ammo.Filters.PatchMagazines)
                if (string.IsNullOrWhiteSpace(mag) || !Hex24.IsMatch(mag))
                    errors.Add($"{prefix}: filter magazine id '{mag}' must be a 24-character hex string.");

            foreach (var weapon in ammo.Filters.PatchWeapons)
                if (string.IsNullOrWhiteSpace(weapon) || !Hex24.IsMatch(weapon))
                    errors.Add($"{prefix}: filter weapon id '{weapon}' must be a 24-character hex string.");
        }

        for (var i = 0; i < grenadeList.Count; i++)
        {
            var grenade = grenadeList[i];
            var prefix = $"Grenade[{i}]";

            if (string.IsNullOrWhiteSpace(grenade.Id))
            {
                errors.Add($"{prefix}: 'id' is required.");
            }
            else if (!Hex24.IsMatch(grenade.Id))
            {
                errors.Add($"{prefix}: 'id' must be a 24-character hex string.");
            }
            else if (!seenIds.Add(grenade.Id))
            {
                errors.Add($"{prefix}: duplicate id '{grenade.Id}'.");
            }

            if (string.IsNullOrWhiteSpace(grenade.BaseTpl) || !Hex24.IsMatch(grenade.BaseTpl))
                errors.Add($"{prefix}: 'baseTpl' must be a 24-character hex string.");

            if (string.IsNullOrWhiteSpace(grenade.Name))
                errors.Add($"{prefix}: 'name' is required.");

            if (string.IsNullOrWhiteSpace(grenade.ShortName))
                errors.Add($"{prefix}: 'shortName' is required.");

            if (string.IsNullOrWhiteSpace(grenade.Description))
                errors.Add($"{prefix}: 'description' is required.");

            ValidateLootEntry(grenade.Loot, "loot", prefix, errors);

            if (grenade.Economy.HandbookPriceRoubles < 0)
                errors.Add($"{prefix}: 'economy.handbookPriceRoubles' cannot be negative.");

            if (grenade.Economy.FleaPriceRoubles < 0)
                errors.Add($"{prefix}: 'economy.fleaPriceRoubles' cannot be negative.");

            if (grenade.Stats.MinExplosionDistance < 0)
                errors.Add($"{prefix}: 'stats.minExplosionDistance' cannot be negative.");

            if (grenade.Stats.MaxExplosionDistance < 0)
                errors.Add($"{prefix}: 'stats.maxExplosionDistance' cannot be negative.");

            if (grenade.Stats.ContusionDistance < 0)
                errors.Add($"{prefix}: 'stats.contusionDistance' cannot be negative.");

            if (grenade.Stats.ExplDelay < 0)
                errors.Add($"{prefix}: 'stats.explDelay' cannot be negative.");

            // minTimeToContactExplode can be negative (-1 means disabled) for impact-fuze grenades like VOG.

            if (grenade.Stats.Strength < 0)
                errors.Add($"{prefix}: 'stats.strength' cannot be negative.");

            if (grenade.Stats.ThrowDamMax < 0)
                errors.Add($"{prefix}: 'stats.throwDamMax' cannot be negative.");

            if (grenade.Stats.Weight < 0)
                errors.Add($"{prefix}: 'stats.weight' cannot be negative.");

            if (grenade.Stats.ArmorDistanceDistanceDamage.X < 0 || grenade.Stats.ArmorDistanceDistanceDamage.Y < 0 || grenade.Stats.ArmorDistanceDistanceDamage.Z < 0)
                errors.Add($"{prefix}: 'stats.armorDistanceDistanceDamage' components cannot be negative.");

            if (grenade.Stats.Contusion.X < 0 || grenade.Stats.Contusion.Y < 0 || grenade.Stats.Contusion.Z < 0)
                errors.Add($"{prefix}: 'stats.contusion' components cannot be negative.");

            if (grenade.Stats.Blindness.X < 0 || grenade.Stats.Blindness.Y < 0 || grenade.Stats.Blindness.Z < 0)
                errors.Add($"{prefix}: 'stats.blindness' components cannot be negative.");

            for (var j = 0; j < grenade.Traders.Count; j++)
            {
                var trader = grenade.Traders[j];
                if (!trader.Enabled)
                    continue;

                var tPrefix = $"{prefix}.traders[{j}]";
                if (string.IsNullOrWhiteSpace(trader.TraderId) || !Hex24.IsMatch(trader.TraderId))
                    errors.Add($"{tPrefix}: 'traderId' must be a 24-character hex string.");

                if (trader.LoyaltyLevel < 1)
                    errors.Add($"{tPrefix}: 'loyaltyLevel' must be >= 1.");

                if (trader.PriceRoubles < 0)
                    errors.Add($"{tPrefix}: 'priceRoubles' cannot be negative.");

                if (trader.StockCount < 0)
                    errors.Add($"{tPrefix}: 'stockCount' cannot be negative.");
            }

            if (grenade.Crafting.Enabled)
            {
                if (grenade.Crafting.WorkbenchLevel < 1)
                    errors.Add($"{prefix}: 'crafting.workbenchLevel' must be >= 1.");

                if (grenade.Crafting.CraftTimeSeconds < 1)
                    errors.Add($"{prefix}: 'crafting.craftTimeSeconds' must be >= 1.");

                if (grenade.Crafting.OutputCount < 1)
                    errors.Add($"{prefix}: 'crafting.outputCount' must be >= 1.");

                for (var j = 0; j < grenade.Crafting.Requirements.Count; j++)
                {
                    var req = grenade.Crafting.Requirements[j];
                    if (string.IsNullOrWhiteSpace(req.Tpl) || !Hex24.IsMatch(req.Tpl))
                        errors.Add($"{prefix}.crafting.requirements[{j}]: 'tpl' must be a 24-character hex string.");
                    if (req.Count < 1)
                        errors.Add($"{prefix}.crafting.requirements[{j}]: 'count' must be >= 1.");
                }
            }
        }

        return errors;
    }

    private static void ValidateLootEntry(LootEntry loot, string propertyName, string prefix, List<string> errors)
    {
        if (!loot.Enabled) return;

        if (loot.ContainerIds == null || loot.ContainerIds.Count == 0)
        {
            errors.Add($"{prefix}: '{propertyName}.containerIds' must contain at least one container ID when {propertyName} is enabled.");
            return;
        }

        for (var c = 0; c < loot.ContainerIds.Count; c++)
        {
            var containerId = loot.ContainerIds[c];
            if (string.IsNullOrWhiteSpace(containerId) || !Hex24.IsMatch(containerId))
                errors.Add($"{prefix}: '{propertyName}.containerIds[{c}]' must be a 24-character hex string.");
        }
    }
}
