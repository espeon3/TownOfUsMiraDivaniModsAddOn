using IlCppInterop.Runtime.Attributes;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Crewmate;

public sealed class BountyModifier : TOUGameModifier, IColoredModifier, IWikiDiscoverable
{
    public static readonly Color BountyColor = new Color32(66, 33, 99, 255);

    public override string ModifierName => "Bounty";
    public override string LocaleKey => "Bounty";
    public override string IntroInfo => "The more tasks you do, the higher your bounty.";
    public override ModifierFaction FactionType => ModifierFaction.Crewmate.Postmortem;
    public override Color ModifierColor => BountyColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.BountyIcon;

    public override string GetDescription() => "Every time you do a task, your bounty raises, once you die your KIller has their Kill Cooldown decreased by a set percent for every task done.";

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() => (Int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.Bountychance.Value;

    public override GetAmountPerGame() => (Int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.Bountychance.Value;

    public override bool IsModifierValidOn(RoleBehaviour Role)
   {
        return role.IsCrewmate() && base.IsModifierValidOn(role) && !ModifierExclusions.ConflictsWithOwned(role.player, this),      
   } 
   public override void OnActivate();
   {
   }
}