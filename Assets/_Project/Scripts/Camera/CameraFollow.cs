using Sirenix.OdinInspector;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform the camera follows, usually the player.")]
    [SerializeField] private Transform playerTransform;

    [Space]
    [Header("Positioning")]
    [Tooltip("Camera offset from the target in world units.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -5f);

    [Space]
    [Header("Smoothing")]
    [Tooltip("How quickly the camera interpolates toward the target position.")]
    [Min(0f)]
    [SerializeField] private float smoothSpeed = 10f;

    [Space]
    [Header("Bounds")]
    [Tooltip("Whether the camera position is clamped to the arena bounds after following the target.")]
    [SerializeField] private bool clampToArenaBounds;

    [Tooltip("Arena bounds source used for camera clamping. Falls back to the manual bounds below if empty.")]
    [SceneObjectsOnly]
    [SerializeField] private ArenaMapBounds mapBounds;

    [Tooltip("Minimum allowed camera X/Z position in world units.")]
    [SerializeField] private Vector2 arenaMinBounds = new Vector2(-25f, -25f);

    [Tooltip("Maximum allowed camera X/Z position in world units.")]
    [SerializeField] private Vector2 arenaMaxBounds = new Vector2(25f, 25f);

    [Tooltip("Extra inward camera clamp padding in world units.")]
    [Min(0f)]
    [SerializeField] private float arenaBoundsPadding;

    private void Awake()
    {
        if (mapBounds == null)
        {
            mapBounds = FindFirstObjectByType<ArenaMapBounds>();
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null)
        {
            return;
        }

        Vector3 targetPosition = playerTransform.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        if (clampToArenaBounds)
        {
            Vector2 minBounds = GetMinBounds();
            Vector2 maxBounds = GetMaxBounds();
            smoothedPosition.x = ClampAxis(smoothedPosition.x, minBounds.x, maxBounds.x);
            smoothedPosition.z = ClampAxis(smoothedPosition.z, minBounds.y, maxBounds.y);
        }

        transform.position = smoothedPosition;
    }

    private void OnValidate()
    {
        smoothSpeed = Mathf.Max(0f, smoothSpeed);
        arenaBoundsPadding = Mathf.Max(0f, arenaBoundsPadding);
    }

    private Vector2 GetMinBounds()
    {
        Vector2 minBounds = mapBounds != null ? mapBounds.MinBounds : arenaMinBounds;
        return minBounds + Vector2.one * arenaBoundsPadding;
    }

    private Vector2 GetMaxBounds()
    {
        Vector2 maxBounds = mapBounds != null ? mapBounds.MaxBounds : arenaMaxBounds;
        return maxBounds - Vector2.one * arenaBoundsPadding;
    }

    private static float ClampAxis(float value, float min, float max)
    {
        if (min > max)
        {
            return (min + max) * 0.5f;
        }

        return Mathf.Clamp(value, min, max);
    }
}
