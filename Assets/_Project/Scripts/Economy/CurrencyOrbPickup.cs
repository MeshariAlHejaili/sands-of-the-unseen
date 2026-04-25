using System;
using UnityEngine;

public class CurrencyOrbPickup : MonoBehaviour
{
    [Header("Currency")]
    [Tooltip("Currency amount awarded when this orb is collected.")]
    [Min(1)]
    [SerializeField] private int value = 1;

    [Tooltip("Horizontal pickup radius around the player in world units.")]
    [Min(0f)]
    [SerializeField] private float collectRadius = 1.25f;

    [Space]
    [Header("Animation")]
    [Tooltip("Orb rotation speed in degrees per second.")]
    [Min(0f)]
    [SerializeField] private float rotationSpeed = 90f;

    [Tooltip("Vertical bob distance in world units.")]
    [Min(0f)]
    [SerializeField] private float bobAmplitude = 0.15f;

    [Tooltip("Vertical bob oscillation frequency in cycles per second.")]
    [Min(0f)]
    [SerializeField] private float bobFrequency = 2f;

    private PlayerCurrencyWallet playerWallet;
    private Transform playerTransform;
    private Vector3 startPosition;
    private Action<CurrencyOrbPickup> onReturn;
    private bool returned;

    private void OnEnable()
    {
        startPosition = transform.position;
        ResolvePlayer();
    }

    public void Init(int amount, Action<CurrencyOrbPickup> returnCallback)
    {
        SetValue(amount);
        onReturn = returnCallback;
        returned = false;
        startPosition = transform.position;
    }

    public void SetValue(int amount)
    {
        value = Mathf.Max(1, amount);
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        Animate();
        if (playerTransform == null || playerWallet == null) ResolvePlayer();

        if (playerTransform == null || playerWallet == null)
        {
            return;
        }

        Vector3 offset = playerTransform.position - transform.position;
        offset.y = 0f;

        if (offset.sqrMagnitude <= collectRadius * collectRadius)
        {
            playerWallet.AddCurrency(value);
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        if (returned) return;

        returned = true;

        if (onReturn != null)
            onReturn.Invoke(this);
        else
            Destroy(gameObject);
    }

    private void Animate()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        Vector3 bobbedPosition = startPosition;
        bobbedPosition.y += Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = bobbedPosition;
    }

    private void ResolvePlayer()
    {
        if (playerTransform != null && playerWallet != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(GameTags.Player);
        if (playerObject == null)
        {
            return;
        }

        playerTransform = playerObject.transform;
        playerWallet = playerObject.GetComponent<PlayerCurrencyWallet>();
    }
}
