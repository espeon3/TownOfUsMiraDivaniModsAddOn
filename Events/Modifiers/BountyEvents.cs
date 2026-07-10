using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Crewmate;

namespace DivaniMods.Events.Modifiers;

public static class BountyEvents
{
    [RegisterEvent]
public static void TaskCompleteEventHandler(TaskCompleteEvent e) {
    var p = e.Player;
    if (p.Data.IsDead()) return;

    //todo: decrease Killer CD on Kill + Count tasks done
}
}
