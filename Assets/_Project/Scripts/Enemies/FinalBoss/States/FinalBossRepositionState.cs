using UnityEngine;

/// <summary>
/// Defensive walk toward the player. No commitment, no damage. The boss uses this
/// when stamina is low or when no other action scores well — it lets stamina regen
/// while still maintaining pressure (the boss keeps closing distance).
///
/// Duration is short — we want the brain to re-evaluate quickly so the boss doesn't
/// look like it's just pacing.
/// </summary>
public class FinalBossRepositionState : IFinalBossState
{
    private float duration;
    private float timer;

    public bool IsPunishable => false;
    public float DamageMultiplier => 1f;
    public bool BypassFrontArmor => false;

    public void Enter(FinalBossController boss)
    {
        timer = 0f;
        duration = Random.Range(0.6f, 1.0f);
    }

    public void Tick(FinalBossController boss)
    {
        timer += Time.deltaTime;
        boss.Facing.FaceTarget(Time.deltaTime);

        // Walk forward only if we're outside melee range — don't crash into the player.
        if (boss.DistanceToPlayer() > boss.Stats.MeleeRange * 0.85f)
        {
            Vector3 step = boss.DirectionToPlayer() * boss.Stats.WalkSpeed * Time.deltaTime;
            boss.transform.position += step;
        }

        if (timer >= duration)
            boss.RequestNextAction();
    }

    public void Exit(FinalBossController boss) { }
}
