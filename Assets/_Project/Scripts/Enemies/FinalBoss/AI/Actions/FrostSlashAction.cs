using UnityEngine;

/// <summary>
/// Ranged projectile. The boss's mid-range answer to bullet spam and to healing
/// players who are staying still and far. Scores low in close-quarters (use combos).
///
/// Picks orientation contextually:
///   - Horizontal slash → wide, sweeps the sides. Used when the player is moving
///     LATERALLY (strafing). Punishes sidestep dodges.
///   - Vertical slash → narrower, taller. Used when the player is stationary, healing,
///     or moving directly toward/away from the boss. Harder to dodge by sidestepping
///     because it's already narrow on that axis.
///
/// A 15% randomness epsilon flips the choice occasionally so an experienced player
/// can't perfectly read it. Most of the time the choice is intentional — that's what
/// makes the boss feel like it's responding to YOU instead of rolling dice.
/// </summary>
public class FrostSlashAction : IFinalBossAction
{
    public int Id => FinalBossActionIds.FrostSlash;

    // Choice budget for randomness — 15% of the time we flip the contextual pick.
    private const float OrientationFlipChance = 0.15f;

    public float Score(FinalBossContext ctx)
    {
        FinalBossStats s = ctx.Boss.Stats;
        if (!ctx.Boss.Stamina.CanAfford(s.FrostStaminaCost)) return 0f;

        // Too close? Frost is wasted; player could be hit by a sword instead.
        if (ctx.DistanceToPlayer < s.MeleeRange * 1.1f) return 0f;
        // Too far? Projectile is dodgeable; consider dashing instead.
        if (ctx.DistanceToPlayer > s.FrostProjectileRange * 0.9f) return 0f;

        float score = 0.4f;

        // Punish bullet spam — if player is shooting a lot, return fire.
        if (ctx.PlayerSpamming) score += 0.3f;

        // Punish healing — frost catches stationary targets.
        if (ctx.PlayerLikelyHealing) score += 0.3f;

        // Anti-repetition.
        if (ctx.LastActionId == Id) score *= 0.5f;

        return score;
    }

    public IFinalBossState CreateState()
    {
        // Late-binding the orientation: we don't know it at Score time because the
        // player's behavior at score-time vs commit-time can differ. We re-read here.
        // Note: we don't have direct context access, so we read sensor signals from
        // the live boss singleton. Better: refactor IFinalBossAction.CreateState to
        // take ctx — but for now this is fine and isolated.
        SlashOrientation orientation = ChooseOrientation();
        return new FinalBossFrostSlashState(orientation);
    }

    private SlashOrientation ChooseOrientation()
    {
        // We need access to the boss to read the sensor's player-retreating signal.
        // Find the active boss — only one final boss exists at a time, so this is OK.
        FinalBossController boss = Object.FindFirstObjectByType<FinalBossController>();
        if (boss == null || boss.Sensor == null)
            return Random.value < 0.5f ? SlashOrientation.Horizontal : SlashOrientation.Vertical;

        // Decision rule:
        //   - Player retreating (moving away from boss along their facing) OR
        //     player likely healing (stationary) → VERTICAL. They're not strafing,
        //     so a wide horizontal isn't optimal; vertical catches stillness.
        //   - Otherwise (player likely strafing or neutral) → HORIZONTAL. The wide
        //     sweep cuts off lateral dodges.
        bool wantsVertical = boss.Sensor.IsPlayerLikelyHealing || boss.Sensor.IsPlayerRetreating;
        SlashOrientation pick = wantsVertical ? SlashOrientation.Vertical : SlashOrientation.Horizontal;

        // 15% flip — keeps the boss from being 100% predictable to a veteran player.
        if (Random.value < OrientationFlipChance)
            pick = pick == SlashOrientation.Horizontal ? SlashOrientation.Vertical : SlashOrientation.Horizontal;

        return pick;
    }
}
