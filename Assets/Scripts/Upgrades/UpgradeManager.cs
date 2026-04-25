using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCurrencyWallet playerWallet;
    [SerializeField] private UpgradeLibrary upgradeLibrary;
    [SerializeField] private UpgradeMenuUI upgradeMenuUI;

    [Header("Threshold Scaling")]
    [SerializeField] private int baseThreshold = 2;
    [SerializeField] private int thresholdGrowth = 2;

    private int upgradeLevel;
    private int coinsCollectedSinceLastUpgrade;
    private int lastKnownCurrency;
    private bool isUpgradeActive;

    public event Action<List<UpgradeDefinition>> UpgradesReady;

    private int CurrentThreshold => baseThreshold + upgradeLevel * thresholdGrowth;

    private IEnumerator Start()
    {
        // Skip the wallet's init broadcast that fires in its own Start()
        yield return null;
        lastKnownCurrency = playerWallet.CurrentCurrency;
        playerWallet.CurrencyChanged += OnCurrencyChanged;
    }

    private void OnDestroy()
    {
        if (playerWallet != null)
            playerWallet.CurrencyChanged -= OnCurrencyChanged;
    }

    private void OnCurrencyChanged(int totalCurrency)
    {
        if (isUpgradeActive) return;

        int coinsJustAdded = totalCurrency - lastKnownCurrency;
        lastKnownCurrency = totalCurrency;
        coinsCollectedSinceLastUpgrade += coinsJustAdded;

        if (coinsCollectedSinceLastUpgrade >= CurrentThreshold)
            TriggerUpgrade();
    }

    private void TriggerUpgrade()
    {
        isUpgradeActive = true;
        Time.timeScale = 0f;
        var picks = PickRandomUpgrades(3);
        upgradeMenuUI.Show(picks);
        UpgradesReady?.Invoke(picks);
    }

    public void ApplyUpgrade(UpgradeDefinition chosen)
    {
        chosen.Apply(playerWallet.gameObject);
        coinsCollectedSinceLastUpgrade = 0;
        upgradeLevel++;
        isUpgradeActive = false;
        Time.timeScale = 1f;
        upgradeMenuUI.Hide();
    }

    private List<UpgradeDefinition> PickRandomUpgrades(int count)
    {
        var pool = new List<UpgradeDefinition>(upgradeLibrary.availableUpgrades);
        var result = new List<UpgradeDefinition>();
        count = Mathf.Min(count, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
}
