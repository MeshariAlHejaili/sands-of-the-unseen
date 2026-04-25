using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Upgrade Library")]
public class UpgradeLibrary : ScriptableObject
{
    public List<UpgradeDefinition> availableUpgrades;
}
