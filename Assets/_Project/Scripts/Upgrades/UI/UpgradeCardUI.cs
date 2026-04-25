using UnityEngine;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Text component that displays the upgrade name.")]
    [SerializeField] private Text nameText;

    [Tooltip("Text component that displays the upgrade description.")]
    [SerializeField] private Text descriptionText;

    [Tooltip("Button the player clicks to select this upgrade.")]
    [SerializeField] private Button selectButton;

    private UpgradeDefinition boundUpgrade;
    private UpgradeManager upgradeManager;

    public void Setup(UpgradeDefinition upgrade, UpgradeManager manager)
    {
        boundUpgrade = upgrade;
        upgradeManager = manager;
        if (nameText != null) nameText.text = upgrade.UpgradeName;
        if (descriptionText != null) descriptionText.text = upgrade.Description;
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelected);
        }
    }

    private void OnSelected()
    {
        upgradeManager.ApplyUpgrade(boundUpgrade);
    }
}
