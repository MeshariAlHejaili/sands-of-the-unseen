using UnityEngine;

/// <summary>
/// Bread-and-butter melee combo. Scores well when the player is inside melee range
/// and the boss has the stamina. Slightly reduced if last action was also basic combo
/// — the boss should mix in heavies and frost slashes.
/// </summary>
public class BasicComboAction : IFinalBossAction
{
    public int Id => FinalBossActionIds.BasicCombo;

    public float Score(FinalBossContext ctx)
    {
        FinalBossStats s = ctx.Boss.Stats;
        if (!ctx.Boss.Stamina.CanAfford(s.BasicComboStaminaCost)) return 0f;
        if (ctx.DistanceToPlayer > s.MeleeRange * 1.2f) return 0f;

        float score = 0.55f;

        // Closer = even better.
        if (ctx.DistanceToPlayer < s.MeleeRange * 0.7f) score += 0.15f;

        // Don't repeat the exact same combo back-to-back.
        if (ctx.LastActionId == Id) score *= 0.55f;

        return score;
    }

    public IFinalBossState CreateState() => new FinalBossBasicComboState();
}
