using UnityEngine;

/// <summary>
/// Dev hotkey to spawn/despawn the final boss. Same shape as BossSpawnDebugTrigger.
/// </summary>
public class FinalBossSpawnDebugTrigger : MonoBehaviour
{
    [SerializeField] private FinalBossSpawner spawner;
    [SerializeField] private KeyCode spawnKey = KeyCode.V;
    [SerializeField] private KeyCode despawnKey = KeyCode.C;

    private void Awake()
    {
        if (spawner == null) spawner = FindFirstObjectByType<FinalBossSpawner>();
    }

    private void Update()
    {
        if (spawner == null) return;
        if (Input.GetKeyDown(spawnKey)) spawner.SpawnBoss();
        if (Input.GetKeyDown(despawnKey)) spawner.DespawnBoss();
    }
}
