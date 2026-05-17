using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralOutlier;

namespace DivaniMods.Options;

public class TerroristOptions : AbstractOptionGroup<TerroristRole>
{
    public override string GroupName => "Terrorist";

    /// <summary>Sabotages the Terrorist must successfully detonate to win alone.</summary>
    [ModdedNumberOption("Successful Sabotages To Win", 1f, 4f, 1f)]
    public float SabotagesToWin { get; set; } = 2f;

    /// <summary>Cooldown between Plant attempts. Mirrors the impostor sabotage cooldown so plant/sabo
    /// pace at the same rate.</summary>
    [ModdedNumberOption("Plant Cooldown", 10f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float PlantCooldown { get; set; } = 30f;

    /// <summary>How long the planted sabotage stays active before counting as successful (if not defused).</summary>
    [ModdedNumberOption("Sabotage Duration", 15f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float SabotageDuration { get; set; } = 30f;

    /// <summary>Time the Terrorist must hold to finish the Plant action.</summary>
    [ModdedNumberOption("Plant Time", 2f, 10f, 1f, MiraNumberSuffixes.Seconds)]
    public float PlantTime { get; set; } = 5f;

    /// <summary>Time required to defuse a planted sabotage.</summary>
    [ModdedNumberOption("Defuse Time", 2f, 10f, 1f, MiraNumberSuffixes.Seconds)]
    public float DefuseTime { get; set; } = 5f;

    [ModdedToggleOption("Saboteur Can Vent")]
    public bool CanVent { get; set; } = false;

    /// <summary>After a sabotage explodes, that utility cannot be used by anyone for the rest of the game.</summary>
    [ModdedToggleOption("Disable Exploded Utility For Game")]
    public bool DisableExplodedConsoles { get; set; } = true;
}
