using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Generic pool for one-shot ParticleSystem VFX (impacts, hits, soul collection bursts, etc.).
/// Fixes the "object not cleaned up" warning that comes from Instantiate + delayed Destroy
/// patterns, and eliminates per-spawn GC allocation.
///
/// Usage from any script:
///     VFXPool.Instance.Play(myImpactPrefab, hitPosition, hitRotation);
///
/// Each unique prefab gets its own internal pool. Prewarm/capacity tunable per pool.
///
/// NOT used by EnergyTrailDashEffect — that effect doesn't instantiate, it toggles components.
/// This pool exists for the rest of the game (combat impacts, soul pickups, footstep dust later, etc.).
/// </summary>
public class VFXPool : MonoBehaviour
{
    public static VFXPool Instance { get; private set; }

    [Tooltip("Default capacity per prefab pool. Tune up if a specific effect spawns in bursts.")]
    [SerializeField] private int defaultCapacity = 16;

    [Tooltip("Hard cap per prefab pool. Excess instances are destroyed instead of returned.")]
    [SerializeField] private int maxPoolSize = 64;

    private readonly Dictionary<ParticleSystem, ObjectPool<ParticleSystem>> pools =
        new Dictionary<ParticleSystem, ObjectPool<ParticleSystem>>();

    // Tracks active instances so we can return them when their lifetime expires.
    private readonly Dictionary<ParticleSystem, ParticleSystem> instanceToPrefab =
        new Dictionary<ParticleSystem, ParticleSystem>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Survive scene loads so combat scenes don't lose pool state.
        if (transform.parent == null) DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        // Dispose pools to release any pooled-but-inactive instances cleanly.
        foreach (var p in pools.Values) p.Clear();
        pools.Clear();
        instanceToPrefab.Clear();
    }

    /// <summary>
    /// Spawn a one-shot particle effect at the given position/rotation. Returns the active
    /// ParticleSystem so callers can read it back if needed (rare).
    /// </summary>
    public ParticleSystem Play(ParticleSystem prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        ObjectPool<ParticleSystem> pool = GetOrCreatePool(prefab);
        ParticleSystem instance = pool.Get();

        Transform t = instance.transform;
        t.SetPositionAndRotation(position, rotation);
        instance.Play(true);

        instanceToPrefab[instance] = prefab;
        StartCoroutine(ReturnWhenFinished(instance));
        return instance;
    }

    private ObjectPool<ParticleSystem> GetOrCreatePool(ParticleSystem prefab)
    {
        if (pools.TryGetValue(prefab, out var existing)) return existing;

        // Capture the prefab reference for the closures below (avoids dictionary lookup per call).
        ParticleSystem capturedPrefab = prefab;

        var pool = new ObjectPool<ParticleSystem>(
            createFunc: () =>
            {
                ParticleSystem ps = Instantiate(capturedPrefab, transform);
                // Don't auto-play; the pool will Play() explicitly when handed out.
                var main = ps.main;
                main.playOnAwake = false;
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.gameObject.SetActive(false);
                return ps;
            },
            actionOnGet: ps => ps.gameObject.SetActive(true),
            actionOnRelease: ps =>
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.gameObject.SetActive(false);
            },
            actionOnDestroy: ps =>
            {
                if (ps != null) Destroy(ps.gameObject);
            },
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxPoolSize
        );

        pools[prefab] = pool;
        return pool;
    }

    private System.Collections.IEnumerator ReturnWhenFinished(ParticleSystem instance)
    {
        // IsAlive(true) accounts for sub-emitters and trail particles too.
        while (instance != null && instance.IsAlive(true))
        {
            yield return null;
        }

        if (instance == null) yield break;

        if (instanceToPrefab.TryGetValue(instance, out var prefab) &&
            pools.TryGetValue(prefab, out var pool))
        {
            instanceToPrefab.Remove(instance);
            pool.Release(instance);
        }
    }
}
