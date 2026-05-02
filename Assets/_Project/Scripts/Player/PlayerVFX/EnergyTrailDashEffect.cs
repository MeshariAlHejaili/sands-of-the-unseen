using UnityEngine;

/// <summary>
/// Energy-trail dash effect inspired by anime/action-game dashes (continuous emissive ribbon
/// behind the player + a stream of sparks). Uses a TrailRenderer for the ribbon and an
/// optional looping ParticleSystem for the spark stream.
///
/// Allocation profile per dash: ZERO. Both components are toggled, never instantiated.
/// The TrailRenderer fades existing points naturally after emitting is set to false, which
/// produces the "draw fast, fade smoothly" behavior the user requested.
///
/// Lives as a serializable MonoBehaviour rather than a plain class because Unity's inspector
/// can't serialize interface fields directly, and authoring a TrailRenderer in the inspector
/// requires it to live on a GameObject anyway.
/// </summary>
[DisallowMultipleComponent]
public class EnergyTrailDashEffect : MonoBehaviour, IDashEffect
{
    [Header("Trail")]
    [Tooltip("TrailRenderer that draws the dash ribbon. If null, one is auto-added at runtime with sane defaults.")]
    [SerializeField] private TrailRenderer trail;

    [Tooltip("Color gradient applied to the trail. Bright/opaque on the left, transparent on the right.")]
    [SerializeField] private Gradient trailColor = CreateDefaultTrailGradient();

    [Tooltip("Trail lifetime in seconds. Controls how long the tail persists after the dash ends.")]
    [Min(0.01f)]
    [SerializeField] private float trailTime = 0.3f;

    [Tooltip("Trail width at the head (player end).")]
    [Min(0f)]
    [SerializeField] private float trailStartWidth = 0.6f;

    [Tooltip("Trail width at the tail (oldest end). Smaller values give a tapered streak.")]
    [Min(0f)]
    [SerializeField] private float trailEndWidth = 0.05f;

    [Header("Sparks (optional)")]
    [Tooltip("Looping particle system that emits sparks/embers during the dash. Leave null to skip.")]
    [SerializeField] private ParticleSystem sparkStream;
    
    [Tooltip("One-shot particle system played when a dash begins.")]
    [SerializeField] private ParticleSystem dashStartBurst;

    [Tooltip("Looping spark particle system played for the duration of the dash.")]
    [SerializeField] private ParticleSystem dashSparkStream;

    [Tooltip("Looping smoke particle system played for the duration of the dash.")]
    [SerializeField] private ParticleSystem dashSmokeStream;

    [Tooltip("Light enabled only while the dash trail is active.")]
    [SerializeField] private Light dashLight;

    private Transform owner;
    private bool initialized;

    public void Initialize(Transform owner)
    {
        if (initialized) return;
        this.owner = owner;

        EnsureTrail();
        ConfigureTrail();
        ConfigureSparks();

        initialized = true;
    }

    public void OnDashStarted()
    {
        if (!initialized) return;

        // Clear any leftover trail points from a previous dash so the new ribbon starts clean.
        trail.Clear();
        trail.emitting = true;

        if (dashStartBurst != null)
            dashStartBurst.Play(true);

        if (dashSparkStream != null)
            dashSparkStream.Play(true);

        if (dashSmokeStream != null)
            dashSmokeStream.Play(true);

        if (dashLight != null)
            dashLight.enabled = true;
    }

    public void OnDashEnded()
    {
        if (!initialized) return;

        // Stop emitting new points, but DON'T clear — existing points fade out naturally
        // over `trail.time` seconds. This is the "fast appear, smooth fade" behavior.
        trail.emitting = false;

        if (dashSparkStream != null)
            dashSparkStream.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (dashSmokeStream != null)
            dashSmokeStream.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (dashLight != null)
            dashLight.enabled = false;
    }

    public void Cleanup()
    {
        // Trail/particles live on this GameObject; Unity destroys them with the component.
        // Nothing to free explicitly because we never instantiate at runtime.
    }

    private void EnsureTrail()
    {
        if (trail != null) return;

        // Auto-add so designers don't have to wire it manually if they accept defaults.
        trail = gameObject.GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
        }
    }

    private void ConfigureTrail()
    {
        trail.time = trailTime;
        trail.startWidth = trailStartWidth;
        trail.endWidth = trailEndWidth;
        trail.colorGradient = trailColor;
        trail.emitting = false; // Start off — only emit during dashes.
        trail.minVertexDistance = 0.05f; // Smooth without spamming verts.
        trail.autodestruct = false;

        // Use a default trail material if none assigned, so the trail renders out of the box.
        if (trail.sharedMaterial == null)
        {
            trail.sharedMaterial = CreateDefaultTrailMaterial();
        }
    }

    private void ConfigureSparks()
    {
        if (sparkStream == null) return;

        // Force-stop on init in case the prefab was authored with Play On Awake = true.
        if (sparkStream.isPlaying) sparkStream.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private static Gradient CreateDefaultTrailGradient()
    {
        // Ember orange → soul purple → transparent: matches the GDD color palette
        // (deep oranges, hazy purples) and gives a hot-to-cool fade for the tail.
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.55f, 0.15f), 0f),  // ember orange (head)
                new GradientColorKey(new Color(0.7f, 0.2f, 0.5f), 0.6f), // soul purple (mid)
                new GradientColorKey(new Color(0.3f, 0.05f, 0.4f), 1f),  // dark purple (tail)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.6f, 0.5f),
                new GradientAlphaKey(0f, 1f),
            }
        );
        return g;
    }

    private static Material CreateDefaultTrailMaterial()
    {
        // Try URP unlit first since the project uses URP per the GDD.
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default"); // last-resort fallback

        Material mat = new Material(shader);
        mat.name = "DashTrail_Default (Runtime)";

        // Configure additive-friendly transparency so the trail glows over dark biomes.
        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
        if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 1f); // 1 = Additive in URP particles
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;

        return mat;
    }
}
