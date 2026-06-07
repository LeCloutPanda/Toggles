# v1.1.0

## Updated
- Reorganized code for improved source readability
- Updated `en.json` locale
- Split wiki integration options into two one for inspectors and one for the protoflux tool

## Added

### Overview Toggle Solves issue solves https://github.com/LeCloutPanda/Toggles/issues/1
- Added an option to toggle the **"Overview"** button in the Protoflux Tool
- When disabled, the "Overview" button is removed from the context menu

### Gizmo Snapping solves https://github.com/LeCloutPanda/Toggles/issues/2
- Added an option to **"Toggle Gizmo Snapping"** when translating gizmos via a generated context menu item under **"Gizmo Options"** in the DevTool
- Includes a toggle to enable or disable generation of the menu item itself
- Toggling **"Toggle Gizmo Snapping"** via the context menu updates the default configuration value
- Changing the default value in the settings page updates the tool's runtime variables

### Hide "Ask to Join" Button solves https://github.com/LeCloutPanda/Toggles/issues/4
- Added an option to disable the **"Ask to Join"** button in the Contacts page for a contact
- Note: There is a known issue where headless host accounts for sessions that are not joinable may still display the "Ask to Join" button