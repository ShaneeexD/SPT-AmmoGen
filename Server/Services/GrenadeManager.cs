using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Color = Spectre.Console.Color;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Services.Modding.Custom;
using AmmoGen.Helpers;
using AmmoGen.Models;

namespace AmmoGen.Services;

// Registers custom grenade items by cloning an existing grenade template and applying overrides.
public static class GrenadeManager
{
    private const string GrenadeCategoryParentId = "543be6564bdc2df4348b4568";

    public static void RegisterAll(
        CustomItemService customItemService,
        TemplateTable templateTable,
        IReadOnlyList<GrenadeDefinition> definitions,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var smokeColors = new Dictionary<string, string>();
        var bodyColors = new Dictionary<string, string>();
        var smokeSettings = new Dictionary<string, SmokeSettingsConfig>();
        var registered = 0;
        var failed = 0;

        foreach (var def in definitions)
        {
            try
            {
                if (RegisterGrenade(def, customItemService, templateTable, logger))
                    registered++;
                else
                    failed++;

                if (!string.IsNullOrWhiteSpace(def.Stats.SmokeColor))
                    smokeColors[def.Id] = def.Stats.SmokeColor;
                if (!string.IsNullOrWhiteSpace(def.Stats.BodyColor))
                    bodyColors[def.Id] = def.Stats.BodyColor;

                var settings = new SmokeSettingsConfig();
                if (def.Stats.OverrideSmokeRadius)
                    settings.SmokeRadius = def.Stats.SmokeRadius;
                if (def.Stats.OverrideSmokeDuration)
                    settings.SmokeDuration = def.Stats.SmokeDuration;
                if (def.Stats.OverrideSmokeFillSize)
                    settings.SmokeFillSize = def.Stats.SmokeFillSize;
                if (def.Stats.OverrideSmokeSizeOverTime)
                    settings.SmokeSizeOverTime = def.Stats.SmokeSizeOverTime;
                if (def.Stats.OverrideSmokeStartSpeed)
                    settings.SmokeStartSpeed = def.Stats.SmokeStartSpeed;

                if (settings.SmokeRadius != 0 || settings.SmokeDuration != 0 || settings.SmokeFillSize != 0 ||
                    settings.SmokeSizeOverTime.Count > 0 || settings.SmokeStartSpeed.Count > 0)
                    smokeSettings[def.Id] = settings;
            }
            catch (Exception ex)
            {
                failed++;
                logger.LogWithColor($"[AmmoGen] Failed to register grenade '{def.Name}': {ex.Message}", Color.Red);
            }
        }

        logger.LogWithColor($"[AmmoGen] Registered {registered} grenade type(s).", Color.Green);
        if (failed > 0)
            logger.LogWithColor($"[AmmoGen] {failed} grenade registration(s) failed.", Color.Red);

        ConfigWriter.WriteJsonConfig(smokeColors, "smoke_colors.json", logger, "grenade(s)");
        ConfigWriter.WriteJsonConfig(bodyColors, "body_colors.json", logger, "grenade(s)");
        ConfigWriter.WriteJsonConfig(smokeSettings, "smoke_settings.json", logger, "grenade(s)");
    }

    private static bool RegisterGrenade(
        GrenadeDefinition def,
        CustomItemService customItemService,
        TemplateTable templateTable,
        ISptLogger<AmmoGenPlugin> logger)
    {
        var handbookParentId = !string.IsNullOrWhiteSpace(def.HandbookParentId)
            ? def.HandbookParentId
            : ItemHelper.ResolveHandbookParent(templateTable, def.BaseTpl, GrenadeCategoryParentId);

        var overrides = PropertiesHelper.DeserializeProperties(def.Properties) ?? new TemplateItemProperties();
        overrides.Name = def.ShortName;
        overrides.ShortName = def.ShortName;
        overrides.Description = def.Description;
        overrides.MinExplosionDistance = def.Stats.MinExplosionDistance;
        overrides.MaxExplosionDistance = def.Stats.MaxExplosionDistance;
        overrides.FragmentsCount = def.Stats.FragmentsCount > 0 ? def.Stats.FragmentsCount : null;
        overrides.FragmentType = def.Stats.FragmentType;
        overrides.ExplosionEffectType = def.Stats.ExplosionEffectType;
        overrides.ArmorDistanceDistanceDamage = ItemHelper.CreateXYZ(def.Stats.ArmorDistanceDistanceDamage);
        overrides.Contusion = ItemHelper.CreateXYZ(def.Stats.Contusion);
        overrides.Blindness = ItemHelper.CreateXYZ(def.Stats.Blindness);
        overrides.ContusionDistance = def.Stats.ContusionDistance;
        // TemplateItemProperties has both capitalized and camel-case JSON aliases for some fields.
        // Set both to ensure the value is respected regardless of which serializer path the client reads.
        overrides.ExplDelay = def.Stats.ExplDelay;
        overrides.explDelay = def.Stats.ExplDelay;
        overrides.MinTimeToContactExplode = def.Stats.MinTimeToContactExplode;
        overrides.PlayFuzeSound = def.Stats.PlayFuzeSound;
        overrides.Strength = def.Stats.Strength;
        overrides.ThrowType = string.IsNullOrWhiteSpace(def.Stats.ThrowType)
            ? null
            : Enum.Parse<ThrowWeapType>(def.Stats.ThrowType, true);
        overrides.ThrowDamMax = def.Stats.ThrowDamMax;
        overrides.Weight = def.Stats.Weight;

        var details = new NewItemFromCloneDetails
        {
            NewId = def.Id,
            NewItemName = def.Name,
            ItemTplToClone = def.BaseTpl,
            ParentId = GrenadeCategoryParentId,
            HandbookParentId = handbookParentId,
            HandbookPriceRoubles = def.Economy.HandbookPriceRoubles,
            FleaPriceRoubles = def.Economy.FleaPriceRoubles,
            OverrideProperties = overrides,
            Locales = ItemHelper.CreateEnLocale(def.Name, def.ShortName, def.Description),
        };

        var result = customItemService.CreateItemFromClone(details);

        if (result.Success != true)
        {
            logger.LogWithColor(
                $"[AmmoGen] CreateItemFromClone reported failure for grenade '{def.Name}': {string.Join(", ", result.Errors ?? [])}",
                Color.Yellow);
            return false;
        }

        var items = templateTable.Items;
        if (items.TryGetValue(def.Id, out var tpl) && tpl.Properties != null)
        {
            ItemHelper.ApplyCommonPostRegistration(
                tpl.Properties, def.Economy.RarityPvE, def.Economy.FleaBanned,
                def.Stats.BackgroundColor, def.Stats.BackgroundAlpha);

            // SPT's TemplateItemProperties does not expose these fields directly, so set them via reflection
            // if the underlying cloned template has them.
            if (def.Stats.MinFragmentDamage > 0)
                ReflectionHelper.SetPropertyOrField(tpl.Properties, "MinFragmentDamage", (float)def.Stats.MinFragmentDamage);
            if (def.Stats.CanPlantOnGround)
                ReflectionHelper.SetPropertyOrField(tpl.Properties, "CanPlantOnGround", true);
        }

        return true;
    }

}
