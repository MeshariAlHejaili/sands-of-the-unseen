using UnityEngine;

public class DamageableHitbox : MonoBehaviour, IDamageable
{
    private IDamageable owner;

    public bool IsAlive => owner != null && owner.IsAlive;

    private void Awake()
    {
        owner = FindOwnerDamageable();

        if (owner == null)
        {
            Debug.LogWarning("DamageableHitbox requires an IDamageable component on this GameObject or a parent.", this);
        }
    }

    public void TakeDamage(float amount)
    {
        owner?.TakeDamage(amount);
    }

    private IDamageable FindOwnerDamageable()
    {
        MonoBehaviour[] behaviours = GetComponentsInParent<MonoBehaviour>();

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != this && behaviours[i] is IDamageable damageable)
            {
                return damageable;
            }
        }

        return null;
    }
}
