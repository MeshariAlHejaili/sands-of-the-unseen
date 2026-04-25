using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Bullet Damage Upgrade")]
public class BulletDamageUpgrade : UpgradeDefinition
{
    public float damageBonus = 5f;

    public override void Apply(GameObject player)
    {
        var stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;
        stats.bulletDamage += damageBonus;
    }
}
