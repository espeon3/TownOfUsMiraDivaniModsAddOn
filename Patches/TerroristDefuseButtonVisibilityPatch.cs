using HarmonyLib;
using MiraAPI.Hud;
using DivaniMods.Buttons.Neutral.NeutralOutlier;
using DivaniMods.Roles.Neutral.NeutralOutlier;
using DivaniMods.Utilities;

namespace DivaniMods.Patches;

/// <summary>
/// Shows the Defuse button only while at the planted utility console
/// (same proximity rules as <see cref="TerroristPlantButtonVisibilityPatch"/>).
/// </summary>
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
internal static class TerroristDefuseButtonVisibilityPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix()
    {
        var defuse = ResolveDefuseButton();
        if (defuse?.Button == null) return;

        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead
            || IsTerrorist(player))
        {
            defuse.Button.gameObject.SetActive(false);
            return;
        }

        if (MeetingHud.Instance != null || ExileController.Instance != null)
        {
            defuse.Button.gameObject.SetActive(false);
            return;
        }

        if (!TerroristSabotageState.IsActive)
        {
            defuse.Button.gameObject.SetActive(false);
            return;
        }

        var nearPlanted = TerroristSabotageState.IsLocalPlayerAtPlantedConsole();
        defuse.Button.gameObject.SetActive(nearPlanted);

        if (!nearPlanted)
        {
            return;
        }

        if (defuse.CanUse())
        {
            defuse.Button.SetEnabled();
        }
        else
        {
            defuse.Button.SetDisabled();
        }
    }

    private static TerroristDefuseButton? ResolveDefuseButton()
    {
        if (TerroristDefuseButton.Instance != null)
        {
            return TerroristDefuseButton.Instance;
        }

        foreach (var button in CustomButtonManager.Buttons)
        {
            if (button is TerroristDefuseButton defuse)
            {
                TerroristDefuseButton.Instance = defuse;
                return defuse;
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
