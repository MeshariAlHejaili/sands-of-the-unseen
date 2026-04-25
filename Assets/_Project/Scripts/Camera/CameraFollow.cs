using UnityEngine;

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

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // 1. Calculate the desired position based on the player and our offset
        Vector3 targetPosition = playerTransform.position + offset;

        // Smoothly transition from current position to target position (Vector3.Lerp makes the movement feel fluid rather than snappy)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Apply the new position
        transform.position = smoothedPosition;
    }
}
