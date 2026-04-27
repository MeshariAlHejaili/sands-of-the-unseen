using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player input, stamina spending, and dash timing while delegating collision-safe movement to PlayerCollisionMotor.
/// </summary>
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerCollisionMotor))]
public class PlayerMovement : MonoBehaviour
{  
    [Header("Audio")]
    [Tooltip("Audio clip played once when the player starts a dash.")]
    [SerializeField] private AudioClip dashSound;

    [Tooltip("Dash sound playback volume from 0 to 1.")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;

    private PlayerStats stats;
    private PlayerCollisionMotor collisionMotor;
    private AudioSource audioSource;
    private Coroutine dashCoroutine;
    private bool isDashing;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        collisionMotor = GetComponent<PlayerCollisionMotor>();
        audioSource = GetComponent<AudioSource>();

        if (collisionMotor == null)
        {
            collisionMotor = gameObject.AddComponent<PlayerCollisionMotor>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnDisable()
    {
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            dashCoroutine = null;
        }

        isDashing = false;
    }

    private void Update()
    {
        if (isDashing) return;

        HandleMovement();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            AttemptDash();
        }
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);
        if (moveDirection.sqrMagnitude <= 0.01f)
        {
            return;
        }

        bool isSprinting = TryUseSprintStamina();
        float currentSpeed = isSprinting ? stats.SprintSpeed : stats.MoveSpeed;
        Vector3 displacement = moveDirection.normalized * currentSpeed * Time.deltaTime;

        collisionMotor.Move(displacement, true, out _);
    }

    private void AttemptDash()
    {
        if (stats.UseStamina(stats.DashStaminaCost))
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 dashDirection = new Vector3(h, 0, v).normalized;

            // If standing still, dash in the direction the player is facing
            if (dashDirection.sqrMagnitude < 0.01f)
            {
                dashDirection = GetForwardDashDirection();
            }

            dashCoroutine = StartCoroutine(DashRoutine(dashDirection.normalized));
        }
    }

    private IEnumerator DashRoutine(Vector3 direction)
    {
        isDashing = true;
        float remainingDistance = stats.DashDistance;
        float dashDuration = Mathf.Max(0.01f, stats.DashDuration);
        float dashSpeed = remainingDistance / dashDuration;

        if (dashSound != null)
        {
            audioSource.PlayOneShot(dashSound, volume);
        }

        while (remainingDistance > 0f)
        {
            float stepDistance = Mathf.Min(remainingDistance, dashSpeed * Time.unscaledDeltaTime);
            if (stepDistance <= Mathf.Epsilon)
            {
                yield return null;
                continue;
            }

            Vector3 requestedDisplacement = direction * stepDistance;
            Vector3 movedDisplacement = collisionMotor.Move(requestedDisplacement, false, out RaycastHit blockingHit);

            remainingDistance -= movedDisplacement.magnitude;

            if (blockingHit.collider != null || movedDisplacement.sqrMagnitude < requestedDisplacement.sqrMagnitude - 0.0001f)
            {
                break;
            }

            yield return null; 
        }

        isDashing = false;
        dashCoroutine = null;
    }

    private bool TryUseSprintStamina()
    {
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            return false;
        }

        float drainThisFrame = stats.StaminaDrainPerSecond * Time.deltaTime;
        return stats.UseStamina(drainThisFrame);
    }

    private Vector3 GetForwardDashDirection()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= 0.01f)
        {
            return Vector3.forward;
        }

        return forward.normalized;
    }
}
