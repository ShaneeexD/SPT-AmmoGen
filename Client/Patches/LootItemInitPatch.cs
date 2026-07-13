using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using UnityEngine;

namespace AmmoGenClient.Patches;

[HarmonyPatch(typeof(LootItem), nameof(LootItem.Init))]
public class LootItemInitPatch
{
    private static int _callCount = 0;
    private static int _watchedCount = 0;

    static void Postfix(LootItem __instance, Item __0)
    {
        if (!AmmoGenClientPlugin.DebugEnabled.Value)
            return;

        var item = __0;
        if (item == null)
            return;

        var templateId = item.TemplateId.ToString().ToLowerInvariant();
        if (string.IsNullOrEmpty(templateId))
            return;

        _callCount++;
        var isWatched = AmmoGenClientPlugin.WatchedTemplateIds.Contains(templateId);
        if (isWatched)
        {
            _watchedCount++;
        }

        if (_callCount <= 10 || _callCount % 100 == 0 || isWatched)
        {
            AmmoGenClientPlugin.Log.LogInfo(
                $"[AmmoGenClient][DEBUG] LootItem.Init #{_callCount} (watched total: {_watchedCount}) tpl: {templateId} watched: {isWatched}");
        }

        if (!isWatched)
            return;

        var position = __instance != null ? __instance.transform.position : Vector3.zero;
        var staticId = __instance?.StaticId ?? "n/a";
        var name = __instance?.Name ?? item.ToString() ?? "unknown";

        AmmoGenClientPlugin.Log.LogInfo(
            $"[AmmoGenClient][SPAWN] {name} (Tpl: {templateId}, Id: {item.Id}, StaticId: {staticId}) at {position}");
    }
}
