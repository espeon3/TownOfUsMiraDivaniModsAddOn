using HarmonyLib;
using DivaniMods.Utilities;
using TownOfUs.Buttons.Crewmate;

namespace DivaniMods.Patches;

public static class ClericCleanseExtraEffectsPatch
{
    [HarmonyPatch(typeof(ClericCleanseButton), "OnClick")]
    private static class OnClickPatch
    {
        private static void Postfix(ClericCleanseButton __instance)
        {
            DivaniNegativeEffects.CleanseAll(__instance.Target);
        }
    }
}
