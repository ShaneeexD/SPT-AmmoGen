using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using AmmoGen.Models;

namespace AmmoGen.Services;

// Adds custom ammo to vanilla trader assortments (item listing, barter scheme, loyalty level).
public static class TraderManager
{
    private const string RoublesTpl = "5449016a4bdc2d6f028b456f";

    public static void RegisterAll(
        DatabaseService databaseService,
        IReadOnlyList<AmmoDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var traders = databaseService.GetTraders();

        foreach (var def in definitions)
        {
            foreach (var traderEntry in def.Traders)
            {
                if (!traderEntry.Enabled)
                    continue;

                try
                {
                    AddToTrader(def.Id, def.Name, traderEntry, traders, logger);
                }
                catch (Exception ex)
                {
                    logger.LogWithColor($"[AmmoGen] Failed to add trader entry for '{def.Name}' / '{traderEntry.TraderId}': {ex.Message}", LogTextColor.Red);
                }
            }

            if (!def.AmmoBox.Enabled || !def.AmmoBox.SellToTraders)
                continue;

            foreach (var traderEntry in def.Traders)
            {
                if (!traderEntry.Enabled)
                    continue;

                try
                {
                    var boxTraderEntry = new TraderEntry
                    {
                        Enabled = traderEntry.Enabled,
                        TraderId = traderEntry.TraderId,
                        LoyaltyLevel = traderEntry.LoyaltyLevel,
                        PriceRoubles = def.AmmoBox.TraderPriceRoubles,
                        StockCount = traderEntry.StockCount,
                        BuyRestrictionMax = traderEntry.BuyRestrictionMax,
                    };
                    AddBoxToTrader(def, boxTraderEntry, traders, logger);
                }
                catch (Exception ex)
                {
                    logger.LogWithColor($"[AmmoGen] Failed to add ammo box trader entry for '{def.AmmoBox.Name}' / '{traderEntry.TraderId}': {ex.Message}", LogTextColor.Red);
                }
            }
        }
    }

    private static void AddToTrader(string itemId, string itemName, TraderEntry traderEntry, Dictionary<MongoId, Trader> traders, ISptLogger<AmmoGenPlugin> logger)
    {
        MongoId traderId = new MongoId(traderEntry.TraderId);

        if (!traders.TryGetValue(traderId, out var trader))
        {
            logger.LogWithColor($"[AmmoGen] Trader '{traderEntry.TraderId}' not found for '{itemName}'. Skipping.", LogTextColor.Red);
            return;
        }

        var assort = trader.Assort;
        if (assort == null)
        {
            logger.LogWithColor($"[AmmoGen] Trader '{traderEntry.TraderId}' has no assort. Skipping '{itemName}'.", LogTextColor.Red);
            return;
        }

        MongoId assortItemId = new MongoId(itemId);

        var typedItem = new Item
        {
            Id = assortItemId,
            Template = new MongoId(itemId),
            ParentId = "hideout",
            SlotId = "hideout",
            Upd = new Upd
            {
                StackObjectsCount = traderEntry.StockCount,
                UnlimitedCount = false,
                BuyRestrictionMax = traderEntry.BuyRestrictionMax,
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

        logger.LogWithColor($"[AmmoGen] Added {itemName} to trader '{traderEntry.TraderId}' (LL{traderEntry.LoyaltyLevel}, {traderEntry.PriceRoubles}₽)", LogTextColor.Green);
    }

    private static void AddBoxToTrader(AmmoDefinition def, TraderEntry traderEntry, Dictionary<MongoId, Trader> traders, ISptLogger<AmmoGenPlugin> logger)
    {
        var box = def.AmmoBox;
        MongoId traderId = new MongoId(traderEntry.TraderId);

        if (!traders.TryGetValue(traderId, out var trader))
        {
            logger.LogWithColor($"[AmmoGen] Trader '{traderEntry.TraderId}' not found for ammo box '{box.Name}'. Skipping.", LogTextColor.Red);
            return;
        }

        var assort = trader.Assort;
        if (assort == null)
        {
            logger.LogWithColor($"[AmmoGen] Trader '{traderEntry.TraderId}' has no assort. Skipping ammo box '{box.Name}'.", LogTextColor.Red);
            return;
        }

        MongoId boxAssortId = new MongoId(box.Id);

        var boxItem = new Item
        {
            Id = boxAssortId,
            Template = new MongoId(box.Id),
            ParentId = "hideout",
            SlotId = "hideout",
            Upd = new Upd
            {
                StackObjectsCount = traderEntry.StockCount,
                UnlimitedCount = false,
                BuyRestrictionMax = traderEntry.BuyRestrictionMax,
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

        logger.LogWithColor($"[AmmoGen] Added ammo box {box.Name} to trader '{traderEntry.TraderId}' (LL{traderEntry.LoyaltyLevel}, {traderEntry.PriceRoubles}₽) with {box.Count} rounds", LogTextColor.Green);
    }
}
