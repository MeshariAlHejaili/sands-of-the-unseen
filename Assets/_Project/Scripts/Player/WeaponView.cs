using UnityEngine;

[DisallowMultipleComponent]
public class WeaponView : MonoBehaviour
{
    [Header("Muzzle")]
    [Tooltip("Visual muzzle point used as the shot spawn position. Shot rotation comes from PlayerAim.")]
    [SerializeField] private Transform muzzlePoint;

    [Space]
    [Header("Hand IK")]
    [Tooltip("Optional transform marking where this weapon should align to the right hand socket in future equip flows.")]
    [SerializeField] private Transform rightHandGripPoint;

    [Tooltip("Optional transform the humanoid left hand should follow while this weapon is equipped.")]
    [SerializeField] private Transform leftHandGripTarget;

    public Transform MuzzlePoint => muzzlePoint != null ? muzzlePoint : transform;
    public Transform RightHandGripPoint => rightHandGripPoint;
    public Transform LeftHandGripTarget => leftHandGripTarget;
}
