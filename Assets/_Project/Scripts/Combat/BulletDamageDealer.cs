using UnityEngine;

public class BulletDamageDealer : MonoBehaviour
{
    private const int MaxHitResults = 16;

    [Header("Hit Detection")]
    [Tooltip("Radius in world units used by the bullet sphere cast.")]
    [Range(0f, 1f)]
    [SerializeField] private float hitRadius = 0.12f;

    [Tooltip("Physics layers that can be hit by this bullet.")]
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
        int hitCount = Physics.OverlapSphereNonAlloc(position, hitRadius, overlapResults, enemyLayers, QueryTriggerInteraction.Collide);

        for (int i = 0; i < hitCount; i++)
        {
            IDamageable damageable = GetDamageable(overlapResults[i]);
            if (TryDamageTarget(damageable))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryDamageTargetAlongPath(Vector3 origin, Vector3 direction, float distance)
    {
        int hitCount = Physics.SphereCastNonAlloc(origin, hitRadius, direction, hitResults, distance, enemyLayers, QueryTriggerInteraction.Collide);
        IDamageable nearestDamageable = null;
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            IDamageable damageable = GetDamageable(hitResults[i].collider);
            if (damageable != null && damageable.IsAlive && hitResults[i].distance < nearestDistance)
            {
                nearestDamageable = damageable;
                nearestDistance = hitResults[i].distance;
            }
        }

        return TryDamageTarget(nearestDamageable);
    }

    private bool TryDamageTarget(IDamageable damageable)
    {
        if (damageable == null || !damageable.IsAlive)
        {
            return false;
        }

        float damage = bullet != null ? bullet.Damage : 0f;
        damageable.TakeDamage(damage);
        bullet?.ReturnToPool();
        return true;
    }

    private IDamageable GetDamageable(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return null;
        }

        return hitCollider.GetComponentInParent<IDamageable>();
    }
}
