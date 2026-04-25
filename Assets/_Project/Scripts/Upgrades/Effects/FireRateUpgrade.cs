using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Fire Rate Upgrade")]
public class FireRateUpgrade : UpgradeDefinition
{
    [Header("Effect")]
    [Tooltip("Flat increase added to the player's fire rate in bullets per second.")]
    [Min(0f)]
    [SerializeField] private float fireRateBonus = 1f;

    public override void Apply(GameObject player)
    {
        var stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;
        stats.AddFireRate(fireRateBonus);
    }
}
