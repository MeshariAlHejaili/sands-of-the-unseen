using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player health component that provides health values and change events.")]
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("UI image whose fill amount displays the player's current health percentage.")]
    [SerializeField] private Image fillImage;

    private void Start()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.HealthChanged += OnHealthChanged;
            OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float current, float max)
    {
        fillImage.fillAmount = max > 0f ? current / max : 0f;
    }
}
