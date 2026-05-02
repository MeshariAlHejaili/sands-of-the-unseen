using UnityEngine;

public class FinalBossHitbox : MonoBehaviour
{
    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponentInParent<EnemyHealth>();

        if (health == null)
            Debug.LogError("[FinalBossHitbox] No EnemyHealth found in parent.", this);
    }

    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        if (health == null) return;

        health.TakeDamage(amount);
    }
}