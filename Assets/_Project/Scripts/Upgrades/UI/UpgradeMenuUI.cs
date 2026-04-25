using System.Collections.Generic;
using UnityEngine;

public class UpgradeMenuUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Root panel GameObject shown while the upgrade selection menu is active.")]
    [SerializeField] private GameObject panel;

    [Tooltip("Card UI slots used to display the offered upgrades.")]
    [SerializeField] private UpgradeCardUI[] cards;

    [Tooltip("Upgrade manager that applies the selected upgrade.")]
    [SerializeField] private UpgradeManager upgradeManager;

    private void Awake()
    {
        if (panel == null)
        {
            Debug.LogWarning("UpgradeMenuUI: 'panel' is not assigned in the Inspector.", this);
            return;
        }
        panel.SetActive(false);
    }

    public void Show(List<UpgradeDefinition> upgrades)
    {
        if (panel == null) return;
        panel.SetActive(true);
        for (int i = 0; i < cards.Length; i++)
        {
            bool hasUpgrade = i < upgrades.Count;
            cards[i].gameObject.SetActive(hasUpgrade);
            if (hasUpgrade)
                cards[i].Setup(upgrades[i], upgradeManager);
        }
    }

    public void Hide()
    {
        if (panel == null) return;
        panel.SetActive(false);
    }
}
