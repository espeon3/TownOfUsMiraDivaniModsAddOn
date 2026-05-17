using HarmonyLib;
using MiraAPI.Hud;
using DivaniMods.Buttons.Neutral.NeutralOutlier;
using DivaniMods.Roles.Neutral.NeutralOutlier;
using DivaniMods.Utilities;

namespace DivaniMods.Patches;

/// <summary>
/// Shows the Terrorist Plant button only while standing at a utility console
/// (same pattern as <see cref="UsePortalButtonVisibilityPatch"/>).
/// </summary>
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
internal static class TerroristPlantButtonVisibilityPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix()
    {
        var plant = ResolvePlantButton();
        if (plant?.Button == null) return;

        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead
            || !IsTerrorist(player))
        {
            plant.Button.gameObject.SetActive(false);
            return;
        }

        if (MeetingHud.Instance != null || ExileController.Instance != null)
        {
            plant.Button.gameObject.SetActive(false);
            return;
        }

        if (TerroristSabotageState.IsActive || TerroristSabotageState.IsImpostorSabotageActive())
        {
            plant.Button.gameObject.SetActive(false);
            return;
        }

        var nearUtility = TerroristUtilityConsoles.TryGetClosest(player, out _, out _, forTerroristPlant: true);
        plant.Button.gameObject.SetActive(nearUtility);

        if (!nearUtility)
        {
            return;
        }

        if (plant.CanUse())
        {
            plant.Button.SetEnabled();
        }
        else
        {
            plant.Button.SetDisabled();
        }
    }

    private static TerroristPlantButton? ResolvePlantButton()
    {
        if (TerroristPlantButton.Instance != null)
        {
            return TerroristPlantButton.Instance;
        }

        foreach (var button in CustomButtonManager.Buttons)
        {
            if (button is TerroristPlantButton plant)
            {
                TerroristPlantButton.Instance = plant;
                return plant;
            }
        }

        return null;
    }

    private static bool IsTerrorist(PlayerControl player)
    {
        var role = player.Data?.Role;
        if (role == null) return false;
        if (role is TerroristRole) return true;
        return role.GetType().Name == nameof(TerroristRole);
    }
}
