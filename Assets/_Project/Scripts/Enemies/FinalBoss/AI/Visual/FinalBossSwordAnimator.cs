using UnityEngine;

public class FinalBossSwordAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform sword;

    [Header("Sword Setup")]
    [SerializeField] private bool autoCreateWrist = true;

    [Tooltip("Sword size controlled from this script.")]
    [SerializeField] private Vector3 swordLocalScale = Vector3.one;

    [Tooltip("Local offset from the wrist/grip to the sword mesh. Adjust this until the sword handle sits on the wrist.")]
    [SerializeField] private Vector3 swordLocalPosition = new Vector3(0f, 0f, 1f);

    [Tooltip("Local rotation of the sword mesh under the wrist.")]
    [SerializeField] private Vector3 swordLocalEulerRotation = Vector3.zero;

    [Header("Swing Angles")]
    [SerializeField] private Vector3 horizontalStartEuler = new Vector3(0f, -90f, 0f);
    [SerializeField] private Vector3 horizontalEndEuler = new Vector3(0f, 90f, 0f);

    [SerializeField] private Vector3 verticalStartEuler = new Vector3(-110f, 0f, 0f);
    [SerializeField] private Vector3 verticalEndEuler = new Vector3(30f, 0f, 0f);

    [SerializeField] private Vector3 thrustStartEuler = new Vector3(-15f, 0f, 0f);
    [SerializeField] private Vector3 thrustEndEuler = new Vector3(15f, 0f, 0f);

    [Header("Debug")]
    [SerializeField] private bool applySwordSettingsEveryFrame = true;

    private Transform wrist;
    private Quaternion idleRotation = Quaternion.identity;

    private enum Swing
    {
        None,
        Horizontal,
        Vertical,
        Thrust
    }

    private Swing active = Swing.None;

    private float elapsed;
    private float duration;

    private Quaternion fromRot;
    private Quaternion toRot;

    private void Reset()
    {
        if (sword == null)
        {
            Transform t = transform.Find("Body/Sword");
            if (t == null) t = transform.Find("Sword");
            sword = t;
        }
    }

    private void Awake()
    {
        if (sword == null)
        {
            Debug.LogWarning("[FinalBossSwordAnimator] No sword assigned.", this);
            return;
        }

        SetupWrist();
        ApplySwordSettings();
    }

    private void SetupWrist()
    {
        if (sword.parent != null && sword.parent.name == "SwordWrist")
        {
            wrist = sword.parent;
            idleRotation = wrist.localRotation;
            return;
        }

        if (!autoCreateWrist)
        {
            wrist = sword.parent != null ? sword.parent : sword;
            idleRotation = wrist.localRotation;
            return;
        }

        Transform originalParent = sword.parent;

        GameObject wristGO = new GameObject("SwordWrist");
        wrist = wristGO.transform;

        wrist.SetParent(originalParent, false);

        // Put wrist at the sword's current position first.
        // Then use swordLocalPosition to offset the sword visually from the grip.
        wrist.position = sword.position;
        wrist.localRotation = Quaternion.identity;
        wrist.localScale = Vector3.one;

        sword.SetParent(wrist, false);

        idleRotation = Quaternion.identity;
    }

    private void ApplySwordSettings()
    {
        if (sword == null) return;

        sword.localPosition = swordLocalPosition;
        sword.localRotation = Quaternion.Euler(swordLocalEulerRotation);
        sword.localScale = swordLocalScale;
    }

    public void PlaySwingHorizontal(float swingDuration)
    {
        PlaySwing(
            Quaternion.Euler(horizontalStartEuler),
            Quaternion.Euler(horizontalEndEuler),
            swingDuration,
            Swing.Horizontal
        );
    }

    public void PlaySwingVertical(float swingDuration)
    {
        PlaySwing(
            Quaternion.Euler(verticalStartEuler),
            Quaternion.Euler(verticalEndEuler),
            swingDuration,
            Swing.Vertical
        );
    }

    public void PlayThrust(float swingDuration)
    {
        PlaySwing(
            Quaternion.Euler(thrustStartEuler),
            Quaternion.Euler(thrustEndEuler),
            swingDuration,
            Swing.Thrust
        );
    }

    private void PlaySwing(Quaternion start, Quaternion end, float swingDuration, Swing swingType)
    {
        if (wrist == null) return;

        active = swingType;
        elapsed = 0f;
        duration = Mathf.Max(0.01f, swingDuration);

        fromRot = idleRotation * start;
        toRot = idleRotation * end;

        wrist.localRotation = fromRot;
    }

    public void HoldHorizontalWindup()
    {
        HoldRotation(Quaternion.Euler(horizontalStartEuler));
    }

    public void HoldVerticalWindup()
    {
        HoldRotation(Quaternion.Euler(verticalStartEuler));
    }

    public void HoldThrustWindup()
    {
        HoldRotation(Quaternion.Euler(thrustStartEuler));
    }

    private void HoldRotation(Quaternion rotation)
    {
        if (wrist == null) return;

        active = Swing.None;
        wrist.localRotation = idleRotation * rotation;
    }

    public void ReturnToIdle()
    {
        active = Swing.None;

        if (wrist != null)
            wrist.localRotation = idleRotation;
    }

    private void Update()
    {
        if (applySwordSettingsEveryFrame)
            ApplySwordSettings();

        if (active == Swing.None || wrist == null) return;

        elapsed += Time.deltaTime;

        float t = Mathf.Clamp01(elapsed / duration);

        // Smooth ease-in/ease-out, better than pure ease-out for sword swings.
        float eased = t * t * (3f - 2f * t);

        wrist.localRotation = Quaternion.Slerp(fromRot, toRot, eased);

        if (t >= 1f)
            active = Swing.None;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sword != null)
            ApplySwordSettings();
    }
#endif
}