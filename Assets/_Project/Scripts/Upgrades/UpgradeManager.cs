using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player wallet used to track and spend currency for upgrade choices.")]
    [SerializeField] private PlayerCurrencyWallet playerWallet;

    [Tooltip("Upgrade library that generates the stat and rarity combinations shown to the player.")]
    [SerializeField] private UpgradeLibrary upgradeLibrary;

    [Tooltip("Upgrade menu UI used to display the generated upgrade choices.")]
    [SerializeField] private UpgradeMenuUI upgradeMenuUI;

    [Tooltip("Session controller used to enter and exit the upgrade pause state.")]
    [SerializeField] private GameSessionController session;

    [Space]
    [Header("Currency Requirements")]
    [Tooltip("Currency required to trigger the first upgrade choice in a new run.")]
    [Min(1)]
    [SerializeField] private int startingCurrencyRequirement = 10;

    [Tooltip("Additional currency required after each selected upgrade.")]
    [Min(1)]
    [SerializeField] private int currencyRequirementIncrease = 5;

    private int currentCurrencyRequirement;
    private bool isUpgradeActive;

    private void Awake()
    {
        if (session == null)
        {
            session = FindFirstObjectByType<GameSessionController>();
        }

        currentCurrencyRequirement = startingCurrencyRequirement;
    }

    private void Start()
    {
        if (playerWallet != null)
        {
            playerWallet.CurrencyChanged += OnCurrencyChanged;
        }

        if (session != null)
        {
            session.StateChanged += OnSessionStateChanged;
        }

        TryTriggerUpgrade();
    }

    private void OnDestroy()
    {
        if (playerWallet != null)
        {
            playerWallet.CurrencyChanged -= OnCurrencyChanged;
        }

        if (session != null)
        {
            session.StateChanged -= OnSessionStateChanged;
        }
    }

    private void OnCurrencyChanged(int currentCurrency)
    {
        if (isUpgradeActive)
        {
            return;
        }

        TryTriggerUpgrade();
    }

    private void OnSessionStateChanged(GameSessionState state)
    {
        if (state == GameSessionState.Playing || state == GameSessionState.BossPhase)
        {
            TryTriggerUpgrade();
        }
    }

    public void ApplyUpgrade(UpgradeOffer chosen)
    {
        if (chosen == null || playerWallet == null)
        {
            return;
        }

        if (!playerWallet.TrySpendCurrency(currentCurrencyRequirement))
        {
            return;
        }

        chosen.Apply(playerWallet.gameObject);
        currentCurrencyRequirement += currencyRequirementIncrease;
        isUpgradeActive = false;

        if (upgradeMenuUI != null)
        {
            upgradeMenuUI.Hide();
        }

        if (session != null)
        {
            session.ExitUpgradeSelection();
        }

        TryTriggerUpgrade();
    }

    private void TryTriggerUpgrade()
    {
        if (!CanTriggerUpgrade())
        {
            return;
        }

        TriggerUpgrade();
    }

    private bool CanTriggerUpgrade()
    {
        if (isUpgradeActive || playerWallet == null || upgradeLibrary == null || upgradeMenuUI == null || session == null)
        {
            return false;
        }

        bool canPauseForUpgrade = session.CurrentState == GameSessionState.Playing || session.CurrentState == GameSessionState.BossPhase;
        return canPauseForUpgrade && playerWallet.CurrentCurrency >= currentCurrencyRequirement;
    }

    private void TriggerUpgrade()
    {
        var picks = upgradeLibrary.CreateRandomOffers(3);

        if (picks.Count == 0)
        {
            return;
        }

        isUpgradeActive = true;
        session.EnterUpgradeSelection();
        upgradeMenuUI.Show(picks);
    }
}
