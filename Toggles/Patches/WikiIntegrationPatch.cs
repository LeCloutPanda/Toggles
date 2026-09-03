using System.Reflection;
using System.Reflection.Emit;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using Toggles;

// Patch by (Arti)[https://github.com/art0007i]
[HarmonyPatch]
public static class WikiIntegrationPatch
{
    private static MethodInfo wikiFunc = AccessTools.Method(typeof(Hyperlink), nameof(Hyperlink.AttachForWikiPage));
    
    [HarmonyPatch(typeof(WorkerInspector), "BuildUIForComponent")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> InspectorTranspiler(IEnumerable<CodeInstruction> codes)
    {
        var found = false;
        foreach (var code in codes)
        {
            if (code.Calls(wikiFunc))
            {
                found = true;
                yield return new(OpCodes.Call, ((Delegate)FakeWikiFuncInspector).Method);
            }
            else
            {
                yield return code;
            }
        }

        if (!found)
        {
            Plugin.Log.LogError("Failed to patch WikiIntegrationComponentPatch");
        }
    }

    [HarmonyPatch(typeof(ProtoFluxTool), nameof(ProtoFluxTool.GenerateMenuItems))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ProtofluxToolTranspiler(IEnumerable<CodeInstruction> codes)
    {
        var found = false;
        foreach (var code in codes)
        {
            if (code.Calls(wikiFunc))
            {
                found = true;
                yield return new(OpCodes.Call, ((Delegate)FakeWikiFuncProtoflux).Method);
            }
            else
            {
                yield return code;
            }
        }

        if (!found)
        {
            Plugin.Log.LogError("Failed to patch ProtofluxPatch");
        }
    }

    public static Hyperlink? FakeWikiFuncInspector(Slot slot, Type type)
    {
        if (Plugin.WIKI_INTEGRATION_INSPECTOR.Value) return Hyperlink.AttachForWikiPage(slot, type);
        slot.ActiveSelf = false;
        return null;
    }

    public static Hyperlink? FakeWikiFuncProtoflux(Slot slot, Type type)
    {
        if (Plugin.WIKI_INTEGRATION_PROTOFLUX.Value) return Hyperlink.AttachForWikiPage(slot, type);
        slot.ActiveSelf = false;
        return null;
    }
}