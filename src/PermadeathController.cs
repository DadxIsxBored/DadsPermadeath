using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DadsPermadeath;

internal static class PermadeathController
{
    private static readonly MethodInfo? ShutdownMethod =
        AccessTools.Method(typeof(Game), "Shutdown", new[] { typeof(bool) });

    private static readonly MethodInfo? ContinueLogoutMethod =
        AccessTools.Method(
            typeof(Game),
            "ContinueLogout",
            new[] { typeof(bool), typeof(bool), typeof(bool) });

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

        if (ShutdownMethod == null || ContinueLogoutMethod == null)
        {
            DadsPermadeathPlugin.Log.LogError(
                "Permadeath could not locate Valheim's no-save shutdown methods; saves were not removed.");
            Triggered = false;
            yield break;
        }

        try
        {
            // Game.Shutdown closes ZNetScene before ZNet. Calling ZNet directly leaves
            // ZNetScene updating against a stopped network session and blocks scene loading.
            ShutdownMethod.Invoke(game, new object[] { false });

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

            // shouldExit=true allows Valheim's private no-save continuation to run;
            // changeToStartScene=true loads the main menu after the completed shutdown.
            ContinueLogoutMethod.Invoke(game, new object[] { false, true, true });
        }
        catch (Exception exception)
        {
            DadsPermadeathPlugin.Log.LogError($"Permadeath save removal stopped: {exception}");
            Triggered = false;
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
