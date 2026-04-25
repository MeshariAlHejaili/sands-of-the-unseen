using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyBoxAgent enemyPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Wave Settings")]
    [SerializeField] private int startingEnemiesPerWave = 3;
    [SerializeField] private int enemiesPerWaveGrowth = 1;
    [SerializeField] private float initialDelay = 1f;
    [SerializeField] private float delayBetweenWaves = 3f;

    [Header("Power Scaling")]
    [SerializeField] private float healthGrowthPerWave = 0.2f;
    [SerializeField] private float damageGrowthPerWave = 0.15f;
    [SerializeField] private float speedGrowthPerWave = 0.05f;
    [SerializeField] private int currencyGrowthEveryWave = 0;

    [Header("Pooling")]
    [SerializeField] private int initialPoolSize = 6;

    private readonly HashSet<EnemyBoxAgent> activeEnemies = new HashSet<EnemyBoxAgent>();
    private readonly Queue<EnemyBoxAgent> pooledEnemies = new Queue<EnemyBoxAgent>();

    private PlayerHealth playerHealth;
    private PlayerHealth subscribedHealth;
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
