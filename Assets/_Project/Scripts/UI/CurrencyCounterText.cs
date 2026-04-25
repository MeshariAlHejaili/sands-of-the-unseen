using UnityEngine;
using UnityEngine.UI;

public class CurrencyCounterText : MonoBehaviour
{
    [SerializeField] private PlayerCurrencyWallet playerWallet;
    [SerializeField] private Text targetText;
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
