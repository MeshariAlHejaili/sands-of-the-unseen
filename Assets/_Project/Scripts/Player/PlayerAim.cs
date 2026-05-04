using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInputReader))]
[DefaultExecutionOrder(50)]
public class PlayerAim : MonoBehaviour
{
    private const float MinAimDistanceSqr = 0.0001f;

    [Header("Aiming")]
    [Tooltip("Rotation interpolation speed while turning toward the mouse cursor. Use 0 for instant rotation.")]
    [Min(0f)]
    [SerializeField] private float rotationSpeed = 0f;

    [Tooltip("World Y height used for mouse aiming raycasts. Match this to the playable ground plane height in world units.")]
    [Range(-10f, 10f)]
    [SerializeField] private float aimPlaneHeight = 0f;

    [Tooltip("Optional transform whose world Y height is used for mouse aiming raycasts. Use the weapon muzzle to avoid tilted-camera parallax.")]
    [SerializeField] private Transform aimPlaneSource;

    private Camera cam;
    private PlayerInputReader inputReader;

    public Vector3 AimDirection { get; private set; }
    public Quaternion AimRotation { get; private set; }
    public Vector3 AimWorldPoint { get; private set; }

    private void Awake()
    {
        cam = Camera.main;
        inputReader = PlayerInputReader.GetOrAdd(gameObject);

        AimDirection = GetPlanarForward();
        AimRotation = Quaternion.LookRotation(AimDirection, Vector3.up);
        AimWorldPoint = transform.position + AimDirection;
    }

    private void LateUpdate()
    {
        UpdateAim();
    }

    public bool TryGetAimRotationFrom(Vector3 origin, out Quaternion rotation)
    {
        Vector3 aimPoint = AimWorldPoint;
        if (TryGetPointerPointOnPlane(origin.y, out Vector3 pointOnOriginPlane))
        {
            aimPoint = pointOnOriginPlane;
        }

        Vector3 direction = aimPoint - origin;
        direction.y = 0f;

        if (direction.sqrMagnitude <= MinAimDistanceSqr)
        {
            direction = AimDirection;
        }

        if (direction.sqrMagnitude <= MinAimDistanceSqr)
        {
            rotation = Quaternion.identity;
            return false;
        }

        rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        return true;
    }

    private void UpdateAim()
    {
        if (cam == null)
        {
            return;
        }

        if (!TryGetPointerPointOnPlane(GetAimPlaneHeight(), out Vector3 targetPoint))
        {
            return;
        }

        Vector3 targetDirection = targetPoint - transform.position;
        targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude <= MinAimDistanceSqr)
        {
            return;
        }

        AimWorldPoint = targetPoint;
        AimDirection = targetDirection.normalized;
        AimRotation = Quaternion.LookRotation(AimDirection, Vector3.up);

        ApplyRotation(AimRotation);
    }

    private void ApplyRotation(Quaternion targetRotation)
    {
        if (rotationSpeed <= 0f)
        {
            transform.rotation = targetRotation;
            return;
        }

        float interpolation = Mathf.Clamp01(Time.deltaTime * rotationSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, interpolation);
    }

    private Vector3 GetPlanarForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= MinAimDistanceSqr)
        {
            return Vector3.forward;
        }

        return forward.normalized;
    }

    private bool TryGetPointerPointOnPlane(float planeHeight, out Vector3 point)
    {
        point = default;

        if (cam == null)
        {
            return false;
        }

        Plane aimPlane = new Plane(Vector3.up, new Vector3(0f, planeHeight, 0f));
        Ray ray = cam.ScreenPointToRay(inputReader.PointerPosition);

        if (!aimPlane.Raycast(ray, out float hitDist))
        {
            return false;
        }

        point = ray.GetPoint(hitDist);
        return true;
    }

    private float GetAimPlaneHeight()
    {
        return aimPlaneSource != null ? aimPlaneSource.position.y : aimPlaneHeight;
    }
}
