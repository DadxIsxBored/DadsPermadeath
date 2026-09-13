using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace DadsPermadeath;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class DadsPermadeathPlugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.dadisbored.dadspermadeath";
    public const string PluginName = "DadsPermadeath";
    public const string PluginVersion = "1.0.0";

    internal static DadsPermadeathPlugin Instance = null!;
    internal static ConfigEntry<bool> ModEnabled = null!;
    internal static ManualLogSource Log = null!;

    private Harmony? _harmony;

    private void Awake()
    {
        Instance = this;
        Log = Logger;
        ModEnabled = Config.Bind(
            "1 - General",
            "Mod Enabled",
            true,
            "Enable DadsPermadeath.");

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(DadsPermadeathPlugin).Assembly);
        Logger.LogInfo($"{PluginName} {PluginVersion} loaded for Valheim 1.0.12.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }
}
