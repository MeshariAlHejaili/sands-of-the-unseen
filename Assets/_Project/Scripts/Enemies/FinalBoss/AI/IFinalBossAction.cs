/// <summary>
/// A scored, schedulable action the boss can take. The brain queries each action's
/// Score against the current context and instantiates the highest-scorer's state.
///
/// Why this split (action vs state):
///   - The brain needs to ASK an action "would you be good right now?" cheaply,
///     without instantiating a state every tick.
///   - It also lets us add new actions in Phase 2 (PrecisionShot, ReloadAction)
///     without touching the brain or any existing files. Open/closed principle.
/// </summary>
public interface IFinalBossAction
{
    /// <summary>Stable identifier used to detect action repetition. Match values in FinalBossActionIds.</summary>
    int Id { get; }

    /// <summary>Returns a score in roughly [0, 1] given the context. Higher = more desirable now. Return 0 to abstain.</summary>
    float Score(FinalBossContext ctx);

    /// <summary>Builds the state that executes this action. Called only on the winning action.</summary>
    IFinalBossState CreateState();
}

/// <summary>Stable IDs so we can compare without strings.</summary>
public static class FinalBossActionIds
{
    public const int DashEngage = 1;
    public const int BasicCombo = 2;
    public const int HeavyCombo = 3;
    public const int FrostSlash = 4;
    public const int Reposition = 5;
    public const int Idle       = 6;
}
