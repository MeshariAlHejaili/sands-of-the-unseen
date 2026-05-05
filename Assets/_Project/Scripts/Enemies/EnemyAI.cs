using UnityEngine;

public class EnemyAI : MonoBehaviour, IEnemyBehaviour
{
    private EnemyHealth enemyHealth;
    private EnemyObstacleMotor obstacleMotor;
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
        obstacleMotor = GetComponent<EnemyObstacleMotor>();
        if (obstacleMotor == null)
        {
            obstacleMotor = gameObject.AddComponent<EnemyObstacleMotor>();
        }
    }

    public void Init(EnemyStatsContext statsContext)
    {
        playerTransform = statsContext.Target;
        playerHealth = statsContext.TargetHealth;
        moveSpeed = statsContext.MoveSpeed;
        contactDamage = statsContext.ContactDamage;
        contactRange = statsContext.ContactRange;
        contactCooldown = statsContext.ContactDamageCooldown;
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
        Vector3 displacement = direction * moveSpeed * Time.deltaTime;
        obstacleMotor.Move(displacement, true);
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
