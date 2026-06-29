using System.Text.Json.Serialization;

namespace AmmoGen.Models;

// Root model for an ammo pack JSON file.
// Users (or the AmmoGen Tool) create these files and place them in AmmoGen/ammo/.
public class AmmoPackDefinition
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ammo")]
    public List<AmmoDefinition> Ammo { get; set; } = [];
}

public class AmmoDefinition
{
    // Unique 24-character hex ID for the new ammo item.
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    // Existing ammo template to clone (e.g. "59e655cb86f77411dc52a77b").
    [JsonPropertyName("baseTpl")]
    public string BaseTpl { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("shortName")]
    public string ShortName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    // SPT handbook parent category. Defaults to the base ammo's category if omitted.
    [JsonPropertyName("handbookParentId")]
    public string? HandbookParentId { get; set; }

    [JsonPropertyName("stats")]
    public AmmoStats Stats { get; set; } = new();

    [JsonPropertyName("economy")]
    public AmmoEconomy Economy { get; set; } = new();

    [JsonPropertyName("traders")]
    public List<TraderEntry> Traders { get; set; } = [];

    [JsonPropertyName("crafting")]
    public CraftingEntry Crafting { get; set; } = new();

    [JsonPropertyName("filters")]
    public FilterEntry Filters { get; set; } = new();

    [JsonPropertyName("ammoBox")]
    public AmmoBoxEntry AmmoBox { get; set; } = new();

    [JsonPropertyName("ammoLoot")]
    public LootEntry AmmoLoot { get; set; } = new();

    [JsonPropertyName("ammoBoxLoot")]
    public LootEntry AmmoBoxLoot { get; set; } = new();
}

public class AmmoStats
{
    [JsonPropertyName("damage")] public int Damage { get; set; }
    [JsonPropertyName("penetration")] public int PenetrationPower { get; set; }
    [JsonPropertyName("armorDamage")] public int ArmorDamage { get; set; }
    [JsonPropertyName("initialSpeed")] public int InitialSpeed { get; set; }
    [JsonPropertyName("ammoAccr")] public int AmmoAccr { get; set; }
    [JsonPropertyName("ammoRec")] public int AmmoRec { get; set; }
    [JsonPropertyName("stackMaxSize")] public int StackMaxSize { get; set; }
    [JsonPropertyName("lightBleedingDelta")] public double LightBleedingDelta { get; set; }
    [JsonPropertyName("heavyBleedingDelta")] public double HeavyBleedingDelta { get; set; }
    [JsonPropertyName("durabilityBurnModificator")] public double DurabilityBurnModificator { get; set; } = 1;
    [JsonPropertyName("ballisticCoeficient")] public double BallisticCoeficient { get; set; } = 1;
}

public class AmmoEconomy
{
    [JsonPropertyName("handbookPriceRoubles")] public int HandbookPriceRoubles { get; set; }
    [JsonPropertyName("fleaPriceRoubles")] public int FleaPriceRoubles { get; set; }
    [JsonPropertyName("rarityPvE")] public string RarityPvE { get; set; } = "Rare";
}

public class TraderEntry
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("traderId")] public string TraderId { get; set; } = string.Empty;
    [JsonPropertyName("loyaltyLevel")] public int LoyaltyLevel { get; set; } = 1;
    [JsonPropertyName("priceRoubles")] public int PriceRoubles { get; set; }
    [JsonPropertyName("stockCount")] public int? StockCount { get; set; } = 200;
    [JsonPropertyName("buyRestrictionMax")] public int? BuyRestrictionMax { get; set; } = 200;
    [JsonPropertyName("unlimitedStock")] public bool UnlimitedStock { get; set; }
    [JsonPropertyName("unlimitedBuyRestriction")] public bool UnlimitedBuyRestriction { get; set; }
}

public class CraftingEntry
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("workbenchLevel")] public int WorkbenchLevel { get; set; } = 2;
    [JsonPropertyName("craftTimeSeconds")] public int CraftTimeSeconds { get; set; } = 10800;
    [JsonPropertyName("outputCount")] public int OutputCount { get; set; } = 100;
    [JsonPropertyName("requirements")] public List<CraftRequirement> Requirements { get; set; } = [];
}

public class CraftRequirement
{
    [JsonPropertyName("tpl")] public string Tpl { get; set; } = string.Empty;
    [JsonPropertyName("count")] public int Count { get; set; } = 1;
}

public class FilterEntry
{
    // Optional magazine template IDs to patch so they accept this ammo.
    [JsonPropertyName("patchMagazines")]
    public List<string> PatchMagazines { get; set; } = [];

    // Optional weapon template IDs to patch so their chambers accept this ammo.
    [JsonPropertyName("patchWeapons")]
    public List<string> PatchWeapons { get; set; } = [];
}

public class AmmoBoxEntry
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("enabled")] public bool Enabled { get; set; }
    [JsonPropertyName("baseTpl")] public string BaseTpl { get; set; } = string.Empty;
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("shortName")] public string ShortName { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("handbookPriceRoubles")] public int HandbookPriceRoubles { get; set; }
    [JsonPropertyName("rarityPvE")] public string RarityPvE { get; set; } = "Rare";
    [JsonPropertyName("sellToTraders")] public bool SellToTraders { get; set; }
    [JsonPropertyName("traderPriceRoubles")] public int TraderPriceRoubles { get; set; }
    [JsonPropertyName("traderId")] public string? TraderId { get; set; }
    [JsonPropertyName("loyaltyLevel")] public int? LoyaltyLevel { get; set; }
    [JsonPropertyName("stockCount")] public int? StockCount { get; set; }
    [JsonPropertyName("buyRestrictionMax")] public int? BuyRestrictionMax { get; set; }
    [JsonPropertyName("unlimitedStock")] public bool UnlimitedStock { get; set; }
    [JsonPropertyName("unlimitedBuyRestriction")] public bool UnlimitedBuyRestriction { get; set; }
}

public class LootEntry
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; }
    [JsonPropertyName("containerIds")] public List<string> ContainerIds { get; set; } = [];
    [JsonPropertyName("rarity")] public string Rarity { get; set; } = "Rare";
}
