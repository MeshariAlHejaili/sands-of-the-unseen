using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Upgrade Library")]
public class UpgradeLibrary : ScriptableObject
{
    [Header("Upgrade Pool")]
    [Tooltip("List of upgrade definitions that can be randomly offered to the player.")]
    [SerializeField] private List<UpgradeDefinition> availableUpgrades;

    public IReadOnlyList<UpgradeDefinition> AvailableUpgrades => availableUpgrades;
}
