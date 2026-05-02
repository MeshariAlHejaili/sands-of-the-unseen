using UnityEngine;

/// <summary>
/// Closes the gap. Scores high when:
///   - Player is far (distance > meleeRange).
///   - Player is retreating (cuts off escape).
/// Scores low if recently used or stamina is tight.
/// </summary>
public class DashEngageAction : IFinalBossAction
{
    public int Id => FinalBossActionIds.DashEngage;

    public float Score(FinalBossContext ctx)
    {
        FinalBossStats s = ctx.Boss.Stats;
        if (!ctx.Boss.Stamina.CanAfford(s.DashStaminaCost)) return 0f;

        // Already in melee range? No need to dash.
        if (ctx.DistanceToPlayer <= s.MeleeRange * 1.1f) return 0f;

        // Distance pull: linear from 0 at meleeRange to 1 at farRange*1.5
        float distancePull = Mathf.InverseLerp(s.MeleeRange, s.FarRange * 1.5f, ctx.DistanceToPlayer);
        float score = 0.45f * distancePull;

        // Retreat punisher: if the player is running away, dash is the right answer.
        if (ctx.PlayerRetreating) score += 0.35f;

        // Stamina caution: scale down when low so we don't dump our reserve.
        score *= Mathf.Clamp01(ctx.StaminaPercent + 0.2f);

        // Anti-repetition: if we just dashed, deprioritize.
        if (ctx.LastActionId == Id) score *= 0.4f;

        return score;
    }

    public IFinalBossState CreateState() => new FinalBossDashEngageState();
}
