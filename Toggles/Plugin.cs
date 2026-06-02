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

namespace Toggles;

[ResonitePlugin(PluginMetadata.GUID, PluginMetadata.NAME, PluginMetadata.VERSION, PluginMetadata.AUTHORS, PluginMetadata.REPOSITORY_URL)]
[BepInDependency(BepInExResoniteShim.PluginMetadata.GUID, BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource? Log;

    private static ConfigEntry<bool> TOGGLE_INVENTORY_SAVE_AVATAR_BUTTON;
    private static ConfigEntry<bool> TOGGLE_INVENTORY_GET_ITEM_URL_BUTTON;
    private static ConfigEntry<bool> TOGGLE_WIKI_INTEGRATION;

    public override void Load()
    {
        Log = base.Log;
        HarmonyInstance.PatchAll();

        TOGGLE_INVENTORY_SAVE_AVATAR_BUTTON = Config.Bind("Inventory", "Inventory Save Avatar Button", true, new ConfigDescription("Toggle Save Avatar Button", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.Inventory.ToggleSaveAvatarButton", "Settings.dev.lecloutpanda.Toggles.Inventory.ToggleSaveAvatarButton.Description")));
        TOGGLE_INVENTORY_GET_ITEM_URL_BUTTON = Config.Bind("Inventory", "Inventory Get Item Url Button", true, new ConfigDescription("Toggle Get Item Link Button", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.Inventory.ToggleGetItemLinkButton", "Settings.dev.lecloutpanda.Toggles.Inventory.ToggleGetItemLinkButton.Description")));
        TOGGLE_WIKI_INTEGRATION = Config.Bind("Misc", "Toggle Wiki Integration", true, new ConfigDescription("Toggle Wiki Integration", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.Misc.ToggleWikiIntegration", "Settings.dev.lecloutpanda.Toggles.Misc.ToggleWikiIntegration.Description")));
    }

    private static MethodInfo wikiFunc = AccessTools.Method(typeof(Hyperlink), nameof(Hyperlink.AttachForWikiPage));

    /// Patch by (Arti)[https://github.com/art0007i]
    /// Patches out wiki integrations via a toggle 
    [HarmonyPatch(typeof(ProtoFluxTool), nameof(ProtoFluxTool.GenerateMenuItems))]
    public static class ProtofluxPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            var found = false;
            foreach (var code in codes)
            {
                if (code.Calls(wikiFunc))
                {
                    found = true;
                    yield return new(OpCodes.Call, ((Delegate)FakeWikiFunc).Method);
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

        public static Hyperlink? FakeWikiFunc(Slot slot, Type type)
        {
            if(TOGGLE_WIKI_INTEGRATION.Value) return Hyperlink.AttachForWikiPage(slot, type);
            slot.Destroy();
            return null;
        }
    }

    [HarmonyPatch(typeof(WorkerInspector), "BuildUIForComponent")]
    public static class WikiIntegrationComponentPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            var found = false;
            foreach (var code in codes)
            {
                if (code.Calls(wikiFunc))
                {
                    found = true;
                    yield return new(OpCodes.Call, ((Delegate)FakeWikiFunc).Method);
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

        public static Hyperlink? FakeWikiFunc(Slot slot, Type type)
        {
            if(TOGGLE_WIKI_INTEGRATION.Value) return Hyperlink.AttachForWikiPage(slot, type);
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
        public static void Postfix(InventoryBrowser __instance, SyncRef<Button> ____addCurrentAvatar, SyncRef<Button> ____copyLink) 
        {
            //
            try 
            {
                if (!TOGGLE_INVENTORY_SAVE_AVATAR_BUTTON.Value && !____addCurrentAvatar.Slot.IsRemoved)
                {
                    ____addCurrentAvatar.Target.Slot.Destroy();
                }

                if (!TOGGLE_INVENTORY_GET_ITEM_URL_BUTTON.Value && !____copyLink.Slot.IsRemoved) 
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
}