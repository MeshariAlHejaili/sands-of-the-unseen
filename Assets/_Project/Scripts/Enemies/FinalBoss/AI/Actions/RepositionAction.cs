using UnityEngine;

/// <summary>
/// Defensive default. The boss walks toward the player at low speed without committing
/// to an attack. This is the answer to "low stamina, can't dash, can't combo."
/// Scores only when other options are weak — a baseline of 0.2 + a stamina-low bonus.
/// </summary>
public class RepositionAction : IFinalBossAction
{
    public int Id => FinalBossActionIds.Reposition;

    public float Score(FinalBossContext ctx)
    {
        // Always available as a fallback.
        float score = 0.15f;

        // The lower the stamina, the more attractive this becomes.
        score += (1f - ctx.StaminaPercent) * 0.4f;

        // If player is very far, prefer dash — penalize repositioning.
        if (ctx.DistanceToPlayer > ctx.Boss.Stats.FarRange * 1.3f) score *= 0.5f;

        // Don't reposition twice in a row — feels passive.
        if (ctx.LastActionId == Id) score *= 0.4f;

        return score;
    }

    public IFinalBossState CreateState() => new FinalBossRepositionState();
}
