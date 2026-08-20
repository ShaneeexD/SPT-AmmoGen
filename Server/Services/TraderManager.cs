using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Color = Spectre.Console.Color;
using SPTarkov.Common.Models.Logging;
using AmmoGen.Models;

namespace AmmoGen.Services;

// Adds custom ammo to vanilla trader assortments (item listing, barter scheme, loyalty level).
public static class TraderManager
{
    private const string RoublesTpl = "5449016a4bdc2d6f028b456f";

    public static void RegisterAll(
        TradersTable tradersTable,
        IReadOnlyList<AmmoDefinition> definitions,
        IReadOnlyList<GrenadeDefinition> grenades,
        IReadOnlyList<FlareDefinition> flares,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var traders = tradersTable;
        var addedEntries = 0;
        var addedBoxes = 0;
        var failedEntries = 0;
        var failedBoxes = 0;

        ProcessTraders(definitions, "ammo", traders, logger, ref addedEntries, ref failedEntries);
        ProcessTraders(grenades, "grenade", traders, logger, ref addedEntries, ref failedEntries);
        ProcessTraders(flares, "flare", traders, logger, ref addedEntries, ref failedEntries);

        foreach (var def in definitions)
        {
            if (!def.AmmoBox.Enabled || !def.AmmoBox.SellToTraders)
                continue;

            foreach (var traderEntry in def.Traders)
            {
                if (!traderEntry.Enabled)
                    continue;

                try
                {
                    var box = def.AmmoBox;
                    var boxStock = box.StockCount ?? traderEntry.StockCount;
                    var boxRestriction = box.BuyRestrictionMax ?? traderEntry.BuyRestrictionMax;
                    var boxUnlimitedStock = box.UnlimitedStock || (box.StockCount is null && traderEntry.UnlimitedStock);
                    var boxUnlimitedBuyRestriction = box.UnlimitedBuyRestriction || (box.BuyRestrictionMax is null && traderEntry.UnlimitedBuyRestriction);
                    var boxTraderEntry = new TraderEntry
                    {
                        Enabled = traderEntry.Enabled,
                        TraderId = !string.IsNullOrWhiteSpace(box.TraderId) ? box.TraderId : traderEntry.TraderId,
                        LoyaltyLevel = box.LoyaltyLevel is > 0 ? box.LoyaltyLevel.Value : traderEntry.LoyaltyLevel,
                        PriceRoubles = box.TraderPriceRoubles,
                        StockCount = boxStock,
                        BuyRestrictionMax = boxRestriction,
                        UnlimitedStock = boxUnlimitedStock,
                        UnlimitedBuyRestriction = boxUnlimitedBuyRestriction,
                    };
                    AddBoxToTrader(def, boxTraderEntry, traders);
                    addedBoxes++;
                }
                catch (Exception ex)
                {
                    failedBoxes++;
                    logger.LogWithColor($"[AmmoGen] Failed to add ammo box trader entry for '{def.AmmoBox.Name}' / '{traderEntry.TraderId}': {ex.Message}", Color.Red);
                }
            }
        }

        logger.LogWithColor(
            $"[AmmoGen] Added {addedEntries} trader assortment entry(ies) and {addedBoxes} ammo box trader entry(ies).",
            Color.Green);
        if (failedEntries + failedBoxes > 0)
            logger.LogWithColor($"[AmmoGen] {failedEntries + failedBoxes} trader addition(s) failed.", Color.Red);
    }

    private static void ProcessTraders<T>(
        IReadOnlyList<T> items,
        string label,
        Dictionary<MongoId, Trader> traders,
        ISptLogger<AmmoGenPlugin> logger,
        ref int addedEntries,
        ref int failedEntries) where T : ITradable
    {
        foreach (var def in items)
        {
            foreach (var traderEntry in def.Traders)
            {
                if (!traderEntry.Enabled)
                    continue;

                try
                {
                    AddToTrader(def.Id, def.Name, traderEntry, traders);
                    addedEntries++;
                }
                catch (Exception ex)
                {
                    failedEntries++;
                    logger.LogWithColor($"[AmmoGen] Failed to add trader entry for {label} '{def.Name}' / '{traderEntry.TraderId}': {ex.Message}", Color.Red);
                }
            }
        }
    }

    private static void AddToTrader(string itemId, string itemName, TraderEntry traderEntry, Dictionary<MongoId, Trader> traders)
    {
        MongoId traderId = new MongoId(traderEntry.TraderId);

        if (!traders.TryGetValue(traderId, out var trader))
        {
            throw new InvalidOperationException($"Trader '{traderEntry.TraderId}' not found for '{itemName}'.");
        }

        var assort = trader.Assort;
        if (assort == null)
        {
            throw new InvalidOperationException($"Trader '{traderEntry.TraderId}' has no assort.");
        }

        MongoId assortItemId = new MongoId(itemId);

        var stockCount = traderEntry.UnlimitedStock ? 999999 : traderEntry.StockCount ?? 0;
        var buyRestrictionMax = traderEntry.UnlimitedBuyRestriction ? null : traderEntry.BuyRestrictionMax;

        var typedItem = new Item
        {
            Id = assortItemId,
            Template = new MongoId(itemId),
            ParentId = "hideout",
            SlotId = "hideout",
            Upd = new Upd
            {
                StackObjectsCount = stockCount,
                UnlimitedCount = traderEntry.UnlimitedStock,
                BuyRestrictionMax = buyRestrictionMax,
                BuyRestrictionCurrent = 0,
            }
        };
        assort.Items.Add(typedItem);

        var barterEntry = new List<List<BarterScheme>>
        {
            new List<BarterScheme>
            {
                new BarterScheme
                {
                    Count = traderEntry.PriceRoubles,
                    Template = new MongoId(RoublesTpl),
                }
            }
        };
        assort.BarterScheme[assortItemId] = barterEntry;
        assort.LoyalLevelItems[assortItemId] = traderEntry.LoyaltyLevel;
    }

    private static void AddBoxToTrader(AmmoDefinition def, TraderEntry traderEntry, Dictionary<MongoId, Trader> traders)
    {
        var box = def.AmmoBox;
        MongoId traderId = new MongoId(traderEntry.TraderId);

        if (!traders.TryGetValue(traderId, out var trader))
        {
            throw new InvalidOperationException($"Trader '{traderEntry.TraderId}' not found for ammo box '{box.Name}'.");
        }

        var assort = trader.Assort;
        if (assort == null)
        {
            throw new InvalidOperationException($"Trader '{traderEntry.TraderId}' has no assort.");
        }

        MongoId boxTemplateId = new MongoId(box.Id);
        MongoId boxAssortId = new MongoId();

        var boxStockCount = traderEntry.UnlimitedStock ? 999999 : traderEntry.StockCount ?? 0;
        var boxBuyRestrictionMax = traderEntry.UnlimitedBuyRestriction ? null : traderEntry.BuyRestrictionMax;

        var boxItem = new Item
        {
            Id = boxAssortId,
            Template = boxTemplateId,
            ParentId = "hideout",
            SlotId = "hideout",
            Upd = new Upd
            {
                StackObjectsCount = boxStockCount,
                UnlimitedCount = traderEntry.UnlimitedStock,
                BuyRestrictionMax = boxBuyRestrictionMax,
                BuyRestrictionCurrent = 0,
            }
        };
        assort.Items.Add(boxItem);

        // Add the custom ammo as a child item inside the box
        var ammoChild = new Item
        {
            Id = new MongoId().ToString(),
            Template = new MongoId(def.Id),
            ParentId = boxAssortId.ToString(),
            SlotId = "cartridges",
            Upd = new Upd
            {
                StackObjectsCount = box.Count,
            }
        };
        assort.Items.Add(ammoChild);

        var barterEntry = new List<List<BarterScheme>>
        {
            new List<BarterScheme>
            {
                new BarterScheme
                {
                    Count = traderEntry.PriceRoubles,
                    Template = new MongoId(RoublesTpl),
                }
            }
        };
        assort.BarterScheme[boxAssortId] = barterEntry;
        assort.LoyalLevelItems[boxAssortId] = traderEntry.LoyaltyLevel;
    }
}
