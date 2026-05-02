using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private Image fillImage;

    [Header("Style")]
    [SerializeField] private string bossName = "CHAIN WARDEN";
    [SerializeField] private Color phase1Color = new Color(0.8f, 0.05f, 0.08f);
    [SerializeField] private Color phase2Color = new Color(0.55f, 0f, 1f);

    private BossController boss;

    private void Awake()
    {
        if (root == null)
            root = gameObject;

        if (bossNameText != null)
            bossNameText.text = bossName;

        root.SetActive(false);
    }

    private void Update()
    {
        if (boss == null || boss.health == null || boss.health.IsDead)
        {
            root.SetActive(false);
            return;
        }

        root.SetActive(true);

        healthSlider.value = boss.HealthPercent;

        if (fillImage != null)
        {
            fillImage.color = boss.currentPhase == 2 ? phase2Color : phase1Color;
        }
    }

    public void SetBoss(BossController newBoss)
    {
        boss = newBoss;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }

        root.SetActive(boss != null);
    }

    public void ClearBoss()
    {
        boss = null;
        root.SetActive(false);
    }
}