using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;

[HarmonyPatch]
public static class ProtofluxToolPatch
{
    [HarmonyPatch(typeof(ProtoFluxTool), "")]
    [HarmonyPostfix]
    private static void RemovePackInPlace(ProtoFluxTool __instance)
    {

    }
}
