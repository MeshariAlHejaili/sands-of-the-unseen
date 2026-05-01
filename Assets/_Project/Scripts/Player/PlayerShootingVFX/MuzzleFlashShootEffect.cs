using UnityEngine;

public class MuzzleFlashShootEffect : MonoBehaviour, IShootEffect
{
    [Header("Muzzle Flash")]
    [SerializeField] private ParticleSystem muzzleFlashPrefab;

    [Header("Optional Extra Burst")]
    [SerializeField] private ParticleSystem smokePrefab;
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