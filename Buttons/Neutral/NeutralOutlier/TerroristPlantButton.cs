using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Patches;
using DivaniMods.Roles.Neutral.NeutralOutlier;
using DivaniMods.Utilities;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Neutral.NeutralOutlier;

/// <summary>
/// Plant button (<see cref="CustomActionButton"/>). Shown while near any utility
/// console (admin / cameras / vitals / door log). Hold duration fills the button
/// ring like Sentinel beacon placement.
/// </summary>
public class TerroristPlantButton : CustomActionButton
{
    public override string Name => "Plant";
    public override float Cooldown => OptionGroupSingleton<TerroristOptions>.Instance.PlantCooldown;
    public override float EffectDuration => OptionGroupSingleton<TerroristOptions>.Instance.PlantTime;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.TerroristSabotageButton;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => TerroristRole.TerroristColor;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;

    public static TerroristPlantButton? Instance { get; set; }

    private Vector2 _capturedPosition;
    private int _capturedConsoleKey;
    private TerroristUtilityKind _capturedKind;
    private bool _isPlanting;

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        return role is TerroristRole;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        if (MeetingHud.Instance != null || ExileController.Instance != null) return false;

        if (TerroristSabotageState.IsImpostorSabotageActive()) return false;
        if (TerroristSabotageState.IsActive) return false;
        if (_isPlanting || EffectActive) return false;
        if (!TerroristUtilityConsoles.TryGetClosest(player, out _, out _, forTerroristPlant: true)) return false;

        return base.CanUse();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;

        if (!TerroristUtilityConsoles.TryGetClosest(player, out var consolePosition, out var kind, forTerroristPlant: true)
            || kind == TerroristUtilityKind.None)
        {
            return;
        }

        if (TerroristSabotageState.IsActive || TerroristSabotageState.IsImpostorSabotageActive()) return;

        _capturedPosition = consolePosition;
        _capturedKind = kind;
        _capturedConsoleKey = TerroristUtilityConsoles.GetStableId(kind, consolePosition);
        Coroutines.Start(PlantCoroutine(player));
    }

    private IEnumerator PlantCoroutine(PlayerControl player)
    {
        _isPlanting = true;
        var plantTime = EffectDuration;
        var colorHex = ColorUtility.ToHtmlStringRGB(TerroristRole.TerroristColor);

        EffectActive = true;
        Timer = plantTime;

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{colorHex}>Planting sabotage...</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.TerroristSabotageButton.LoadAsset());

        var elapsed = 0f;
        while (elapsed < plantTime)
        {
            if (player == null || player.Data == null || player.Data.IsDead)
            {
                AbortPlant();
                yield break;
            }

            if (!TerroristUtilityConsoles.TryGetClosest(player, out var currentPos, out var currentKind, forTerroristPlant: true)
                || currentKind != _capturedKind
                || TerroristUtilityConsoles.GetStableId(currentKind, currentPos) != _capturedConsoleKey)
            {
                MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                    $"<b><color=#{colorHex}>Plant aborted — too far from console!</color></b>",
                    Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: DivaniAssets.TerroristSabotageButton.LoadAsset());
                AbortPlant();
                yield break;
            }

            if (TerroristSabotageState.IsImpostorSabotageActive() || TerroristSabotageState.IsActive)
            {
                AbortPlant();
                yield break;
            }

            elapsed += Time.deltaTime;
            Timer = plantTime - elapsed;
            if (Button != null)
            {
                Button.SetFillUp(Timer, plantTime);
            }

            yield return null;
        }

        if (player == null || player.Data == null || player.Data.IsDead)
        {
            AbortPlant();
            yield break;
        }

        var duration = OptionGroupSingleton<TerroristOptions>.Instance.SabotageDuration;
        TerroristSabotageState.RpcPlantSabotage(
            player,
            _capturedPosition.x,
            _capturedPosition.y,
            duration,
            _capturedConsoleKey,
            (byte)_capturedKind);

        EffectActive = false;
        Timer = Cooldown;
        _isPlanting = false;
    }

    private void AbortPlant()
    {
        EffectActive = false;
        Timer = Cooldown;
        _isPlanting = false;
    }
}
