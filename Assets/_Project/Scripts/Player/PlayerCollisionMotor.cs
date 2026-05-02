using UnityEngine;

/// <summary>
/// Performs allocation-free swept box movement for the player so high-speed dashes cannot tunnel through blockers.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class PlayerCollisionMotor : MonoBehaviour
{
    private const int MaxSweepHits = 64;
    private const int MaxOverlapResults = 64;
    private const int MaxOverlapRecoveryIterations = 3;
    private const float MinMoveDistance = 0.0001f;
    private const float MinCastExtent = 0.001f;
    private const float MovingIntoSurfaceDot = -0.001f;

    [Header("Collision Sweep")]
    [Tooltip("Physics layers that stop player walking, sprinting, and dashing.")]
    [SerializeField] private LayerMask blockingLayers = Physics.DefaultRaycastLayers;

    [Tooltip("Small buffer in world units kept between the player collider and blockers.")]
    [Range(0.001f, 0.2f)]
    [SerializeField] private float collisionSkinWidth = 0.03f;

    private readonly RaycastHit[] sweepHits = new RaycastHit[MaxSweepHits];
    private readonly Collider[] overlapResults = new Collider[MaxOverlapResults];
    private BoxCollider boxCollider;
    private Collider[] selfColliders;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnValidate()
    {
        collisionSkinWidth = Mathf.Max(0.001f, collisionSkinWidth);
    }

    /// <summary>
    /// Moves the player by the requested displacement while preventing the box collider from passing through blockers.
    /// </summary>
    /// <param name="displacement">Requested world-space movement for this frame.</param>
    /// <param name="allowSlide">Whether blocked movement can project along the hit surface for walking-style sliding.</param>
    /// <param name="blockingHit">The nearest blocker that stopped movement, if any.</param>
    /// <returns>The world-space displacement actually applied.</returns>
    public Vector3 Move(Vector3 displacement, bool allowSlide, out RaycastHit blockingHit)
    {
        blockingHit = default;

        if (displacement.sqrMagnitude <= MinMoveDistance * MinMoveDistance)
        {
            return Vector3.zero;
        }

        EnsureCachedCollider();

        if (boxCollider == null)
        {
            transform.position += displacement;
            return displacement;
        }

        ResolveBlockingOverlaps();

        Vector3 moved = MoveSingle(displacement, out blockingHit);

        if (!allowSlide || blockingHit.collider == null)
        {
            ResolveBlockingOverlaps();
            return moved;
        }

        Vector3 remainingDisplacement = displacement - moved;
        Vector3 slideDisplacement = Vector3.ProjectOnPlane(remainingDisplacement, blockingHit.normal);

        if (slideDisplacement.sqrMagnitude <= MinMoveDistance * MinMoveDistance)
        {
            return moved;
        }

        moved += MoveSingle(slideDisplacement, out _);
        ResolveBlockingOverlaps();
        return moved;
    }

    private Vector3 MoveSingle(Vector3 displacement, out RaycastHit blockingHit)
    {
        blockingHit = default;

        float distance = displacement.magnitude;
        if (distance <= MinMoveDistance)
        {
            return Vector3.zero;
        }

        Vector3 direction = displacement / distance;

        if (!TryFindNearestHit(direction, distance, out RaycastHit nearestHit))
        {
            transform.position += displacement;
            return displacement;
        }

        float allowedDistance = Mathf.Clamp(nearestHit.distance - collisionSkinWidth, 0f, distance);
        if (allowedDistance >= distance - MinMoveDistance)
        {
            transform.position += displacement;
            return displacement;
        }

        Vector3 allowedDisplacement = direction * allowedDistance;
        transform.position += allowedDisplacement;
        blockingHit = nearestHit;
        return allowedDisplacement;
    }

    private bool TryFindNearestHit(Vector3 direction, float distance, out RaycastHit nearestHit)
    {
        nearestHit = default;

        int hitCount = Physics.BoxCastNonAlloc(
            GetColliderCenter(),
            GetCastHalfExtents(),
            direction,
            sweepHits,
            transform.rotation,
            distance + collisionSkinWidth,
            blockingLayers,
            QueryTriggerInteraction.Ignore);

        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = sweepHits[i];
            if (!IsValidBlockingHit(hit, direction) || hit.distance >= nearestDistance)
            {
                continue;
            }

            nearestHit = hit;
            nearestDistance = hit.distance;
        }

        return nearestHit.collider != null;
    }

    private bool IsValidBlockingHit(RaycastHit hit, Vector3 direction)
    {
        if (hit.collider == null)
        {
            return false;
        }

        if (IsSelfCollider(hit.collider))
        {
            return false;
        }

        if (hit.distance > collisionSkinWidth)
        {
            return true;
        }

        return IsMovingIntoSurface(hit.normal, direction);
    }

    private bool IsMovingIntoSurface(Vector3 normal, Vector3 direction)
    {
        if (normal.sqrMagnitude <= Mathf.Epsilon)
        {
            return true;
        }

        return Vector3.Dot(direction, normal.normalized) < MovingIntoSurfaceDot;
    }

    private void ResolveBlockingOverlaps()
    {
        for (int iteration = 0; iteration < MaxOverlapRecoveryIterations; iteration++)
        {
            int overlapCount = Physics.OverlapBoxNonAlloc(
                GetColliderCenter(),
                GetOverlapHalfExtents(),
                overlapResults,
                transform.rotation,
                blockingLayers,
                QueryTriggerInteraction.Ignore);

            Vector3 correction = Vector3.zero;

            for (int i = 0; i < overlapCount; i++)
            {
                Collider other = overlapResults[i];
                if (other == null || IsSelfCollider(other))
                {
                    continue;
                }

                bool isOverlapping = Physics.ComputePenetration(
                    boxCollider,
                    transform.position,
                    transform.rotation,
                    other,
                    other.transform.position,
                    other.transform.rotation,
                    out Vector3 separationDirection,
                    out float separationDistance);

                if (!isOverlapping || separationDistance <= MinMoveDistance)
                {
                    continue;
                }

                correction += separationDirection * separationDistance;
            }

            if (correction.sqrMagnitude <= MinMoveDistance * MinMoveDistance)
            {
                break;
            }

            transform.position += correction + correction.normalized * collisionSkinWidth;
        }
    }

    private bool IsSelfCollider(Collider other)
    {
        if (selfColliders == null)
        {
            return false;
        }

        for (int i = 0; i < selfColliders.Length; i++)
        {
            if (selfColliders[i] == other)
            {
                return true;
            }
        }

        return false;
    }

    private Vector3 GetColliderCenter()
    {
        return transform.TransformPoint(boxCollider.center);
    }

    private Vector3 GetCastHalfExtents()
    {
        Vector3 halfExtents = Vector3.Scale(boxCollider.size * 0.5f, Abs(transform.lossyScale));
        float skinWidth = Mathf.Max(0f, collisionSkinWidth);

        return new Vector3(
            Mathf.Max(MinCastExtent, halfExtents.x - skinWidth),
            Mathf.Max(MinCastExtent, halfExtents.y - skinWidth),
            Mathf.Max(MinCastExtent, halfExtents.z - skinWidth));
    }

    private Vector3 GetOverlapHalfExtents()
    {
        Vector3 halfExtents = Vector3.Scale(boxCollider.size * 0.5f, Abs(transform.lossyScale));

        return new Vector3(
            Mathf.Max(MinCastExtent, halfExtents.x),
            Mathf.Max(MinCastExtent, halfExtents.y),
            Mathf.Max(MinCastExtent, halfExtents.z));
    }

    private void EnsureCachedCollider()
    {
        if (boxCollider == null)
        {
            CacheComponents();
        }
    }

    private void CacheComponents()
    {
        boxCollider = GetComponent<BoxCollider>();
        selfColliders = GetComponentsInChildren<Collider>(true);
    }

    private static Vector3 Abs(Vector3 value)
    {
        return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}
