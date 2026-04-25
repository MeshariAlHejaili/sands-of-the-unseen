using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Speed Upgrade")]
public class SpeedUpgrade : UpgradeDefinition
{
    public float moveSpeedBonus = 1f;
    public float sprintSpeedBonus = 1.5f;

    public override void Apply(GameObject player)
    {
        var stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;
        stats.moveSpeed += moveSpeedBonus;
        stats.sprintSpeed += sprintSpeedBonus;
    }
}
