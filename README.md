# DadsPermadeath

DadsPermadeath adds a confirmed Permadeath preset to Valheim's World Modifiers screen. When the local player dies in a world saved with this modifier, the mod removes every primary and backup save file associated with the active character, removes every primary and backup save file associated with the locally hosted world, and returns to Valheim's main menu without saving during shutdown.

## Behavior

- Adds a `DadsPermadeath` button after Valheim's standard world modifier presets.
- Requires a Yes or No confirmation before selection.
- Saves Permadeath as a world-specific modifier only when `Done` is pressed.
- Keeps the selected standard preset and custom settings when Permadeath is confirmed.
- Shows `Permadeath` for default settings and `Permadeath+` for any additional modifier; worlds without Permadeath retain Valheim's standard summaries.
- Selecting No clears Permadeath and restores the Normal preset.
- Activates only for the local player's death in a world with the modifier.
- Deletes local and cloud variants exposed through Valheim's save system.
- Deletes primary files and automatic backups for the active character and world.
- Waits for an in-progress world save to finish before deletion.
- Blocks character and world saves after death is detected.
- Shuts down the session without saving before file removal.
- Returns directly to the main menu instead of respawning.
- In a remotely hosted multiplayer world, deletes the local character and exits; only the host can remove the remote world save.

## Configuration

- `Mod Enabled`: enables or disables DadsPermadeath.

Configuration path: `BepInEx/config/com.dadisbored.dadspermadeath.cfg`.

## Installation

Install BepInExPack for Valheim, then place `DadsPermadeath.dll` in `BepInEx/plugins/DadsPermadeath/`.

## Compatibility

- Built against Valheim `1.0.12`.
- Requires BepInEx only.
- Install on the client running the character.
