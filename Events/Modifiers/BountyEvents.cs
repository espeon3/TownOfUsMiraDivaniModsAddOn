using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Crewmate;

namespace DivaniMods.Events.Modifiers;

public static class BountyEvents
{
public static string TaskInfo(this PlayerControl player)
    {
        var completed = player.myTasks.ToArray().Count(x => x.IsComplete); }

    [RegisterEvent]
    public static void AfterMurderEvent (AfterMurderEvent @event) {
  var kcdr = OptionsGroupSingleton<BountyOptions>.Instance.DecreasePerTask.Value * completed;   
  var target = @event.Target;
  var source = @event.Source;

  if (!Target.HasModifier<BountyModifier>() || target == source || MeetingHud.Instance || !source.AmOwner)
  {
    return;
  }
  
  source.SetKillTimer(source.GetKillCooldown() - kcdr);
}
}
