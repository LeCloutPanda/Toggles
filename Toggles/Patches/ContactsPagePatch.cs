using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using SkyFrost.Base;
using Toggles;

// Fixes issue #4 https://github.com/LeCloutPanda/Toggles/issues/4
[HarmonyPatch(typeof(ContactItem), nameof(ContactItem.Update), new[] { typeof(Contact), typeof(ContactData) })]
public static class ContactsPagePatch
{
    public static void Postfix(Contact contact, ContactData data, SyncRef<Button> ____joinButton)
    {
        if (Plugin.CONTACT_ASK_TO_JOIN_BUTTON.Value) return;
        if (!contact.IsAccepted || contact.ContactStatus != ContactStatus.Accepted) return;
        if (data == null) return;
        if (data.CurrentSessionInfo != null) return;
        if (data.CurrentStatus.OnlineStatus.GetValueOrDefault() == OnlineStatus.Offline) return;
        ____joinButton?.Target?.Slot.ActiveSelf = false;
    }
}