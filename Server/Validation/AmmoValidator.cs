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

        if (pack.Ammo == null || pack.Ammo.Count == 0)
        {
            errors.Add($"Pack '{fileName}': at least one ammo entry is required.");
            return errors;
        }

        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < pack.Ammo.Count; i++)
        {
            var ammo = pack.Ammo[i];
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

            if (ammo.Economy.HandbookPriceRoubles < 0)
                errors.Add($"{prefix}: 'economy.handbookPriceRoubles' cannot be negative.");

            if (ammo.Economy.FleaPriceRoubles < 0)
                errors.Add($"{prefix}: 'economy.fleaPriceRoubles' cannot be negative.");

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

                if (trader.StockCount < 1)
                    errors.Add($"{tPrefix}: 'stockCount' must be >= 1.");
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

        return errors;
    }
}
