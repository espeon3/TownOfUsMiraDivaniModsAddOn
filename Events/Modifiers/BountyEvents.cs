using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Crewmate;

namespace DivaniMods.Events.Modifiers;

public static class BountyEvents
{
    public static void TaskCompleteEventHandler(TaskCompleteEvent e); {
  var p = e.Player
  if (p.Data.IsDead()) return;
  SetKillTimer e.source => GetKillCooldown() - OptionsGroupSingleton<BountyModifierOptions>.Instance.Instance.DecreasePerTask.Value * ...
}
