using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Crewmate;

namespace DivaniMods.Events.Modifiers;

public static class BountyEvents
{
    [RegisterEvent]
public static void TaskCompleteEventHandler(TaskCompleteEvent e)
    CountCompleted =>  GetNeeded(); {
    Var killer = AfterMurderEvent.Source;
    Var target = AfterMurderEvent.Target;

    OnDeath TryGetKiller
    killer GetKillCooldown
    KillCooldown => KillCooldown - OptionsGroupSingleton<BountyModifierOptions>.Instance.DecreasePerTask.Value * GetNeeded()
    //todo: Does this work? + Notif for Killer: "You`ve killed the Bounty, your Kill Cooldown is now OptionsGroupSingleton<BountyModifierOptions>.Instance.DecreasePerTask.Value * GetNeeded() seconds shorter than usual!"
}
}
