using UnityEngine;

/// <summary>
/// Stamina-depleted vulnerability window. Triggered automatically by the controller
/// when FinalBossStamina.Depleted fires. Boss takes 1.75x damage, armor is bypassed,
/// no actions are taken. Exits when stagger duration elapses.
///
/// This is the "fight back" payoff for managing the boss's stamina by baiting actions.
/// </summary>
public class FinalBossStaggerState : IFinalBossState
{
    private float timer;
    private float cachedDuration;

    public bool IsPunishable => true;
    public float DamageMultiplier => 1.75f;
    public bool BypassFrontArmor => true;

    public void Enter(FinalBossController boss)
    {
        timer = 0f;
        cachedDuration = boss.Stats.StaggerDuration;
        Debug.Log("[FinalBoss] STAGGERED — punish window open.");
    }

    public void Tick(FinalBossController boss)
    {
        timer += Time.deltaTime;
        if (timer >= cachedDuration)
            boss.RequestNextAction();
    }

    public void Exit(FinalBossController boss)
    {
        // Give the boss a small recovery dose so it has at least one move available.
        // Without this, brain picks Reposition forever until natural regen catches up,
        // which would feel like the boss is permanently broken.
        boss.Stamina.TopUp(boss.Stats.MaxStamina * 0.25f);
    }
}
