using UnityEngine;

/// <summary>
/// Fades a single dash afterimage from its start color to its end color over a given lifetime,
/// then destroys itself. Uses a per-instance material so multiple ghosts fade independently.
/// </summary>
public class AfterimageFader : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private Renderer ghostRenderer;
    private Material instanceMaterial;
    private Mesh ownedMesh;

    private float lifetime;
    private float elapsed;
    private Color startColor;
    private Color endColor;

    public void Initialize(float lifetime, Color start, Color end, Mesh ownedMeshToCleanup = null)
    {
        this.lifetime = Mathf.Max(0.01f, lifetime);
        this.startColor = start;
        this.endColor = end;
        this.ownedMesh = ownedMeshToCleanup;

        ghostRenderer = GetComponent<Renderer>();
        if (ghostRenderer != null)
        {
            // Make a unique material instance so we can fade it without affecting other afterimages.
            instanceMaterial = new Material(ghostRenderer.sharedMaterial);
            ghostRenderer.material = instanceMaterial;
            ApplyColor(startColor);
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);
        ApplyColor(Color.Lerp(startColor, endColor, t));

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyColor(Color c)
    {
        if (instanceMaterial == null) return;

        // URP uses _BaseColor, built-in uses _Color. Set both to be safe.
        if (instanceMaterial.HasProperty(BaseColorId)) instanceMaterial.SetColor(BaseColorId, c);
        if (instanceMaterial.HasProperty(ColorId)) instanceMaterial.SetColor(ColorId, c);
    }

    private void OnDestroy()
    {
        if (instanceMaterial != null) Destroy(instanceMaterial);
        if (ownedMesh != null) Destroy(ownedMesh);
    }
}
