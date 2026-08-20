namespace AmmoGen.Models;

public interface ICraftable
{
    string Id { get; set; }
    string Name { get; set; }
    CraftingEntry Crafting { get; set; }
}

public interface ITradable
{
    string Id { get; set; }
    string Name { get; set; }
    List<TraderEntry> Traders { get; set; }
}

public interface ILootable
{
    string Id { get; set; }
    string Name { get; set; }
    LootEntry Loot { get; set; }
}
