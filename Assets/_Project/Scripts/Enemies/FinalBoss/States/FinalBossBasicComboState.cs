using UnityEngine;

/// <summary>
/// Aggressive moving combo.
/// Boss keeps walking toward the player while slashing.
/// Supports multiple horizontal and vertical slashes in one combo.
/// </summary>
public class FinalBossBasicComboState : IFinalBossState
{
    private enum Phase
    {
        Windup,
        Combo,
        Recovery
    }

    private Phase phase;
    private float phaseTimer;
    private float slashTimer;

    private int slashIndex;
    private bool hitThisSlash;

    private const float WINDUP = 0.18f;
    private const float SLASH_DURATION = 0.18f;
    private const float TIME_BETWEEN_SLASHES = 0.12f;
    private const float RECOVERY = 0.35f;

    // Change these numbers for now.
    // Later we can move them into FinalBossStats inspector.
    private const int HORIZONTAL_SLASH_COUNT = 3;
    private const int VERTICAL_SLASH_COUNT = 1;

    private int TotalSlashes => HORIZONTAL_SLASH_COUNT + VERTICAL_SLASH_COUNT;

    public bool IsPunishable => phase == Phase.Recovery;
    public float DamageMultiplier => phase == Phase.Recovery ? 1.1f : 1f;
    public bool BypassFrontArmor => phase == Phase.Recovery;

    public void Enter(FinalBossController boss)
    {
        if (!boss.Stamina.TryConsume(boss.Stats.BasicComboStaminaCost))
        {
            boss.ChangeState(new FinalBossIdleState());
            return;
        }

        phase = Phase.Windup;
        phaseTimer = 0f;
        slashTimer = 0f;
        slashIndex = 0;
        hitThisSlash = false;

        if (boss.SwordAnimator != null)
            boss.SwordAnimator.HoldHorizontalWindup();
    }

    public void Tick(FinalBossController boss)
    {
        phaseTimer += Time.deltaTime;

        switch (phase)
        {
            case Phase.Windup:
                boss.Facing.FaceTarget(Time.deltaTime);
                MoveTowardPlayer(boss, boss.Stats.WalkSpeed * 0.65f);

                if (phaseTimer >= WINDUP)
                {
                    phase = Phase.Combo;
                    phaseTimer = 0f;
                    slashTimer = TIME_BETWEEN_SLASHES;
                }
                break;

            case Phase.Combo:
                boss.Facing.FaceTarget(Time.deltaTime);
                MoveTowardPlayer(boss, boss.Stats.WalkSpeed * 0.85f);

                slashTimer += Time.deltaTime;

                if (slashTimer >= TIME_BETWEEN_SLASHES)
                {
                    StartSlash(boss);
                    slashTimer = 0f;
                    hitThisSlash = false;
                }

                if (!hitThisSlash && slashTimer >= SLASH_DURATION * 0.45f)
                {
                    TryDealArcDamage(boss);
                    hitThisSlash = true;
                }

                if (slashIndex >= TotalSlashes && slashTimer >= SLASH_DURATION)
                {
                    phase = Phase.Recovery;
                    phaseTimer = 0f;
                    if (boss.SwordAnimator != null)
                        boss.SwordAnimator.ReturnToIdle();
                }
                break;

            case Phase.Recovery:
                boss.Facing.FaceTarget(Time.deltaTime);

                if (phaseTimer >= RECOVERY)
                    boss.RequestNextAction();

                break;
        }
    }

    private void StartSlash(FinalBossController boss)
    {
        if (slashIndex >= TotalSlashes)
            return;

        if (boss.SwordAnimator != null)
        {
            bool useVertical = slashIndex >= HORIZONTAL_SLASH_COUNT;

            if (useVertical)
                boss.SwordAnimator.PlaySwingVertical(SLASH_DURATION);
            else
                boss.SwordAnimator.PlaySwingHorizontal(SLASH_DURATION);
        }

        slashIndex++;
    }

    private void MoveTowardPlayer(FinalBossController boss, float speed)
    {
        if (boss.Player == null) return;

        Vector3 dir = boss.DirectionToPlayer();

        float distance = boss.DistanceToPlayer();

        // Do not stand inside the player.
        if (distance <= 1.4f)
            return;

        boss.transform.position += dir * speed * Time.deltaTime;
    }

    private void TryDealArcDamage(FinalBossController boss)
    {
        if (boss.Player == null || boss.PlayerHealth == null || boss.PlayerHealth.IsDead)
            return;

        Vector3 toPlayer = boss.Player.position - boss.transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;
        if (dist > boss.Stats.BasicComboRange)
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