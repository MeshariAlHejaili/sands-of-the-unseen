using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base walking movement speed in world units per second.")]
    [Min(0f)]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Sprint movement speed in world units per second.")]
    [Min(0f)]
    [SerializeField] private float sprintSpeed = 10f;

    [Space]
    [Header("Stamina Settings")]
    [Tooltip("Maximum stamina points available to the player.")]
    [Min(1f)]
    [SerializeField] private float maxStamina = 100f;

    [Tooltip("Stamina points restored per second after the regeneration delay.")]
    [Min(0f)]
    [SerializeField] private float staminaRegenPerSecond = 15f;

    [Tooltip("Stamina points consumed per second while sprinting.")]
    [Min(0f)]
    [SerializeField] private float staminaDrainPerSecond = 20f;

    [Tooltip("Delay in seconds after stamina use before regeneration can resume.")]
    [Min(0f)]
    [SerializeField] private float secondsToWaitBeforeRegen = 1.5f;

    [Space]
    [Header("Dash Settings")]
    [Tooltip("Stamina points required to perform one dash.")]
    [Min(0f)]
    [SerializeField] private float dashStaminaCost = 30f;

    [Tooltip("Dash travel distance in world units.")]
    [Min(0f)]
    [SerializeField] private float dashDistance = 5f;

    [Tooltip("Dash duration in seconds.")]
    [Min(0.01f)]
    [SerializeField] private float dashDuration = 0.2f;

    [Space]
    [Header("Combat Settings")]
    [Tooltip("Damage dealt by each bullet hit in health points.")]
    [Min(0f)]
    [SerializeField] private float bulletDamage = 10f;

    [Tooltip("Number of bullets the player can fire per second.")]
    [Min(0.01f)]
    [SerializeField] private float fireRate = 5f;

    private float currentStamina;
    private float timeAtLastStaminaUse;

    public float MoveSpeed => moveSpeed;
    public float SprintSpeed => sprintSpeed;
    public float MaxStamina => maxStamina;
    public float StaminaRegenPerSecond => staminaRegenPerSecond;
    public float StaminaDrainPerSecond => staminaDrainPerSecond;
    public float SecondsToWaitBeforeRegen => secondsToWaitBeforeRegen;
    public float DashStaminaCost => dashStaminaCost;
    public float DashDistance => dashDistance;
    public float DashDuration => dashDuration;
    public float BulletDamage => bulletDamage;
    public float FireRate => fireRate;
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

    public void AddMoveSpeed(float amount)
    {
        moveSpeed = Mathf.Max(0f, moveSpeed + amount);
    }

    public void AddSprintSpeed(float amount)
    {
        sprintSpeed = Mathf.Max(0f, sprintSpeed + amount);
    }

    public void AddBulletDamage(float amount)
    {
        bulletDamage = Mathf.Max(0f, bulletDamage + amount);
    }

    public void AddFireRate(float amount)
    {
        fireRate = Mathf.Max(0.01f, fireRate + amount);
    }
}
