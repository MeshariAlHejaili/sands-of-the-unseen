using UnityEngine;

public abstract class UpgradeDefinition : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;

    public abstract void Apply(GameObject player);
}
