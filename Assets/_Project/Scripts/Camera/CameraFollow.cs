using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerTransform;

    [Header("Positioning")]
    // The distance between the camera and the player
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -5f);

    [Header("Smoothing")]
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