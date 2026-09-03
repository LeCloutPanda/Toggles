using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using Toggles;

/// Patch by (Panda)[https://github.com/lecloutpanda], reworked by [NepuShiro](https://github.com/NepuShiro)
[HarmonyPatch(typeof(InventoryBrowser), "OnItemSelected")]
public static class InventoryBrowserPatch
{
    [HarmonyPostfix]
    public static void Postfix(SyncRef<Button> ____addCurrentAvatar, SyncRef<Button> ____copyLink)
    {
        try
        {
            if (!Plugin.INVENTORY_SAVE_AVATAR_BUTTON.Value)
            {
                ____addCurrentAvatar.Target?.Slot.ActiveSelf = false;
            }

            if (!Plugin.INVENTORY_GET_URL_BUTTON.Value)
            {
                ____copyLink.Target?.Slot.ActiveSelf = false;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError(ex);
        }
    }
}