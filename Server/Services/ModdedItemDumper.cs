using System.Text.Json;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using AmmoGen.Models;

namespace AmmoGen.Services;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.PostSptModLoader + 1)]
public class ModdedItemDumper(
    ISptLogger<ModdedItemDumper> logger,
    DatabaseService databaseService,
    LocaleService localeService)
    : IOnLoad
{
    public Task OnLoad()
    {
        var configPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "user", "mods", "AmmoGen", "config", "config.json");
        var config = ModConfig.Load(configPath);
        if (!config.DumpModdedItems)
        {
            return Task.CompletedTask;
        }

        var vanillaItemsPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "SPT_Data", "database", "templates", "items.json");
        if (!File.Exists(vanillaItemsPath))
        {
            logger.LogWithColor($"[AmmoGen] Modded item dump enabled but vanilla items file not found: {vanillaItemsPath}", LogTextColor.Yellow);
            return Task.CompletedTask;
        }

        try
        {
            var vanillaIds = LoadVanillaItemIds(vanillaItemsPath);
            var currentItems = databaseService.GetItems();
            var localeDb = localeService.GetLocaleDb();
            var moddedItems = currentItems
                .Where(kvp => !vanillaIds.Contains(kvp.Key.ToString()))
                .Select(kvp => kvp.Value)
                .ToList();

            var categorized = new Dictionary<string, List<DumpEntry>>();
            foreach (var item in moddedItems)
            {
                var category = Classify(item, currentItems);
                if (!categorized.TryGetValue(category, out var list))
                {
                    list = [];
                    categorized[category] = list;
                }

                var itemId = item.Id.ToString();
                var localeName = localeDb.TryGetValue($"{itemId} Name", out var locName) && !string.IsNullOrWhiteSpace(locName)
                    ? locName
                    : item.Properties?.Name ?? item.Name ?? string.Empty;
                var localeShortName = localeDb.TryGetValue($"{itemId} ShortName", out var locShortName) && !string.IsNullOrWhiteSpace(locShortName)
                    ? locShortName
                    : item.Properties?.ShortName ?? localeName;
                var localeDescription = localeDb.TryGetValue($"{itemId} Description", out var locDesc) && !string.IsNullOrWhiteSpace(locDesc)
                    ? locDesc
                    : item.Properties?.Description ?? string.Empty;

                list.Add(new DumpEntry
                {
                    Id = itemId,
                    Name = localeName,
                    Locale = new DumpEntryLocale
                    {
                        Name = localeName,
                        ShortName = localeShortName,
                        Description = localeDescription
                    }
                });
            }

            // Sort each category alphabetically by name for easy reading.
            foreach (var list in categorized.Values)
            {
                list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            }

            var output = new DumpOutput
            {
                GeneratedAt = DateTime.Now.ToString("O"),
                TotalModdedItems = moddedItems.Count,
                Categories = categorized
            };

            var outputPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "user", "mods", "AmmoGen", "modded_item_id_dump.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            File.WriteAllText(outputPath, JsonSerializer.Serialize(output, options));

            logger.LogWithColor($"[AmmoGen] Modded item dump written to {outputPath} ({moddedItems.Count} items).", LogTextColor.Green);
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[AmmoGen] Failed to dump modded items: {ex.Message}", LogTextColor.Red);
        }

        return Task.CompletedTask;
    }

    private static HashSet<string> LoadVanillaItemIds(string path)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var document = JsonDocument.Parse(File.ReadAllBytes(path));
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return ids;
        }

        foreach (var property in document.RootElement.EnumerateObject())
        {
            ids.Add(property.Name);
        }

        return ids;
    }

    private static string Classify(TemplateItem item, Dictionary<MongoId, TemplateItem> currentItems)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var current = item;
        while (current != null)
        {
            var id = current.Id.ToString();
            if (!visited.Add(id))
            {
                break;
            }

            var category = GetCategoryName(current);
            if (category != null)
            {
                return category;
            }

            if (current.Parent.IsEmpty)
            {
                break;
            }

            if (!currentItems.TryGetValue(current.Parent, out current))
            {
                break;
            }
        }

        return "Uncategorized";
    }

    private static string? GetCategoryName(TemplateItem item)
    {
        var name = item.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (name.Equals("Item", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("CompoundItem", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("StackableItem", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (name.Equals("Pistol", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Smg", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("AssaultRifle", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("AssaultCarbine", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Shotgun", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("MarksmanRifle", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("SniperRifle", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("MachineGun", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("GrenadeLauncher", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("SpecialWeapon", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Revolver", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("RocketLauncher", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Weapon", StringComparison.OrdinalIgnoreCase))
        {
            return "Weapons";
        }

        if (name.Equals("Magazine", StringComparison.OrdinalIgnoreCase))
        {
            return "Magazines";
        }

        if (name.Equals("Ammo", StringComparison.OrdinalIgnoreCase))
        {
            return "Ammo";
        }

        if (name.Equals("AmmoBox", StringComparison.OrdinalIgnoreCase))
        {
            return "AmmoBoxes";
        }

        if (name.Equals("Money", StringComparison.OrdinalIgnoreCase))
        {
            return "Money";
        }

        if (name.Equals("BarterItem", StringComparison.OrdinalIgnoreCase))
        {
            return "Barter";
        }

        if (name.Equals("FoodDrink", StringComparison.OrdinalIgnoreCase))
        {
            return "FoodDrink";
        }

        if (name.Equals("Meds", StringComparison.OrdinalIgnoreCase))
        {
            return "Meds";
        }

        if (name.Equals("Key", StringComparison.OrdinalIgnoreCase))
        {
            return "Keys";
        }

        if (name.Equals("Info", StringComparison.OrdinalIgnoreCase))
        {
            return "Info";
        }

        if (name.Equals("Knife", StringComparison.OrdinalIgnoreCase))
        {
            return "Melee";
        }

        if (name.Equals("SpecItem", StringComparison.OrdinalIgnoreCase))
        {
            return "Special";
        }

        if (name.Equals("Map", StringComparison.OrdinalIgnoreCase))
        {
            return "Map";
        }

        if (name.Equals("ThrowWeap", StringComparison.OrdinalIgnoreCase))
        {
            return "Throwables";
        }

        if (name.Equals("Equipment", StringComparison.OrdinalIgnoreCase))
        {
            return "Gear";
        }

        if (name.Equals("Inventory", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("SearchableItem", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Stash", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("LockableContainer", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("StationaryContainer", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("SimpleContainer", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("HideoutAreaContainer", StringComparison.OrdinalIgnoreCase))
        {
            return "Containers";
        }

        if (name.Equals("Mod", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("GearMod", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("FunctionalMod", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Stock", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Shaft", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Charge", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Launcher", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Mount", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Muzzle", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Handguard", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("PistolGrip", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Receiver", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Barrel", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Tactical", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Sight", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Scope", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("LightLaser", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Bipod", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("FlashHider", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Compensator", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Suppressor", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Foregrip", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Gasblock", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Mag", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("ChargingHandle", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Trigger", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("CompactCollimator", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Collimator", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("AssaultScope", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("SniperScope", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("OpticScope", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("ThermalVision", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("NightVision", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("SpecialScope", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("AuxiliaryPart", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Flashlight", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Laser", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Rounds", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Rounds_S", StringComparison.OrdinalIgnoreCase))
        {
            return "Mods";
        }

        return null;
    }

    private class DumpEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DumpEntryLocale Locale { get; set; } = new DumpEntryLocale();
    }

    private class DumpEntryLocale
    {
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private class DumpOutput
    {
        public string GeneratedAt { get; set; } = string.Empty;
        public int TotalModdedItems { get; set; }
        public Dictionary<string, List<DumpEntry>> Categories { get; set; } = new();
    }
}
