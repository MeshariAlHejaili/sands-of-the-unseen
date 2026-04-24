using UnityEngine;

public class CurrencyOrbPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;
    [SerializeField] private float collectRadius = 1.25f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobFrequency = 2f;

    private PlayerCurrencyWallet playerWallet;
    private Transform playerTransform;
    private Vector3 startPosition;

    private void OnEnable()
    {
        startPosition = transform.position;
        ResolvePlayer();
    }

    public void SetValue(int amount)
    {
        value = Mathf.Max(1, amount);
    }

    private void Update()
    {
        Animate();
        ResolvePlayer();

        if (playerTransform == null || playerWallet == null)
        {
            return;
        }

        Vector3 offset = playerTransform.position - transform.position;
        offset.y = 0f;

        if (offset.sqrMagnitude <= collectRadius * collectRadius)
        {
            playerWallet.AddCurrency(value);
            Destroy(gameObject);
        }
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

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            return;
        }

        playerTransform = playerObject.transform;
        playerWallet = playerObject.GetComponent<PlayerCurrencyWallet>();
    }
}
