using UnityEngine;

public class BossSpawnDebugTrigger : MonoBehaviour
{
    [SerializeField] private BossSpawner spawner;
    [SerializeField] private KeyCode spawnKey = KeyCode.B;
    [SerializeField] private KeyCode despawnKey = KeyCode.N;

    private void Awake()
    {
        if (spawner == null) spawner = FindFirstObjectByType<BossSpawner>();
    }

    private void Update()
    {
        if (spawner == null) return;

        if (Input.GetKeyDown(spawnKey))
        {
            spawner.SpawnBoss();
        }
        if (Input.GetKeyDown(despawnKey))
        {
            spawner.DespawnBoss();
        }
    }
}