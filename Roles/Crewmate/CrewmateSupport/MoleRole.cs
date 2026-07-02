using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Anims;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmateSupport;

public sealed class MoleRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color MoleColor = new Color32(150, 255, 171, 255);

    [HideFromIl2Cpp] public List<Vent> Vents { get; set; } = [];

    public string RoleName => "Mole";
    public string RoleDescription => "Dig your own tunnel network!";
    public string RoleLongDescription => "Dig vents around the map to connect a tunnel network.";
    public Color RoleColor => MoleColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Dig", "Dig a vent at your current position. All Mole vents connect to each other.", DivaniAssets.MoleDigButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.MoleIcon,
        MaxRoleCount = 1,
        IntroSound = TouAudio.MineSound
    };

    public static bool MoleVentsExist =>
        ShipStatus.Instance != null && ShipStatus.Instance.AllVents.Any(v =>
            v.name.StartsWith("MoleVent") && v.gameObject.activeSelf && v.myRend.enabled);

    public static bool VentsDisabledByPlayerCount()
    {
        var aliveCount = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied());
        var minimum = (int)OptionGroupSingleton<GameMechanicOptions>.Instance.PlayerCountWhenVentsDisable.Value;
        return aliveCount <= minimum;
    }

    public static bool CanUseMoleVents(PlayerControl player)
    {
        if (player.Data?.Role is MoleRole)
        {
            return true;
        }

        return OptionGroupSingleton<MoleOptions>.Instance.VentUsage switch
        {
            MoleVentUsage.Anyone => true,
            MoleVentUsage.Crewmates => player.IsCrewmate(),
            _ => false,
        };
    }

    [MethodRpc((uint)DivaniRpcCalls.MolePlaceVent)]
    public static void RpcPlaceVent(PlayerControl player, int ventId, Vector2 position, float zAxis, bool immediate)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }

        if (player.Data.Role is not MoleRole mole)
        {
            return;
        }

        var ventPrefab = ShipStatus.Instance.AllVents[0];
        var vent = Instantiate(ventPrefab, ventPrefab.transform.parent);
        vent.EnterVentAnim = null!;
        vent.ExitVentAnim = null!;
        if (vent.myAnim)
        {
            vent.transform.localScale = new Vector3(0.9f, 0.9f, 1);
            var collider = vent.transform.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(0.75f, 0.34f);
            collider.offset = new Vector2(-0.005f, 0);
            vent.Offset = new Vector3(0, 0.15f, 0);
            vent.myAnim.Stop();
            vent.myAnim.Destroy();
            vent.myAnim = null!;
        }

        vent.numFramesUntilPlayerDisappearsOnEnter = 0;
        vent.numFramesUntilPlayerReappearsOnExit = 0;
        vent.myRend.sprite = TouAssets.MinerVentSprite.LoadAsset();
        vent.name = $"MoleVent-{player.PlayerId}-{ventId}";

        if (!player.AmOwner && !immediate)
        {
            vent.gameObject.SetActive(false);
        }

        vent.Id = ventId;
        vent.transform.position = new Vector3(position.x, position.y, zAxis + 0.001f);

        if (mole.Vents.Count > 0)
        {
            var leftVent = mole.Vents[^1];
            vent.Left = leftVent;
            leftVent.Right = vent;
        }
        else
        {
            vent.Left = null;
        }

        vent.Right = null;
        vent.Center = null;

        var allVents = ShipStatus.Instance.AllVents.ToList();
        allVents.Add(vent);
        ShipStatus.Instance.AllVents = allVents.ToArray();

        mole.Vents.Add(vent);

        if (player.AmOwner || immediate)
        {
            Coroutines.Start(mole.CoExplode(new Vector3(position.x, position.y + 1.33f, zAxis - 0.0001f)));
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.MoleShowVent)]
    public static void RpcShowVent(PlayerControl player, int ventId)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }

        if (player.Data.Role is not MoleRole mole)
        {
            return;
        }

        var vent = mole.Vents.FirstOrDefault(x => x.Id == ventId);

        if (vent != null)
        {
            vent.gameObject.SetActive(true);
        }
    }

    [HideFromIl2Cpp]
    public IEnumerator CoExplode(Vector3 position)
    {
        var explodeAnim = AnimStore.SpawnAnimAtPlayer(Player, TouAssets.VentExplodePrefab.LoadAsset());
        explodeAnim.transform.position = position;
        yield return new WaitForSeconds(1.166f);
        explodeAnim.Destroy();
    }
}
