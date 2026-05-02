using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponentInParent<EnemyHealth>();

        if (health == null)
            Debug.LogError("[BossHitbox] No EnemyHealth found in parent.");
    }

    public void TakeDamage(float amount)
    {
        if (health == null || health.IsDead)
            return;

        Debug.Log($"[BossHitbox] Boss took {amount} damage. HP before: {health.CurrentHealth}");

        health.TakeDamage(amount);

        Debug.Log($"[BossHitbox] Boss HP after: {health.CurrentHealth}");
    }
}