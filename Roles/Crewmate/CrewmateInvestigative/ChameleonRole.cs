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

        public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());
    

    public Color RoleColor => TownOfUsColors.Crewmates;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Canouflage", "Turn invisible for a short period of time.", DivaniAssets.ChameleonCamouflageButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = (ChameleonVent)OptionGroupSingleton<ChameleonOptions>.Instance.CanVent.Value,
        Icon = TouRoleIcons.Swooper,
        IntroSound = TouAudio.PhantomIntroSound
            };
        }
    }
}