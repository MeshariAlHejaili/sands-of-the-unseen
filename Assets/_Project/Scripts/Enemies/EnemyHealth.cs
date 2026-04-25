using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private float currentHealth;
    private float maxHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;

    public event Action<float, float, float> Damaged; // amount, current, max
    public event Action Died;

    public void Init(float max)
    {
        maxHealth = Mathf.Max(1f, max);
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        Damaged?.Invoke(amount, currentHealth, maxHealth);

        if (currentHealth <= 0f)
            Died?.Invoke();
    }
}
