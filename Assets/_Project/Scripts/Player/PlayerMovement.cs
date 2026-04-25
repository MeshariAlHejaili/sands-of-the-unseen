using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{  
    [Header("Audio")]
    [Tooltip("Audio clip played once when the player starts a dash.")]
    [SerializeField] private AudioClip dashSound;

    [Tooltip("Dash sound playback volume from 0 to 1.")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;

    private PlayerStats stats;
    private AudioSource audioSource;
    private bool isDashing = false;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        audioSource = GetComponent<AudioSource>();

        // Ensure an AudioSource exists on the Player
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        // Stop all input/logic if the player is currently in a dash
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

        Vector3 movement = new Vector3(horizontal, 0, vertical);
        bool isSprinting = false;

        // 1. Process movement and stamina if the player is actually pressing WASD
        if (movement.magnitude > 0.1f)
        {
            // 2. Only check for sprint/drain stamina if we are actively moving and holding Shift Key
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                float drainThisFrame = stats.StaminaDrainPerSecond * Time.deltaTime;
                
                if (stats.UseStamina(drainThisFrame))
                {
                    isSprinting = true;
                }
            }

            // 3. Apply the correct speed
            float currentSpeed = isSprinting ? stats.SprintSpeed : stats.MoveSpeed;

            // 4. Move the character
            transform.Translate(movement.normalized * currentSpeed * Time.deltaTime, Space.World);
        }
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
                dashDirection = transform.forward;
            }

            StartCoroutine(DashRoutine(dashDirection));
        }
    }

    private IEnumerator DashRoutine(Vector3 direction)
    {
        isDashing = true;
        
        float startTime = Time.unscaledTime;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + (direction * stats.DashDistance);

        if (dashSound != null)
        {
            audioSource.PlayOneShot(dashSound, volume);
        }

        while (Time.unscaledTime < startTime + stats.DashDuration)
        {
            // Calculate how far through the dash we are (0.0 to 1.0)
            float elapsed = (Time.unscaledTime - startTime) / stats.DashDuration;
            
            // Move smoothly between start and end
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed);
            
            yield return null; 
        }

        transform.position = targetPos;
        isDashing = false;
    }

}
