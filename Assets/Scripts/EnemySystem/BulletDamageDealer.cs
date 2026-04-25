using UnityEngine;

public class BulletDamageDealer : MonoBehaviour
{
    [SerializeField] private float hitRadius = 0.12f;
    [SerializeField] private LayerMask enemyLayers = ~0;

    private Bullet bullet;
    private Vector3 previousPosition;

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
        Vector3 travel = currentPosition - previousPosition;
        float distance = travel.magnitude;

        if (distance <= Mathf.Epsilon)
        {
            previousPosition = currentPosition;
            return;
        }

        Vector3 direction = travel / distance;

        if (Physics.SphereCast(previousPosition, hitRadius, direction, out RaycastHit hit, distance, enemyLayers, QueryTriggerInteraction.Ignore))
        {
            EnemyBoxAgent enemy = hit.collider.GetComponentInParent<EnemyBoxAgent>();
            if (enemy != null && enemy.IsAlive)
            {
                float damage = bullet != null ? bullet.Damage : 0f;
                enemy.TakeDamage(damage);
                bullet?.ReturnToPool();
                return;
            }
        }

        previousPosition = currentPosition;
    }
}
