using System;
using UnityEngine;

/// <summary>
/// Boss stamina system. Mirrors the PlayerStats stamina pattern: TryConsume returns
/// false if not enough, regen has a delay after the last spend, and changes fire an event.
///
/// The Depleted event is the trigger for stagger — when the boss tries an action it can't
/// afford in the middle of an ongoing drain (e.g., chain sweep), it gets staggered.
/// </summary>
[RequireComponent(typeof(FinalBossStats))]
public class FinalBossStamina : MonoBehaviour
{
    private FinalBossStats stats;
    private float current;
    private float lastSpendTime = -999f;

    public float Current => current;
    public float Max => stats != null ? stats.MaxStamina : 0f;
    public float Percent => Max > 0f ? current / Max : 0f;
    public bool IsEmpty => current <= 0f;

    /// <summary>Fires whenever stamina value changes. (current, max)</summary>
    public event Action<float, float> StaminaChanged;

    /// <summary>Fires the moment stamina hits 0.</summary>
    public event Action Depleted;

    private void Awake()
    {
        stats = GetComponent<FinalBossStats>();
    }

    private void Start()
    {
        current = stats.MaxStamina;
        StaminaChanged?.Invoke(current, stats.MaxStamina);
    }

    private void Update()
    {
        Regenerate();
    }

    private void Regenerate()
    {
        if (current >= stats.MaxStamina) return;
        if (Time.time < lastSpendTime + stats.StaminaRegenDelay) return;

        current = Mathf.Min(stats.MaxStamina, current + stats.StaminaRegenPerSecond * Time.deltaTime);
        StaminaChanged?.Invoke(current, stats.MaxStamina);
    }

    /// <summary>Attempts to spend stamina. Returns false if insufficient — caller must handle gracefully.</summary>
    public bool TryConsume(float amount)
    {
        if (amount <= 0f) return true;
        if (current < amount) return false;

        current -= amount;
        lastSpendTime = Time.time;
        StaminaChanged?.Invoke(current, stats.MaxStamina);

        if (current <= 0f)
        {
            current = 0f;
            Depleted?.Invoke();
        }
        return true;
    }

    /// <summary>True if the boss has at least 'amount' stamina available right now.</summary>
    public bool CanAfford(float amount) => current >= amount;

    /// <summary>Grants stamina without affecting the regen-delay timer. Used after stagger so the boss can act again.</summary>
    public void TopUp(float amount)
    {
        if (amount <= 0f) return;
        current = Mathf.Min(stats.MaxStamina, current + amount);
        StaminaChanged?.Invoke(current, stats.MaxStamina);
    }
}
