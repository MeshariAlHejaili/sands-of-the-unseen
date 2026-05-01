using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerShootingVFXObserver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;

    [Header("VFX")]
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    [SerializeField] private ParticleSystem sparkPrefab;
    [SerializeField] private ParticleSystem smokePrefab;

    private PlayerStats stats;
    private float nextVFXTime;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (!Input.GetMouseButton(0)) return;
        if (Time.time < nextVFXTime) return;
        if (stats.FireRate <= 0f) return;

        PlayShootVFX();

        nextVFXTime = Time.time + 1f / stats.FireRate;
    }

    private void PlayShootVFX()
    {
        if (firePoint == null) return;

        if (VFXPool.Instance == null)
        {
            Debug.LogWarning("No VFXPool found in scene.", this);
            return;
        }

        Vector3 position = firePoint.position;
        Quaternion rotation = firePoint.rotation;

        if (muzzleFlashPrefab != null)
            VFXPool.Instance.Play(muzzleFlashPrefab, position, rotation);

        if (sparkPrefab != null)
            VFXPool.Instance.Play(sparkPrefab, position, rotation);

        if (smokePrefab != null)
            VFXPool.Instance.Play(smokePrefab, position, rotation);
    }
}