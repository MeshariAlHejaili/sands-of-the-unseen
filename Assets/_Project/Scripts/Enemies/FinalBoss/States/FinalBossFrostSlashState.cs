using UnityEngine;

/// <summary>
/// Aggressive frost slash.
/// Boss moves sideways while aiming, then fires multiple fast slash projectiles.
/// </summary>
public class FinalBossFrostSlashState : IFinalBossState
{
    private enum Phase
    {
        Telegraph,
        Firing,
        Recovery
    }

    private Phase phase;
    private float phaseTimer;
    private float fireTimer;

    private int firedCount;
    private int totalShots;

    private Vector3 strafeDirection;
    private readonly SlashOrientation orientation;

    public bool IsPunishable => phase == Phase.Recovery;
    public float DamageMultiplier => phase == Phase.Recovery ? 1.1f : 1f;
    public bool BypassFrontArmor => phase == Phase.Recovery;

    public FinalBossFrostSlashState() : this(SlashOrientation.Horizontal) { }

    public FinalBossFrostSlashState(SlashOrientation orientation)
    {
        this.orientation = orientation;
    }

    public void Enter(FinalBossController boss)
    {
        if (!boss.Stamina.TryConsume(boss.Stats.FrostStaminaCost))
        {
            boss.ChangeState(new FinalBossIdleState());
            return;
        }

        phase = Phase.Telegraph;
        phaseTimer = 0f;
        fireTimer = 0f;
        firedCount = 0;

        totalShots = orientation == SlashOrientation.Horizontal
            ? boss.Stats.FrostHorizontalSlashCount
            : boss.Stats.FrostVerticalSlashCount;

        totalShots = Mathf.Max(1, totalShots);

        Vector3 right = boss.transform.right;
        strafeDirection = Random.value > 0.5f ? right : -right;

        if (boss.SwordAnimator != null)
        {
            if (orientation == SlashOrientation.Horizontal)
                boss.SwordAnimator.HoldHorizontalWindup();
            else
                boss.SwordAnimator.HoldVerticalWindup();
        }
    }

    public void Tick(FinalBossController boss)
    {
        phaseTimer += Time.deltaTime;

        switch (phase)
        {
            case Phase.Telegraph:
                boss.Facing.FaceTarget(Time.deltaTime);
                Strafe(boss, boss.Stats.FrostStrafeSpeed);

                ShowTelegraph(boss);

                if (phaseTimer >= boss.Stats.FrostTelegraphTime)
                {
                    phase = Phase.Firing;
                    phaseTimer = 0f;
                    fireTimer = boss.Stats.FrostTimeBetweenSlashes;
                }

                break;

            case Phase.Firing:
                boss.Facing.FaceTarget(Time.deltaTime);
                Strafe(boss, boss.Stats.FrostStrafeSpeed);

                fireTimer += Time.deltaTime;

                if (fireTimer >= boss.Stats.FrostTimeBetweenSlashes && firedCount < totalShots)
                {
                    FireOneSlash(boss);
                    fireTimer = 0f;
                    firedCount++;
                }

                if (firedCount >= totalShots && fireTimer >= boss.Stats.FrostLaunchTime)
                {
                    phase = Phase.Recovery;
                    phaseTimer = 0f;

                    if (boss.SwordAnimator != null)
                        boss.SwordAnimator.ReturnToIdle();
                }

                break;

            case Phase.Recovery:
                boss.Facing.FaceTarget(Time.deltaTime);

                if (phaseTimer >= boss.Stats.FrostRecoveryTime)
                    boss.RequestNextAction();

                break;
        }
    }

    private void FireOneSlash(FinalBossController boss)
    {
        if (boss.Player == null)
            return;

        Vector3 dir = boss.DirectionToPlayer();

        if (boss.SwordAnimator != null)
        {
            if (orientation == SlashOrientation.Horizontal)
                boss.SwordAnimator.PlaySwingHorizontal(boss.Stats.FrostLaunchTime);
            else
                boss.SwordAnimator.PlaySwingVertical(boss.Stats.FrostLaunchTime);
        }

        SpawnProjectile(boss, dir);
    }

    private void SpawnProjectile(FinalBossController boss, Vector3 direction)
    {
        FrostProjectilePool pool = FrostProjectilePool.Instance;

        if (pool == null)
        {
            Debug.LogWarning("[FinalBoss] No FrostProjectilePool in scene.", boss);
            return;
        }

        Vector3 origin = boss.transform.position + Vector3.up * 1.4f + direction * 1.5f;

        FrostProjectile p = pool.Get();

        p.transform.position = origin;
        p.transform.rotation = Quaternion.LookRotation(direction);

        p.Init(
            direction: direction,
            speed: boss.Stats.FrostProjectileSpeed,
            range: boss.Stats.FrostProjectileRange,
            width: boss.Stats.FrostProjectileWidth,
            damage: boss.Stats.FrostDamage,
            target: boss.PlayerHealth,
            returnCallback: pool.Return,
            orientation: orientation
        );
    }

    private void Strafe(FinalBossController boss, float speed)
    {
        boss.transform.position += strafeDirection * speed * Time.deltaTime;
    }

    private void ShowTelegraph(FinalBossController boss)
    {
        if (boss.Telegraph == null || boss.Player == null)
            return;

        Vector3 from = boss.transform.position + Vector3.up * 1.4f;
        Vector3 to = boss.Player.position + Vector3.up * 1.0f;

        float remaining = Mathf.Max(0f, boss.Stats.FrostTelegraphTime - phaseTimer);

        boss.Telegraph.ShowLine(from, to, remaining);
    }

    public void Exit(FinalBossController boss)
    {
        if (boss.Telegraph != null)
            boss.Telegraph.HideAll();

        if (boss.SwordAnimator != null)
            boss.SwordAnimator.ReturnToIdle();
    }
}