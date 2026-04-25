using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;

    [Header("Stamina Settings")]
    public float maxStamina = 100f; 
    public float staminaRegenPerSecond = 15f; // 15 units per second
    public float staminaDrainPerSecond = 20f; // 20 units per second 
    public float secondsToWaitBeforeRegen = 1.5f; 

    [Header("Dash Settings")]
    public float dashStaminaCost = 30f;
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;

    private float currentStamina;
    private float timeAtLastStaminaUse;

    [Header("Combat Settings")]
    public float bulletDamage = 10f;
    public float fireRate = 5f; // Bullets per second

    void Start()
    {
        currentStamina = maxStamina;
    }

    void Update()
    {
        HandleStaminaRegeneration();
    }

    /// Governs the logic for refilling the stamina bar over time.
    private void HandleStaminaRegeneration()
    {
        // 1. Check if the cooldown period has passed
        bool hasCooldownEnded = Time.time >= timeAtLastStaminaUse + secondsToWaitBeforeRegen;
        
        // 2. Check if we actually need to regenerate
        bool isNotAtMaxStamina = currentStamina < maxStamina;

        if (hasCooldownEnded && isNotAtMaxStamina)
        {
            // Calculation: (Total * %) * time passed
            float amountToRecover = staminaRegenPerSecond * Time.deltaTime;
            
            currentStamina = Mathf.Clamp(currentStamina + amountToRecover, 0, maxStamina);
        }
    }

    /// Reduces stamina and resets the regeneration timer. Returns true if the action is allowed.
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            timeAtLastStaminaUse = Time.time;
            return true;
        }
        return false;
    }

}