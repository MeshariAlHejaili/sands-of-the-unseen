using System.Collections.Generic;
using UnityEngine;

public class UpgradeMenuUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Root panel GameObject shown while the upgrade selection menu is active.")]
    [SerializeField] private GameObject panel;

    [Tooltip("Card UI slots used to display the offered upgrades.")]
    [SerializeField] private UpgradeCardUI[] cards;

    [Tooltip("Upgrade manager that applies the selected upgrade.")]
    [SerializeField] private UpgradeManager upgradeManager;

    [Space]
    [Header("Card Visuals")]
    [Tooltip("Optional sprite assigned to all upgrade card backgrounds when the menu opens.")]
    [SerializeField] private Sprite cardBackgroundSprite;

    [Space]
    [Header("Audio")]
    [Tooltip("Audio clip played once when a new set of random upgrades appears.")]
    [SerializeField] private AudioClip menuOpenSound;

    [Tooltip("Volume multiplier for the upgrade menu open sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float menuOpenVolume = 0.5f;

    [Tooltip("Audio clip played once when the player selects an upgrade card.")]
    [SerializeField] private AudioClip cardSelectedSound;

    [Tooltip("Volume multiplier for the upgrade card selected sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float cardSelectedVolume = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (panel == null)
        {
            Debug.LogWarning("UpgradeMenuUI: 'panel' is not assigned in the Inspector.", this);
            return;
        }

        panel.SetActive(false);

        if (menuOpenSound != null || cardSelectedSound != null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    public void Show(IReadOnlyList<UpgradeOffer> upgrades)
    {
        if (panel == null)
        {
            return;
        }

        panel.SetActive(true);

        if (audioSource != null && menuOpenSound != null)
        {
            audioSource.PlayOneShot(menuOpenSound, menuOpenVolume);
        }

        for (int i = 0; i < cards.Length; i++)
        {
            bool hasUpgrade = i < upgrades.Count;
            cards[i].gameObject.SetActive(hasUpgrade);

            if (hasUpgrade)
            {
                cards[i].Setup(upgrades[i], upgradeManager, cardBackgroundSprite);
            }
        }
    }

    public void Hide()
    {
        if (panel == null)
        {
            return;
        }

        panel.SetActive(false);
    }

    public void PlaySelectionSound()
    {
        if (audioSource != null && cardSelectedSound != null)
        {
            audioSource.PlayOneShot(cardSelectedSound, cardSelectedVolume);
        }
    }
}
