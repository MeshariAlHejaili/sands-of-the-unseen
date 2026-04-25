using UnityEngine;
using UnityEngine.UI;

public class CurrencyCounterText : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player wallet that provides the current currency value; if empty, it is resolved by the Player tag.")]
    [SerializeField] private PlayerCurrencyWallet playerWallet;

    [Tooltip("UI text component that displays the current currency amount.")]
    [SerializeField] private Text targetText;

    [Space]
    [Header("Display")]
    [Tooltip("Text prefix shown before the numeric currency amount.")]
    [SerializeField] private string prefix = "Currency: ";

    private bool isSubscribed;

    private void Start()
    {
        if (targetText == null)
        {
            targetText = GetComponent<Text>();
        }

        TryBindWallet();
        Refresh();
    }

    private void Update()
    {
        if (playerWallet == null)
        {
            TryBindWallet();
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void TryBindWallet()
    {
        if (playerWallet == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
            {
                return;
            }

            playerWallet = playerObject.GetComponent<PlayerCurrencyWallet>();
        }

        if (playerWallet == null || isSubscribed)
        {
            return;
        }

        playerWallet.CurrencyChanged += HandleCurrencyChanged;
        isSubscribed = true;
        Refresh();
    }

    private void HandleCurrencyChanged(int currentAmount)
    {
        if (targetText != null)
        {
            targetText.text = prefix + currentAmount;
        }
    }

    private void Refresh()
    {
        if (targetText == null)
        {
            return;
        }

        int currentAmount = playerWallet != null ? playerWallet.CurrentCurrency : 0;
        targetText.text = prefix + currentAmount;
    }

    private void Unsubscribe()
    {
        if (playerWallet != null && isSubscribed)
        {
            playerWallet.CurrencyChanged -= HandleCurrencyChanged;
        }

        isSubscribed = false;
    }
}
