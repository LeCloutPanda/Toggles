using FrooxEngine;
using HarmonyLib;
using Toggles;

// Fixes issue #7 https://github.com/LeCloutPanda/Toggles/issues/7
[HarmonyPatch(typeof(HapticHelper))]
public static class ControllerHapticsPatch
{
    [HarmonyPatch("TryVibrateShort")]
    [HarmonyPrefix]
    public static bool TryShortVibratePatch(this Slot slot)
    {
        if (Plugin.CONTROLLER_HAPTIC_DASH.Value == false && slot.World.IsUserspace()) return false;
        if (Plugin.CONTROLLER_HAPTIC_WORLD.Value == false && !slot.World.IsUserspace()) return false;
        return true;
    }

    [HarmonyPatch("TryVibrateMedium")]
    [HarmonyPrefix]
    public static bool TryVibrateMediumPatch(this Slot slot)
    {
        if (Plugin.CONTROLLER_HAPTIC_DASH.Value == false && slot.World.IsUserspace()) return false;
        if (Plugin.CONTROLLER_HAPTIC_WORLD.Value == false && !slot.World.IsUserspace()) return false;
        return true;
    }

    [HarmonyPatch("TryVibrateLong")]
    [HarmonyPrefix]
    public static bool TryVibrateLongPatch(this Slot slot)
    {
        if (Plugin.CONTROLLER_HAPTIC_DASH.Value == false && slot.World.IsUserspace()) return false;
        if (Plugin.CONTROLLER_HAPTIC_WORLD.Value == false && !slot.World.IsUserspace()) return false;
        return true;
    }

    [HarmonyPatch("TryVibrateRelative")]
    [HarmonyPrefix]
    public static bool TryVibrateRelativePatch(this Slot slot)
    {
        if (Plugin.CONTROLLER_HAPTIC_DASH.Value == false && slot.World.IsUserspace()) return false;
        if (Plugin.CONTROLLER_HAPTIC_WORLD.Value == false && !slot.World.IsUserspace()) return false;
        return true;
    }
}