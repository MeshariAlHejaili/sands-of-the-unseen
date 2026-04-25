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

        if (TryDamageOverlappingEnemy(previousPosition))
            return;

        Vector3 travel = currentPosition - previousPosition;
        float distance = travel.magnitude;

        if (distance <= Mathf.Epsilon)
        {
            previousPosition = currentPosition;
            return;
        }

        Vector3 direction = travel / distance;

        if (TryDamageEnemyAlongPath(previousPosition, direction, distance))
            return;

        previousPosition = currentPosition;
    }

    private bool TryDamageOverlappingEnemy(Vector3 position)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(position, hitRadius, overlapResults, enemyLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            EnemyBoxAgent enemy = overlapResults[i].GetComponentInParent<EnemyBoxAgent>();
            if (TryDamageEnemy(enemy))
                return true;
        }

        return false;
    }

    private bool TryDamageEnemyAlongPath(Vector3 origin, Vector3 direction, float distance)
    {
        int hitCount = Physics.SphereCastNonAlloc(origin, hitRadius, direction, hitResults, distance, enemyLayers, QueryTriggerInteraction.Ignore);
        EnemyBoxAgent nearestEnemy = null;
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            EnemyBoxAgent enemy = hitResults[i].collider.GetComponentInParent<EnemyBoxAgent>();
            if (enemy != null && enemy.IsAlive && hitResults[i].distance < nearestDistance)
            {
                nearestEnemy = enemy;
                nearestDistance = hitResults[i].distance;
            }
        }

        return TryDamageEnemy(nearestEnemy);
    }

    private bool TryDamageEnemy(EnemyBoxAgent enemy)
    {
        if (enemy == null || !enemy.IsAlive)
            return false;

        float damage = bullet != null ? bullet.Damage : 0f;
        enemy.TakeDamage(damage);
        bullet?.ReturnToPool();
        return true;
    }
}
