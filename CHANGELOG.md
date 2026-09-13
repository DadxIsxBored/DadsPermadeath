# Changelog

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
