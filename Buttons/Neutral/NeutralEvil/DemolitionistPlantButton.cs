using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Patches;
using DivaniMods.Roles.Neutral.NeutralEvil;
using DivaniMods.Utilities;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Neutral.NeutralEvil;

/// <summary>
/// Plant button (<see cref="CustomActionButton"/>). Shown while near any utility
/// console (admin / cameras / vitals / door log). Hold duration fills the button
/// ring like Sentinel beacon placement.
/// </summary>
public class DemolitionistPlantButton : CustomActionButton
{
    public override string Name => "Plant";
    public override float Cooldown => OptionGroupSingleton<DemolitionistOptions>.Instance.PlantCooldown;
    public override float EffectDuration => OptionGroupSingleton<DemolitionistOptions>.Instance.IsTimedSabotageStyle
        ? OptionGroupSingleton<DemolitionistOptions>.Instance.PlantTime.Value
        : 0f;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.DemolitionistSabotageButton;
    /// <summary>BottomRight conflicts with impostor vent when <see cref="DemolitionistOptions.CanVent"/> is on — use left row for plant in that case.</summary>
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight; 
    public override Color TextOutlineColor => DemolitionistRole.DemolitionistColor;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;

    public static DemolitionistPlantButton? Instance { get; set; }

    private Vector2 _capturedPosition;
    private int _capturedConsoleKey;
    private DemolitionistUtilityKind _capturedKind;
    private bool _isPlanting;

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        return role is DemolitionistRole;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        if (MeetingHud.Instance || ExileController.Instance) return false;

        if (DemolitionistSabotageState.IsCriticalVanillaSabotageActive()) return false;
        if (DemolitionistSabotageState.IsActive) return false;
        if (DemolitionistNumpad.Controller.InProgress) return false;
        if (_isPlanting || EffectActive) return false;
        if (!DemolitionistUtilityConsoles.TryGetClosest(player, out _, out _, forDemolitionistPlant: true)) return false;

        return base.CanUse();
    }

    /// <summary>
    /// Do not start cooldown on click — only after a successful plant (numpad or timed).
    /// </summary>
    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
    }

    /// <summary>Called when sabotage ends so the plant button is not stuck grey/disabled.</summary>
    public static void SyncAfterSabotageEnded(bool startCooldown)
    {
        var plant = Instance;
        if (plant == null)
        {
            return;
        }

        plant._isPlanting = false;
        plant.EffectActive = false;
        if (startCooldown)
        {
            plant.Timer = plant.Cooldown;
        }

        if (plant.Button == null)
        {
            return;
        }

        if (plant.CanUse())
        {
            plant.Button.SetEnabled();
        }
        else if (plant.Timer > 0f)
        {
            plant.Button.SetDisabled();
        }
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;

        if (!DemolitionistUtilityConsoles.TryGetClosest(player, out var consolePosition, out var kind, forDemolitionistPlant: true)
            || kind == DemolitionistUtilityKind.None)
        {
            return;
        }

        if (DemolitionistSabotageState.IsActive || DemolitionistSabotageState.IsCriticalVanillaSabotageActive()) return;

        _capturedPosition = consolePosition;
        _capturedKind = kind;
        _capturedConsoleKey = DemolitionistUtilityConsoles.GetStableId(kind, consolePosition);

        if (!OptionGroupSingleton<DemolitionistOptions>.Instance.IsTimedSabotageStyle)
        {
            Coroutines.Start(PlantNumpadCoroutine(player));
            return;
        }

        Coroutines.Start(PlantTimedCoroutine(player));
    }

    private IEnumerator PlantTimedCoroutine(PlayerControl player)
    {
        _isPlanting = true;
        var plantTime = EffectDuration;
        var colorHex = ColorUtility.ToHtmlStringRGB(DemolitionistRole.DemolitionistColor);

        EffectActive = true;
        Timer = plantTime;

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{colorHex}>Planting sabotage...</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.DemolitionistSabotageButton.LoadAsset());

        var elapsed = 0f;
        while (elapsed < plantTime)
        {
            if (player == null || player.Data == null || player.Data.IsDead)
            {
                AbortPlant();
                yield break;
            }

            if (!DemolitionistUtilityConsoles.TryGetClosest(player, out var currentPos, out var currentKind, forDemolitionistPlant: true)
                || currentKind != _capturedKind
                || DemolitionistUtilityConsoles.GetStableId(currentKind, currentPos) != _capturedConsoleKey)
            {
                MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                    $"<b><color=#{colorHex}>Plant aborted — too far from console!</color></b>",
                    Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: DivaniAssets.DemolitionistSabotageButton.LoadAsset());
                AbortPlant();
                yield break;
            }

            if (DemolitionistSabotageState.IsCriticalVanillaSabotageActive() || DemolitionistSabotageState.IsActive)
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

        var duration = OptionGroupSingleton<DemolitionistOptions>.Instance.SabotageDuration;
        DemolitionistSabotageState.RpcPlantSabotage(
            player,
            player.PlayerId,
            _capturedPosition.x,
            _capturedPosition.y,
            duration,
            _capturedConsoleKey,
            (byte)_capturedKind);

        EffectActive = false;
        Timer = 0f;
        _isPlanting = false;
    }

    private IEnumerator PlantNumpadCoroutine(PlayerControl player)
    {
        _isPlanting = true;
        EffectActive = true;

        if (!DemolitionistNumpad.Controller.OpenPlant(player, _capturedPosition, _capturedConsoleKey, _capturedKind))
        {
            AbortPlant();
            yield break;
        }

        // Do not call TryGetClosest while minigame is open — vanilla Use/couldUse often false during KeypadGame,
        // so first frame would abort even though player is still at the utility.
        while (DemolitionistNumpad.Controller.InProgress)
        {
            if (player == null || player.Data == null || player.Data.IsDead
                || DemolitionistSabotageState.IsCriticalVanillaSabotageActive())
            {
                AbortPlant();
                yield break;
            }

            yield return null;
        }

        EffectActive = false;
        Timer = 0f;
        _isPlanting = false;
    }

    private void AbortPlant()
    {
        if (DemolitionistNumpad.Controller.InProgress)
        {
            DemolitionistNumpad.Controller.CancelActive();
        }

        EffectActive = false;
        Timer = 0f;
        _isPlanting = false;
    }
}
