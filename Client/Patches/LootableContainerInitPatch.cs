using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace AmmoGenClient.Patches;

[HarmonyPatch(typeof(LootableContainer), nameof(LootableContainer.Init))]
public class LootableContainerInitPatch
{
    private static int _callCount = 0;
    private static int _watchedCount = 0;

    static void Postfix(LootableContainer __instance)
    {
        if (!AmmoGenClientPlugin.DebugEnabled.Value)
            return;

        _callCount++;
        var root = __instance.ItemOwner?.RootItem;
        var containerTemplate = __instance.Template ?? "unknown";
        var containerName = __instance.gameObject?.name ?? "unknown";
        var visibleItems = root?.GetAllVisibleItems()?.ToList();
        var allItems = root?.GetAllItems()?.ToList();
        var visibleCount = visibleItems?.Count ?? 0;
        var allCount = allItems?.Count ?? 0;
        var rootType = root?.GetType().Name ?? "null";

        if (_callCount <= 10 || _callCount % 50 == 0)
        {
            AmmoGenClientPlugin.Log.LogInfo(
                $"[AmmoGenClient][DEBUG] LootableContainer.Init #{_callCount} (watched total: {_watchedCount}) template: {containerTemplate} name: {containerName} rootType: {rootType} visibleItems: {visibleCount} allItems: {allCount}");
        }

        if (root == null || allItems == null)
            return;

        if (allCount <= 1)
        {
            if (_callCount <= 10)
            {
                AmmoGenClientPlugin.Log.LogInfo(
                    $"[AmmoGenClient][DEBUG] Container {containerName} has no nested items (rootType: {rootType}).");
            }
            return;
        }

        var items = allItems;

        bool sawAny = false;
        var watchedTemplates = new List<string>();

        foreach (var item in items)
        {
            if (item == root)
                continue;

            var templateId = item.TemplateId.ToString().ToLowerInvariant();
            if (string.IsNullOrEmpty(templateId))
                continue;

            if (!AmmoGenClientPlugin.WatchedTemplateIds.Contains(templateId))
                continue;

            sawAny = true;
            watchedTemplates.Add(templateId);
            _watchedCount++;
            AmmoGenClientPlugin.Log.LogInfo(
                $"[AmmoGenClient][CONTAINER] {containerTemplate} ({containerName}) contains watched item {item} (Tpl: {templateId}, Id: {item.Id})");
        }

        if (sawAny)
        {
            AmmoGenClientPlugin.Log.LogInfo(
                $"[AmmoGenClient][CONTAINER] Total items in {containerTemplate} ({containerName}): {items.Count - 1} watched templates: {string.Join(", ", watchedTemplates)}");
        }
    }
}
