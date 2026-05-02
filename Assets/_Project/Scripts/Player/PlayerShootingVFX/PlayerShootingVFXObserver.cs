using UnityEngine;

[RequireComponent(typeof(PlayerShooting))]
public class PlayerShootingVFXObserver : MonoBehaviour
{
    [Header("VFX")]
    [Tooltip("Muzzle flash particle prefab played once when the player fires.")]
    [SerializeField] private ParticleSystem muzzleFlashPrefab;

    [Tooltip("Spark particle prefab played once when the player fires.")]
    [SerializeField] private ParticleSystem sparkPrefab;

    [Tooltip("Smoke particle prefab played once when the player fires.")]
    [SerializeField] private ParticleSystem smokePrefab;

    private PlayerShooting playerShooting;

    private void Awake()
    {
        playerShooting = GetComponent<PlayerShooting>();
    }

    private void OnEnable()
    {
        if (playerShooting != null)
        {
            playerShooting.ShotFired += PlayShootVFX;
        }
    }

    private void OnDisable()
    {
        if (playerShooting != null)
        {
            playerShooting.ShotFired -= PlayShootVFX;
        }
    }

    private void PlayShootVFX(Vector3 position, Quaternion rotation)
    {
        if (VFXPool.Instance == null)
        {
            Debug.LogWarning("No VFXPool found in scene.", this);
            return;
        }

        if (muzzleFlashPrefab != null)
        {
            VFXPool.Instance.Play(muzzleFlashPrefab, position, rotation);
        }

        if (sparkPrefab != null)
        {
            VFXPool.Instance.Play(sparkPrefab, position, rotation);
        }

        if (smokePrefab != null)
        {
            VFXPool.Instance.Play(smokePrefab, position, rotation);
        }
    }
}
