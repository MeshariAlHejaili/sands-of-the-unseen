using System;
using UnityEngine;

public class EnemyBoxAgent : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float contactDamageCooldown = 1f;
    [SerializeField] private float contactRange = 1.35f;

    [Header("Drops")]
    [SerializeField] private CurrencyOrbPickup currencyOrbPrefab;
    [SerializeField] private int currencyValue = 1;
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, 0.5f, 0f);

    private Collider[] cachedColliders;
    private Renderer[] cachedRenderers;
    private EnemyWaveSpawner ownerSpawner;
    private PlayerHealth playerHealth;
    private Transform playerTransform;
    private float currentHealth;
    private float currentMaxHealth;
    private float halfHeight = 0.5f;
    private float nextDamageTime;
    private float currentMoveSpeed;
    private float currentContactDamage;
    private int currentCurrencyValue;
    private float currentContactRange;
    private bool isAlive;

    public bool IsAlive => isAlive;

    // damage amount, current health, max health
    public event Action<float, float, float> Damaged;
    // current health, max health — fired when enemy spawns so the bar can initialise
    public event Action<float, float> Spawned;

    private void Awake()
    {
        cachedColliders = GetComponentsInChildren<Collider>(true);
        cachedRenderers = GetComponentsInChildren<Renderer>(true);

        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null)
        {
            halfHeight = mainCollider.bounds.extents.y;
        }
    }

    private void Update()
    {
        if (!isAlive || playerTransform == null || playerHealth == null || playerHealth.IsDead)
        {
            return;
        }

        MoveTowardsPlayer();
        TryDamagePlayer();
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
        playerTransform = target;
        playerHealth = targetHealth;
        ownerSpawner = spawner;
        currentMaxHealth = maxHealth * Mathf.Max(0.1f, healthMultiplier);
        currentHealth = currentMaxHealth;
        currentMoveSpeed = moveSpeed * Mathf.Max(0.1f, speedMultiplier);
        currentContactDamage = contactDamage * Mathf.Max(0.1f, damageMultiplier);
        currentCurrencyValue = Mathf.Max(1, currencyValue + bonusCurrency);
        currentContactRange = contactRange;
        nextDamageTime = 0f;
        isAlive = true;

        spawnPosition.y += halfHeight;
        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;

        SetVisualAndCollisionState(true);
        gameObject.SetActive(true);
        Spawned?.Invoke(currentHealth, currentMaxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (!isAlive || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        Damaged?.Invoke(amount, currentHealth, currentMaxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    public void ReturnToPool()
    {
        isAlive = false;
        playerTransform = null;
        playerHealth = null;
        ownerSpawner = null;
        gameObject.SetActive(false);
    }

    private void MoveTowardsPlayer()
    {
        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f;

        float sqrDistance = toPlayer.sqrMagnitude;
        if (sqrDistance <= currentContactRange * currentContactRange)
        {
            return;
        }

        Vector3 direction = toPlayer.normalized;
        transform.position += direction * currentMoveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void TryDamagePlayer()
    {
        if (Time.time < nextDamageTime)
        {
            return;
        }

        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude > currentContactRange * currentContactRange)
        {
            return;
        }

        playerHealth.TakeDamage(currentContactDamage);
        nextDamageTime = Time.time + contactDamageCooldown;
    }

    private void Die()
    {
        if (!isAlive)
        {
            return;
        }

        isAlive = false;
        SpawnCurrencyOrb();
        SetVisualAndCollisionState(false);

        if (ownerSpawner != null)
        {
            ownerSpawner.ReleaseEnemy(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void SpawnCurrencyOrb()
    {
        if (currencyOrbPrefab == null || currentCurrencyValue <= 0)
        {
            return;
        }

        CurrencyOrbPickup orb = Instantiate(currencyOrbPrefab, transform.position + dropOffset, Quaternion.identity);
        orb.SetValue(currentCurrencyValue);
    }

    private void SetVisualAndCollisionState(bool isEnabled)
    {
        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            cachedRenderers[i].enabled = isEnabled;
        }

        for (int i = 0; i < cachedColliders.Length; i++)
        {
            cachedColliders[i].enabled = isEnabled;
        }
    }
}
