using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmmoGen.Models;

public class ModConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("debug")]
    public bool Debug { get; set; } = false;

    [JsonPropertyName("dumpModdedItems")]
    public bool DumpModdedItems { get; set; } = false;

    [JsonPropertyName("patchModdedItemFilters")]
    public bool PatchModdedItemFilters { get; set; } = true;

    public static ModConfig Load(string path)
    {
        if (!File.Exists(path))
        {
            return new ModConfig();
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ModConfig>(json) ?? new ModConfig();
        }
        catch
        {
            return new ModConfig();
        }
    }
}
