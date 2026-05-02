using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class MenuButtonHover : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, 
    ISelectHandler, IDeselectHandler
{
    [Header("Frame & Icon References")]
    [Tooltip("Frame graphic tinted between idle and hover colors.")]
    [SerializeField] private Graphic frameImage;

    [Tooltip("Icon graphic scaled when the button is hovered or selected.")]
    [SerializeField] private Graphic iconImage;

    [Tooltip("Text label tinted between idle and hover colors.")]
    [SerializeField] private TMP_Text labelText;

    [Space]
    [Header("Glow Effect References")]
    [Tooltip("Rect transform for the glow line that fades in on hover.")]
    [SerializeField] private RectTransform glowLine;

    [Tooltip("Rect transform for the ember highlight that travels across the glow line.")]
    [SerializeField] private RectTransform ember;

    [Tooltip("Inner glow graphic faded in while the button is hovered or selected.")]
    [SerializeField] private Graphic innerGlow;

    [Space]
    [Header("Particle Effect (optional)")]
    [Tooltip("Particle system that plays while hovered. Should have a UIParticle component.")]
    [SerializeField] private ParticleSystem emberParticles;

    [Space]
    [Header("Colors")]
    [Tooltip("Frame color when the button is idle.")]
    [SerializeField] private Color idleColor      = new Color(0.79f, 0.66f, 0.46f, 0.5f);

    [Tooltip("Frame color when the button is hovered or selected.")]
    [SerializeField] private Color hoverColor     = new Color(1.0f,  0.55f, 0.25f, 1.0f);

    [Tooltip("Label color when the button is idle.")]
    [SerializeField] private Color labelIdle      = new Color(0.91f, 0.85f, 0.74f, 1.0f);

    [Tooltip("Label color when the button is hovered or selected.")]
    [SerializeField] private Color labelHover     = new Color(1.0f,  1.0f,  1.0f,  1.0f);

    [Space]
    [Header("Animation")]
    [Tooltip("Duration in seconds for hover fade and scale transitions.")]
    [Range(0f, 1f)]
    [SerializeField] private float fadeInDuration = 0.3f;

    [Tooltip("Duration in seconds for one ember travel pass across the glow line.")]
    [Range(0.1f, 5f)]
    [SerializeField] private float emberTravelDuration = 1.5f;

    [Tooltip("Multiplier applied to the icon scale while hovered or selected.")]
    [Range(1f, 1.5f)]
    [SerializeField] private float iconScaleHover = 1.1f;

    [Space]
    [Header("Audio (optional)")]
    [Tooltip("Audio clip played once when hover or selection starts.")]
    [SerializeField] private AudioClip hoverSound;

    [Tooltip("Volume multiplier for the hover sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float hoverVolume = 0.4f;
    
    private Vector3 iconStartScale;
    private float glowLineWidth;
    private float emberStartX;
    private float emberEndX;
    private bool isHovered;
    private AudioSource audioSource;
    private Sequence emberLoopSequence;
    private Graphic glowLineGraphic;
    private Graphic emberGraphic;
    
    private void Awake()
    {
        if (iconImage != null)
            iconStartScale = iconImage.transform.localScale;
        
        if (glowLine != null)
        {
            glowLineGraphic = glowLine.GetComponent<Graphic>();
            glowLineWidth = glowLine.rect.width;
        }
        
        if (ember != null)
        {
            emberGraphic = ember.GetComponent<Graphic>();
            emberStartX = ember.anchoredPosition.x;
            emberEndX = emberStartX + glowLineWidth;
        }
        
        if (frameImage != null) frameImage.color = idleColor;
        if (labelText != null) labelText.color = labelIdle;
        
        if (glowLineGraphic != null)
        {
            var c = glowLineGraphic.color;
            c.a = 0;
            glowLineGraphic.color = c;
        }
        
        if (emberGraphic != null)
        {
            var c = emberGraphic.color;
            c.a = 0;
            emberGraphic.color = c;
        }
        
        if (innerGlow != null)
        {
            var c = innerGlow.color;
            c.a = 0;
            innerGlow.color = c;
        }
        
        if (emberParticles != null)
        {
            emberParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        if (hoverSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    public void OnPointerEnter(PointerEventData e) => Activate();
    public void OnPointerExit(PointerEventData e) => Deactivate();
    public void OnSelect(BaseEventData e) => Activate();
    public void OnDeselect(BaseEventData e) => Deactivate();
    
    private void Activate()
    {
        if (isHovered) return;
        isHovered = true;
        
        DOTween.Kill(transform);
        
        if (frameImage != null)
            frameImage.DOColor(hoverColor, fadeInDuration).SetId(transform);
        
        if (labelText != null)
            labelText.DOColor(labelHover, fadeInDuration).SetId(transform);
        
        if (iconImage != null)
            iconImage.transform.DOScale(iconStartScale * iconScaleHover, fadeInDuration)
                .SetEase(Ease.OutBack)
                .SetId(transform);
        
        if (glowLineGraphic != null)
            glowLineGraphic.DOFade(1f, fadeInDuration).SetId(transform);
        
        if (innerGlow != null)
            innerGlow.DOFade(0.25f, fadeInDuration).SetId(transform);
        
        if (emberGraphic != null)
        {
            emberGraphic.DOFade(1f, fadeInDuration).SetId(transform);
            StartEmberLoop();
        }
        
        if (emberParticles != null)
        {
            emberParticles.Play(true);
        }
        
        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound, hoverVolume);
    }
    
    private void Deactivate()
    {
        if (!isHovered) return;
        isHovered = false;
        
        DOTween.Kill(transform);
        StopEmberLoop();
        
        if (frameImage != null)
            frameImage.DOColor(idleColor, fadeInDuration).SetId(transform);
        
        if (labelText != null)
            labelText.DOColor(labelIdle, fadeInDuration).SetId(transform);
        
        if (iconImage != null)
            iconImage.transform.DOScale(iconStartScale, fadeInDuration)
                .SetEase(Ease.OutCubic).SetId(transform);
        
        if (glowLineGraphic != null)
            glowLineGraphic.DOFade(0f, fadeInDuration).SetId(transform);
        
        if (innerGlow != null)
            innerGlow.DOFade(0f, fadeInDuration).SetId(transform);
        
        if (emberGraphic != null)
            emberGraphic.DOFade(0f, fadeInDuration).SetId(transform);
        
        if (emberParticles != null)
        {
            emberParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
    
    private void StartEmberLoop()
    {
        StopEmberLoop();
        if (ember == null) return;
        
        ember.anchoredPosition = new Vector2(emberStartX, ember.anchoredPosition.y);
        
        emberLoopSequence = DOTween.Sequence();
        emberLoopSequence.Append(
            ember.DOAnchorPosX(emberEndX, emberTravelDuration)
                 .SetEase(Ease.InOutSine)
        );
        emberLoopSequence.AppendCallback(() => {
            ember.anchoredPosition = new Vector2(emberStartX, ember.anchoredPosition.y);
        });
        emberLoopSequence.SetLoops(-1);
    }
    
    private void StopEmberLoop()
    {
        if (emberLoopSequence != null && emberLoopSequence.IsActive())
        {
            emberLoopSequence.Kill();
            emberLoopSequence = null;
        }
    }
    
    private void OnDisable()
    {
        DOTween.Kill(transform);
        StopEmberLoop();
        if (emberParticles != null)
            emberParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
