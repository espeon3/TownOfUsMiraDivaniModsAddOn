using System.Reflection;
using HarmonyLib;
using MiraAPI.GameOptions;
using DivaniMods.Buttons.Neutral.NeutralOutlier;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralOutlier;
using DivaniMods.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

/// <summary>
/// Glue patches for the Terrorist:
/// 1. Block vanilla <see cref="SabotageButton.DoClick"/> while our sabotage is
///    live so impostors can't stack their sabo on top.
/// 2. While our sabotage is live, force the impostor sabotage timer back up
///    so it doesn't tick down during ours (mirrors SaboteurPatches).
/// 3. While an impostor sabotage is live, force the Terrorist plant button
///    cooldown back up so the Terrorist can't start a plant during it.
/// 4. Reset Terrorist state on game start / end / intro.
/// </summary>
[HarmonyPatch]
public static class TerroristPatch
{
    private static bool _wasImpSaboActive;

    /// <summary>
    /// Vanilla emergency UI during sabotage: menu opens but the button is closed with
    /// <see cref="StringNames.EmergencyDuringCrisis"/> (same pattern as TOU role blocks).
    /// </summary>
    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Begin))]
    [HarmonyPostfix]
    public static void EmergencyMinigameBeginPostfix(EmergencyMinigame __instance)
    {
        if (!TerroristSabotageState.IsActive)
        {
            return;
        }

        ApplySabotageEmergencyDisabledUi(__instance);
    }

    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    [HarmonyPostfix]
    public static void EmergencyMinigameUpdatePostfix(EmergencyMinigame __instance)
    {
        if (!TerroristSabotageState.IsActive)
        {
            return;
        }

        ApplySabotageEmergencyDisabledUi(__instance);
    }

    /// <summary>
    /// Keep the emergency Use button visible in range but not usable (vanilla sabo UX).
    /// </summary>
    [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.CanUse))]
    [HarmonyPostfix]
    public static void EmergencyConsoleCanUsePostfix(
        SystemConsole __instance,
        NetworkedPlayerInfo pc,
        ref bool canUse,
        ref bool couldUse)
    {
        if (!TerroristSabotageState.IsActive || !IsEmergencyConsole(__instance) || pc?.Object == null)
        {
            return;
        }

        if (!IsWithinEmergencyUseDistance(__instance, pc.Object))
        {
            return;
        }

        couldUse = true;
        canUse = false;
    }

    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool SabotageButtonDoClickPrefix()
    {
        // While the Terrorist sabotage is up the impostor sabotage button is
        // disabled across the board. Mirrors the spec: "they cant sabotage
        // when there is a Terrorist sabotage".
        return !TerroristSabotageState.IsActive;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePostfix(HudManager __instance)
    {
        if (__instance == null) return;

        var impSaboActive = TerroristSabotageState.IsImpostorSabotageActive();

        // Edge: impostor sabotage just started → wipe the Terrorist plant
        // cooldown back to full so they cannot piggyback. Spec also says the
        // reverse — see TerroristSabotageState.ForceImpSabotageCooldown for
        // the imp-side clamp.
        if (impSaboActive && !_wasImpSaboActive)
        {
            var plant = TerroristPlantButton.Instance;
            if (plant != null)
            {
                plant.Timer = plant.Cooldown;
                plant.Button?.SetDisabled();
            }
        }
        _wasImpSaboActive = impSaboActive;

        // Continuous clamp while imp sabo is up so the timer doesn't tick down.
        if (impSaboActive)
        {
            var plant = TerroristPlantButton.Instance;
            if (plant != null && plant.Timer < plant.Cooldown)
            {
                plant.Timer = plant.Cooldown;
                plant.Button?.SetDisabled();
            }
        }
    }

    /// <summary>
    /// Mirror SaboteurPatches but in our direction: while the Terrorist
    /// sabotage is active, keep the impostor SabotageSystemType.Timer pinned
    /// at the cooldown so it can't drain to zero behind the scenes.
    /// </summary>
    [HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
    [HarmonyPrefix]
    public static bool SabotageSystemUpdatePrefix(
        SabotageSystemType __instance,
        PlayerControl player,
        [HarmonyArgument(1)] object reader)
    {
        if (!TerroristSabotageState.IsActive)
        {
            return true;
        }

        // Only block new sabotage triggers (1-byte system id). Allow repair/clear
        // packets so imp sabotage state does not get stuck and lock the plant button.
        if (GetReaderBytesRemaining(reader) != 1)
        {
            return true;
        }

        return false;
    }

    private static int GetReaderBytesRemaining(object? reader)
    {
        if (reader == null)
        {
            return -1;
        }

        var prop = reader.GetType().GetProperty("BytesRemaining", BindingFlags.Instance | BindingFlags.Public);
        if (prop == null)
        {
            return -1;
        }

        return (int)prop.GetValue(reader)!;
    }

    private static void ApplySabotageEmergencyDisabledUi(EmergencyMinigame minigame)
    {
        minigame.StatusText.text = GetEmergencyDuringSabotageText();
        minigame.NumberText.text = string.Empty;
        minigame.ClosedLid.gameObject.SetActive(true);
        minigame.OpenLid.gameObject.SetActive(false);
        minigame.ButtonActive = false;
    }

    private static string GetEmergencyDuringSabotageText()
    {
        try
        {
            return TranslationController.Instance.GetString(StringNames.EmergencyDuringCrisis);
        }
        catch
        {
            return "You cannot call an emergency meeting during a sabotage.";
        }
    }

    private static bool IsEmergencyConsole(SystemConsole console)
    {
        return console?.MinigamePrefab != null
            && console.MinigamePrefab.TryCast<EmergencyMinigame>() != null;
    }

    private static bool IsWithinEmergencyUseDistance(SystemConsole console, PlayerControl player)
    {
        var dist = Vector2.Distance(player.GetTruePosition(), (Vector2)console.transform.position);
        return dist <= TerroristUtilityConsoles.GetUsableDistance(console);
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPostfix]
    public static void OnGameEndPostfix()
    {
        TerroristSabotageState.ResetAll();
        TerroristUtilityConsoles.InvalidateCache();
        _wasImpSaboActive = false;
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
        TerroristSabotageState.ResetAll();
        TerroristUtilityConsoles.InvalidateCache();
        _wasImpSaboActive = false;
    }
}
