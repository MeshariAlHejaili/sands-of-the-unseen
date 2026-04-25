using System.Collections.Generic;
using UnityEngine;

public class UpgradeMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private UpgradeCardUI[] cards;
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
