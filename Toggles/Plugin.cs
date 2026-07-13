using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.NET.Common;
using BepInExResoniteShim;
using BepisLocaleLoader;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using HarmonyLib;
using SkyFrost.Base;

namespace Toggles;

[ResonitePlugin(PluginMetadata.GUID, PluginMetadata.NAME, PluginMetadata.VERSION, PluginMetadata.AUTHORS, PluginMetadata.REPOSITORY_URL)]
[BepInDependency(BepInExResoniteShim.PluginMetadata.GUID, BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log = null!;

    private static ConfigEntry<bool> INVENTORY_SAVE_AVATAR_BUTTON = null!;
    private static ConfigEntry<bool> INVENTORY_GET_URL_BUTTON = null!;
    private static ConfigEntry<bool> PROTOFLUX_OVERVIEW_BUTTON = null!;
    private static ConfigEntry<bool> DEVTOOL_GIZMOSNAPPING_BUTTON = null!;
    private static ConfigEntry<bool> DEVTOOL_GIZMOSNAPPING_VALUE = null!;
    private static ConfigEntry<bool> CONTACT_ASK_TO_JOIN_BUTTON = null!;
    private static ConfigEntry<bool> WIKI_INTEGRATION_INSPECTOR = null!;
    private static ConfigEntry<bool> WIKI_INTEGRATION_PROTOFLUX = null!;
    private static ConfigEntry<bool> SLOT_TAG_INHERITANCE = null!;
    private static ConfigEntry<bool> MAYBE_JUMP_LEFT = null!;
    private static ConfigEntry<bool> MAYBE_JUMP_RIGHT = null!;

    public override void Load()
    {
        Log = base.Log;
        HarmonyInstance.PatchAll();

        INVENTORY_SAVE_AVATAR_BUTTON = Config.Bind("Inventory", "Inventory Save Avatar Button", true, new ConfigDescription("Toggle generation of 'Save Avatar' button in inventory", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.Inventory.ToggleSaveAvatarButton", "Settings.dev.lecloutpanda.Toggles.Inventory.ToggleSaveAvatarButton.Description")));
        INVENTORY_GET_URL_BUTTON = Config.Bind("Inventory", "Inventory Get Item Url Button", true, new ConfigDescription("Toggle generation of 'Get Url' button in inventory", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.Inventory.ToggleGetUrlButton", "Settings.dev.lecloutpanda.Toggles.Inventory.ToggleGetUrlButton.Description")));
        PROTOFLUX_OVERVIEW_BUTTON = Config.Bind("Protoflux Tool", "Overview Button", true, new ConfigDescription("Toggle generation of 'Overview' context menu button on protoflux tools", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.ProtofluxTool.ToggleOverviewButton", "Settings.dev.lecloutpanda.Toggles.ProtofluxTool.ToggleOverviewButton.Description")));
        DEVTOOL_GIZMOSNAPPING_BUTTON = Config.Bind("Dev Tool", "Gizmo Snapping Button", true, new ConfigDescription("Toggle generation of a 'Gizmo Snapping' context menu button in the 'Gizmo Options' menu on a Dev Tool", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.DevTool.ToggleGizmoSnappingButton", "Settings.dev.lecloutpanda.Toggles.DevTool.ToggleGizmoSnappingButton.Description")));
        DEVTOOL_GIZMOSNAPPING_VALUE = Config.Bind("Dev Tool", "Default Gizmo Snapping Value", true, new ConfigDescription("Default value for 'Gizmo Snapping'", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.DevTool.DefaultGizmoSnappingValue", "Settings.dev.lecloutpanda.Toggles.DevTool.DefaultGizmoSnappingValue.Description")));
        CONTACT_ASK_TO_JOIN_BUTTON = Config.Bind("Contacts", "Ask To Join Button", true, new ConfigDescription("Toggle Visibility of 'Ask To Join' button", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.Contacts.ToggleAskToJoinButton", "Settings.dev.lecloutpanda.Toggles.Contacts.ToggleAskToJoinButton.Description")));
        WIKI_INTEGRATION_INSPECTOR = Config.Bind("Misc", "Wiki Integration Inspector", true, new ConfigDescription("Toggle 'Wiki Hyperlink' button for components in inspectors", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.Misc.ToggleWikiIntegrationInspector", "Settings.dev.lecloutpanda.Toggles.Misc.ToggleWikiIntegrationInspector.Description")));
        WIKI_INTEGRATION_PROTOFLUX = Config.Bind("Misc", "Wiki Integration Protoflux", true, new ConfigDescription("Toggle 'Wiki Hyperlink' button for the protoflux tool context menu item", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.Misc.ToggleWikiIntegrationProtoflux", "Settings.dev.lecloutpanda.Toggles.Misc.ToggleWikiIntegrationProtoflux.Description")));
        SLOT_TAG_INHERITANCE = Config.Bind("Inspector", "Tag Inheritance", true, new ConfigDescription("Toggle 'Tag Inheritance' when creating a child slot via the inspector", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.Inspector.SlotTagInheritance", "Settings.dev.lecloutpanda.Toggles.Inspector.SlotTagInheritance.Description")));
        MAYBE_JUMP_LEFT = Config.Bind("Controls", "Maybe Jump Left", true, new ConfigDescription("TEMP DESCRIPTOR", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.Controls.MaybeJumpLeft", "Settings.dev.lecloutpanda.Toggles.Controls.MaybeJumpLeft.Description")));
        MAYBE_JUMP_RIGHT = Config.Bind("Controls", "Maybe Jump Right", true, new ConfigDescription("TEMP DESCRIPTOR", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.Controls.MaybeJumpRight", "Settings.dev.lecloutpanda.Toggles.Controls.MaybeJumpRight.Description")));

        DevToolPatches.InitSubs();
    }


    /// Patch by (Arti)[https://github.com/art0007i]
    /// Patches out wiki integrations via a toggle 
    [HarmonyPatch]
    public static class WikiIntegrationPatches
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
                Log.LogError("Failed to patch WikiIntegrationComponentPatch");
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
                Log.LogError("Failed to patch ProtofluxPatch");
            }
        }

        public static Hyperlink? FakeWikiFuncInspector(Slot slot, Type type)
        {
            if (WIKI_INTEGRATION_INSPECTOR.Value) return Hyperlink.AttachForWikiPage(slot, type);
            slot.Destroy();
            return null;
        }

        public static Hyperlink? FakeWikiFuncProtoflux(Slot slot, Type type)
        {
            if (WIKI_INTEGRATION_PROTOFLUX.Value) return Hyperlink.AttachForWikiPage(slot, type);
            slot.Destroy();
            return null;
        }
    }

    /// Patch by (Panda)[https://github.com/lecloutpanda], reworked by [NepuShiro](https://github.com/NepuShiro)
    /// Patch inventory browser to remove buttons via toggles
    [HarmonyPatch(typeof(InventoryBrowser))]
    public static class InventoryBrowserPatch
    {
        [HarmonyPatch("OnItemSelected")]
        [HarmonyPostfix]
        public static void Postfix(SyncRef<Button> ____addCurrentAvatar, SyncRef<Button> ____copyLink)
        {
            try
            {
                if (!INVENTORY_SAVE_AVATAR_BUTTON.Value)
                {
                    ____addCurrentAvatar.Target?.Slot.ActiveSelf = false;
                }

                if (!INVENTORY_GET_URL_BUTTON.Value)
                {
                    ____copyLink.Target?.Slot.ActiveSelf = false;
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex);
            }
        }
    }

    // Fixes issue #1 (https://github.com/LeCloutPanda/Toggles/issues/1), reworked by [NepuShiro](https://github.com/NepuShiro)
    [HarmonyPatch(typeof(ProtoFluxTool), nameof(ProtoFluxTool.GenerateMenuItems))]
    class HideOverviewPatch
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
                if (PROTOFLUX_OVERVIEW_BUTTON.Value && menu != null)
                {
                    menu.AddItem("Tools.ProtoFlux.Overview".AsLocaleKey(), (Uri?)null, new colorX(1.0f, 1.0f, 0.0f), ToggleOverviewMethod.CreateDelegate<ButtonEventHandler>(tool));
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex);
            }
        }
    }

    // Fixes issue #2 https://github.com/LeCloutPanda/Toggles/issues/2, reworked by [NepuShiro](https://github.com/NepuShiro)
    [HarmonyPatch]
    public static class DevToolPatches
    {
        private static DynamicVariableSpace? DynvarSpace { get; set; }
        private static DynamicValueVariable<bool>? Dynvar { get; set; }
        private const string DynvarSpaceName = "Mod.Toggles";
        private const string DynvarName = DynvarSpaceName + "/ToggleGizmoSnapping";
        private static bool ShouldSnap => Dynvar?.Value.Value ?? true;
        private static DevTool? _tool;

        internal static void InitSubs()
        {
            DEVTOOL_GIZMOSNAPPING_VALUE.SettingChanged += (_, _) =>
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

                        dynvarSlot.WriteDynamicVariable(DynvarName, DEVTOOL_GIZMOSNAPPING_VALUE.Value);
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
                Dynvar.Value.Value = DEVTOOL_GIZMOSNAPPING_VALUE.Value;
            });
        }

        [HarmonyPatch]
        public static class DevToolAddItemPatch
        {
            static MethodBase TargetMethod() => AccessTools.Method(typeof(ContextMenu), "AddItem", new Type[] { typeof(LocaleString).MakeByRefType(), typeof(Uri), typeof(colorX?).MakeByRefType(), typeof(ButtonEventHandler) });

            [HarmonyPostfix]
            private static void Postfix(ContextMenu __instance, in LocaleString label, Uri? icon, in colorX? color, ButtonEventHandler action)
            {
                if (__instance == null || !__instance.Slot.ActiveUser.IsLocalUser) return;
                if (!DEVTOOL_GIZMOSNAPPING_BUTTON.Value) return;
                if (label.content != "Tools.Dev.Scale") return;
                if (Dynvar == null) return;

                ContextMenuItem item = __instance.AddItem("Settings.dev.lecloutpanda.Toggles.DevTool.GizmoSnapping".T("Toggle Gizmo Snapping", true), new Uri("resdb:///2cc67da92ecbf3ff611e177f8e53aca5d535dd6339f0779b3dc8d660ed0585c3.png"), Dynvar.Value ? colorX.Green : colorX.Red);
                item.Button.LocalPressed += (IButton button, ButtonEventData eventData) =>
                {
                    if (Dynvar != null)
                    {
                        _tool?.Slot.WriteDynamicVariable<bool>(DynvarName, !Dynvar.Value);
                        item.Color.Value = Dynvar.Value ? colorX.Green : colorX.Red;
                        DEVTOOL_GIZMOSNAPPING_VALUE.Value = Dynvar.Value;
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
                Log.LogError("Failed to patch ToggleSnappingTranspiler");
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

    // Fixes issue #4 https://github.com/LeCloutPanda/Toggles/issues/4
    [HarmonyPatch(typeof(ContactItem), nameof(ContactItem.Update), new[] { typeof(Contact), typeof(ContactData) })]
    public static class ContactsPagePatches
    {
        public static void Postfix(Contact contact, ContactData data, SyncRef<Button> ____joinButton)
        {
            if (CONTACT_ASK_TO_JOIN_BUTTON.Value) return;
            if (!contact.IsAccepted || contact.ContactStatus != ContactStatus.Accepted) return;
            if (data == null) return;
            if (data.CurrentSessionInfo != null) return;
            if (data.CurrentStatus.OnlineStatus.GetValueOrDefault() == OnlineStatus.Offline) return;
            ____joinButton?.Target?.Slot.ActiveSelf = false;
        }
    }

    // Fixes issue #3 https://github.com/LeCloutPanda/mToggles/issues/3, patch by [Gyztor](https://github.com/Gyztor)
    [HarmonyPatch(typeof(DualControllerBindingGenerator), "BindJump")]
    static class NoJumpPatch
    {
        [HarmonyPrefix]
        private static bool BindJumpPrefix(InputGroup group, IDualBindingController left, IDualBindingController right, ref AnyInput __result)
        {
            AnyInput anyInput = new AnyInput();

            if (MAYBE_JUMP_LEFT.Value)
            {
                left?.BindNodeActions(group, anyInput, "Jump");
            }
            if (MAYBE_JUMP_RIGHT.Value)
            {
                right?.BindNodeActions(group, anyInput, "Jump");
            }

            __result = anyInput;
            return false;
        }
    }

    // Fixes issue #6 https://github.com/LeCloutPanda/Toggles/issues/6
    [HarmonyPatch]
    public static class InspectorPatches
    {
        private static Slot ComponentViewTarget = null;
        private static ButtonEventData EventData;

        [HarmonyPatch(typeof(SceneInspector), "OnAddChildPressed")]
        [HarmonyPrefix]
        public static void AddChildPostfix(SyncRef<Slot> ___ComponentView, IButton button, ButtonEventData eventData)
        {
            ComponentViewTarget = ___ComponentView;
            EventData = eventData;
        }

        [HarmonyPatch(typeof(Slot), nameof(Slot.AddSlot))]
        [HarmonyPostfix]
        public static void AddSlotPostfix(Slot __result)
        {
            if (SLOT_TAG_INHERITANCE.Value) return;

            try 
            {
                if (__result == null || __result.IsDestroying || __result.IsDestroyed) return;
                if (EventData.source == null) return;
                if (!EventData.source.IsUnderLocalUser) return;
                if (ComponentViewTarget == null || ComponentViewTarget.IsDestroying || ComponentViewTarget.IsDestroyed) return;
                if (__result.Name.Contains(ComponentViewTarget.Name) == false) return;
                if (__result.Tag != ComponentViewTarget.Tag) return;
                if (__result.Parent == null || __result.Parent.IsDestroying || __result.Parent.IsDestroyed) return;
                if (__result.Parent != ComponentViewTarget) return;
                __result.Tag = null; 
                ComponentViewTarget = null;
            } 
            catch(Exception ex) 
            {
                Log.LogMessage("Failed to remove inherited tag for reason: " + ex);
            }
        }
    }
}