/// <summary>
/// Snapshot of decision-relevant state, taken once per brain tick and passed to
/// each action's Score(). Action scorers should be pure functions of this snapshot
/// — no side-channels, no Time.time reads. Easier to reason about, easier to unit
/// test, and avoids inconsistent reads if state changes mid-decision.
/// </summary>
public readonly struct FinalBossContext
{
    public readonly FinalBossController Boss;

    public readonly float DistanceToPlayer;
    public readonly float StaminaPercent;
    public readonly float HealthPercent;
    public readonly float PlayerHpPercent;

    public readonly bool PlayerLikelyHealing;
    public readonly bool PlayerSpamming;
    public readonly bool PlayerRetreating;

    public readonly int LastActionId;
    public readonly int CurrentPhase;

    public FinalBossContext(FinalBossController boss)
    {
        Boss = boss;
        DistanceToPlayer = boss.DistanceToPlayer();
        StaminaPercent = boss.Stamina.Percent;
        HealthPercent = boss.Health.MaxHealth > 0f ? boss.Health.CurrentHealth / boss.Health.MaxHealth : 1f;
        PlayerHpPercent = boss.Sensor.PlayerHpPercent;
        PlayerLikelyHealing = boss.Sensor.IsPlayerLikelyHealing;
        PlayerSpamming = boss.Sensor.IsPlayerSpamming;
        PlayerRetreating = boss.Sensor.IsPlayerRetreating;
        LastActionId = boss.RecentActionId;
        CurrentPhase = boss.CurrentPhase;
    }
}
