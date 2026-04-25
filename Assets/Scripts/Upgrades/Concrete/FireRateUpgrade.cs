using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Fire Rate Upgrade")]
public class FireRateUpgrade : UpgradeDefinition
{
    public float fireRateBonus = 1f;

    public override void Apply(GameObject player)
    {
        var stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;
        stats.fireRate += fireRateBonus;
    }
}
