using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Configuration")]
    [Tooltip("The boss prefab to spawn.")]
    [SerializeField] private GameObject bossPrefab;

    [Tooltip("Where the boss appears. If null, uses this object's position.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Spawn Trigger")]
    [Tooltip("If true, boss spawns automatically when game enters Playing state.")]
    [SerializeField] private bool spawnOnGameStart = false;

    [Tooltip("Delay in seconds after Playing state before boss spawns. Useful to let player get oriented.")]
    [SerializeField] private float spawnDelay = 0f;

    [Header("Session Integration")]
    [SerializeField] private GameSessionController sessionController;

    [Header("Debug (read-only)")]
    [SerializeField] private GameObject currentBossInstance;
    [SerializeField] private bool hasSpawned = false;
    [Header("Boss UI")]
    [SerializeField] private BossHealthBarUI bossHealthBarUI;

    public GameObject CurrentBoss => currentBossInstance;
    public bool HasSpawned => hasSpawned;
    public bool IsBossAlive => currentBossInstance != null;

    public System.Action<GameObject> OnBossSpawned;
    public System.Action OnBossDefeated;

    private float spawnTimer;
    private bool waitingToSpawn;

    private void Awake()
    {
        if (sessionController == null)
            sessionController = FindFirstObjectByType<GameSessionController>();
        if (bossHealthBarUI == null)
            bossHealthBarUI = FindFirstObjectByType<BossHealthBarUI>(FindObjectsInactive.Include);
    }

    private void OnEnable()
    {
        if (sessionController != null)
            sessionController.StateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (sessionController != null)
            sessionController.StateChanged -= HandleStateChanged;
    }

    private void Update()
    {
        if (waitingToSpawn)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                waitingToSpawn = false;
                SpawnBoss();
            }
        }
    }

    private void HandleStateChanged(GameSessionState state)
    {
        // Only auto-spawn on game start if enabled
        if (spawnOnGameStart && state == GameSessionState.Playing && !hasSpawned)
        {
            if (spawnDelay > 0f)
            {
                waitingToSpawn = true;
                spawnTimer = spawnDelay;
            }
            else
            {
                SpawnBoss();
            }
        }

        // Clean up boss if game returns to menu / victory / defeat
        if (state != GameSessionState.Playing && currentBossInstance != null)
        {
            DespawnBoss();
        }
    }

    /// <summary>
    /// Public method to manually spawn the boss. Hook this to wave system, button, etc.
    /// </summary>
    public void SpawnBoss()
    {
        if (currentBossInstance != null)
        {
            Debug.LogWarning("[BossSpawner] Boss already spawned — ignoring spawn request.");
            return;
        }

        if (bossPrefab == null)
        {
            Debug.LogError("[BossSpawner] No boss prefab assigned!");
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        currentBossInstance = Instantiate(bossPrefab, pos, rot);
        BossController bossController = currentBossInstance.GetComponent<BossController>();

        if (bossHealthBarUI != null && bossController != null)
        {
            bossHealthBarUI.SetBoss(bossController);
        }
        hasSpawned = true;

        // Hook into death event so we know when boss is defeated
        var bossHealth = currentBossInstance.GetComponent<EnemyHealth>();
        if (bossHealth != null)
        {
            bossHealth.Died += HandleBossDied;
        }

        Debug.Log($"[BossSpawner] Boss spawned at {pos}");
        OnBossSpawned?.Invoke(currentBossInstance);
    }

    public void DespawnBoss()
    {
        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.ClearBoss();
        }
        if (currentBossInstance != null)
        {
            Destroy(currentBossInstance);
            currentBossInstance = null;
            Debug.Log("[BossSpawner] Boss despawned");
        }
    }

    private void HandleBossDied()
    {
        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.ClearBoss();
        }
        Debug.Log("[BossSpawner] Boss defeated!");
        OnBossDefeated?.Invoke();
        // Note: we don't destroy here — let the death animation play out.
        // The session controller can transition to Victory state.
    }

    // Visualize the spawn point in scene view
    private void OnDrawGizmos()
    {
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(pos, 1.5f);
        Gizmos.DrawLine(pos, pos + Vector3.up * 4f);
    }
}