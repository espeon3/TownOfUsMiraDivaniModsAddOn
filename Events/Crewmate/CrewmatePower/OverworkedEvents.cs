using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using DivaniMods.Modifiers.Crewmate.CrewmatePower;
using DivaniMods.Modules;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using TownOfUs.Modifiers;

namespace DivaniMods.Events.Crewmate.CrewmatePower;

public static class OverworkedEvents
{
    [RegisterEvent]