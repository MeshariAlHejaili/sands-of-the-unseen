using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class PlayerWeaponIK : MonoBehaviour
{
    [Header("Source Components")]
    [Tooltip("Player weapon mount that owns the active weapon view. Auto-found on parent if left empty.")]
    [SerializeField] private PlayerWeaponMount weaponMount;

    [Space]
    [Header("Left Hand IK")]
    [Tooltip("How strongly the left hand follows the equipped weapon grip target.")]
    [Range(0f, 1f)]
    [SerializeField] private float leftHandIkWeight = 1f;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (weaponMount == null)
        {
            weaponMount = GetComponentInParent<PlayerWeaponMount>();
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        Transform gripTarget = GetLeftHandGripTarget();
        float weight = gripTarget != null ? leftHandIkWeight : 0f;

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, weight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, weight);

        if (gripTarget == null)
        {
            return;
        }

        animator.SetIKPosition(AvatarIKGoal.LeftHand, gripTarget.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, gripTarget.rotation);
    }

    private Transform GetLeftHandGripTarget()
    {
        if (weaponMount == null)
        {
            return null;
        }

        WeaponView weaponView = weaponMount.ActiveWeaponView;
        return weaponView != null ? weaponView.LeftHandGripTarget : null;
    }
}
