using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.NET.Common;
using BepInExResoniteShim;
using BepisLocaleLoader;

namespace Toggles;

[ResonitePlugin(PluginMetadata.GUID, PluginMetadata.NAME, PluginMetadata.VERSION, PluginMetadata.AUTHORS, PluginMetadata.REPOSITORY_URL)]
[BepInDependency(BepInExResoniteShim.PluginMetadata.GUID, BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log = null!;

    public static ConfigEntry<bool> INVENTORY_SAVE_AVATAR_BUTTON = null!;
    public static ConfigEntry<bool> INVENTORY_GET_URL_BUTTON = null!;
    public static ConfigEntry<bool> PROTOFLUX_OVERVIEW_BUTTON = null!;
    public static ConfigEntry<bool> DEVTOOL_GIZMOSNAPPING_BUTTON = null!;
    public static ConfigEntry<bool> DEVTOOL_GIZMOSNAPPING_VALUE = null!;
    public static ConfigEntry<bool> CONTACT_ASK_TO_JOIN_BUTTON = null!;
    public static ConfigEntry<bool> WIKI_INTEGRATION_INSPECTOR = null!;
    public static ConfigEntry<bool> WIKI_INTEGRATION_PROTOFLUX = null!;
    public static ConfigEntry<bool> SLOT_TAG_INHERITANCE = null!;
    public static ConfigEntry<bool> MAYBE_JUMP_LEFT = null!;
    public static ConfigEntry<bool> MAYBE_JUMP_RIGHT = null!;
    public static ConfigEntry<bool> CONTROLLER_HAPTIC_DASH = null!;
    public static ConfigEntry<bool> CONTROLLER_HAPTIC_WORLD = null!;

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
        
        CONTROLLER_HAPTIC_DASH = Config.Bind("Haptics", "Dash Haptics", true, new ConfigDescription("Toggle haptics when interacting with the Dash", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.Haptics.Dash", "Settings.dev.lecloutpanda.Toggles.Haptics.Dash.Description")));
        CONTROLLER_HAPTIC_WORLD = Config.Bind("Haptics", "World Haptics", true, new ConfigDescription("Toggle haptics when interacting with stuff in World", null, new ConfigLocale("Settings.dev.lecloutpanda.Toggles.Haptics.World", "Settings.dev.lecloutpanda.Toggles.Haptics.World.Description")));

        DevToolPatch.InitSubs();
    }
}