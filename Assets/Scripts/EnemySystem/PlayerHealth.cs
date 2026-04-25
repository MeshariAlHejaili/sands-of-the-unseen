using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool disableControlsOnDeath = true;
    [SerializeField] private MonoBehaviour[] behavioursToDisableOnDeath;
    [SerializeField] private bool hidePlayerOnDeath = true;

    private float currentHealth;
    private Collider[] cachedColliders;
    private Renderer[] cachedRenderers;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead { get; private set; }

    public event Action<float, float> HealthChanged;
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

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
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
