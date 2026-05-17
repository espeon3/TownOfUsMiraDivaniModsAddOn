using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Patches;
using DivaniMods.Roles.Neutral.NeutralOutlier;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Neutral.NeutralOutlier;

/// <summary>
/// Defuse button for non-terrorists while a Terrorist sabotage is active.
/// Hold fills the button ring (same pattern as <see cref="TerroristPlantButton"/>).
/// </summary>
public class TerroristDefuseButton : CustomActionButton
{
    public override string Name => "Defuse";
    public override float Cooldown => 1f;
    public override float EffectDuration => 0f;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.TerroristSabotageButton;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomLeft;
    public override Color TextOutlineColor => TerroristRole.TerroristColor;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;

    public static TerroristDefuseButton? Instance { get; set; }

    private bool _isDefusing;

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        return role is not TerroristRole;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        if (MeetingHud.Instance != null || ExileController.Instance != null) return false;
        if (!TerroristSabotageState.IsActive) return false;
        if (player.Data.Role is TerroristRole) return false;
        if (_isDefusing) return false;
        if (!TerroristSabotageState.IsLocalPlayerAtPlantedConsole()) return false;

        return base.CanUse();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;
        if (!TerroristSabotageState.IsActive) return;
        if (player.Data.Role is TerroristRole) return;
        if (!TerroristSabotageState.IsLocalPlayerAtPlantedConsole()) return;
        if (_isDefusing) return;

        Coroutines.Start(DefuseCoroutine(player));
    }

    private IEnumerator DefuseCoroutine(PlayerControl player)
    {
        _isDefusing = true;
        var defuseTime = OptionGroupSingleton<TerroristOptions>.Instance.DefuseTime;
        var colorHex = ColorUtility.ToHtmlStringRGB(TerroristRole.TerroristColor);

        EffectActive = true;
        Timer = defuseTime;

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{colorHex}>Defusing...</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.TerroristSabotageButton.LoadAsset());

        var elapsed = 0f;
        while (elapsed < defuseTime)
        {
            if (player == null || player.Data == null || player.Data.IsDead)
            {
                AbortDefuse();
                yield break;
            }

            if (!TerroristSabotageState.IsActive)
            {
                AbortDefuse();
                yield break;
            }

            if (!TerroristSabotageState.IsLocalPlayerAtPlantedConsole())
            {
                MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                    $"<b><color=#{colorHex}>Defuse aborted — too far from sabotage!</color></b>",
                    Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: DivaniAssets.TerroristSabotageButton.LoadAsset());
                AbortDefuse();
                yield break;
            }

            elapsed += Time.deltaTime;
            Timer = defuseTime - elapsed;
            if (Button != null)
            {
                Button.SetFillUp(Timer, defuseTime);
            }

            yield return null;
        }

        if (player == null || player.Data == null || player.Data.IsDead)
        {
            AbortDefuse();
            yield break;
        }

        if (!TerroristSabotageState.IsActive)
        {
            AbortDefuse();
            yield break;
        }

        if (!TerroristSabotageState.IsLocalPlayerAtPlantedConsole())
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{colorHex}>Defuse aborted — too far from sabotage!</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.TerroristSabotageButton.LoadAsset());
            AbortDefuse();
            yield break;
        }

        TerroristSabotageState.RpcDefuseSabotage(player);
        EffectActive = false;
        Timer = Cooldown;
        _isDefusing = false;
    }

    private void AbortDefuse()
    {
        EffectActive = false;
        Timer = Cooldown;
        _isDefusing = false;
    }
}
