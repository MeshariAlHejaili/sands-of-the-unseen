using UnityEngine;

/// <summary>
/// Observes the player and exposes behavioral signals that the brain uses for scoring.
///
/// We don't have a "player is healing" flag in PlayerHealth, so we infer the SHAPE of
/// healing intent: low HP + stationary + not shooting. Same approach for spamming and
/// retreating — read from cheap signals, refresh at 5Hz, expose as booleans.
///
/// This is what lets the boss FEEL like it's reading the player. The brain doesn't have
/// to do any of this work — it just reads IsPlayerLikelyHealing etc.
/// </summary>
public class FinalBossPlayerSensor : MonoBehaviour
{
    private const float PollInterval = 0.2f; // 5Hz

    [Header("Heuristic Thresholds")]
    [Tooltip("Player HP fraction below which they're considered 'low'.")]
    [Range(0f, 1f)] [SerializeField] private float lowHpThreshold = 0.6f;

    [Tooltip("Seconds player must be stationary + silent to count as healing.")]
    [Min(0.1f)] [SerializeField] private float healingDwellSeconds = 0.6f;

    [Tooltip("Bullet count in the rolling window that counts as spamming.")]
    [Min(1)] [SerializeField] private int spamBulletCount = 4;

    [Tooltip("Rolling window length for spam detection.")]
    [Min(0.1f)] [SerializeField] private float spamWindowSeconds = 1.5f;

    [Tooltip("Seconds the player must be moving away to count as retreating.")]
    [Min(0.1f)] [SerializeField] private float retreatDwellSeconds = 0.8f;

    private Transform playerTransform;
    private PlayerHealth playerHealth;

    // Tracking state
    private Vector3 lastPlayerPosition;
    private float playerStationarySince = -999f;
    private float playerLastShotTime = -999f;
    private float playerRetreatingSince = -999f;
    private int bulletsInWindowCount;
    private float bulletWindowResetTime;
    private float nextPollTime;

    // Cached results (read by the brain)
    public bool IsPlayerLikelyHealing { get; private set; }
    public bool IsPlayerSpamming { get; private set; }
    public bool IsPlayerRetreating { get; private set; }
    public float PlayerHpPercent { get; private set; } = 1f;
    public float DistanceToPlayer { get; private set; }

    public void SetPlayer(Transform t, PlayerHealth health)
    {
        playerTransform = t;
        playerHealth = health;
        lastPlayerPosition = t != null ? t.position : Vector3.zero;
    }

    /// <summary>Hook this from a player-shooting event when integrated, or call manually for now.</summary>
    public void NotifyPlayerFiredBullet()
    {
        playerLastShotTime = Time.time;

        // Rolling window using a simple decay timestamp.
        if (Time.time > bulletWindowResetTime)
        {
            bulletsInWindowCount = 0;
            bulletWindowResetTime = Time.time + spamWindowSeconds;
        }
        bulletsInWindowCount++;
    }

    private void Update()
    {
        if (Time.time < nextPollTime) return;
        nextPollTime = Time.time + PollInterval;
        Poll();
    }

    private void Poll()
    {
        if (playerTransform == null) return;

        Vector3 pos = playerTransform.position;
        Vector3 frameDelta = pos - lastPlayerPosition; // before we update lastPlayerPosition
        Vector3 toPlayer = pos - transform.position;
        toPlayer.y = 0f;
        DistanceToPlayer = toPlayer.magnitude;

        if (playerHealth != null && playerHealth.MaxHealth > 0f)
            PlayerHpPercent = playerHealth.CurrentHealth / playerHealth.MaxHealth;

        // Stationary check
        bool stationary = frameDelta.sqrMagnitude < 0.0025f; // ~5cm threshold
        if (!stationary) playerStationarySince = Time.time;

        // Healing inference: low HP + stationary for a while + no recent shot
        bool stationaryLongEnough = Time.time - playerStationarySince >= healingDwellSeconds;
        bool notShooting = Time.time - playerLastShotTime > 0.4f;
        IsPlayerLikelyHealing = PlayerHpPercent < lowHpThreshold && stationaryLongEnough && notShooting;

        // Spam detection — window auto-decays when timestamp passes
        if (Time.time > bulletWindowResetTime) bulletsInWindowCount = 0;
        IsPlayerSpamming = bulletsInWindowCount >= spamBulletCount;

        // Retreat detection: player frame motion is away from boss.
        // Dot(frame motion direction, away-from-boss direction) > threshold means retreating.
        bool movingAway = false;
        if (!stationary && toPlayer.sqrMagnitude > 0.01f)
        {
            Vector3 awayFromBoss = toPlayer.normalized;
            Vector3 frameDir = frameDelta; frameDir.y = 0f;
            if (frameDir.sqrMagnitude > 0.0001f)
                movingAway = Vector3.Dot(frameDir.normalized, awayFromBoss) > 0.5f;
        }
        if (!movingAway) playerRetreatingSince = Time.time;
        IsPlayerRetreating = Time.time - playerRetreatingSince >= retreatDwellSeconds;

        lastPlayerPosition = pos;
    }
}
