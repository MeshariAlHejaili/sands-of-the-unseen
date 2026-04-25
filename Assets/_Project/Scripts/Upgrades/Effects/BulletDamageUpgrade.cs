using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Bullet Damage Upgrade")]
public class BulletDamageUpgrade : UpgradeDefinition
{
    [Header("Effect")]
    [Tooltip("Flat damage added to the player's bullet damage in health points.")]
    [Min(0f)]
    [SerializeField] private float damageBonus = 5f;

    public override void Apply(GameObject player)
    {
        var stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;
        stats.AddBulletDamage(damageBonus);
    }
}
