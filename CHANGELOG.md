# Changelog

## 1.1.8

- Enforced the Permadeath and Reset rows after Unity's automatic UI layout pass.
- Kept DadsPermadeath centered in the new third row and Reset centered in the fourth row every rendered frame.

## 1.1.7

- Put DadsPermadeath in a dedicated centered row beneath the six built-in presets.
- Removed both DadsPermadeath and Reset to normal from the parent layout so they cannot be placed beside or over one another.
- Moved Reset to normal down by one complete preset-row interval beneath DadsPermadeath.
- Moved Customize and the bottom controls down with the new rows.

## 1.1.6

- Fixed the post-death black screen by shutting down ZNetScene before ZNet.
- Replaced the ineffective public no-save logout call with Valheim's complete main-menu transition.
- Kept save removal and scene transition in the same frame after shutdown.

## 1.1.5

- Excluded DadsPermadeath from Valheim's three-column preset grid positioning.
- Centered DadsPermadeath at Reset to normal's original position.
- Calculated preset spacing from matching columns instead of sibling order.
- Restricted Reset, Customize, and bottom-control movement to the vertical axis.

## 1.1.4

- Added a dedicated centered DadsPermadeath preset row.
- Moved Reset to normal down by one complete preset-row interval.
- Moved Customize and the bottom controls down by the same interval.
- Increased the World Modifiers panel height to contain the added row.
- Changed every non-default Permadeath modifier combination to display as Permadeath+.

## 1.1.3

- Replaced manual third-row positioning with a Unity horizontal layout group.
- Standardized DadsPermadeath and Reset to normal to the same preset-button dimensions.
- Aligned both buttons through the World Modifiers UI layout hierarchy.

## 1.1.2

- Fixed the Harmony world-summary patch failing during plugin startup.
- Changed summary patch arguments to Harmony positional names for Valheim parameter-name compatibility.

## 1.1.1

- Replaced the Reset-button clone with a standard preset-button clone.
- Integrated DadsPermadeath and Reset to normal as a centered two-button third row.
- Retained other preset and custom modifier settings when Permadeath is confirmed.
- Added Permadeath, Permadeath+, and Custom modifiers world-list summaries.

## 1.1.0

- Added a DadsPermadeath preset button to the World Modifiers screen.
- Added the requested side-panel tip text.
- Added the requested Yes or No confirmation popup.
- Added Normal preset restoration when No is selected.
- Stored Permadeath as a world-specific global key when Done is pressed.
- Restricted permanent-death processing to worlds with the modifier.

## 1.0.0

- Added permanent death handling for the local player.
- Added deletion of all active character save variants and backups.
- Added deletion of all locally hosted world save variants and backups.
- Added save suppression and no-save session shutdown after death.
- Added respawn suppression once permanent death begins.
- Added automatic return to the main menu.
