using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaRegenPerSecond = 15f;
    public float staminaDrainPerSecond = 20f;
    public float secondsToWaitBeforeRegen = 1.5f;

    [Header("Dash Settings")]
    public float dashStaminaCost = 30f;
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;

    [Header("Combat Settings")]
    public float bulletDamage = 10f;
    public float fireRate = 5f;

    private float currentStamina;
    private float timeAtLastStaminaUse;

    public float CurrentStamina => currentStamina;

    public event Action<float, float> StaminaChanged;

    void Start()
    {
        currentStamina = maxStamina;
        StaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    void Update()
    {
        HandleStaminaRegeneration();
    }

    private void HandleStaminaRegeneration()
    {
        bool hasCooldownEnded = Time.time >= timeAtLastStaminaUse + secondsToWaitBeforeRegen;
        bool isNotAtMaxStamina = currentStamina < maxStamina;

        if (hasCooldownEnded && isNotAtMaxStamina)
        {
            currentStamina = Mathf.Clamp(currentStamina + staminaRegenPerSecond * Time.deltaTime, 0, maxStamina);
            StaminaChanged?.Invoke(currentStamina, maxStamina);
        }
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            timeAtLastStaminaUse = Time.time;
            StaminaChanged?.Invoke(currentStamina, maxStamina);
            return true;
        }
        return false;
    }
}