using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Crewmate;

namespace DivaniMods.Events.Modifiers;

public static class BountyEvents
{
    public static void AfterMurderEvent (AfterMurderEvent e) {
  var kcdr = OptionsGroupSingleton<BountyOptions>.Instance.DecreasePerTask.Value * CountCompleted();   
  var bmod = e.Target;
  var hitman = e.Source; 
  hitman.SetKillTimer(source.GetKillCooldown() - kcdr);
} 
}
