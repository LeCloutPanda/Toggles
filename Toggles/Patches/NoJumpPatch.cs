using FrooxEngine;
using HarmonyLib;
using Toggles;

// // Fixes issue #3 https://github.com/LeCloutPanda/mToggles/issues/3, patch by [Gyztor](https://github.com/Gyztor)
[HarmonyPatch(typeof(DualControllerBindingGenerator), "BindJump")]
static class NoJumpPatch
{
    [HarmonyPrefix]
    private static bool BindJumpPrefix(InputGroup group, IDualBindingController left, IDualBindingController right, ref AnyInput __result)
    {
        AnyInput anyInput = new AnyInput();

        if (Plugin.MAYBE_JUMP_LEFT.Value)
        {
            left?.BindNodeActions(group, anyInput, "Jump");
        }
        if (Plugin.MAYBE_JUMP_RIGHT.Value)
        {
            right?.BindNodeActions(group, anyInput, "Jump");
        }

        __result = anyInput;
        return false;
    }
}