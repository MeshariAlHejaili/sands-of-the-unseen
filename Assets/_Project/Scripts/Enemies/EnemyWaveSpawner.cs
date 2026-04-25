using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Enemy prefab spawned and pooled by this wave spawner.")]
    [SerializeField] private EnemyBoxAgent enemyPrefab;

    [Tooltip("Player transform used as the enemy target; if empty, it is resolved by the Player tag.")]
    [SerializeField] private Transform player;

    [Tooltip("Fixed world spawn points used to distribute enemies around the arena.")]
    [SerializeField] private Transform[] spawnPoints;

    [Space]
    [Header("Wave Settings")]
    [Tooltip("Number of enemies spawned in the first wave.")]
    [Min(1)]
    [SerializeField] private int startingEnemiesPerWave = 3;

    [Tooltip("Additional enemies added to each new wave.")]
    [Min(0)]
    [SerializeField] private int enemiesPerWaveGrowth = 1;

    [Tooltip("Delay in seconds before the first wave starts.")]
    [Min(0f)]
    [SerializeField] private float initialDelay = 1f;

    [Tooltip("Delay in seconds after a wave is cleared before the next wave starts.")]
    [Min(0f)]
    [SerializeField] private float delayBetweenWaves = 3f;

    [Space]
    [Header("Power Scaling")]
    [Tooltip("Additional enemy health multiplier added per wave, expressed as a decimal fraction.")]
    [Min(0f)]
    [SerializeField] private float healthGrowthPerWave = 0.2f;

    [Tooltip("Additional enemy damage multiplier added per wave, expressed as a decimal fraction.")]
    [Min(0f)]
    [SerializeField] private float damageGrowthPerWave = 0.15f;

    [Tooltip("Additional enemy speed multiplier added per wave, expressed as a decimal fraction.")]
    [Min(0f)]
    [SerializeField] private float speedGrowthPerWave = 0.05f;

    [Tooltip("Flat bonus currency added to enemy drops for each wave survived.")]
    [Min(0)]
    [SerializeField] private int currencyGrowthEveryWave = 0;

    [Space]
    [Header("Pooling")]
    [Tooltip("Number of enemies created in the initial pool before waves begin.")]
    [Min(0)]
    [SerializeField] private int initialPoolSize = 6;

    [Tooltip("Number of currency orbs created in the initial orb pool once an orb prefab is known.")]
    [Min(0)]
    [SerializeField] private int initialCurrencyOrbPoolSize = 12;

    private readonly HashSet<EnemyBoxAgent> activeEnemies = new HashSet<EnemyBoxAgent>();
    private readonly Queue<EnemyBoxAgent> pooledEnemies = new Queue<EnemyBoxAgent>();
    private readonly Queue<CurrencyOrbPickup> pooledCurrencyOrbs = new Queue<CurrencyOrbPickup>();
    private readonly HashSet<CurrencyOrbPickup> activeCurrencyOrbs = new HashSet<CurrencyOrbPickup>();

    private PlayerHealth playerHealth;
    private PlayerHealth subscribedHealth;
    private CurrencyOrbPickup currencyOrbPrefab;
    private bool playerDead;
    private int currentWave;

    private void Start()
    {
        ResolvePlayer();

        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemyWaveSpawner needs an enemy prefab.", this);
            enabled = false;
            return;
        }

        PrewarmPool();
        StartCoroutine(WaveLoop());
    }

    private void OnDestroy()
    {
        if (subscribedHealth != null)
            subscribedHealth.Died -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        playerDead = true;
    }

    public void ReleaseEnemy(EnemyBoxAgent enemy)
    {
        if (enemy == null || !activeEnemies.Remove(enemy))
            return;

        enemy.ReturnToPool();
        pooledEnemies.Enqueue(enemy);
    }

    public void SpawnCurrencyOrb(CurrencyOrbPickup prefab, Vector3 position, int value)
    {
        if (prefab == null) return;

        CacheCurrencyOrbPrefab(prefab);

        CurrencyOrbPickup orb = GetCurrencyOrbFromPool();
        if (orb == null) return;

        activeCurrencyOrbs.Add(orb);
        orb.transform.SetPositionAndRotation(position, Quaternion.identity);
        orb.Init(value, ReleaseCurrencyOrb);
        orb.gameObject.SetActive(true);
    }

    private IEnumerator WaveLoop()
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            ResolvePlayer();
            if (player == null || playerHealth == null || playerDead)
            {
                yield return null;
                continue;
            }

            currentWave++;
            SpawnWave(GetEnemyCountForWave(currentWave));

            while (activeEnemies.Count > 0)
                yield return null;

            if (delayBetweenWaves > 0f)
                yield return new WaitForSeconds(delayBetweenWaves);
        }
    }

    private void PrewarmPool()
    {
        int requiredCount = Mathf.Max(initialPoolSize, startingEnemiesPerWave);
        EnsurePoolSize(requiredCount);
    }

    private void SpawnWave(int enemyCount)
    {
        EnsurePoolSize(enemyCount);

        float healthMultiplier = GetWaveMultiplier(healthGrowthPerWave);
        float damageMultiplier = GetWaveMultiplier(damageGrowthPerWave);
        float speedMultiplier = GetWaveMultiplier(speedGrowthPerWave);
        int bonusCurrency = Mathf.Max(0, (currentWave - 1) * currencyGrowthEveryWave);

        for (int i = 0; i < enemyCount; i++)
        {
            EnemyBoxAgent enemy = GetEnemyFromPool();
            if (enemy == null) continue;

            activeEnemies.Add(enemy);
            enemy.Spawn(
                player,
                playerHealth,
                GetSpawnPosition(i),
                this,
                healthMultiplier,
                damageMultiplier,
                speedMultiplier,
                bonusCurrency);
        }
    }

    private void EnsurePoolSize(int requiredAvailable)
    {
        int missingCount = requiredAvailable - pooledEnemies.Count;
        for (int i = 0; i < missingCount; i++)
        {
            EnemyBoxAgent enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity, transform);
            enemy.ReturnToPool();
            pooledEnemies.Enqueue(enemy);
        }
    }

    private EnemyBoxAgent GetEnemyFromPool()
    {
        if (pooledEnemies.Count == 0)
            EnsurePoolSize(1);

        return pooledEnemies.Count > 0 ? pooledEnemies.Dequeue() : null;
    }

    private void CacheCurrencyOrbPrefab(CurrencyOrbPickup prefab)
    {
        if (currencyOrbPrefab != null) return;

        currencyOrbPrefab = prefab;
        EnsureCurrencyOrbPoolSize(initialCurrencyOrbPoolSize);
    }

    private void EnsureCurrencyOrbPoolSize(int requiredAvailable)
    {
        if (currencyOrbPrefab == null) return;

        int missingCount = requiredAvailable - pooledCurrencyOrbs.Count;
        for (int i = 0; i < missingCount; i++)
        {
            CurrencyOrbPickup orb = Instantiate(currencyOrbPrefab, transform.position, Quaternion.identity, transform);
            orb.gameObject.SetActive(false);
            pooledCurrencyOrbs.Enqueue(orb);
        }
    }

    private CurrencyOrbPickup GetCurrencyOrbFromPool()
    {
        if (pooledCurrencyOrbs.Count == 0)
            EnsureCurrencyOrbPoolSize(1);

        return pooledCurrencyOrbs.Count > 0 ? pooledCurrencyOrbs.Dequeue() : null;
    }

    private void ReleaseCurrencyOrb(CurrencyOrbPickup orb)
    {
        if (orb == null || !activeCurrencyOrbs.Remove(orb))
            return;

        orb.gameObject.SetActive(false);
        pooledCurrencyOrbs.Enqueue(orb);
    }

    private Vector3 GetSpawnPosition(int spawnIndex)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int index = (currentWave + spawnIndex - 1) % spawnPoints.Length;
            return spawnPoints[index].position;
        }

        return transform.position;
    }

    private int GetEnemyCountForWave(int waveNumber)
    {
        return Mathf.Max(1, startingEnemiesPerWave + ((waveNumber - 1) * enemiesPerWaveGrowth));
    }

    private float GetWaveMultiplier(float growthPerWave)
    {
        return 1f + (Mathf.Max(0, currentWave - 1) * Mathf.Max(0f, growthPerWave));
    }

    private void ResolvePlayer()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(GameTags.Player);
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player != null && playerHealth == null)
            playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null && subscribedHealth != playerHealth)
        {
            if (subscribedHealth != null)
                subscribedHealth.Died -= OnPlayerDied;

            subscribedHealth = playerHealth;
            subscribedHealth.Died += OnPlayerDied;
            playerDead = playerHealth.IsDead;
        }
    }
}
