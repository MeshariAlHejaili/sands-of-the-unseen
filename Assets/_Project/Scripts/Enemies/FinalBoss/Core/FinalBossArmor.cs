using UnityEngine;

/// <summary>
/// Damage filter. Sits between FinalBossHitbox and EnemyHealth and applies:
///   1. Front-armor reduction if the hit comes from the boss's front cone
///      AND the current state does not bypass armor.
///   2. Punish multiplier from the current state (rewards player for hitting
///      during recovery windows).
///
/// This is intentionally a small, focused class. Adding new damage rules
/// (e.g., "vulnerable while frozen") means extending here, not editing the
/// hitbox or the states.
/// </summary>
[RequireComponent(typeof(FinalBossStats))]
[RequireComponent(typeof(FinalBossFacing))]
public class FinalBossArmor : MonoBehaviour
{
    private FinalBossStats stats;
    private FinalBossFacing facing;
    private FinalBossController boss;

    private void Awake()
    {
        stats = GetComponent<FinalBossStats>();
        facing = GetComponent<FinalBossFacing>();
        boss = GetComponent<FinalBossController>();
    }

    /// <summary>Computes the damage actually applied after armor + punish modifiers.</summary>
    /// <param name="rawDamage">Incoming damage before modification.</param>
    /// <param name="hitWorldPosition">World position of the hit (for angle calculation).</param>
    public float ModifyDamage(float rawDamage, Vector3 hitWorldPosition)
    {
        if (rawDamage <= 0f) return 0f;

        float result = rawDamage;
        IFinalBossState state = boss != null ? boss.CurrentState : null;

        // Punish window comes first — it's a damage reward, applied unconditionally.
        if (state != null && state.IsPunishable)
            result *= state.DamageMultiplier;

        // Armor reduction — skipped if state opts out (e.g., during recovery).
        if (state == null || !state.BypassFrontArmor)
        {
            Vector3 hitDirFromBoss = hitWorldPosition - transform.position;
            if (facing.IsHitFromFront(hitDirFromBoss))
                result *= stats.FrontArmorDamageMultiplier;
        }

        return result;
    }
}
