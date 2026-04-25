using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaBar : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Image fillImage;

    private void Start()
    {
        if (playerStats == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerStats = player.GetComponent<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.StaminaChanged += OnStaminaChanged;
            OnStaminaChanged(playerStats.CurrentStamina, playerStats.maxStamina);
        }
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.StaminaChanged -= OnStaminaChanged;
    }

    private void OnStaminaChanged(float current, float max)
    {
        fillImage.fillAmount = max > 0f ? current / max : 0f;
    }
}
