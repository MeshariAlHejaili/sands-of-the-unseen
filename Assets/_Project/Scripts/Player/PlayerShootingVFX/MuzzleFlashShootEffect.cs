using UnityEngine;

public class MuzzleFlashShootEffect : MonoBehaviour, IShootEffect
{
    [Header("Muzzle Flash")]
    [Tooltip("Primary muzzle flash particle prefab spawned at the firing point.")]
    [SerializeField] private ParticleSystem muzzleFlashPrefab;

    [Header("Optional Extra Burst")]
    [Tooltip("Optional smoke particle prefab spawned alongside the muzzle flash.")]
    [SerializeField] private ParticleSystem smokePrefab;

    [Tooltip("Optional spark particle prefab spawned alongside the muzzle flash.")]
    [SerializeField] private ParticleSystem sparkPrefab;

    public void Initialize(Transform owner)
    {
        // Nothing needed yet.
        // VFXPool handles spawning and returning.
    }

    public void OnShotFired(Vector3 position, Quaternion rotation)
    {
        if (VFXPool.Instance == null)
        {
            Debug.LogWarning("No VFXPool found in scene.", this);
            return;
        }

        VFXPool.Instance.Play(muzzleFlashPrefab, position, rotation);

        if (smokePrefab != null)
            VFXPool.Instance.Play(smokePrefab, position, rotation);

        if (sparkPrefab != null)
            VFXPool.Instance.Play(sparkPrefab, position, rotation);
    }

    public void Cleanup()
    {
    }
}
