using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Maximum player health points at scene start.")]
    [Min(1f)]
    [SerializeField] private float maxHealth = 100f;

    [Space]
    [Header("Death Behavior")]
    [Tooltip("If enabled, assigned player control behaviours are disabled when the player dies.")]
    [SerializeField] private bool disableControlsOnDeath = true;

    [Tooltip("Behaviours disabled when the player dies, such as movement, aiming, and shooting.")]
    [SerializeField] private MonoBehaviour[] behavioursToDisableOnDeath;

    [Tooltip("If enabled, player renderers and colliders are hidden when the player dies.")]
    [SerializeField] private bool hidePlayerOnDeath = true;

    private float currentHealth;
    private bool isShieldActive;
    private Collider[] cachedColliders;
    private Renderer[] cachedRenderers;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsShieldActive => isShieldActive;
    public bool IsDead { get; private set; }

    public event Action<float, float> HealthChanged;
    public event Action<float, float, float> Damaged;
    public event Action Died;

    private void Awake()
    {
        currentHealth = maxHealth;
        cachedColliders = GetComponentsInChildren<Collider>(true);
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
    }

    private void Start()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f)
        {
            return;
        }

        if (isShieldActive)
        {
            isShieldActive = false;
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);
        Damaged?.Invoke(amount, currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void IncreaseMaxHealth(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        maxHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ActivateShield()
    {
        if (IsDead)
        {
            return;
        }

        isShieldActive = true;
    }

    private void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        if (disableControlsOnDeath)
        {
            DisableAssignedBehaviours();
        }

        if (hidePlayerOnDeath)
        {
            SetPlayerVisible(false);
        }

        Died?.Invoke();
        Debug.Log("Player died.");
    }

    private void DisableAssignedBehaviours()
    {
        if (behavioursToDisableOnDeath == null)
        {
            return;
        }

        for (int i = 0; i < behavioursToDisableOnDeath.Length; i++)
        {
            if (behavioursToDisableOnDeath[i] != null)
            {
                behavioursToDisableOnDeath[i].enabled = false;
            }
        }
    }

    private void SetPlayerVisible(bool isVisible)
    {
        if (cachedRenderers != null)
        {
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null)
                {
                    cachedRenderers[i].enabled = isVisible;
                }
            }
        }

        if (cachedColliders != null)
        {
            for (int i = 0; i < cachedColliders.Length; i++)
            {
                if (cachedColliders[i] != null)
                {
                    cachedColliders[i].enabled = isVisible;
                }
            }
        }
    }
}
