using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [Header("Aiming")]
    [Tooltip("Rotation interpolation speed while turning toward the mouse cursor.")]
    [Min(0f)]
    [SerializeField] private float rotationSpeed = 20f;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleRotation();
    }

    /// Rotates the player to look exactly where the mouse cursor is on the ground.
    private void HandleRotation()
    {
        // Create a plane at the player's current height (Y position)
        Plane playerPlane = new Plane(Vector3.up, transform.position);

        // Cast a ray from the camera through the mouse position
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (playerPlane.Raycast(ray, out float hitDist))
        {
            // The point in the world where the mouse is hovering
            Vector3 targetPoint = ray.GetPoint(hitDist);

            // Calculate the direction from the player to that point
            Vector3 lookDir = targetPoint - transform.position;

            // Ensure the player doesn't tilt up or down (keep Y at 0)
            lookDir.y = 0;

            if (lookDir != Vector3.zero)
            {
                // Smoothly rotate toward the target direction
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }
}
