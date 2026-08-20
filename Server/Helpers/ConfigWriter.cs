using System.Text.Json;
using Color = Spectre.Console.Color;
using SPTarkov.Common.Models.Logging;

namespace AmmoGen.Helpers;

public static class ConfigWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void WriteJsonConfig<T>(
        Dictionary<string, T> data,
        string fileName,
        ISptLogger<AmmoGenPlugin> logger,
        string? label = null)
    {
        if (data.Count == 0)
            return;

        try
        {
            var configDir = Path.Combine(Directory.GetCurrentDirectory(), "user", "mods", "AmmoGen", "config");
            Directory.CreateDirectory(configDir);
            var configPath = Path.Combine(configDir, fileName);
            File.WriteAllText(configPath, JsonSerializer.Serialize(data, JsonOptions));
            var itemLabel = label ?? "item(s)";
            logger.LogWithColor($"[AmmoGen] Wrote {fileName} for {data.Count} {itemLabel} to {configPath}", Color.Green);
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[AmmoGen] Failed to write {fileName}: {ex.Message}", Color.Red);
        }
    }
}
