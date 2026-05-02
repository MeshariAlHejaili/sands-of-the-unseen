using UnityEngine;

/// <summary>
/// Spawns/despawns the final boss and controls the final boss health bar UI.
/// </summary>
public class FinalBossSpawner : MonoBehaviour
{
    [Header("Boss Configuration")]
    [Tooltip("Final boss prefab. Should have FinalBossController + components on it.")]
    [SerializeField] private GameObject bossPrefab;

    [Tooltip("Spawn position. If null, uses this object's transform.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Spawn Trigger")]
    [SerializeField] private bool spawnOnGameStart = false;
    [Min(0f)] [SerializeField] private float spawnDelay = 0f;

    [Header("Session Integration")]
    [SerializeField] private GameSessionController sessionController;

    [Header("UI")]
    [SerializeField] private FinalBossHealthBarUI bossHealthBarUI;

    [Header("Debug - read-only")]
    [SerializeField] private GameObject currentBossInstance;
    [SerializeField] private bool hasSpawned;

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
        if (!waitingToSpawn) return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            waitingToSpawn = false;
            SpawnBoss();
        }
    }

    private void HandleStateChanged(GameSessionState state)
    {
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

        if (state != GameSessionState.Playing && currentBossInstance != null)
            DespawnBoss();
    }

    public void SpawnBoss()
    {
        if (currentBossInstance != null)
        {
            Debug.LogWarning("[FinalBossSpawner] Boss already spawned - ignoring.");
            return;
        }

        if (bossPrefab == null)
        {
            Debug.LogError("[FinalBossSpawner] No boss prefab assigned!");
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        currentBossInstance = Instantiate(bossPrefab, pos, rot);
        hasSpawned = true;

        FinalBossController bossController = currentBossInstance.GetComponent<FinalBossController>();

        if (bossHealthBarUI != null)
            bossHealthBarUI.Show(bossController);

        EnemyHealth health = currentBossInstance.GetComponent<EnemyHealth>();

        if (health != null)
            health.Died += HandleBossDied;

        Debug.Log($"[FinalBossSpawner] Boss spawned at {pos}");

        OnBossSpawned?.Invoke(currentBossInstance);
    }

    public void DespawnBoss()
    {
        if (bossHealthBarUI != null)
            bossHealthBarUI.Hide();

        if (currentBossInstance != null)
        {
            EnemyHealth health = currentBossInstance.GetComponent<EnemyHealth>();

            if (health != null)
                health.Died -= HandleBossDied;

            Destroy(currentBossInstance);
            currentBossInstance = null;

            Debug.Log("[FinalBossSpawner] Boss despawned");
        }
    }

    private void HandleBossDied()
    {
        Debug.Log("[FinalBossSpawner] Boss defeated!");

        if (bossHealthBarUI != null)
            bossHealthBarUI.Hide();

        OnBossDefeated?.Invoke();
    }

    private void OnDrawGizmos()
    {
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;

        Gizmos.color = new Color(0.4f, 0.2f, 1f, 1f);
        Gizmos.DrawWireSphere(pos, 1.5f);
        Gizmos.DrawLine(pos, pos + Vector3.up * 4f);
    }
}