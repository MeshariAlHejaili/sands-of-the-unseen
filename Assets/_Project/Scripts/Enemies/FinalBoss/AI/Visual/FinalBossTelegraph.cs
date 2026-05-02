using UnityEngine;

/// <summary>
/// Reusable telegraph visual. States call ShowCone/ShowLine/ShowCircle with a duration
/// and the component handles drawing it (LineRenderer-based) and auto-hiding.
///
/// One instance per boss, attached to the boss root. The component pre-creates its
/// LineRenderers in Awake — no allocations during the fight. Color pulses red as the
/// telegraph nears its end so the player has clear timing.
///
/// Three modes:
///   - Cone:   melee arc shown as a fan of lines (e.g., heavy combo windup).
///   - Line:   straight line from boss to a target point (e.g., frost slash aim).
///   - Circle: flat ring on the ground (e.g., dash impact zone, AOE preview).
///
/// Why one component instead of three: states should not know about Unity rendering
/// types. They just call boss.Telegraph.ShowCone(...). Adding a 4th telegraph shape
/// later is a single new method here, not a new component on the prefab.
/// </summary>
public class FinalBossTelegraph : MonoBehaviour
{
    [Header("Style")]
    [Tooltip("Initial color of the telegraph at the start of its duration.")]
    [SerializeField] private Color startColor = new Color(1f, 0.85f, 0.2f, 0.9f);

    [Tooltip("Color the telegraph pulses to as it nears its end (the 'danger imminent' warning).")]
    [SerializeField] private Color endColor = new Color(1f, 0.1f, 0.1f, 1f);

    [Tooltip("Line width for cone and straight-line telegraphs.")]
    [Min(0.01f)] [SerializeField] private float lineWidth = 0.08f;

    [Tooltip("Number of segments used to draw the circle ring. 32 looks smooth.")]
    [Range(8, 64)] [SerializeField] private int circleSegments = 32;

    [Tooltip("Number of fan lines drawn for cone telegraphs. Higher = smoother arc.")]
    [Range(3, 16)] [SerializeField] private int coneFanLines = 7;

    private LineRenderer circleRenderer;
    private LineRenderer lineRenderer;
    private LineRenderer[] coneRenderers;

    // Active telegraph tracking
    private float duration;
    private float elapsed;
    private bool active;
    private Mode activeMode;

    private enum Mode { None, Cone, Line, Circle }

    private void Awake()
    {
        // We pre-create all three so states never wait on instantiation.
        circleRenderer = CreateLineRenderer("TelegraphCircle", circleSegments + 1, true);
        lineRenderer   = CreateLineRenderer("TelegraphLine", 2, false);

        coneRenderers = new LineRenderer[coneFanLines];
        for (int i = 0; i < coneFanLines; i++)
            coneRenderers[i] = CreateLineRenderer($"TelegraphConeLine_{i}", 2, false);

        HideAll();
    }

    private LineRenderer CreateLineRenderer(string name, int positionCount, bool loop)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = positionCount;
        lr.loop = loop;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        // URP/Built-in compatible default unlit material — Unity's Sprites/Default
        // exists in both pipelines and is unlit + cheap.
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = startColor;
        lr.endColor = startColor;
        lr.enabled = false;
        return lr;
    }

    private void Update()
    {
        if (!active) return;

        elapsed += Time.deltaTime;
        float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;

        // Pulse: color goes from start to end as we approach impact.
        Color c = Color.Lerp(startColor, endColor, t);

        switch (activeMode)
        {
            case Mode.Circle:
                circleRenderer.startColor = c;
                circleRenderer.endColor = c;
                break;
            case Mode.Line:
                lineRenderer.startColor = c;
                lineRenderer.endColor = c;
                break;
            case Mode.Cone:
                for (int i = 0; i < coneRenderers.Length; i++)
                {
                    coneRenderers[i].startColor = c;
                    coneRenderers[i].endColor = c;
                }
                break;
        }

        if (elapsed >= duration)
            HideAll();
    }

    /// <summary>Draws an arc fan from the boss origin in its forward direction.</summary>
    /// <param name="originWorld">World-space origin (usually boss position + slight Y offset).</param>
    /// <param name="forward">Forward direction the arc opens around.</param>
    /// <param name="halfAngleDeg">Half angle in degrees (e.g., 70 for a 140° cone).</param>
    /// <param name="radius">Length of the lines.</param>
    /// <param name="durationSeconds">How long the cone is visible.</param>
    public void ShowCone(Vector3 originWorld, Vector3 forward, float halfAngleDeg, float radius, float durationSeconds)
    {
        HideAll();
        activeMode = Mode.Cone;
        active = true;
        elapsed = 0f;
        duration = durationSeconds;

        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
        forward.Normalize();

        // Distribute fan lines symmetrically across the arc.
        for (int i = 0; i < coneRenderers.Length; i++)
        {
            float t = coneRenderers.Length == 1
                ? 0.5f
                : (float)i / (coneRenderers.Length - 1);
            float angle = Mathf.Lerp(-halfAngleDeg, halfAngleDeg, t);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;
            coneRenderers[i].SetPosition(0, originWorld);
            coneRenderers[i].SetPosition(1, originWorld + dir * radius);
            coneRenderers[i].enabled = true;
        }
    }

    /// <summary>Draws a straight line, useful for ranged-aim previews.</summary>
    public void ShowLine(Vector3 fromWorld, Vector3 toWorld, float durationSeconds)
    {
        HideAll();
        activeMode = Mode.Line;
        active = true;
        elapsed = 0f;
        duration = durationSeconds;

        lineRenderer.SetPosition(0, fromWorld);
        lineRenderer.SetPosition(1, toWorld);
        lineRenderer.enabled = true;
    }

    /// <summary>Draws a flat ring on the XZ plane at the given center.</summary>
    public void ShowCircle(Vector3 centerWorld, float radius, float durationSeconds)
    {
        HideAll();
        activeMode = Mode.Circle;
        active = true;
        elapsed = 0f;
        duration = durationSeconds;

        int count = circleSegments + 1;
        for (int i = 0; i < count; i++)
        {
            float a = (float)i / circleSegments * Mathf.PI * 2f;
            Vector3 p = centerWorld + new Vector3(Mathf.Cos(a) * radius, 0.05f, Mathf.Sin(a) * radius);
            circleRenderer.SetPosition(i, p);
        }
        circleRenderer.enabled = true;
    }

    /// <summary>Force-hides any active telegraph. Call this in state Exit() if you want to clear early.</summary>
    public void HideAll()
    {
        active = false;
        activeMode = Mode.None;
        if (circleRenderer != null) circleRenderer.enabled = false;
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (coneRenderers != null)
        {
            for (int i = 0; i < coneRenderers.Length; i++)
                if (coneRenderers[i] != null) coneRenderers[i].enabled = false;
        }
    }
}
