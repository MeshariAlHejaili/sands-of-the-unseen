using UnityEngine;

/// <summary>
/// THE punish-window move. Long readable telegraph, one massive arc swing, then a
/// long recovery during which the boss takes increased damage with NO front armor.
///
/// This is the most important state for the "I earned this" feeling. Tune the
/// recovery duration and punish multiplier in FinalBossStats — they're the levers
/// that decide whether the fight feels fair or oppressive.
/// </summary>
public class FinalBossHeavyComboState : IFinalBossState
{
    private enum Phase { Windup, Active, Recovery }
    private Phase phase;
    private float phaseTimer;
    private bool hitLanded;
    private float cachedPunishMultiplier = 1f;

    private const float WINDUP = 0.85f; // long, readable telegraph
    private const float ACTIVE = 0.22f;

    public bool IsPunishable => phase == Phase.Recovery;
    public float DamageMultiplier => phase == Phase.Recovery ? cachedPunishMultiplier : 1f;
    public bool BypassFrontArmor => phase == Phase.Recovery;

    public void Enter(FinalBossController boss)
    {
        if (!boss.Stamina.TryConsume(boss.Stats.HeavyComboStaminaCost))
        {
            boss.ChangeState(new FinalBossIdleState());
            return;
        }
        phase = Phase.Windup;
        phaseTimer = 0f;
        hitLanded = false;
        cachedPunishMultiplier = boss.Stats.HeavyComboPunishMultiplier;

        // Sword: raise overhead during the windup so the player sees the chop coming.
        if (boss.SwordAnimator != null) boss.SwordAnimator.HoldVerticalWindup();

        // Telegraph: wide cone aimed forward for the entire windup.
        if (boss.Telegraph != null)
        {
            boss.Telegraph.ShowCone(
                originWorld: boss.transform.position + Vector3.up * 0.1f,
                forward: boss.transform.forward,
                halfAngleDeg: boss.Stats.HeavyComboArc * 0.5f,
                radius: boss.Stats.HeavyComboRange,
                durationSeconds: WINDUP);
        }
    }

    public void Tick(FinalBossController boss)
    {
        phaseTimer += Time.deltaTime;

        switch (phase)
        {
            case Phase.Windup:
                boss.Facing.FaceTarget(Time.deltaTime);
                // Re-show every frame so the cone tracks the boss's current facing —
                // the boss is rotating, so a static cone would mislead the player.
                if (boss.Telegraph != null)
                {
                    boss.Telegraph.ShowCone(
                        originWorld: boss.transform.position + Vector3.up * 0.1f,
                        forward: boss.transform.forward,
                        halfAngleDeg: boss.Stats.HeavyComboArc * 0.5f,
                        radius: boss.Stats.HeavyComboRange,
                        durationSeconds: WINDUP - phaseTimer);
                }
                if (phaseTimer >= WINDUP)
                {
                    phase = Phase.Active;
                    phaseTimer = 0f;
                }
                break;

            case Phase.Active:
                // Play the swing once at the start of Active.
                if (phaseTimer <= Time.deltaTime + 0.001f && boss.SwordAnimator != null)
                    boss.SwordAnimator.PlaySwingVertical(ACTIVE);

                if (!hitLanded && phaseTimer >= ACTIVE * 0.4f)
                {
                    TryDealArcDamage(boss);
                    hitLanded = true;
                }
                if (phaseTimer >= ACTIVE)
                {
                    phase = Phase.Recovery;
                    phaseTimer = 0f;
                }
                break;

            case Phase.Recovery:
                if (phaseTimer >= boss.Stats.HeavyComboRecovery)
                    boss.RequestNextAction();
                break;
        }
    }

    private void TryDealArcDamage(FinalBossController boss)
    {
        if (boss.Player == null || boss.PlayerHealth == null || boss.PlayerHealth.IsDead) return;

        Vector3 toPlayer = boss.Player.position - boss.transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;
        if (dist > boss.Stats.HeavyComboRange) return;

        float angle = Vector3.Angle(boss.transform.forward, toPlayer);
        if (angle <= boss.Stats.HeavyComboArc * 0.5f)
            boss.PlayerHealth.TakeDamage(boss.Stats.HeavyComboDamage);
    }

    public void Exit(FinalBossController boss)
    {
        if (boss.Telegraph != null) boss.Telegraph.HideAll();
        if (boss.SwordAnimator != null) boss.SwordAnimator.ReturnToIdle();
    }
}
