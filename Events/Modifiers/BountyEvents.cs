using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Crewmate;

namespace DivaniMods.Events.Modifiers;

public static class BountyEvents
{
    [RegisterEvent]
    public static void AfterMurderEvent (AfterMurderEvent @event) {
  var kcdr = OptionsGroupSingleton<BountyOptions>.Instance.DecreasePerTask.Value * CountCompleted();   
  var target = @event.Target;
  var source = @event.Source;

  if (!Target.HasModifier<BountyModifier>() || target == source || MeetingHud.Instance || !source.AmOwner)
  {
    return;
  }
  
  source.SetKillTimer(source.GetKillCooldown() - kcdr);
}
}
