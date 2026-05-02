using UnityEngine;

/// <summary>
/// Aggressive multi-dash engage.
/// Boss can dash more than once and slash during the dash.
/// This makes him feel like he is attacking while moving, not stopping to perform.
/// </summary>
public class FinalBossDashEngageState : IFinalBossState
{
    private enum Phase
    {
        Telegraph,
        Dash,
        MicroPause,
        Recovery
    }

    private Phase phase;
    private float phaseTimer;

    private Vector3 dashDirection;

    private int dashIndex;
    private bool dashHitLanded;
    private bool slashStarted;
    private bool slashDamageDone;

    private const float TELEGRAPH = 0.18f;
    private const float MICRO_PAUSE = 0.08f;
    private const float RECOVERY = 0.22f;

    private const int MAX_DASHES = 3;

    private const float SLASH_START_TIME = 0.08f;
    private const float SLASH_DAMAGE_TIME = 0.14f;
    private const float SLASH_DURATION = 0.18f;

    private const float STOP_CHAIN_DISTANCE = 2.2f;

    public bool IsPunishable => phase == Phase.Recovery;
    public float DamageMultiplier => phase == Phase.Recovery ? 1.15f : 1f;
    public bool BypassFrontArmor => phase == Phase.Recovery;

    public void Enter(FinalBossController boss)
    {
        if (!boss.Stamina.TryConsume(boss.Stats.DashStaminaCost))
        {
            boss.ChangeState(new FinalBossIdleState());
            return;
        }

        phase = Phase.Telegraph;
        phaseTimer = 0f;

        dashIndex = 0;

        dashHitLanded = false;
        slashStarted = false;
        slashDamageDone = false;

        boss.Facing.SnapToTarget();

        if (boss.SwordAnimator != null)
            boss.SwordAnimator.HoldHorizontalWindup();
    }

    public void Tick(FinalBossController boss)
    {
        phaseTimer += Time.deltaTime;

        switch (phase)
        {
            case Phase.Telegraph:
                boss.Facing.FaceTarget(Time.deltaTime);

                if (phaseTimer >= TELEGRAPH)
                    BeginDash(boss);

                break;

            case Phase.Dash:
                TickDash(boss);
                break;

            case Phase.MicroPause:
                boss.Facing.FaceTarget(Time.deltaTime);

                if (phaseTimer >= MICRO_PAUSE)
                {
                    if (ShouldDashAgain(boss))
                        BeginDash(boss);
                    else
                        BeginRecovery(boss);
                }

                break;

            case Phase.Recovery:
                boss.Facing.FaceTarget(Time.deltaTime);

                if (phaseTimer >= RECOVERY)
                    boss.RequestNextAction();

                break;
        }
    }

    private void BeginDash(FinalBossController boss)
    {
        dashIndex++;

        phase = Phase.Dash;
        phaseTimer = 0f;

        dashHitLanded = false;
        slashStarted = false;
        slashDamageDone = false;

        boss.Facing.SnapToTarget();
        dashDirection = boss.DirectionToPlayer();
    }

    private void TickDash(FinalBossController boss)
    {
        float dur = boss.Stats.DashDuration;
        float speed = boss.Stats.DashDistance / Mathf.Max(0.01f, dur);

        boss.transform.position += dashDirection * speed * Time.deltaTime;

        // Start sword animation during the dash.
        if (!slashStarted && phaseTimer >= SLASH_START_TIME)
        {
            slashStarted = true;

            if (boss.SwordAnimator != null)
            {
                if (dashIndex % 2 == 0)
                    boss.SwordAnimator.PlaySwingVertical(SLASH_DURATION);
                else
                    boss.SwordAnimator.PlaySwingHorizontal(SLASH_DURATION);
            }
        }

        // Slash damage timing.
        if (!slashDamageDone && phaseTimer >= SLASH_DAMAGE_TIME)
        {
            slashDamageDone = true;
            TryDealDashSlashDamage(boss);
        }

        // Contact damage if he physically reaches the player.
        if (!dashHitLanded && boss.PlayerHealth != null && !boss.PlayerHealth.IsDead)
        {
            if (boss.DistanceToPlayer() <= boss.Stats.DashContactRange)
            {
                boss.PlayerHealth.TakeDamage(boss.Stats.DashContactDamage);
                dashHitLanded = true;
            }
        }

        if (phaseTimer >= dur)
        {
            phase = Phase.MicroPause;
            phaseTimer = 0f;
        }
    }

    private bool ShouldDashAgain(FinalBossController boss)
    {
        if (dashIndex >= MAX_DASHES)
            return false;

        if (boss.Player == null)
            return false;

        return boss.DistanceToPlayer() > STOP_CHAIN_DISTANCE;
    }

    private void BeginRecovery(FinalBossController boss)
    {
        phase = Phase.Recovery;
        phaseTimer = 0f;

        if (boss.SwordAnimator != null)
            boss.SwordAnimator.ReturnToIdle();
    }

    private void TryDealDashSlashDamage(FinalBossController boss)
    {
        if (boss.Player == null || boss.PlayerHealth == null || boss.PlayerHealth.IsDead)
            return;

        Vector3 toPlayer = boss.Player.position - boss.transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;

        // Slightly bigger than basic combo because the boss is moving fast.
        float range = boss.Stats.BasicComboRange + 0.7f;

        if (dist > range)
            return;

        float angle = Vector3.Angle(boss.transform.forward, toPlayer);

        if (angle <= boss.Stats.BasicComboArc * 0.5f)
            boss.PlayerHealth.TakeDamage(boss.Stats.BasicComboDamage);
    }

    public void Exit(FinalBossController boss)
    {
        if (boss.SwordAnimator != null)
            boss.SwordAnimator.ReturnToIdle();
    }
}