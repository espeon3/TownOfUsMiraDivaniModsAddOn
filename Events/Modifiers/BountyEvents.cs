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
    
    OnDeath TryGetKiller
    Killer GetKillCooldown
    KillCooldown => KillCooldown - OptionsGroupSingleton<BountyModifierOptions>.Instance.DecreasePerTask.Value * GetNeeded()
    //todo: Does this work?
}
}
