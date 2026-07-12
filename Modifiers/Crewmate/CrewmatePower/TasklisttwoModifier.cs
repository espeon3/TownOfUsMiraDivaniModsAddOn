using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using DivaniMods.Options;
using TownOfUs.Assets;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class TasklisttwoModifier : BaseModifier
{
    public override string ModifierName => "Task list two";
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.OverworkedIcon;
    public override bool HideOnUi => true;


var lt = OptionsGroupSingleton<OverworkedRole>.Instance.ExtraLongTasks.Value

var overworked = 
PlayerTask.GetOrCreateTask<ShortTask>(overworked, lt);