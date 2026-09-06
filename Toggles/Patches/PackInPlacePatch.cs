
using System.Reflection;
using System.Reflection.Emit;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using Toggles;

// Fixes issue #10 (https://github.com/LeCloutPanda/Toggles/issues/10), Copied from OverviewModePatch which was reworked by [NepuShiro](https://github.com/NepuShiro)
[HarmonyPatch(typeof(ProtoFluxTool), nameof(ProtoFluxTool.GenerateMenuItems))]
public static class PackInToolPatch
{
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes, ILGenerator generator)
    {
        MethodInfo addItemMethod = AccessTools.Method(typeof(ContextMenu), "AddItem", new Type[] { typeof(LocaleString).MakeByRefType(), typeof(Uri), typeof(colorX?).MakeByRefType(), typeof(ButtonEventHandler) });

        List<CodeInstruction> codeList = codes.ToList();
        CodeMatcher matcher = new CodeMatcher(codeList, generator);

        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldarg_2), new CodeMatch(OpCodes.Ldstr, "Tools.ProtoFlux.PackInPlace"));
        if (!matcher.IsValid) return codeList;

        int start = matcher.Pos;

        matcher.MatchEndForward(new CodeMatch(OpCodes.Callvirt, addItemMethod), new CodeMatch(OpCodes.Pop));
        if (!matcher.IsValid) return codeList;

        int end = matcher.Pos;

        matcher.Start().Advance(start).RemoveInstructions(end - start + 1).Insert(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2), new CodeInstruction(OpCodes.Call, ((Delegate)AddItemProxy).Method));

        return matcher.InstructionEnumeration();
    }

    static void AddItemProxy(ProtoFluxTool tool, ContextMenu menu)
    {
        try
        {
            if (Plugin.PROTOFLUX_PACK_IN_PLACE_BUTTON.Value && menu != null)
            {
                MethodInfo onPackInPlaceMethod = tool.GetType().GetMethod(
                    "OnPackInPlace",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

                ButtonEventHandler<ProtoFluxNode> onPackInPlaceHandler =
                    (ButtonEventHandler<ProtoFluxNode>)onPackInPlaceMethod.CreateDelegate(
                        typeof(ButtonEventHandler<ProtoFluxNode>),
                        tool
                    );

                menu.AddRefItem(
                    "Tools.ProtoFlux.PackInPlace".AsLocaleKey(),
                    null,
                    new colorX?(colorX.Purple),
                    onPackInPlaceHandler,
                    (ProtoFluxNode)null
                );

            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError(ex);
        }
    }
}