using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using AmmoGen.Models;

namespace AmmoGen.Helpers;

public static class ItemHelper
{
    private static readonly Dictionary<string, string> NamedColors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["yellow"] = "#ffff00",
        ["blue"] = "#0000ff",
        ["green"] = "#00ff00",
        ["red"] = "#ff0000",
        ["violet"] = "#ee82ee",
        ["black"] = "#000000",
        ["grey"] = "#808080",
        ["white"] = "#ffffff",
        ["orange"] = "#ffa500",
    };

    public static string FormatBackgroundColor(string color, double alpha)
    {
        if (string.IsNullOrWhiteSpace(color) || color == "default")
            return "default";
        if (alpha >= 1)
            return color;

        var baseHex = NamedColors.TryGetValue(color, out var hex) ? hex : color;
        if (!baseHex.StartsWith("#"))
            baseHex = "#ffffff";

        var alphaByte = (byte)Math.Round(Math.Max(0, Math.Min(1, alpha)) * 255);
        return $"{baseHex}{alphaByte:x2}";
    }

    public static string ResolveHandbookParent(TemplateTable templateTable, string baseTpl, string defaultParentId)
    {
        var items = templateTable.Items;
        if (items.TryGetValue(baseTpl, out _))
        {
            var handbook = templateTable.Handbook.Items.FirstOrDefault(h => h.Id == baseTpl);
            if (handbook != null && !string.IsNullOrWhiteSpace(handbook.ParentId))
                return handbook.ParentId;
        }
        return defaultParentId;
    }

    public static Dictionary<string, LocaleDetails> CreateEnLocale(string name, string shortName, string description)
    {
        return new Dictionary<string, LocaleDetails>
        {
            ["en"] = new LocaleDetails
            {
                Name = name,
                ShortName = shortName,
                Description = description,
            }
        };
    }

    public static SPTarkov.Server.Core.Models.Eft.Common.Vector3 CreateXYZ(AmmoGen.Models.Vector3 v)
    {
        return new SPTarkov.Server.Core.Models.Eft.Common.Vector3(v.X, v.Y, v.Z);
    }

    public static void ApplyCommonPostRegistration(
        TemplateItemProperties properties,
        string rarityPvE,
        bool fleaBanned,
        string? backgroundColor,
        double backgroundAlpha)
    {
        properties.RarityPvE = rarityPvE;
        ReflectionHelper.SetPropertyOrField(properties, "CanSellOnRagfair", !fleaBanned);

        if (!string.IsNullOrWhiteSpace(backgroundColor) && backgroundColor != "default")
            ReflectionHelper.SetPropertyOrField(properties, "BackgroundColor", FormatBackgroundColor(backgroundColor, backgroundAlpha));
    }

    public static void ApplyCustomPrefabPaths(TemplateItemProperties properties, string customModel, string customUsePrefab)
    {
        if (!string.IsNullOrWhiteSpace(customModel) && properties.Prefab != null)
            properties.Prefab.Path = customModel;
        if (!string.IsNullOrWhiteSpace(customUsePrefab) && properties.UsePrefab != null)
            properties.UsePrefab.Path = customUsePrefab;
    }
}
