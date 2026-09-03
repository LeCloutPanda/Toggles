using FrooxEngine;
using HarmonyLib;
using Toggles;

// Fixes issue #6 https://github.com/LeCloutPanda/Toggles/issues/6
[HarmonyPatch]
public static class InspectorPatch
{
    private static Slot ComponentViewTarget = null;
    private static ButtonEventData EventData;

    [HarmonyPatch(typeof(SceneInspector), "OnAddChildPressed")]
    [HarmonyPrefix]
    public static void AddChildPostfix(IButton button, ButtonEventData eventData, SceneInspector __instance)
    {
        ComponentViewTarget = __instance.ComponentView.Target;
        EventData = eventData;
    }

    [HarmonyPatch(typeof(Slot), nameof(Slot.AddSlot))]
    [HarmonyPostfix]
    public static void AddSlotPostfix(Slot __result)
    {
        if (Plugin.SLOT_TAG_INHERITANCE.Value) return;

        try 
        {
            if (__result == null || __result.IsDestroying || __result.IsDestroyed) return;
            if (EventData.source == null) return;
            if (!EventData.source.IsUnderLocalUser) return;
            if (ComponentViewTarget == null) return;
            if (ComponentViewTarget.IsDestroying || ComponentViewTarget.IsDestroyed)
            if (__result.Name.Contains(ComponentViewTarget.Name) == false) return;
            if (__result.Tag != ComponentViewTarget.Tag) return;
            if (__result.Parent == null || __result.Parent.IsDestroying || __result.Parent.IsDestroyed) return;
            if (__result.Parent != ComponentViewTarget) return;
            __result.Tag = null; 
            ComponentViewTarget = null;
        } 
        catch(Exception ex) 
        {
            Plugin.Log.LogMessage("Failed to remove inherited tag for reason: " + ex);
        }
    }
}