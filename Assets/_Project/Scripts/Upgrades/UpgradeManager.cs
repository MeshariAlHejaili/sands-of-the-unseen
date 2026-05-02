using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player wallet used to track collected currency toward the next upgrade.")]
    [SerializeField] private PlayerCurrencyWallet playerWallet;

    [Tooltip("Upgrade library containing the pool of upgrades that can be offered.")]
    [SerializeField] private UpgradeLibrary upgradeLibrary;

    [Tooltip("Upgrade menu UI used to display random upgrade choices.")]
    [SerializeField] private UpgradeMenuUI upgradeMenuUI;

    [Tooltip("Session controller used to enter and exit the upgrade pause state.")]
    [SerializeField] private GameSessionController session;

    [Space]
    [Header("Threshold Scaling")]
    [Tooltip("Currency required to trigger the first upgrade choice.")]
    [Min(1)]
    [SerializeField] private int baseThreshold = 2;

    [Tooltip("Additional currency required for each later upgrade level.")]
    [Min(0)]
    [SerializeField] private int thresholdGrowth = 2;

    private int upgradeLevel;
    private int coinsCollectedSinceLastUpgrade;
    private int lastKnownCurrency;
    private bool isUpgradeActive;

    private int CurrentThreshold => baseThreshold + upgradeLevel * thresholdGrowth;

    private void Awake()
    {
        if (session == null)
        {
            session = FindFirstObjectByType<GameSessionController>();
        }
    }

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
        session.EnterUpgradeSelection();
        var picks = PickRandomUpgrades(3);
        upgradeMenuUI.Show(picks);
    }

    public void ApplyUpgrade(UpgradeDefinition chosen)
    {
        chosen.Apply(playerWallet.gameObject);
        coinsCollectedSinceLastUpgrade = 0;
        upgradeLevel++;
        isUpgradeActive = false;
        session.ExitUpgradeSelection();
        upgradeMenuUI.Hide();
    }

    private List<UpgradeDefinition> PickRandomUpgrades(int count)
    {
        var pool = new List<UpgradeDefinition>(upgradeLibrary.AvailableUpgrades);
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
