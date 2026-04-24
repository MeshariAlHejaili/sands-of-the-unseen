using System;
using UnityEngine;

public class PlayerCurrencyWallet : MonoBehaviour
{
    [SerializeField] private int startingCurrency;

    private int currentCurrency;

    public int CurrentCurrency => currentCurrency;

    public event Action<int> CurrencyChanged;

    private void Awake()
    {
        currentCurrency = Mathf.Max(0, startingCurrency);
    }

    private void Start()
    {
        CurrencyChanged?.Invoke(currentCurrency);
    }

    public void AddCurrency(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentCurrency += amount;
        CurrencyChanged?.Invoke(currentCurrency);
    }
}
