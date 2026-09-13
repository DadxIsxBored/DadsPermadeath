using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DadsPermadeath;

internal static class PermadeathController
{
    internal static bool Triggered { get; private set; }

    internal static void HandleDeath(Player player)
    {
        if (!DadsPermadeathPlugin.ModEnabled.Value ||
            !PermadeathWorldModifier.IsActiveInCurrentWorld() ||
            Triggered ||
            player != Player.m_localPlayer)
        {
            return;
        }

        Game? game = Game.instance;
        ZNet? znet = ZNet.instance;
        PlayerProfile? profile = game?.GetPlayerProfile();
        World? localWorld = ZNet.GetWorldIfIsHost();

        if (game == null || znet == null || profile == null)
        {
            DadsPermadeathPlugin.Log.LogError("Permadeath could not identify the active game, network session, or character.");
            return;
        }

        Triggered = true;
        string? characterFilename = profile.GetFilename();
        if (string.IsNullOrWhiteSpace(characterFilename))
        {
            Triggered = false;
            DadsPermadeathPlugin.Log.LogError("Permadeath could not identify the active character save filename.");
            return;
        }

        string? worldFilename = localWorld?.m_name;
        DadsPermadeathPlugin.Instance.StartCoroutine(
            DeleteSavesAndExit(game, znet, characterFilename!, worldFilename));
    }

    internal static void Reset()
    {
        Triggered = false;
    }

    private static IEnumerator DeleteSavesAndExit(
        Game game,
        ZNet znet,
        string characterFilename,
        string? worldFilename)
    {
        yield return null;

        while (znet != null && znet.IsSaving())
        {
            yield return null;
        }

        try
        {
            if (znet != null)
            {
                znet.ShutdownWithoutSave(false);
            }

            int characterFiles = DeleteSaveSet(characterFilename, SaveDataType.Character);
            int worldFiles = string.IsNullOrWhiteSpace(worldFilename)
                ? 0
                : DeleteSaveSet(worldFilename!, SaveDataType.World);

            SaveSystem.ClearWorldListCache(true);

            DadsPermadeathPlugin.Log.LogInfo(
                $"Permadeath removed {characterFiles} character save file(s) and {worldFiles} world save file(s).");

            if (worldFilename == null)
            {
                DadsPermadeathPlugin.Log.LogInfo(
                    "The active world is hosted remotely; only its host can remove that world save.");
            }
        }
        catch (Exception exception)
        {
            DadsPermadeathPlugin.Log.LogError($"Permadeath save removal stopped: {exception}");
        }

        yield return null;

        if (game != null)
        {
            game.Logout(false, true);
        }
    }

    private static int DeleteSaveSet(string saveName, SaveDataType dataType)
    {
        if (string.IsNullOrWhiteSpace(saveName) ||
            !SaveSystem.TryGetSaveByName(saveName, dataType, out SaveWithBackups saveSet) ||
            saveSet == null)
        {
            return 0;
        }

        int removed = 0;
        SaveFile[] files = saveSet.AllFiles.ToArray();
        foreach (SaveFile file in files)
        {
            if (SaveSystem.Delete(file))
            {
                removed++;
            }
            else
            {
                DadsPermadeathPlugin.Log.LogError(
                    $"Permadeath could not remove save file '{file.Name}' from source '{file.m_source}'.");
            }
        }

        return removed;
    }
}
