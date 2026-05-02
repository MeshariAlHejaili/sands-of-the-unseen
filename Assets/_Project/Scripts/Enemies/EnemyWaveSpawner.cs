using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// LOCKED FOUNDATION FILE: after Issue #0, only lead may edit. Open a needs-lead issue for changes.
public class EnemyWaveSpawner : MonoBehaviour
{
    [System.Serializable]
    private sealed class EnemySpawnEntry
    {
        [Tooltip("Additional enemy prefab that can appear in regular waves.")]
        [SerializeField] private EnemyBoxAgent prefab;

        [Tooltip("Relative spawn weight from 0 to 1. A value of 0 keeps this entry disabled.")]
        [Range(0f, 1f)]
        [SerializeField] private float spawnWeight;

        public EnemyBoxAgent Prefab => prefab;
        public float SpawnWeight => spawnWeight;
    }

    [Header("References")]
    [Tooltip("Default melee enemy prefab spawned when no additional enemy entry is selected.")]
    [SerializeField] private EnemyBoxAgent enemyPrefab;

    [Tooltip("Additional enemy prefabs and spawn weights used for future enemy types such as ranged enemies.")]
    [SerializeField] private EnemySpawnEntry[] additionalEnemyPrefabs;

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
    private readonly Dictionary<EnemyBoxAgent, Queue<EnemyBoxAgent>> pooledEnemiesByPrefab = new Dictionary<EnemyBoxAgent, Queue<EnemyBoxAgent>>();
    private readonly Dictionary<EnemyBoxAgent, EnemyBoxAgent> prefabByEnemy = new Dictionary<EnemyBoxAgent, EnemyBoxAgent>();
    private readonly List<EnemyBoxAgent> clearBuffer = new List<EnemyBoxAgent>();
    private readonly Queue<CurrencyOrbPickup> pooledCurrencyOrbs = new Queue<CurrencyOrbPickup>();
    private readonly HashSet<CurrencyOrbPickup> activeCurrencyOrbs = new HashSet<CurrencyOrbPickup>();

    private PlayerHealth playerHealth;
    private PlayerHealth subscribedHealth;
    private CurrencyOrbPickup currencyOrbPrefab;
    private bool playerDead;
    private bool isSpawningStopped;
    private int currentWave;
    private Coroutine waveRoutine;

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
        waveRoutine = StartCoroutine(WaveLoop());
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

    public void StopSpawning()
    {
        isSpawningStopped = true;

        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
            waveRoutine = null;
        }
    }

    public void ClearActiveEnemies()
    {
        clearBuffer.Clear();
        clearBuffer.AddRange(activeEnemies);

        for (int i = 0; i < clearBuffer.Count; i++)
        {
            EnemyBoxAgent enemy = clearBuffer[i];
            if (enemy == null)
            {
                continue;
            }

            activeEnemies.Remove(enemy);
            enemy.ReturnToPool();
            EnqueueEnemy(enemy);
        }

        clearBuffer.Clear();
    }

    public void SpawnChildWave(int count, Vector3 center, float radius)
    {
        if (count <= 0)
        {
            return;
        }

        ResolvePlayer();
        if (player == null || playerHealth == null || playerDead)
        {
            return;
        }

        float healthMultiplier = GetWaveMultiplier(healthGrowthPerWave);
        float damageMultiplier = GetWaveMultiplier(damageGrowthPerWave);
        float speedMultiplier = GetWaveMultiplier(speedGrowthPerWave);
        int bonusCurrency = Mathf.Max(0, (currentWave - 1) * currencyGrowthEveryWave);

        EnsurePoolSize(enemyPrefab, count);

        for (int i = 0; i < count; i++)
        {
            EnemyBoxAgent enemy = GetEnemyFromPool(enemyPrefab);
            if (enemy == null)
            {
                continue;
            }

            activeEnemies.Add(enemy);
            enemy.Spawn(
                player,
                playerHealth,
                GetChildSpawnPosition(center, radius),
                this,
                healthMultiplier,
                damageMultiplier,
                speedMultiplier,
                bonusCurrency);
        }
    }

    public void ReleaseEnemy(EnemyBoxAgent enemy)
    {
        if (enemy == null || !activeEnemies.Remove(enemy))
            return;

        enemy.ReturnToPool();
        EnqueueEnemy(enemy);
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

        while (!isSpawningStopped)
        {
            ResolvePlayer();
            if (player == null || playerHealth == null || playerDead)
            {
                yield return null;
                continue;
            }

            currentWave++;
            SpawnWave(GetEnemyCountForWave(currentWave));

            while (!isSpawningStopped && activeEnemies.Count > 0)
                yield return null;

            if (delayBetweenWaves > 0f)
                yield return new WaitForSeconds(delayBetweenWaves);
        }
    }

    private void PrewarmPool()
    {
        int requiredCount = Mathf.Max(initialPoolSize, startingEnemiesPerWave);
        EnsurePoolSize(enemyPrefab, requiredCount);

        if (additionalEnemyPrefabs == null)
        {
            return;
        }

        for (int i = 0; i < additionalEnemyPrefabs.Length; i++)
        {
            if (!IsValidSpawnEntry(additionalEnemyPrefabs[i]))
            {
                continue;
            }

            int requiredAdditionalCount = Mathf.CeilToInt(requiredCount * additionalEnemyPrefabs[i].SpawnWeight);
            EnsurePoolSize(additionalEnemyPrefabs[i].Prefab, requiredAdditionalCount);
        }
    }

    private void SpawnWave(int enemyCount)
    {
        EnsurePoolSize(enemyPrefab, enemyCount);

        float healthMultiplier = GetWaveMultiplier(healthGrowthPerWave);
        float damageMultiplier = GetWaveMultiplier(damageGrowthPerWave);
        float speedMultiplier = GetWaveMultiplier(speedGrowthPerWave);
        int bonusCurrency = Mathf.Max(0, (currentWave - 1) * currencyGrowthEveryWave);

        for (int i = 0; i < enemyCount; i++)
        {
            EnemyBoxAgent selectedPrefab = GetPrefabForRegularSpawn();
            EnemyBoxAgent enemy = GetEnemyFromPool(selectedPrefab);
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

    private void EnsurePoolSize(EnemyBoxAgent prefab, int requiredAvailable)
    {
        if (prefab == null)
        {
            return;
        }

        Queue<EnemyBoxAgent> pool = GetPool(prefab);
        int missingCount = requiredAvailable - pool.Count;
        for (int i = 0; i < missingCount; i++)
        {
            EnemyBoxAgent enemy = Instantiate(prefab, transform.position, Quaternion.identity, transform);
            prefabByEnemy[enemy] = prefab;
            enemy.ReturnToPool();
            pool.Enqueue(enemy);
        }
    }

    private EnemyBoxAgent GetEnemyFromPool(EnemyBoxAgent prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        Queue<EnemyBoxAgent> pool = GetPool(prefab);

        if (pool.Count == 0)
            EnsurePoolSize(prefab, 1);

        return pool.Count > 0 ? pool.Dequeue() : null;
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

    private Vector3 GetChildSpawnPosition(Vector3 center, float radius)
    {
        if (radius <= 0f)
        {
            return center;
        }

        Vector2 offset = Random.insideUnitCircle * radius;
        return center + new Vector3(offset.x, 0f, offset.y);
    }

    private EnemyBoxAgent GetPrefabForRegularSpawn()
    {
        if (additionalEnemyPrefabs == null || additionalEnemyPrefabs.Length == 0)
        {
            return enemyPrefab;
        }

        float totalWeight = 1f;
        for (int i = 0; i < additionalEnemyPrefabs.Length; i++)
        {
            if (IsValidSpawnEntry(additionalEnemyPrefabs[i]))
            {
                totalWeight += additionalEnemyPrefabs[i].SpawnWeight;
            }
        }

        float roll = Random.value * totalWeight;
        if (roll <= 1f)
        {
            return enemyPrefab;
        }

        roll -= 1f;
        for (int i = 0; i < additionalEnemyPrefabs.Length; i++)
        {
            if (!IsValidSpawnEntry(additionalEnemyPrefabs[i]))
            {
                continue;
            }

            if (roll <= additionalEnemyPrefabs[i].SpawnWeight)
            {
                return additionalEnemyPrefabs[i].Prefab;
            }

            roll -= additionalEnemyPrefabs[i].SpawnWeight;
        }

        return enemyPrefab;
    }

    private void EnqueueEnemy(EnemyBoxAgent enemy)
    {
        if (!prefabByEnemy.TryGetValue(enemy, out EnemyBoxAgent prefab))
        {
            prefab = enemyPrefab;
        }

        GetPool(prefab).Enqueue(enemy);
    }

    private Queue<EnemyBoxAgent> GetPool(EnemyBoxAgent prefab)
    {
        if (!pooledEnemiesByPrefab.TryGetValue(prefab, out Queue<EnemyBoxAgent> pool))
        {
            pool = new Queue<EnemyBoxAgent>();
            pooledEnemiesByPrefab[prefab] = pool;
        }

        return pool;
    }

    private bool IsValidSpawnEntry(EnemySpawnEntry entry)
    {
        return entry != null && entry.Prefab != null && entry.SpawnWeight > 0f;
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
