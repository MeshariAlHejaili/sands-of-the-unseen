using UnityEngine;

/// <summary>
/// Strategy interface for dash visual effects. Implementations decide what a dash looks like
/// (energy trail, sand dispersal, soul afterimages, etc.) without coupling to PlayerMovement
/// or PlayerDashVFX.
///
/// Why a strategy: the project will need biome-specific dash variations (Burning Expanse should
/// feel hotter than Echoing Sands) and possibly upgrade-driven variations later. Hardcoding the
/// effect into PlayerDashVFX would force edits to that class every time a new style is added,
/// violating Open/Closed.
/// </summary>
public interface IDashEffect
{
    /// <summary>Called once during Awake/OnEnable so the effect can cache references and configure components.</summary>
    void Initialize(Transform owner);

    /// <summary>Called the moment a dash begins. Implementations should start their visuals here.</summary>
    void OnDashStarted();

    /// <summary>Called the moment a dash ends. Implementations should stop emission but allow existing visuals to fade naturally.</summary>
    void OnDashEnded();

    /// <summary>Called when the owning component is disabled or destroyed. Cleanup any pooled or instantiated resources.</summary>
    void Cleanup();
}
