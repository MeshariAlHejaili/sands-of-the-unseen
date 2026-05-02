using UnityEngine;

public class BossChainProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float maxRange;
    private float hitWidth;
    private float damage;
    private PlayerHealth target;

    private Vector3 startPos;
    private bool hasHit;

    public void Init(Vector3 dir, float spd, float range, float width, float dmg, PlayerHealth player)
    {
        direction = dir.normalized;
        speed = spd;
        maxRange = range;
        hitWidth = width;
        damage = dmg;
        target = player;
        startPos = transform.position;

        // Auto-destroy after enough time even if it misses
        Destroy(gameObject, range / spd + 1f);
    }

    void Update()
    {
        // Move forward
        transform.position += direction * speed * Time.deltaTime;

        // Check distance traveled
        float traveled = Vector3.Distance(startPos, transform.position);
        if (traveled >= maxRange)
        {
            Destroy(gameObject);
            return;
        }

        // Spinning visual
        transform.Rotate(Vector3.up, 720f * Time.deltaTime, Space.World);

        // Check collision with player
        if (!hasHit && target != null && !target.IsDead)
        {
            float distToPlayer = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(target.transform.position.x, 0, target.transform.position.z));

            if (distToPlayer <= hitWidth)
            {
                target.TakeDamage(damage);
                hasHit = true;
                Debug.Log("[Boss] Hurl projectile HIT player");
                Destroy(gameObject);
            }
        }
    }
}