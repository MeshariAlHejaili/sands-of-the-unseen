using UnityEngine;

public class EnemyBoxAgent : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float contactDamageCooldown = 1f;
    [SerializeField] private float contactRange = 1.35f;

    [Header("Drops")]
    [SerializeField] private CurrencyOrbPickup currencyOrbPrefab;
    [SerializeField] private int currencyValue = 1;
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, 0.5f, 0f);

    private EnemyHealth enemyHealth;
    private EnemyAI enemyAI;
    private Collider[] cachedColliders;
    private Renderer[] cachedRenderers;
    private EnemyWaveSpawner ownerSpawner;
    private float halfHeight = 0.5f;
    private int currentCurrencyValue;

    public bool IsAlive => enemyHealth != null && !enemyHealth.IsDead;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyAI = GetComponent<EnemyAI>();
        cachedColliders = GetComponentsInChildren<Collider>(true);
        cachedRenderers = GetComponentsInChildren<Renderer>(true);

        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null)
            halfHeight = mainCollider.bounds.extents.y;

        if (enemyHealth != null)
            enemyHealth.Died += OnEnemyDied;
        else
            Debug.LogWarning("EnemyBoxAgent: EnemyHealth component missing on this prefab.", this);
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
            enemyHealth.Died -= OnEnemyDied;
    }

    public void TakeDamage(float amount)
    {
        enemyHealth?.TakeDamage(amount);
    }

    public void Spawn(
        Transform target,
        PlayerHealth targetHealth,
        Vector3 spawnPosition,
        EnemyWaveSpawner spawner,
        float healthMultiplier,
        float damageMultiplier,
        float speedMultiplier,
        int bonusCurrency)
    {
        ownerSpawner = spawner;
        currentCurrencyValue = Mathf.Max(1, currencyValue + bonusCurrency);

        enemyHealth.Init(maxHealth * Mathf.Max(0.1f, healthMultiplier));
        enemyAI.Init(
            target,
            targetHealth,
            moveSpeed * Mathf.Max(0.1f, speedMultiplier),
            contactDamage * Mathf.Max(0.1f, damageMultiplier),
            contactRange,
            contactDamageCooldown);

        spawnPosition.y += halfHeight;
        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;

        SetVisualAndCollisionState(true);
        gameObject.SetActive(true);
    }

    public void ReturnToPool()
    {
        enemyAI?.ResetState();
        ownerSpawner = null;
        gameObject.SetActive(false);
    }

    private void OnEnemyDied()
    {
        SpawnCurrencyOrb();
        SetVisualAndCollisionState(false);
        enemyAI?.ResetState();

        if (ownerSpawner != null)
            ownerSpawner.ReleaseEnemy(this);
        else
            gameObject.SetActive(false);
    }

    private void SpawnCurrencyOrb()
    {
        if (currencyOrbPrefab == null || currentCurrencyValue <= 0) return;

        CurrencyOrbPickup orb = Instantiate(currencyOrbPrefab, transform.position + dropOffset, Quaternion.identity);
        orb.SetValue(currentCurrencyValue);
    }

    private void SetVisualAndCollisionState(bool isEnabled)
    {
        for (int i = 0; i < cachedRenderers.Length; i++)
            cachedRenderers[i].enabled = isEnabled;

        for (int i = 0; i < cachedColliders.Length; i++)
            cachedColliders[i].enabled = isEnabled;
    }
}
