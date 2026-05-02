using UnityEngine;

/// <summary>
/// Inspector-tunable data container for the final boss. Mirrors the PlayerStats pattern:
/// no logic, just exposed properties that other systems read. Keep all magic numbers here
/// instead of scattered across state files so designers can tune the fight in the inspector.
/// </summary>
public class FinalBossStats : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Maximum boss health. Phase 2 triggers at half of this value.")]
    [Min(1f)]
    [SerializeField] private float maxHealth = 2000f;

    [Tooltip("Health percent (0-1) at which Phase 2 begins.")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float phase2HealthThreshold = 0.5f;

    [Header("Stamina")]
    [Tooltip("Maximum boss stamina. Every action costs stamina; running out causes a stagger window.")]
    [Min(1f)]
    [SerializeField] private float maxStamina = 100f;

    [Tooltip("Stamina restored per second after the regen delay.")]
    [Min(0f)]
    [SerializeField] private float staminaRegenPerSecond = 12f;

    [Tooltip("Seconds after a stamina spend before regen resumes.")]
    [Min(0f)]
    [SerializeField] private float staminaRegenDelay = 1.2f;

    [Tooltip("Seconds the boss is staggered (vulnerable, no actions) when stamina hits zero.")]
    [Min(0.1f)]
    [SerializeField] private float staggerDuration = 1.6f;

    [Header("Movement")]
    [Tooltip("Walking speed for repositioning (boss does not walk during attacks).")]
    [Min(0f)]
    [SerializeField] private float walkSpeed = 4.5f;

    [Tooltip("Rotation speed when facing the player (radians per second feel).")]
    [Min(0f)]
    [SerializeField] private float turnSpeed = 8f;

    [Header("Front Armor")]
    [Tooltip("Half-angle of the front-armor cone in degrees. Hits within this cone of the boss's forward direction are reduced.")]
    [Range(0f, 180f)]
    [SerializeField] private float frontArmorHalfAngle = 60f;

    [Tooltip("Damage multiplier applied to hits inside the front cone. 0.3 means front hits do 30% damage.")]
    [Range(0f, 1f)]
    [SerializeField] private float frontArmorDamageMultiplier = 0.3f;

    [Header("Combat — Dash Engage")]
    [Tooltip("Stamina cost of a dash engage.")]
    [Min(0f)] [SerializeField] private float dashStaminaCost = 20f;
    [Tooltip("Distance the boss travels during a dash engage.")]
    [Min(0f)] [SerializeField] private float dashDistance = 9f;
    [Tooltip("Dash duration in seconds.")]
    [Min(0.05f)] [SerializeField] private float dashDuration = 0.35f;
    [Tooltip("Damage dealt if the dash makes contact with the player.")]
    [Min(0f)] [SerializeField] private float dashContactDamage = 18f;
    [Tooltip("Range at which the dash hit is checked.")]
    [Min(0f)] [SerializeField] private float dashContactRange = 1.6f;

    [Header("Combat — Basic Combo (3 hits, no real punish)")]
    [Min(0f)] [SerializeField] private float basicComboStaminaCost = 15f;
    [Min(0f)] [SerializeField] private float basicComboDamage = 14f;
    [Min(0f)] [SerializeField] private float basicComboRange = 3.0f;
    [Range(0f, 180f)] [SerializeField] private float basicComboArc = 110f;

    [Header("Combat — Heavy Combo (big damage, big punish window)")]
    [Min(0f)] [SerializeField] private float heavyComboStaminaCost = 28f;
    [Min(0f)] [SerializeField] private float heavyComboDamage = 32f;
    [Min(0f)] [SerializeField] private float heavyComboRange = 3.6f;
    [Range(0f, 180f)] [SerializeField] private float heavyComboArc = 140f;
    [Tooltip("Recovery seconds after a heavy combo. This is the punish window — keep it generous.")]
    [Min(0f)] [SerializeField] private float heavyComboRecovery = 1.5f;
    [Tooltip("Damage multiplier the player gets for hitting the boss during heavy recovery.")]
    [Min(1f)] [SerializeField] private float heavyComboPunishMultiplier = 1.5f;

    [Header("Combat — Frost Slash (ranged projectile, slows player)")]
    [Min(0f)] [SerializeField] private float frostStaminaCost = 22f;
    [Min(0f)] [SerializeField] private float frostDamage = 16f;
    [Min(0f)] [SerializeField] private float frostProjectileSpeed = 16f;
    [Min(0f)] [SerializeField] private float frostProjectileRange = 22f;
    [Min(0f)] [SerializeField] private float frostProjectileWidth = 1.6f;

    [Header("Engagement Ranges")]
    [Tooltip("Inside this range the boss prefers melee combos.")]
    [Min(0f)] [SerializeField] private float meleeRange = 4f;
    [Tooltip("Outside this range the boss prefers to dash or shoot frost.")]
    [Min(0f)] [SerializeField] private float farRange = 10f;
    [Header("Combat — Frost Slash Tuning")]
    [Min(0.05f)] [SerializeField] private float frostTelegraphTime = 0.35f;
    [Min(0.05f)] [SerializeField] private float frostLaunchTime = 0.16f;
    [Min(0f)] [SerializeField] private float frostRecoveryTime = 0.25f;
    [Min(0.05f)] [SerializeField] private float frostTimeBetweenSlashes = 0.18f;

    [Min(1)] [SerializeField] private int frostHorizontalSlashCount = 3;
    [Min(1)] [SerializeField] private int frostVerticalSlashCount = 2;

    [Min(0f)] [SerializeField] private float frostStrafeSpeed = 3.5f;

    // -------- Public accessors --------
    public float MaxHealth => maxHealth;
    public float Phase2HealthThreshold => phase2HealthThreshold;

    public float MaxStamina => maxStamina;
    public float StaminaRegenPerSecond => staminaRegenPerSecond;
    public float StaminaRegenDelay => staminaRegenDelay;
    public float StaggerDuration => staggerDuration;

    public float WalkSpeed => walkSpeed;
    public float TurnSpeed => turnSpeed;

    public float FrontArmorHalfAngle => frontArmorHalfAngle;
    public float FrontArmorDamageMultiplier => frontArmorDamageMultiplier;

    public float DashStaminaCost => dashStaminaCost;
    public float DashDistance => dashDistance;
    public float DashDuration => dashDuration;
    public float DashContactDamage => dashContactDamage;
    public float DashContactRange => dashContactRange;

    public float BasicComboStaminaCost => basicComboStaminaCost;
    public float BasicComboDamage => basicComboDamage;
    public float BasicComboRange => basicComboRange;
    public float BasicComboArc => basicComboArc;

    public float HeavyComboStaminaCost => heavyComboStaminaCost;
    public float HeavyComboDamage => heavyComboDamage;
    public float HeavyComboRange => heavyComboRange;
    public float HeavyComboArc => heavyComboArc;
    public float HeavyComboRecovery => heavyComboRecovery;
    public float HeavyComboPunishMultiplier => heavyComboPunishMultiplier;
    
    public float FrostTelegraphTime => frostTelegraphTime;
    public float FrostLaunchTime => frostLaunchTime;
    public float FrostRecoveryTime => frostRecoveryTime;
    public float FrostTimeBetweenSlashes => frostTimeBetweenSlashes;

    public int FrostHorizontalSlashCount => frostHorizontalSlashCount;
    public int FrostVerticalSlashCount => frostVerticalSlashCount;

    public float FrostStrafeSpeed => frostStrafeSpeed;

    public float FrostStaminaCost => frostStaminaCost;
    public float FrostDamage => frostDamage;
    public float FrostProjectileSpeed => frostProjectileSpeed;
    public float FrostProjectileRange => frostProjectileRange;
    public float FrostProjectileWidth => frostProjectileWidth;

    public float MeleeRange => meleeRange;
    public float FarRange => farRange;
    
}
