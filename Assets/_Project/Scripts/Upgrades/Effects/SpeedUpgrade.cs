using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Speed Upgrade")]
public class SpeedUpgrade : UpgradeDefinition
{
    [Header("Effect")]
    [Tooltip("Flat speed added to the player's walking movement in world units per second.")]
    [Min(0f)]
    [SerializeField] private float moveSpeedBonus = 1f;

    [Tooltip("Flat speed added to the player's sprint movement in world units per second.")]
    [Min(0f)]
    [SerializeField] private float sprintSpeedBonus = 1.5f;

    public override void Apply(GameObject player)
    {
        var stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;
        stats.AddMoveSpeed(moveSpeedBonus);
        stats.AddSprintSpeed(sprintSpeedBonus);
    }
}
