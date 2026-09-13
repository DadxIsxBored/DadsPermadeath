using HarmonyLib;

namespace DadsPermadeath;

[HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
internal static class PlayerDeathPatch
{
    private static void Prefix(Player __instance)
    {
        PermadeathController.HandleDeath(__instance);
    }
}

[HarmonyPatch(typeof(Game), nameof(Game.RequestRespawn))]
internal static class RespawnPatch
{
    private static bool Prefix()
    {
        return !PermadeathController.Triggered;
    }
}

[HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.Save))]
internal static class PlayerProfileSavePatch
{
    private static bool Prefix()
    {
        return !PermadeathController.Triggered;
    }
}

[HarmonyPatch(typeof(Game), nameof(Game.SavePlayerProfile))]
internal static class GameSavePlayerProfilePatch
{
    private static bool Prefix()
    {
        return !PermadeathController.Triggered;
    }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.Save))]
internal static class ZNetSavePatch
{
    private static bool Prefix()
    {
        return !PermadeathController.Triggered;
    }
}

[HarmonyPatch(typeof(FejdStartup), "Awake")]
internal static class MainMenuPatch
{
    private static void Postfix()
    {
        PermadeathController.Reset();
    }
}

[HarmonyPatch(typeof(ServerOptionsGUI), nameof(ServerOptionsGUI.Awake))]
internal static class ServerOptionsAwakePatch
{
    private static void Postfix(ServerOptionsGUI __instance)
    {
        PermadeathWorldModifier.CreateButton(__instance);
    }
}

[HarmonyPatch(typeof(ServerOptionsGUI), nameof(ServerOptionsGUI.ReadKeys))]
internal static class ServerOptionsReadKeysPatch
{
    private static void Postfix(ServerOptionsGUI __instance, World world)
    {
        PermadeathWorldModifier.ReadWorld(__instance, world);
    }
}

[HarmonyPatch(typeof(ServerOptionsGUI), nameof(ServerOptionsGUI.SetKeys))]
internal static class ServerOptionsSetKeysPatch
{
    private static void Postfix(World world)
    {
        PermadeathWorldModifier.WriteWorld(world);
    }
}

[HarmonyPatch(typeof(ServerOptionsGUI), nameof(ServerOptionsGUI.GetWorldModifierSummary))]
internal static class WorldModifierSummaryPatch
{
    private static void Prefix(ref System.Collections.Generic.IEnumerable<string> __0, out bool __state)
    {
        __state = PermadeathWorldModifier.RemovePermadeathKeyForSummary(ref __0);
    }

    private static void Postfix(bool __1, string __2, bool __state, ref string __result)
    {
        PermadeathWorldModifier.AddPermadeathToSummary(__1, __2, __state, ref __result);
    }
}
