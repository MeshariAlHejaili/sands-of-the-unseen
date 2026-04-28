using UnityEngine;

/// <summary>
/// Translates player gameplay events into Animator parameters. Owns no gameplay state;
/// purely a presentation layer that subscribes to PlayerMovement (and future combat/health) events.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Source Components")]
    [Tooltip("Player movement component this controller listens to. Auto-found on parent if left empty.")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Locomotion Blending")]
    [Tooltip("How quickly MoveX/MoveY values smooth toward target. Lower = snappier, higher = smoother.")]
    [Range(0f, 0.5f)]
    [SerializeField] private float locomotionSmoothing = 0.1f;

    [Tooltip("Threshold below which movement input is treated as idle.")]
    [Range(0f, 1f)]
    [SerializeField] private float movementDeadzone = 0.1f;

    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    private Animator animator;
    private Transform aimRoot;
    private Vector3 latestWorldInput;
    private bool isDashing;

    private Vector2 currentLocomotion;
    private Vector2 locomotionVelocity;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (playerMovement == null)
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
        }

        // The transform that defines "facing direction" — usually the player root that PlayerAim rotates.
        aimRoot = playerMovement != null ? playerMovement.transform : transform;
    }

    private void OnEnable()
    {
        if (playerMovement == null)
        {
            Debug.LogError($"{nameof(PlayerAnimationController)} requires a {nameof(PlayerMovement)} reference.", this);
            enabled = false;
            return;
        }

        playerMovement.MoveInputChanged += HandleMoveInputChanged;
        playerMovement.DashStarted += HandleDashStarted;
        playerMovement.DashEnded += HandleDashEnded;
    }

    private void OnDisable()
    {
        if (playerMovement == null)
        {
            return;
        }

        playerMovement.MoveInputChanged -= HandleMoveInputChanged;
        playerMovement.DashStarted -= HandleDashStarted;
        playerMovement.DashEnded -= HandleDashEnded;
    }

    private void Update()
    {
        UpdateLocomotion();
    }

    private void HandleMoveInputChanged(Vector3 worldInput)
    {
        latestWorldInput = worldInput;
    }

    private void HandleDashStarted()
    {
        isDashing = true;
    }

    private void HandleDashEnded()
    {
        isDashing = false;
    }

    private void UpdateLocomotion()
    {
        // Freeze blend tree input during dash so we don't get a strafe pop while dashing.
        Vector3 effectiveInput = isDashing ? Vector3.zero : latestWorldInput;

        // Convert world input into character-local space so strafes/back animations align with facing direction.
        Vector3 localInput = aimRoot.InverseTransformDirection(effectiveInput);
        Vector2 target = new Vector2(localInput.x, localInput.z);

        currentLocomotion = Vector2.SmoothDamp(currentLocomotion, target, ref locomotionVelocity, locomotionSmoothing);

        animator.SetFloat(MoveXHash, currentLocomotion.x);
        animator.SetFloat(MoveYHash, currentLocomotion.y);
        animator.SetBool(IsMovingHash, effectiveInput.sqrMagnitude > movementDeadzone * movementDeadzone);
    }
}