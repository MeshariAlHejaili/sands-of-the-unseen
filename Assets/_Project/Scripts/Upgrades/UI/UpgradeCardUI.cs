using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const string CommonRarityColor = "#00ff00";
    private const string RareRarityColor = "#00ffff";
    private const string LegendaryRarityColor = "#fbff00";

    [Header("References")]
    [Tooltip("Text component that displays the upgrade name.")]
    [SerializeField] private Text nameText;

    [Tooltip("Text component that displays the upgrade rarity and effect description.")]
    [SerializeField] private Text descriptionText;

    [Tooltip("Button the player clicks to select this upgrade.")]
    [SerializeField] private Button selectButton;

    [Tooltip("Optional image used as the upgrade card background. If empty, the button target graphic is used.")]
    [SerializeField] private Image backgroundImage;

    [Space]
    [Header("Text Styling")]
    [Tooltip("Optional font applied to the upgrade title text at the top of the card.")]
    [SerializeField] private Font titleFont;

    [Tooltip("Shared base color for the card text. Rich-text rarity colors still override only the rarity word.")]
    [SerializeField] private Color cardTextColor = Color.white;

    [Tooltip("Vertical offset in UI units applied to the title text so it can sit slightly lower on the card.")]
    [Range(-40f, 40f)]
    [SerializeField] private float titleVerticalOffset = -8f;

    [Space]
    [Header("Background")]
    [Tooltip("Tint applied to the assigned background sprite. Keep this white to show the sprite's original colors.")]
    [SerializeField] private Color backgroundTint = Color.white;

    [Space]
    [Header("Hover Animation")]
    [Tooltip("Duration in seconds for the card hover scale animation.")]
    [Range(0f, 0.5f)]
    [SerializeField] private float hoverAnimationDuration = 0.15f;

    [Tooltip("Scale multiplier applied while the pointer is hovering over the card.")]
    [Range(1f, 1.25f)]
    [SerializeField] private float hoverScaleMultiplier = 1.05f;

    [Space]
    [Header("Audio")]
    [Tooltip("Audio clip played once when the pointer enters this upgrade card.")]
    [SerializeField] private AudioClip hoverSound;

    [Tooltip("Volume multiplier for the upgrade card hover sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float hoverVolume = 0.4f;

    private UpgradeOffer boundUpgrade;
    private UpgradeManager upgradeManager;
    private UpgradeMenuUI upgradeMenuUI;
    private AudioSource audioSource;
    private RectTransform rectTransform;
    private RectTransform nameTextRectTransform;
    private Vector3 initialScale;
    private Vector2 nameTextInitialAnchoredPosition;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        initialScale = rectTransform != null ? rectTransform.localScale : transform.localScale;
        nameTextRectTransform = nameText != null ? nameText.rectTransform : null;
        upgradeMenuUI = GetComponentInParent<UpgradeMenuUI>();

        if (nameTextRectTransform != null)
        {
            nameTextInitialAnchoredPosition = nameTextRectTransform.anchoredPosition;
        }

        if (backgroundImage == null && selectButton != null)
        {
            backgroundImage = selectButton.targetGraphic as Image;
        }

        if (hoverSound != null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        if (nameText != null)
        {
            nameText.raycastTarget = false;
            nameText.color = cardTextColor;
        }

        if (descriptionText != null)
        {
            descriptionText.raycastTarget = false;
            descriptionText.color = cardTextColor;
        }
    }

    public void Setup(UpgradeOffer upgrade, UpgradeManager manager, Sprite cardBackgroundSprite)
    {
        boundUpgrade = upgrade;
        upgradeManager = manager;

        if (nameText != null)
        {
            nameText.text = upgrade.DisplayName;
            nameText.color = cardTextColor;

            if (titleFont != null)
            {
                nameText.font = titleFont;
            }
        }

        if (nameTextRectTransform != null)
        {
            nameTextRectTransform.anchoredPosition = nameTextInitialAnchoredPosition + new Vector2(0f, titleVerticalOffset);
        }

        if (descriptionText != null)
        {
            descriptionText.supportRichText = true;
            descriptionText.text = BuildCardBody(upgrade);
            descriptionText.color = cardTextColor;
        }

        if (backgroundImage != null && cardBackgroundSprite != null)
        {
            backgroundImage.sprite = cardBackgroundSprite;
            backgroundImage.color = backgroundTint;
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelected);
        }

        ResetHoverVisuals();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AnimateScale(initialScale * hoverScaleMultiplier, Ease.OutBack);

        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound, hoverVolume);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateScale(initialScale, Ease.OutCubic);
    }

    private void OnDisable()
    {
        DOTween.Kill(rectTransform);
        ResetHoverVisuals();
    }

    private void OnSelected()
    {
        if (upgradeMenuUI != null)
        {
            upgradeMenuUI.PlaySelectionSound();
        }

        upgradeManager.ApplyUpgrade(boundUpgrade);
    }

    private void AnimateScale(Vector3 targetScale, Ease ease)
    {
        if (rectTransform == null)
        {
            return;
        }

        DOTween.Kill(rectTransform);
        rectTransform
            .DOScale(targetScale, hoverAnimationDuration)
            .SetEase(ease)
            .SetUpdate(true)
            .SetId(rectTransform);
    }

    private void ResetHoverVisuals()
    {
        if (rectTransform == null)
        {
            transform.localScale = initialScale;
            return;
        }

        rectTransform.localScale = initialScale;
    }

    private static string BuildCardBody(UpgradeOffer upgrade)
    {
        string rarityColor = upgrade.Rarity switch
        {
            UpgradeRarity.Common => CommonRarityColor,
            UpgradeRarity.Rare => RareRarityColor,
            UpgradeRarity.Legendary => LegendaryRarityColor,
            _ => CommonRarityColor
        };

        return $"<color={rarityColor}>{upgrade.Rarity}</color>\n{upgrade.Description}";
    }
}
