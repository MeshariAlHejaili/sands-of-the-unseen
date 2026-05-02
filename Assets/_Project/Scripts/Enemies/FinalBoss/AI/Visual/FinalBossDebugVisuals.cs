using UnityEngine;

/// <summary>
/// Debug-only visuals: a long cube parented under the boss for the sword. The actual
/// damage logic lives in states; this class JUST shows where the sword is and tints it
/// when the boss is in a punishable state, so you can see the windows visually.
///
/// Drop primitive cubes in the prefab and assign them in the inspector. Replace with
/// real meshes later — this class doesn't care what the meshes look like.
/// </summary>
public class FinalBossDebugVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FinalBossController boss;
    [SerializeField] private Renderer swordRenderer;
    [SerializeField] private Renderer bodyRenderer;

    [Header("Colors")]
    [SerializeField] private Color bodyDefault = Color.white;
    [SerializeField] private Color bodyVulnerable = new Color(1f, 0.4f, 0.4f);
    [SerializeField] private Color bodyPhase2 = new Color(0.55f, 0f, 1f);

    private MaterialPropertyBlock mpb;

    private void Awake()
    {
        if (boss == null) boss = GetComponentInParent<FinalBossController>();
        mpb = new MaterialPropertyBlock();
    }

    private void LateUpdate()
    {
        if (boss == null || bodyRenderer == null) return;

        bool punishable = boss.CurrentState != null && boss.CurrentState.IsPunishable;
        Color target = punishable
            ? bodyVulnerable
            : (boss.CurrentPhase >= 2 ? bodyPhase2 : bodyDefault);

        bodyRenderer.GetPropertyBlock(mpb);
        mpb.SetColor("_BaseColor", target); // URP
        mpb.SetColor("_Color", target);     // built-in
        bodyRenderer.SetPropertyBlock(mpb);
    }
}
