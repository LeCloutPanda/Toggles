
using System.Reflection;
using System.Reflection.Emit;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using Toggles;

// Fixes issue #1 (https://github.com/LeCloutPanda/Toggles/issues/1), reworked by [NepuShiro](https://github.com/NepuShiro)
[HarmonyPatch(typeof(ProtoFluxTool), nameof(ProtoFluxTool.GenerateMenuItems))]
public static class OverviewModePatch
{
    static readonly MethodInfo ToggleOverviewMethod = AccessTools.Method(typeof(ProtoFluxTool), "ToggleOverviewMode");

    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes, ILGenerator generator)
    {
        MethodInfo addItemMethod = AccessTools.Method(typeof(ContextMenu), "AddItem", new Type[] { typeof(LocaleString).MakeByRefType(), typeof(Uri), typeof(colorX?).MakeByRefType(), typeof(ButtonEventHandler) });

        List<CodeInstruction> codeList = codes.ToList();
        CodeMatcher matcher = new CodeMatcher(codeList, generator);

        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldarg_2), new CodeMatch(OpCodes.Ldstr, "Tools.ProtoFlux.Overview"));
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
            if (Plugin.PROTOFLUX_OVERVIEW_BUTTON.Value && menu != null)
            {
                menu.AddItem("Tools.ProtoFlux.Overview".AsLocaleKey(), (Uri?)null, new colorX(1.0f, 1.0f, 0.0f), ToggleOverviewMethod.CreateDelegate<ButtonEventHandler>(tool));
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError(ex);
        }
    }
}