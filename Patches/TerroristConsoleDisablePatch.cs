using HarmonyLib;
using MiraAPI.GameOptions;
using DivaniMods.Options;
using DivaniMods.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

/// <summary>
/// After a Terrorist sabotage explodes, the utility console cannot be used for the rest of the game.
/// The Terrorist can still plant at it via the Plant button.
/// </summary>
[HarmonyPatch]
internal static class TerroristConsoleDisablePatch
{
    [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.CanUse))]
    [HarmonyPrefix]
    public static bool MapConsoleCanUsePrefix(
        MapConsole __instance,
        NetworkedPlayerInfo pc,
        ref bool canUse,
        ref bool couldUse,
        ref float __result)
    {
        if (__instance == null || pc == null)
        {
            return true;
        }

        var pos = (Vector2)__instance.transform.position;
        var key = TerroristUtilityConsoles.GetStableId(TerroristUtilityKind.Admin, pos);
        return ApplyDisable(pc, key, TerroristUtilityKind.Admin, ref canUse, ref couldUse, ref __result);
    }

    [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.CanUse))]
    [HarmonyPrefix]
    public static bool SystemConsoleCanUsePrefix(
        SystemConsole __instance,
        NetworkedPlayerInfo pc,
        ref bool canUse,
        ref bool couldUse,
        ref float __result)
    {
        if (__instance == null || pc == null)
        {
            return true;
        }

        if (!TerroristUtilityConsoles.TryClassifySystemConsole(__instance, out var kind))
        {
            return true;
        }

        var pos = (Vector2)__instance.transform.position;
        var key = TerroristUtilityConsoles.GetStableId(kind, pos);
        return ApplyDisable(pc, key, kind, ref canUse, ref couldUse, ref __result);
    }

    private static bool ApplyDisable(
        NetworkedPlayerInfo pc,
        int consoleKey,
        TerroristUtilityKind kind,
        ref bool canUse,
        ref bool couldUse,
        ref float __result)
    {
        if (!OptionGroupSingleton<TerroristOptions>.Instance.DisableExplodedConsoles)
        {
            return true;
        }

        if (!TerroristSabotageState.IsUtilityDisabled(consoleKey, kind))
        {
            return true;
        }

        canUse = false;
        couldUse = false;
        __result = float.MaxValue;
        return false;
    }
}
