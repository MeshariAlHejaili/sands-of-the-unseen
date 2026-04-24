using UnityEngine;

public class StayInsidePlane : MonoBehaviour
{
    [SerializeField] private string groundObjectName = "Ground";
    [SerializeField] private Collider targetCollider;
    [SerializeField] private float padding = 0.05f;

    private Collider groundCollider;

    private void Awake()
    {
        if (targetCollider == null)
        {
            targetCollider = GetComponent<Collider>();
        }

        ResolveGroundCollider();
    }

    private void LateUpdate()
    {
        if (groundCollider == null)
        {
            ResolveGroundCollider();
        }

        if (groundCollider == null)
        {
            return;
        }

        Bounds groundBounds = groundCollider.bounds;
        Vector3 position = transform.position;
        Vector3 extents = GetHorizontalExtents();

        float minX = groundBounds.min.x + extents.x + padding;
        float maxX = groundBounds.max.x - extents.x - padding;
        float minZ = groundBounds.min.z + extents.z + padding;
        float maxZ = groundBounds.max.z - extents.z - padding;

        position.x = ClampAxis(position.x, minX, maxX);
        position.z = ClampAxis(position.z, minZ, maxZ);
        transform.position = position;
    }

    private Vector3 GetHorizontalExtents()
    {
        if (targetCollider == null)
        {
            return Vector3.zero;
        }

        Bounds bounds = targetCollider.bounds;
        return new Vector3(bounds.extents.x, 0f, bounds.extents.z);
    }

    private void ResolveGroundCollider()
    {
        GameObject groundObject = GameObject.Find(groundObjectName);
        if (groundObject != null)
        {
            groundCollider = groundObject.GetComponent<Collider>();
        }
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
