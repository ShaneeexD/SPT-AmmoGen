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

    [JsonPropertyName("grenades")]
    public List<GrenadeDefinition> Grenades { get; set; } = [];

    [JsonPropertyName("flares")]
    public List<FlareDefinition> Flares { get; set; } = [];
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

    // Projectile / flight
    [JsonPropertyName("projectileCount")] public int ProjectileCount { get; set; }
    [JsonPropertyName("ricochetChance")] public double RicochetChance { get; set; }
    [JsonPropertyName("fragmentationChance")] public double FragmentationChance { get; set; }
    [JsonPropertyName("penetrationDamageMod")] public double PenetrationDamageMod { get; set; }
    [JsonPropertyName("penetrationChanceObstacle")] public double PenetrationChanceObstacle { get; set; }
    [JsonPropertyName("ammoLifeTimeSec")] public double AmmoLifeTimeSec { get; set; }
    [JsonPropertyName("bulletMassGram")] public double BulletMassGram { get; set; }
    [JsonPropertyName("bulletDiameterMilimeters")] public double BulletDiameterMilimeters { get; set; }

    // Malfunctions / durability
    [JsonPropertyName("misfireChance")] public double MisfireChance { get; set; }
    [JsonPropertyName("malfMisfireChance")] public double MalfMisfireChance { get; set; }
    [JsonPropertyName("malfFeedChance")] public double MalfFeedChance { get; set; }
    [JsonPropertyName("heatFactor")] public double HeatFactor { get; set; } = 1;
    [JsonPropertyName("staminaBurnPerDamage")] public double StaminaBurnPerDamage { get; set; }

    // Tracer
    [JsonPropertyName("tracer")] public bool Tracer { get; set; }
    [JsonPropertyName("tracerColor")] public string TracerColor { get; set; } = string.Empty;
    [JsonPropertyName("tracerDistance")] public double TracerDistance { get; set; }

    // Audio / visual
    [JsonPropertyName("ammoSfx")] public string AmmoSfx { get; set; } = string.Empty;
    [JsonPropertyName("casingSounds")] public string CasingSounds { get; set; } = string.Empty;

    // Explosive / grenade rounds
    [JsonPropertyName("fuzeArmTimeSec")] public double FuzeArmTimeSec { get; set; }
    [JsonPropertyName("minExplosionDistance")] public double MinExplosionDistance { get; set; }
    [JsonPropertyName("maxExplosionDistance")] public double MaxExplosionDistance { get; set; }
    [JsonPropertyName("fragmentsCount")] public int FragmentsCount { get; set; }
    [JsonPropertyName("fragmentType")] public string FragmentType { get; set; } = string.Empty;
    [JsonPropertyName("explosionType")] public string ExplosionType { get; set; } = string.Empty;
    [JsonPropertyName("explosionStrength")] public double ExplosionStrength { get; set; }
    [JsonPropertyName("showHitEffectOnExplode")] public bool ShowHitEffectOnExplode { get; set; }

    // Light-and-sound rounds (flash/CS)
    [JsonPropertyName("isLightAndSoundShot")] public bool IsLightAndSoundShot { get; set; }
    [JsonPropertyName("lightAndSoundShotAngle")] public double LightAndSoundShotAngle { get; set; }
    [JsonPropertyName("lightAndSoundShotSelfContusionTime")] public double LightAndSoundShotSelfContusionTime { get; set; }
    [JsonPropertyName("lightAndSoundShotSelfContusionStrength")] public double LightAndSoundShotSelfContusionStrength { get; set; }

    // Vector3 effect fields
    [JsonPropertyName("armorDistanceDistanceDamage")] public Vector3 ArmorDistanceDistanceDamage { get; set; } = new();
    [JsonPropertyName("contusion")] public Vector3 Contusion { get; set; } = new();
    [JsonPropertyName("blindness")] public Vector3 Blindness { get; set; } = new();
}

public class Vector3
{
    [JsonPropertyName("x")] public float X { get; set; }
    [JsonPropertyName("y")] public float Y { get; set; }
    [JsonPropertyName("z")] public float Z { get; set; }
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

public class GrenadeDefinition
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("baseTpl")] public string BaseTpl { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("shortName")] public string ShortName { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("handbookParentId")] public string? HandbookParentId { get; set; }
    [JsonPropertyName("stats")] public GrenadeStats Stats { get; set; } = new();
    [JsonPropertyName("economy")] public AmmoEconomy Economy { get; set; } = new();
    [JsonPropertyName("traders")] public List<TraderEntry> Traders { get; set; } = [];
    [JsonPropertyName("crafting")] public CraftingEntry Crafting { get; set; } = new();
    [JsonPropertyName("loot")] public LootEntry Loot { get; set; } = new();
}

public class GrenadeStats
{
    [JsonPropertyName("minExplosionDistance")] public double MinExplosionDistance { get; set; }
    [JsonPropertyName("maxExplosionDistance")] public double MaxExplosionDistance { get; set; }
    [JsonPropertyName("fragmentsCount")] public int FragmentsCount { get; set; }
    [JsonPropertyName("fragmentType")] public string FragmentType { get; set; } = string.Empty;
    [JsonPropertyName("explosionEffectType")] public string ExplosionEffectType { get; set; } = string.Empty;
    [JsonPropertyName("armorDistanceDistanceDamage")] public Vector3 ArmorDistanceDistanceDamage { get; set; } = new();
    [JsonPropertyName("contusion")] public Vector3 Contusion { get; set; } = new();
    [JsonPropertyName("blindness")] public Vector3 Blindness { get; set; } = new();
    [JsonPropertyName("contusionDistance")] public double ContusionDistance { get; set; }
    [JsonPropertyName("explDelay")] public double ExplDelay { get; set; }
    [JsonPropertyName("minTimeToContactExplode")] public double MinTimeToContactExplode { get; set; } = -1;
    [JsonPropertyName("playFuzeSound")] public bool PlayFuzeSound { get; set; } = true;
    [JsonPropertyName("strength")] public int Strength { get; set; }
    [JsonPropertyName("minFragmentDamage")] public double MinFragmentDamage { get; set; }
    [JsonPropertyName("canPlantOnGround")] public bool CanPlantOnGround { get; set; }
    [JsonPropertyName("throwType")] public string ThrowType { get; set; } = string.Empty;
    [JsonPropertyName("throwDamMax")] public double ThrowDamMax { get; set; }
    [JsonPropertyName("weight")] public double Weight { get; set; }
    [JsonPropertyName("smokeColor")] public string SmokeColor { get; set; } = string.Empty;
    [JsonPropertyName("bodyColor")] public string BodyColor { get; set; } = string.Empty;
    [JsonPropertyName("smokeRadius")] public double SmokeRadius { get; set; }
    [JsonPropertyName("smokeDuration")] public double SmokeDuration { get; set; }
    [JsonPropertyName("smokeFillSize")] public double SmokeFillSize { get; set; }
    [JsonPropertyName("smokeSizeOverTime")] public List<SmokeSizeKeyframe> SmokeSizeOverTime { get; set; } = [];
    [JsonPropertyName("smokeStartSpeed")] public List<SmokeSpeedRange> SmokeStartSpeed { get; set; } = [];
    [JsonPropertyName("overrideSmokeRadius")] public bool OverrideSmokeRadius { get; set; }
    [JsonPropertyName("overrideSmokeDuration")] public bool OverrideSmokeDuration { get; set; }
    [JsonPropertyName("overrideSmokeFillSize")] public bool OverrideSmokeFillSize { get; set; }
    [JsonPropertyName("overrideSmokeSizeOverTime")] public bool OverrideSmokeSizeOverTime { get; set; }
    [JsonPropertyName("overrideSmokeStartSpeed")] public bool OverrideSmokeStartSpeed { get; set; }
}

public class SmokeSizeKeyframe
{
    [JsonPropertyName("time")] public float Time { get; set; }
    [JsonPropertyName("value")] public float Value { get; set; }
}

public class SmokeSpeedRange
{
    [JsonPropertyName("x")] public float X { get; set; }
    [JsonPropertyName("y")] public float Y { get; set; }
}

public class SmokeSettingsConfig
{
    [JsonPropertyName("smokeRadius")] public double SmokeRadius { get; set; }
    [JsonPropertyName("smokeDuration")] public double SmokeDuration { get; set; }
    [JsonPropertyName("smokeFillSize")] public double SmokeFillSize { get; set; }
    [JsonPropertyName("smokeSizeOverTime")] public List<SmokeSizeKeyframe> SmokeSizeOverTime { get; set; } = [];
    [JsonPropertyName("smokeStartSpeed")] public List<SmokeSpeedRange> SmokeStartSpeed { get; set; } = [];
}

public class FlareDefinition
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("ammoId")] public string AmmoId { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = "handheld";
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("baseTpl")] public string BaseTpl { get; set; } = string.Empty;
    [JsonPropertyName("ammoBaseTpl")] public string AmmoBaseTpl { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("shortName")] public string ShortName { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("handbookParentId")] public string? HandbookParentId { get; set; }
    [JsonPropertyName("stats")] public FlareStats Stats { get; set; } = new();
    [JsonPropertyName("economy")] public AmmoEconomy Economy { get; set; } = new();
    [JsonPropertyName("traders")] public List<TraderEntry> Traders { get; set; } = [];
    [JsonPropertyName("crafting")] public CraftingEntry Crafting { get; set; } = new();
    [JsonPropertyName("loot")] public LootEntry Loot { get; set; } = new();
}

public class FlareStats
{
    [JsonPropertyName("damage")] public int Damage { get; set; }
    [JsonPropertyName("initialSpeed")] public int InitialSpeed { get; set; }
    [JsonPropertyName("stackMaxSize")] public int StackMaxSize { get; set; }
    [JsonPropertyName("ammoLifeTimeSec")] public double AmmoLifeTimeSec { get; set; }
    [JsonPropertyName("tracer")] public bool Tracer { get; set; } = true;
    [JsonPropertyName("tracerColor")] public string TracerColor { get; set; } = string.Empty;
    [JsonPropertyName("tracerDistance")] public double TracerDistance { get; set; }
    [JsonPropertyName("backgroundColor")] public string BackgroundColor { get; set; } = string.Empty;
    [JsonPropertyName("flareColor")] public string FlareColor { get; set; } = string.Empty;
    [JsonPropertyName("weight")] public double Weight { get; set; }
    [JsonPropertyName("misfireChance")] public double MisfireChance { get; set; }
    [JsonPropertyName("ricochetChance")] public double RicochetChance { get; set; }
    [JsonPropertyName("flareTypes")] public List<string> FlareTypes { get; set; } = [];
    [JsonPropertyName("airDropTemplateId")] public string AirDropTemplateId { get; set; } = string.Empty;
    [JsonPropertyName("casingSounds")] public string CasingSounds { get; set; } = string.Empty;
    [JsonPropertyName("ammoType")] public string AmmoType { get; set; } = string.Empty;
    [JsonPropertyName("weapClass")] public string WeapClass { get; set; } = string.Empty;
    [JsonPropertyName("isSpecialSlotOnly")] public bool IsSpecialSlotOnly { get; set; }
}

