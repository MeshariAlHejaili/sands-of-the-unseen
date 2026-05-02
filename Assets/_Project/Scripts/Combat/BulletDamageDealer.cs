using UnityEngine;

public class BulletDamageDealer : MonoBehaviour
{
    private const int MaxHitResults = 16;

    [Header("Hit Detection")]
    [Range(0f, 1f)]
    [SerializeField] private float hitRadius = 0.12f;

    [SerializeField] private LayerMask enemyLayers = ~0;

    private Bullet bullet;
    private Vector3 previousPosition;

    private readonly RaycastHit[] hitResults = new RaycastHit[MaxHitResults];
    private readonly Collider[] overlapResults = new Collider[MaxHitResults];

    private void Awake()
    {
        bullet = GetComponent<Bullet>();
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        Vector3 currentPosition = transform.position;

        if (TryDamageOverlappingTarget(previousPosition))
            return;

        Vector3 travel = currentPosition - previousPosition;
        float distance = travel.magnitude;

        if (distance <= Mathf.Epsilon)
        {
            previousPosition = currentPosition;
            return;
        }

        Vector3 direction = travel / distance;

        if (TryDamageTargetAlongPath(previousPosition, direction, distance))
            return;

        previousPosition = currentPosition;
    }

    private bool TryDamageOverlappingTarget(Vector3 position)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            position,
            hitRadius,
            overlapResults,
            enemyLayers,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hitCount; i++)
        {
            if (TryDamageCollider(overlapResults[i]))
                return true;
        }

        return false;
    }

    private bool TryDamageTargetAlongPath(Vector3 origin, Vector3 direction, float distance)
    {
        int hitCount = Physics.SphereCastNonAlloc(
            origin,
            hitRadius,
            direction,
            hitResults,
            distance,
            enemyLayers,
            QueryTriggerInteraction.Ignore
        );

        Collider nearestCollider = null;
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            if (hitResults[i].collider == null)
                continue;

            if (hitResults[i].distance < nearestDistance)
            {
                nearestCollider = hitResults[i].collider;
                nearestDistance = hitResults[i].distance;
            }
        }

        return TryDamageCollider(nearestCollider);
    }

    private bool TryDamageCollider(Collider hitCollider)
    {
        if (hitCollider == null)
            return false;

        float damage = bullet != null ? bullet.Damage : 0f;

        EnemyBoxAgent enemy = hitCollider.GetComponentInParent<EnemyBoxAgent>();
        if (enemy != null && enemy.IsAlive)
        {
            enemy.TakeDamage(damage);
            bullet?.ReturnToPool();
            return true;
        }

        // Final boss — must be checked BEFORE the generic EnemyHealth path so the
        // armor/punish-window pipeline runs. Also checked before mini-boss BossHitbox
        // so a final-boss prefab that ALSO has a stray BossHitbox component routes
        // through the right system.
        FinalBossHitbox finalBossHitbox = hitCollider.GetComponentInParent<FinalBossHitbox>();
        if (finalBossHitbox != null)
        {
            // Use the bullet's current world position as the hit point so the armor
            // system can compute the correct angle.
            finalBossHitbox.TakeDamage(damage, transform.position);
            bullet?.ReturnToPool();
            return true;
        }

        BossHitbox bossHitbox = hitCollider.GetComponentInParent<BossHitbox>();
        if (bossHitbox != null)
        {
            bossHitbox.TakeDamage(damage);
            bullet?.ReturnToPool();
            return true;
        }

        EnemyHealth bossHealth = hitCollider.GetComponentInParent<EnemyHealth>();
        if (bossHealth != null && !bossHealth.IsDead)
        {
            Debug.Log($"[Bullet] Direct boss hit for {damage} damage.");
            bossHealth.TakeDamage(damage);
            bullet?.ReturnToPool();
            return true;
        }

        return false;
    }
}
