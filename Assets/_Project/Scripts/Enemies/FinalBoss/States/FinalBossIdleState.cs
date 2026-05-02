using UnityEngine;

/// <summary>
/// Short breather. Faces the player, then immediately asks the brain for the next move.
/// Kept short on purpose — the design doc says aggressive, so idle is a pause, not a rest.
/// </summary>
public class FinalBossIdleState : IFinalBossState
{
    private float duration;
    private float timer;

    public bool IsPunishable => false;
    public float DamageMultiplier => 1f;
    public bool BypassFrontArmor => false;

    public void Enter(FinalBossController boss)
    {
        timer = 0f;
        duration = Random.Range(0.25f, 0.55f);
    }

    public void Tick(FinalBossController boss)
    {
        boss.Facing.FaceTarget(Time.deltaTime);
        timer += Time.deltaTime;
        if (timer >= duration)
            boss.RequestNextAction();
    }

    public void Exit(FinalBossController boss) { }
}
