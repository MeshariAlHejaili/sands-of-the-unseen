using UnityEngine;

public abstract class UpgradeDefinition : ScriptableObject
{
    [Header("Display")]
    [Tooltip("Display name shown on this upgrade card in the upgrade menu.")]
    [SerializeField] private string upgradeName;

    [Tooltip("Short description shown on this upgrade card in the upgrade menu.")]
    [TextArea]
    [SerializeField] private string description;

    public string UpgradeName => upgradeName;
    public string Description => description;

    public abstract void Apply(GameObject player);
}
