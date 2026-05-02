using UnityEngine;

public class BossBrain
{
    private BossController boss;

    private const int ATK_WHIP = 0;
    private const int ATK_HURL = 1;
    private const int ATK_SLAM = 2;
    private const int ATK_SWEEP = 3;

    public BossBrain(BossController boss)
    {
        this.boss = boss;
    }

    public IBossState DecideNextState()
    {
        float dist = boss.DistanceToPlayer();
        float now = Time.time;
        bool phase2 = boss.currentPhase >= 2;

        float speedMultiplier = boss.PhaseAttackSpeed;

        if (phase2 &&
            now - boss.lastSweepTime > boss.sweepCooldown * speedMultiplier &&
            Random.value < 0.4f)
        {
            boss.lastSweepTime = now;
            boss.recentAttackId = ATK_SWEEP;
            return new BossChainSweepState();
        }

        if (now - boss.lastSlamTime > boss.slamCooldown * speedMultiplier)
        {
            boss.lastSlamTime = now;
            boss.recentAttackId = ATK_SLAM;
            return new BossGroundSlamState();
        }

        if (dist > boss.hurlMinRange && boss.recentAttackId != ATK_HURL)
        {
            boss.recentAttackId = ATK_HURL;
            return new BossChainHurlState();
        }

        if (dist <= boss.whipRange && boss.recentAttackId != ATK_WHIP)
        {
            boss.recentAttackId = ATK_WHIP;
            return new BossChainWhipState();
        }

        float idleChance = phase2 ? 0.35f : 0.55f;

        if (Random.value < idleChance)
        {
            return new BossIdleState();
        }

        boss.recentAttackId = ATK_HURL;
        return new BossChainHurlState();
    }
}