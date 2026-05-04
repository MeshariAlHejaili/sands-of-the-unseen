using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player input, stamina spending, and dash timing while delegating collision-safe movement to PlayerCollisionMotor.
/// </summary>
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerCollisionMotor))]
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// Fired every frame with the current world-space movement input direction (not normalized; raw axis values).
    /// </summary>
    public event Action<Vector3> MoveInputChanged;

    /// <summary>
    /// Fired when a dash starts.
    /// </summary>
    public event Action DashStarted;

    /// <summary>
    /// Fired when a dash ends, whether by completing or being interrupted.
    /// </summary>
    public event Action DashEnded;

    [Header("Audio")]
    [Tooltip("Audio clip played once when the player starts a dash.")]
    [SerializeField] private AudioClip dashSound;

    [Tooltip("Dash sound playback volume from 0 to 1.")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;

    private PlayerStats stats;
    private PlayerCollisionMotor collisionMotor;
    private PlayerInputReader inputReader;
    private AudioSource audioSource;
    private Coroutine dashCoroutine;
    private bool isDashing;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        collisionMotor = GetComponent<PlayerCollisionMotor>();
        inputReader = PlayerInputReader.GetOrAdd(gameObject);
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

        if (isDashing)
        {
            isDashing = false;
            DashEnded?.Invoke();
        }
    }

    private void Update()
    {
        if (isDashing) return;

        HandleMovement();

        if (inputReader.WasDashPressedThisFrame)
        {
            AttemptDash();
        }
    }

    private void HandleMovement()
    {
        Vector2 moveInput = inputReader.MoveInput;
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        MoveInputChanged?.Invoke(moveDirection);

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
            Vector2 moveInput = inputReader.MoveInput;
            Vector3 dashDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

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
        DashStarted?.Invoke();

        if (dashSound != null)
        {
            audioSource.PlayOneShot(dashSound, volume);
        }

        try
        {
            float remainingDistance = stats.DashDistance;
            float dashDuration = Mathf.Max(0.01f, stats.DashDuration);
            float dashSpeed = remainingDistance / dashDuration;
            float elapsed = 0f;

            while (remainingDistance > 0f && elapsed < dashDuration)
            {
                float dt = Time.deltaTime;
                float stepDistance = Mathf.Min(remainingDistance, dashSpeed * dt);

                if (stepDistance <= Mathf.Epsilon)
                {
                    elapsed += dt;
                    yield return null;
                    continue;
                }

                Vector3 requestedDisplacement = direction * stepDistance;
                Vector3 movedDisplacement = collisionMotor.Move(requestedDisplacement, false, out RaycastHit blockingHit);

                float movedMag = movedDisplacement.magnitude;

                // Guard against NaN / Infinity from the motor
                if (float.IsNaN(movedMag) || float.IsInfinity(movedMag))
                {
                    break;
                }

                remainingDistance -= movedMag;
                elapsed += dt;

                // Only break when truly blocked (essentially zero movement)
                if (blockingHit.collider != null && movedMag < 1e-5f)
                {
                    break;
                }

                yield return null;
            }
        }
        finally
        {
            isDashing = false;
            dashCoroutine = null;
            DashEnded?.Invoke();
        }
    }

    private bool TryUseSprintStamina()
    {
        if (!inputReader.IsSprintHeld)
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
