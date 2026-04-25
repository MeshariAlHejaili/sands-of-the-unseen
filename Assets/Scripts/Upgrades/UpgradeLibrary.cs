using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Upgrade Library")]
public class UpgradeLibrary : ScriptableObject
{
    [SerializeField] private List<UpgradeDefinition> availableUpgrades;
    public IReadOnlyList<UpgradeDefinition> AvailableUpgrades => availableUpgrades;
}
