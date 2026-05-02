using UnityEngine;

public readonly struct EnemyStatsContext
{
    public EnemyStatsContext(
        Transform target,
        PlayerHealth targetHealth,
        float moveSpeed,
        float contactDamage,
        float contactRange,
        float contactDamageCooldown)
    {
        Target = target;
        TargetHealth = targetHealth;
        MoveSpeed = moveSpeed;
        ContactDamage = contactDamage;
        ContactRange = contactRange;
        ContactDamageCooldown = contactDamageCooldown;
    }

    public Transform Target { get; }
    public PlayerHealth TargetHealth { get; }
    public float MoveSpeed { get; }
    public float ContactDamage { get; }
    public float ContactRange { get; }
    public float ContactDamageCooldown { get; }
}
