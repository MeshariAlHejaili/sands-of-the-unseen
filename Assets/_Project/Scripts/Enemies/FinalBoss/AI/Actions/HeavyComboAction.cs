using UnityEngine;

/// <summary>
/// High-damage melee with a clear punish window on whiff/recovery.
/// Scores up when:
///   - Player is in melee range AND stayed close (committed).
///   - Boss has plenty of stamina (the heavy is expensive and risky).
/// 
///   - Player is healing (capitalize on the opening).
/// Scores down when stamina is tight or the boss just used a heavy.
/// </summary>
public class HeavyComboAction : IFinalBossAction
{
    public int Id => FinalBossActionIds.HeavyCombo;

    public float Score(FinalBossContext ctx)
    {
        FinalBossStats s = ctx.Boss.Stats;
        if (!ctx.Boss.Stamina.CanAfford(s.HeavyComboStaminaCost)) return 0f;
        if (ctx.DistanceToPlayer > s.HeavyComboRange * 1.1f) return 0f;

        float score = 0.4f;

        // Capitalize on healing: huge swing when player is locked in regen behavior.
        if (ctx.PlayerLikelyHealing) score += 0.45f;

        // Boss feels confident at high stamina.
        if (ctx.StaminaPercent > 0.7f) score += 0.15f;

        // Don't use heavy back-to-back; it's the slowest, most punishable move.
        if (ctx.LastActionId == Id) score *= 0.3f;

        // Stamina caution.
        score *= Mathf.Clamp01(ctx.StaminaPercent + 0.1f);

        return score;
    }

    public IFinalBossState CreateState() => new FinalBossHeavyComboState();
}
