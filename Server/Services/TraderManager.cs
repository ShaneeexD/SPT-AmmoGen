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
                    AddToTrader(def, traderEntry, traders, logger);
                }
                catch (Exception ex)
                {
                    logger.LogWithColor($"[AmmoGen] Failed to add trader entry for '{def.Name}' / '{traderEntry.TraderId}': {ex.Message}", LogTextColor.Red);
                }
            }
        }
    }

    private static void AddToTrader(AmmoDefinition def, TraderEntry traderEntry, Dictionary<MongoId, Trader> traders, ISptLogger<AmmoGenPlugin> logger)
    {
        MongoId traderId = new MongoId(traderEntry.TraderId);

        if (!traders.TryGetValue(traderId, out var trader))
        {
            logger.LogWithColor($"[AmmoGen] Trader '{traderEntry.TraderId}' not found for '{def.Name}'. Skipping.", LogTextColor.Red);
            return;
        }

        var assort = trader.Assort;
        if (assort == null)
        {
            logger.LogWithColor($"[AmmoGen] Trader '{traderEntry.TraderId}' has no assort. Skipping '{def.Name}'.", LogTextColor.Red);
            return;
        }

        MongoId assortItemId = new MongoId(def.Id);

        var typedItem = new Item
        {
            Id = assortItemId,
            Template = new MongoId(def.Id),
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

        logger.LogWithColor($"[AmmoGen] Added {def.Name} to trader '{traderEntry.TraderId}' (LL{traderEntry.LoyaltyLevel}, {traderEntry.PriceRoubles}₽)", LogTextColor.Green);
    }
}
