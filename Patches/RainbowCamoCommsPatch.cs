using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.LocalSettings;
using DivaniMods.Modules;
using DivaniMods.Options;
using TownOfUs.Modules;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class RainbowCamoCommsPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix()
    {
        if (!HudManagerPatches.CamouflageCommsEnabled)
        {
            return;
        }

        if (!OptionGroupSingleton<DivaniOptions>.Instance.RainbowCamoComms)
        {
            return;
        }

        // Local opt-out: when set, camo bodies keep their default grey instead of rainbow.
        if (LocalSettingsTabSingleton<DivaniLocalSettings>.Instance.DisableRainbowComms.Value)
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.cosmetics == null)
            {
                continue;
            }

            if (player.GetAppearanceType() != TownOfUsAppearances.Camouflage)
            {
                continue;
            }

            var body = player.cosmetics.currentBodySprite?.BodySprite;
            if (body != null)
            {
                RainbowUtils.SetRainbow(body);
            }
        }

        foreach (var fakePlayer in FakePlayer.FakePlayers)
        {
            if (fakePlayer?.body == null)
            {
                continue;
            }

            var cosmetics = fakePlayer.body.GetComponentInChildren<CosmeticsLayer>();
            var body = cosmetics?.currentBodySprite?.BodySprite;
            if (body != null)
            {
                RainbowUtils.SetRainbow(body);
            }
        }
    }
}
