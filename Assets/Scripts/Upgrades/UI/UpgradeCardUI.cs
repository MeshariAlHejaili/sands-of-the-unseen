using UnityEngine;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button selectButton;

    private UpgradeDefinition boundUpgrade;
    private UpgradeManager upgradeManager;

    public void Setup(UpgradeDefinition upgrade, UpgradeManager manager)
    {
        boundUpgrade = upgrade;
        upgradeManager = manager;
        nameText.text = upgrade.upgradeName;
        descriptionText.text = upgrade.description;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelected);
    }

    private void OnSelected()
    {
        upgradeManager.ApplyUpgrade(boundUpgrade);
    }
}
