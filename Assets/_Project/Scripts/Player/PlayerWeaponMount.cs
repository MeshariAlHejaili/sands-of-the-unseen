using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerAim))]
public class PlayerWeaponMount : MonoBehaviour
{
    [Header("Aiming")]
    [Tooltip("Player aim source that defines gameplay shot rotation. Auto-found on this GameObject if left empty.")]
    [SerializeField] private PlayerAim playerAim;

    [Space]
    [Header("Weapon Sockets")]
    [Tooltip("Parent transform for current and future weapon visuals.")]
    [SerializeField] private Transform weaponSocket;

    [Tooltip("Fallback muzzle transform used when no active weapon view supplies one.")]
    [SerializeField] private Transform fallbackMuzzlePoint;

    [Tooltip("Spawn origin reserved for future throwable weapons.")]
    [SerializeField] private Transform throwableOrigin;

    private WeaponView activeWeaponView;

    public Transform WeaponSocket => weaponSocket;
    public Transform ThrowableOrigin => throwableOrigin;
    public WeaponView ActiveWeaponView
    {
        get
        {
            if (activeWeaponView == null)
            {
                ResolveActiveWeaponView();
            }

            return activeWeaponView;
        }
    }

    private void Awake()
    {
        if (playerAim == null)
        {
            playerAim = GetComponent<PlayerAim>();
        }

        ResolveActiveWeaponView();
    }

    public void SetActiveWeaponView(WeaponView weaponView)
    {
        activeWeaponView = weaponView;
    }

    public bool GetShotPose(out Vector3 position, out Quaternion rotation)
    {
        Transform muzzlePoint = GetMuzzlePoint();
        position = muzzlePoint != null ? muzzlePoint.position : transform.position;

        if (playerAim != null)
        {
            if (!playerAim.TryGetAimRotationFrom(position, out rotation))
            {
                rotation = playerAim.AimRotation;
            }

            return true;
        }

        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= Mathf.Epsilon)
        {
            rotation = Quaternion.identity;
            return false;
        }

        rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        return true;
    }

    private Transform GetMuzzlePoint()
    {
        WeaponView weaponView = ActiveWeaponView;
        if (weaponView != null)
        {
            return weaponView.MuzzlePoint;
        }

        if (fallbackMuzzlePoint != null)
        {
            return fallbackMuzzlePoint;
        }

        return weaponSocket;
    }

    private void ResolveActiveWeaponView()
    {
        if (weaponSocket != null)
        {
            activeWeaponView = weaponSocket.GetComponentInChildren<WeaponView>(true);
        }

        if (activeWeaponView == null)
        {
            activeWeaponView = GetComponentInChildren<WeaponView>(true);
        }
    }
}
