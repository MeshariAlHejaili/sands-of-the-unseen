using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene-singleton pool for frost projectiles. Same Queue&lt;T&gt; pattern as
/// PlayerShooting's bullet pool — kept consistent so anyone reading the codebase
/// recognizes the shape immediately.
///
/// Place ONE of these in the scene, assign a prefab, and the boss will find it
/// via FrostProjectilePool.Instance. Singleton-by-instance is fine for a final
/// boss arena (one boss, one pool); a fancier service-locator isn't justified here.
/// </summary>
public class FrostProjectilePool : MonoBehaviour
{
    public static FrostProjectilePool Instance { get; private set; }

    [Tooltip("Prefab with a FrostProjectile component on the root.")]
    [SerializeField] private FrostProjectile prefab;

    [Tooltip("How many projectiles to pre-create at scene start.")]
    [Min(1)] [SerializeField] private int prewarmCount = 6;

    private readonly Queue<FrostProjectile> pool = new Queue<FrostProjectile>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[FrostProjectilePool] Duplicate pool — destroying extra.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (prefab == null)
        {
            Debug.LogError("[FrostProjectilePool] No prefab assigned.", this);
            return;
        }

        for (int i = 0; i < prewarmCount; i++)
            pool.Enqueue(CreateOne());
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private FrostProjectile CreateOne()
    {
        FrostProjectile p = Instantiate(prefab, transform);
        p.gameObject.SetActive(false);
        return p;
    }

    public FrostProjectile Get()
    {
        if (pool.Count == 0)
            pool.Enqueue(CreateOne());
        return pool.Dequeue();
    }

    public void Return(FrostProjectile p)
    {
        if (p == null) return;
        p.gameObject.SetActive(false);
        pool.Enqueue(p);
    }
}
