using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private EnemyHealth enemyHealth;
    private Transform playerTransform;
    private PlayerHealth playerHealth;
    private float moveSpeed;
    private float contactDamage;
    private float contactRange;
    private float contactCooldown;
    private float nextDamageTime;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    public void Init(Transform player, PlayerHealth ph, float speed, float damage, float range, float cooldown)
    {
        playerTransform = player;
        playerHealth = ph;
        moveSpeed = speed;
        contactDamage = damage;
        contactRange = range;
        contactCooldown = cooldown;
        nextDamageTime = 0f;
    }

    public void ResetState()
    {
        playerTransform = null;
        playerHealth = null;
        nextDamageTime = 0f;
    }

    private void Update()
    {
        if (enemyHealth == null || enemyHealth.IsDead) return;
        if (playerTransform == null || playerHealth == null || playerHealth.IsDead) return;

        MoveTowardsPlayer();
        TryDamagePlayer();
    }

    private void MoveTowardsPlayer()
    {
        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude <= contactRange * contactRange) return;

        Vector3 direction = toPlayer.normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void TryDamagePlayer()
    {
        if (Time.time < nextDamageTime) return;

        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude > contactRange * contactRange) return;

        playerHealth.TakeDamage(contactDamage);
        nextDamageTime = Time.time + contactCooldown;
    }
}
