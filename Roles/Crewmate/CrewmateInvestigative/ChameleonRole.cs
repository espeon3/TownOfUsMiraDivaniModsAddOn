using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class ChameleonRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Hunter;
    public string LocaleKey => "Chameleon";
    public string RoleName => "Chameleon";
    public string RoleDescription => "Camouflage to sneakily get around and gather info!";
    public string RoleLongDescription => "Use your Camouflage ability to turn invisible and gather information.";

    public string  +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Crewmates;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = (ChameleonVent)OptionGroupSingleton<ChameleonOptions>.Instance.CanVent.Value,
        Icon = TouRoleIcons.Swooper,
        IntroSound = TouAudio.PhantomIntroSound
    };



    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Swoop", "Swoop"),
                    TouLocale.GetParsed("TouRole{LocaleKey}Swoop"),
                    TouImpAssets.SwoopSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Unswoop", "Unswoop"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}UnswoopWikiDescription"),
                    TouImpAssets.UnswoopSprite)
            };
        }
    }
}