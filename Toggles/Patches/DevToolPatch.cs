using System.Reflection;
using System.Reflection.Emit;
using BepisLocaleLoader;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;

// Fixes issue #2 https://github.com/LeCloutPanda/Toggles/issues/2, reworked by [NepuShiro](https://github.com/NepuShiro)
[HarmonyPatch]
public static class DevToolPatch
{
    private static DynamicVariableSpace? DynvarSpace { get; set; }
    private static DynamicValueVariable<bool>? Dynvar { get; set; }
    private const string DynvarSpaceName = "Mod.Toggles";
    private const string DynvarName = DynvarSpaceName + "/ToggleGizmoSnapping";
    private static bool ShouldSnap => Dynvar?.Value.Value ?? true;
    private static DevTool? _tool;

    public static void InitSubs()
    {
        Toggles.Plugin.DEVTOOL_GIZMOSNAPPING_VALUE.SettingChanged += (_, _) =>
        {
            List<World> worlds = Pool.BorrowList<World>();
            Engine.Current.WorldManager.GetWorlds(worlds);
            worlds.Do(w =>
            {
                w.LocalUser.GetActiveTools().Do(t =>
                {
                    if (t == null || !t.Slot.ActiveUser.IsLocalUser) return;

                    Slot dynvarSlot = t.Slot;
                    if (t is ToolMultiplexer multiplexer)
                    {
                        dynvarSlot = multiplexer.ActiveTool.Slot;
                    }

                    dynvarSlot.WriteDynamicVariable(DynvarName, Toggles.Plugin.DEVTOOL_GIZMOSNAPPING_VALUE.Value);
                });
            });
        };
    }

    [HarmonyPatch(typeof(DevTool), "OnEquipped")]
    [HarmonyPostfix]
    private static void SetupDynvars(DevTool __instance)
    {
        if (__instance == null || __instance.Slot.ActiveUser != __instance.LocalUser || __instance.World.IsUserspace()) return;
        _tool = __instance;

        __instance.RunInUpdates(3, () =>
        {
            DynvarSpace = __instance.Slot.GetComponentOrAttach<DynamicVariableSpace>(x => x.SpaceName.Value == DynvarSpaceName);
            if (DynvarSpace == null) return;
            DynvarSpace.Persistent = false;
            DynvarSpace.SpaceName.Value = DynvarSpaceName;
            DynvarSpace.OnlyDirectBinding.Value = true;

            Dynvar = __instance.Slot.GetComponentOrAttach<DynamicValueVariable<bool>>(x => x.VariableName.Value == DynvarName);
            if (Dynvar == null) return;
            Dynvar.Persistent = false;
            Dynvar.VariableName.Value = DynvarName;
            Dynvar.Value.Value = Toggles.Plugin.DEVTOOL_GIZMOSNAPPING_VALUE.Value;
        });
    }

    [HarmonyPatch]
    public static class DevToolAddItemPatch
    {
        static MethodBase TargetMethod() => AccessTools.Method(typeof(ContextMenu), "AddItem", new Type[] { typeof(LocaleString).MakeByRefType(), typeof(Uri), typeof(colorX?).MakeByRefType(), typeof(ButtonEventHandler) });

        [HarmonyPostfix]
        private static void Postfix(ContextMenu __instance, in LocaleString label, Uri? icon, in colorX? color, ButtonEventHandler action)
        {
            if (!Toggles.Plugin.DEVTOOL_GIZMOSNAPPING_BUTTON.Value) return;
            if (__instance.World.IsUserspace()) return;
            if (__instance == null || !__instance.Slot.ActiveUser.IsLocalUser) return;
            if (label.content != "Tools.Dev.Scale") return;
            if (Dynvar == null) return;

            ContextMenuItem item = __instance.AddItem("Settings.dev.lecloutpanda.Toggles.DevTool.GizmoSnapping".T("Toggle Gizmo Snapping", true), new Uri("resdb:///2cc67da92ecbf3ff611e177f8e53aca5d535dd6339f0779b3dc8d660ed0585c3.png"), Dynvar.Value ? colorX.Green : colorX.Red);
            item.Button.LocalPressed += (IButton button, ButtonEventData eventData) =>
            {

                if (Dynvar != null)
                {
                    item.Color.Value = Dynvar.Value ? colorX.Green : colorX.Red;
                    Toggles.Plugin.DEVTOOL_GIZMOSNAPPING_VALUE.Value = Dynvar.Value;
                    _tool?.Slot.WriteDynamicVariable<bool>(DynvarName, !Dynvar.Value);
                }
            };
        }
    }

    [HarmonyPatch(typeof(Gizmo), "UpdatePoint", new Type[] { typeof(Component), typeof(float3), typeof(float3) })]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ToggleSnappingTranspiler(IEnumerable<CodeInstruction> codes)
    {
        MethodInfo lookFor = AccessTools.Method(typeof(SnapHelper), "GetBestSnapPoint");

        var found = false;
        foreach (var code in codes)
        {
            if (code.Calls(lookFor))
            {
                found = true;
                yield return new CodeInstruction(OpCodes.Call, ((Delegate)BestSnapPointDummy).Method);
            }
            else
            {
                yield return code;
            }
        }

        if (!found)
        {
            Toggles.Plugin.Log.LogError("Failed to patch ToggleSnappingTranspiler");
        }
    }

    private static float3? BestSnapPointDummy(World world, float3 globalPoint, float checkRadius, out IPointSnappable snappable, Predicate<IPointSnappable> snappableFilter = null)
    {
        if (!ShouldSnap)
        {
            snappable = null!;
            return null;
        }
        return SnapHelper.GetBestSnapPoint(world, globalPoint, checkRadius, out snappable, snappableFilter);
    }
}
