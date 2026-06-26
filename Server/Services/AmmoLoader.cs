using System.Reflection;
using System.Text.Json;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using AmmoGen.Models;
using AmmoGen.Validation;

namespace AmmoGen.Services;

// Discovers and loads user-created ammo pack JSON files from AmmoGen/ammo/.
[Injectable(TypePriority = OnLoadOrder.Database + 1)]
public class AmmoLoader(ISptLogger<AmmoLoader> logger, ModHelper modHelper)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public record LoadedPack(AmmoPackDefinition Definition, string SourceFile, string PackFolder);

    public List<LoadedPack> LoadAllPacks()
    {
        var results = new List<LoadedPack>();
        var assembly = Assembly.GetExecutingAssembly();
        var modPath = modHelper.GetAbsolutePathToModFolder(assembly);
        var ammoDir = Path.Combine(modPath, "ammo");

        if (!Directory.Exists(ammoDir))
        {
            Directory.CreateDirectory(ammoDir);
            logger.LogWithColor("[AmmoGen] Created ammo/ directory. Place ammo pack JSON files here.", LogTextColor.Yellow);
            return results;
        }

        // Load from subfolders
        foreach (var packDir in Directory.GetDirectories(ammoDir))
        {
            foreach (var jsonFile in Directory.GetFiles(packDir, "*.json", SearchOption.TopDirectoryOnly))
            {
                var loaded = TryLoadPackFile(jsonFile, packDir);
                if (loaded != null)
                    results.Add(loaded);
            }
        }

        // Load loose JSON files
        foreach (var jsonFile in Directory.GetFiles(ammoDir, "*.json", SearchOption.TopDirectoryOnly))
        {
            var loaded = TryLoadPackFile(jsonFile, ammoDir);
            if (loaded != null)
                results.Add(loaded);
        }

        return results;
    }

    public LoadedPack? LoadPackFromPath(string jsonFilePath, string packFolder)
    {
        return TryLoadPackFile(jsonFilePath, packFolder);
    }

    private LoadedPack? TryLoadPackFile(string jsonFilePath, string packFolder)
    {
        var fileName = Path.GetFileName(jsonFilePath);
        try
        {
            var jsonContent = File.ReadAllText(jsonFilePath);
            var pack = JsonSerializer.Deserialize<AmmoPackDefinition>(jsonContent, JsonOptions);

            if (pack == null)
            {
                logger.LogWithColor($"[AmmoGen] Failed to parse '{fileName}': JSON deserialized to null.", LogTextColor.Red);
                return null;
            }

            if (!pack.Enabled)
            {
                logger.LogWithColor($"[AmmoGen] Skipping disabled pack '{fileName}'", LogTextColor.Yellow);
                return null;
            }

            var errors = AmmoValidator.ValidatePack(pack, fileName);
            if (errors.Count > 0)
            {
                logger.LogWithColor($"[AmmoGen] Validation errors in '{fileName}':", LogTextColor.Red);
                foreach (var error in errors)
                    logger.LogWithColor($"  - {error}", LogTextColor.Red);
                return null;
            }

            logger.LogWithColor($"[AmmoGen] Loaded pack '{pack.Name}' from '{fileName}' ({pack.Ammo.Count} ammo)", LogTextColor.Green);
            return new LoadedPack(pack, jsonFilePath, packFolder);
        }
        catch (JsonException ex)
        {
            logger.LogWithColor($"[AmmoGen] JSON parse error in '{fileName}': {ex.Message}", LogTextColor.Red);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWithColor($"[AmmoGen] Error loading '{fileName}': {ex.Message}", LogTextColor.Red);
            return null;
        }
    }
}
