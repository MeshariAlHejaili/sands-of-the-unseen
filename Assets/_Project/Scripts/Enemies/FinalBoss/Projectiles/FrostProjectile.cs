using System;
using UnityEngine;

/// <summary>
/// Orientation of a slash projectile. Affects visual shape only — gameplay
/// (range, damage, hit width) is identical for both.
/// Horizontal: wide + short. Best vs lateral dodging.
/// Vertical:   tall + narrow. Best vs stationary / forward-back motion.
/// </summary>
public enum SlashOrientation
{
    Horizontal,
    Vertical
}

/// <summary>
/// Pooled slash projectile. Travels in a locked direction, accelerating from a slow
/// initial speed to its full speed over a brief launch window — feels like the slash
/// "bursts" off the blade then snaps forward.
///
/// Does NOT spin. A slash is a wave of energy that holds its shape; tumbling reads as
/// a thrown disc, not a sword cut.
/// </summary>
public class FrostProjectile : MonoBehaviour
{
    // Speed ramps from this fraction of full speed at launch up to 100% by RampDuration.
    private const float InitialSpeedFraction = 0.35f;
    private const float RampDuration = 0.18f;

    private Vector3 direction;
    private float maxSpeed;
    private float maxRange;
    private float hitWidth;
    private float damage;
    private PlayerHealth target;
    private SlashOrientation orientation;

    private Vector3 startPos;
    private float spawnTime;
    private bool returned;
    private Action<FrostProjectile> onReturn;

    public void Init(
        Vector3 direction,
        float speed,
        float range,
        float width,
        float damage,
        PlayerHealth target,
        Action<FrostProjectile> returnCallback,
        SlashOrientation orientation = SlashOrientation.Horizontal)
    {
        this.direction = direction.normalized;
        this.maxSpeed = speed;
        this.maxRange = range;
        this.hitWidth = width;
        this.damage = damage;
        this.target = target;
        this.onReturn = returnCallback;
        this.startPos = transform.position;
        this.spawnTime = Time.time;
        this.returned = false;
        this.orientation = orientation;

        // Slash faces edge-forward — local Z along travel direction.
        transform.rotation = Quaternion.LookRotation(this.direction);

        // Shape changes with orientation. Hit detection still uses hitWidth (XZ
        // proximity) regardless of visual scale — gameplay is identical between
        // orientations, only readability differs.
        if (orientation == SlashOrientation.Horizontal)
            transform.localScale = new Vector3(4.0f, 0.7f, 0.25f); // wide + short
        else
            transform.localScale = new Vector3(0.7f, 4.0f, 0.25f); // tall + narrow

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (returned) return;

        // Speed ramp: ease from InitialSpeedFraction up to 1.0 over RampDuration.
        // Quadratic ease-in feels punchy — slow launch, snap to max.
        float age = Time.time - spawnTime;
        float rampT = Mathf.Clamp01(age / RampDuration);
        float speedFactor = Mathf.Lerp(InitialSpeedFraction, 1f, rampT * rampT);
        float currentSpeed = maxSpeed * speedFactor;

        transform.position += direction * currentSpeed * Time.deltaTime;

        // Range expiry
        if (Vector3.Distance(startPos, transform.position) >= maxRange)
        {
            ReturnToPool();
            return;
        }

        // XZ proximity check — player's world position is in the same plane.
        if (target != null && !target.IsDead)
        {
            Vector3 a = transform.position; a.y = 0f;
            Vector3 b = target.transform.position; b.y = 0f;
            if (Vector3.Distance(a, b) <= hitWidth)
            {
                target.TakeDamage(damage);
                ReturnToPool();
            }
        }
    }

    public void ReturnToPool()
    {
        if (returned) return;
        returned = true;
        onReturn?.Invoke(this);
    }
}

