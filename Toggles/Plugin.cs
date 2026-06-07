using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
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
    internal static new ManualLogSource? Log;

    private static ConfigEntry<bool> INVENTORY_SAVE_AVATAR_BUTTON;
    private static ConfigEntry<bool> INVENTORY_GET_URL_BUTTON;
    private static ConfigEntry<bool> PROTOFLUX_OVERVIEW_BUTTON;
    private static ConfigEntry<bool> DEVTOOL_GIZMOSNAPPING_BUTTON;
    private static ConfigEntry<bool> DEVTOOL_GIZMOSNAPPING_VALUE;
    private static ConfigEntry<bool> CONTACT_ASK_TO_JOIN_BUTTON;
    private static ConfigEntry<bool> WIKI_INTEGRATION_INSPECTOR;
    private static ConfigEntry<bool> WIKI_INTEGRATION_PROTOFLUX;

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
            if(WIKI_INTEGRATION_INSPECTOR.Value) return Hyperlink.AttachForWikiPage(slot, type);
            slot.Destroy();
            return null;
        }
        
        public static Hyperlink? FakeWikiFuncProtoflux(Slot slot, Type type)
        {
            if(WIKI_INTEGRATION_PROTOFLUX.Value) return Hyperlink.AttachForWikiPage(slot, type);
            slot.Destroy();
            return null;
        }
    }

    /// Patch by (Panda)[https://github.com/lecloutpanda]
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
                if (!INVENTORY_SAVE_AVATAR_BUTTON.Value && !____addCurrentAvatar.Slot.IsRemoved)
                {
                    ____addCurrentAvatar.Target.Slot.Destroy();
                }

                if (!INVENTORY_GET_URL_BUTTON.Value && !____copyLink.Slot.IsRemoved) 
                {
                    ____copyLink.Target.Slot.Destroy();
                }
            }
            catch(Exception ex) 
            { 
                Log.LogError(ex);
            }
        }
    }

    // Fixes issue #1 (https://github.com/LeCloutPanda/Toggles/issues/1)
    // This sucks I hate it so much please someone write it better <3
    [HarmonyPatch(typeof(ProtoFluxTool), nameof(ProtoFluxTool.GenerateMenuItems))]
    class HideOverviewPatch
    {
        static readonly MethodInfo AddItemMethod = typeof(ContextMenu).GetMethods().First(m => m.Name == "AddItem" && m.GetParameters()[3].ParameterType == typeof(ButtonEventHandler));
        static readonly MethodInfo ToggleOverviewMethod = typeof(ProtoFluxTool).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(m => m.Name == "ToggleOverviewMode");
        static bool shouldRun => PROTOFLUX_OVERVIEW_BUTTON.Value;
        static ProtoFluxTool tool;
        static ContextMenu menu;
        [HarmonyPrefix]
        private static void Prefix(ProtoFluxTool __instance) 
        {
            tool = __instance;
            menu = __instance.Slot.ActiveUser.GetUserContextMenu();
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            int start = -1;
            int end = -1;
            var codeList = new List<CodeInstruction>(codes);

            for (int i = 0; i < codeList.Count; i++)
            {
                var code = codeList[i];

                if (code.opcode == OpCodes.Ldstr && code.operand.ToString() == "Tools.ProtoFlux.Overview" && codeList[i - 1].opcode == OpCodes.Ldarg_2)
                {
                    start = i - 1;
                } 
                else if (code.opcode == OpCodes.Pop)
                {
                    end = i;
                }
            }

            if (start != -1 && end != -1) 
            {
                int count = (end - start) + 1;
                codeList.RemoveRange(start, count);
                codeList.Insert(start,  new(OpCodes.Call, ((Delegate)DummyCall).Method));
            }

            return codeList.AsEnumerable();
        }
        
        static void DummyCall()
        {
            try 
            {
                if (PROTOFLUX_OVERVIEW_BUTTON.Value && menu != null && tool != null) 
                {
                    menu.AddItem("Tools.ProtoFlux.Overview".AsLocaleKey(), (Uri?)null, new colorX(1.0f, 1.0f, 0.0f)).Button.LocalPressed += (IButton button, ButtonEventData eventData) => {
                        ToggleOverviewMethod.Invoke(tool, new object[] { button, eventData });
                    };
                }
            } catch (Exception ex) 
            {
                Log.LogError(ex);
            }

        }
    }

    // Fixes issue #2 https://github.com/LeCloutPanda/Toggles/issues/2 
    [HarmonyPatch]
    public static class DevToolPatches 
    {
		private static DynamicVariableSpace DynvarSpace { get; set; }
		private static DynamicValueVariable<bool> Dynvar { get; set; }
        private static string DynvarSpaceName = "Mod.Toggles";
        private static string DynvarName = DynvarSpaceName + "/ToggleGizmoSnapping";
        private static bool shouldSnap => Dynvar.Value.Value;
        private static DevTool tool;

		[HarmonyPatch(typeof(DevTool), "OnEquipped")]
		[HarmonyPostfix]
		private static void SetupDynvars(DevTool __instance)
		{
			if (__instance == null || __instance.Slot.ActiveUser != __instance.LocalUser || __instance.World.IsUserspace()) return;
            tool = __instance;

            // Temp removed cause broken
            // Dash to Tool(In world)
            //DEVTOOL_GIZMOSNAPPING_VALUE.SettingChanged += (object? sender, EventArgs e) => {
            //    Tool activeTool = __instance.LocalUser.GetActiveTool() as Tool;
            //    if (activeTool == null || activeTool.Slot.ActiveUser != activeTool.Slot.LocalUser || activeTool.World.IsUserspace()) return;
            //    Slot dynvarSlot = ((ToolMultiplexer) activeTool).ActiveTool.Slot ?? activeTool.Slot;
            //    dynvarSlot.WriteDynamicVariable<bool>(Dynvar.VariableName, DEVTOOL_GIZMOSNAPPING_VALUE.Value);
            //};

            __instance.RunInUpdates(3, () => {
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
    
        [HarmonyPatch(typeof(ContextMenuExtensions), "OpenContextMenu")]
        [HarmonyPostfix]
        private static void Postfix() 
        {
			if (tool == null || tool.Slot.ActiveUser != tool.LocalUser || tool.World.IsUserspace()) return;
            if (!DEVTOOL_GIZMOSNAPPING_BUTTON.Value) return;  

            try {
                if (CalledFromStack("FrooxEngine.DevTool.OpenGizmoOptions")) 
                {
                    InjectGizmoOption();
                }
            } 
            catch(Exception ex)
            {
                Log.LogError(ex);
            }
        }

        private static void InjectGizmoOption() {
            Log.LogMessage("Injecting Gizmo Contextmenu item");
            ContextMenu menu = tool.Slot.ActiveUser.GetUserContextMenu();
            menu.RunInUpdates(30, () => {
                if (menu.IsOpened) {
                    ContextMenuItem item = menu.AddItem("Settings.dev.lecloutpanda.Toggles.DevTool.GizmoSnapping".T("Toggle Gizmo Snapping", true), new Uri("resdb:///2cc67da92ecbf3ff611e177f8e53aca5d535dd6339f0779b3dc8d660ed0585c3.png"), Dynvar.Value ? colorX.Green : colorX.Red);
                    item.Button.LocalPressed += (IButton button, ButtonEventData eventData) => 
                    {
                        tool.Slot.WriteDynamicVariable<bool>(Dynvar.VariableName, !Dynvar.Value);
                        item.Color.Value = Dynvar.Value ? colorX.Green : colorX.Red; 
                        DEVTOOL_GIZMOSNAPPING_VALUE.Value = Dynvar.Value;
                    };
                }
            });
        }

        [HarmonyPatch(typeof(SnapHelper), nameof(SnapHelper.GetBestSnapPoint))]
        [HarmonyPrefix]
        private static bool ToggleSnapping(ref float3? __result) 
        {
            if (CalledFromStack("FrooxEngine.Gizmo.UpdatePoint") && !shouldSnap) 
            {
                __result = null;
                return false;
            } else return true;
        }

        static bool CalledFromStack(string name) 
        {
            var trace = new StackTrace();

            return trace.GetFrames()?.Any(f => 
            {
                var m = f.GetMethod();
                var methodFullName = $"{m?.DeclaringType?.FullName}.{m?.Name}";
                return methodFullName == name;
            }) ?? false;
        }
    }

    // Fixes issue #4 https://github.com/LeCloutPanda/Toggles/issues/4
    [HarmonyPatch]
    public static class ContactsPagePatches 
    {
        [HarmonyPatch(typeof(ContactItem), nameof(ContactItem.Update), new[] { typeof(Contact), typeof(ContactData) })]
        class ContactItemUpdatePatch
        {
            public static void Postfix(Contact contact, ContactData data, SyncRef<Button> ____joinButton) 
            {
                // have to figure out headless' that are not focused on session/s
                if (CONTACT_ASK_TO_JOIN_BUTTON.Value) return;
                if (!contact.IsAccepted || contact.ContactStatus != ContactStatus.Accepted) return;
                if (data == null) return;
                if (data.CurrentSessionInfo != null) return;
                if (data.CurrentStatus.OnlineStatus == null) return;
                if (data.CurrentStatus.OnlineStatus.GetValueOrDefault() == OnlineStatus.Offline) return;
                ____joinButton.Target.Slot.ActiveSelf = false;
            }
        }
    }
}