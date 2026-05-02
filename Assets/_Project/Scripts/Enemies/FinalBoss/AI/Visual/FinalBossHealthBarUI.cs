using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FinalBossHealthBarUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private TMP_Text healthText;

    [Header("Settings")]
    [SerializeField] private string bossName = "Final Boss";

    private FinalBossController boss;

    private void OnDisable()
    {
        UnbindBoss();
    }

    public void Show(FinalBossController targetBoss)
    {
        if (targetBoss == null) return;

        gameObject.SetActive(true);

        UnbindBoss();

        boss = targetBoss;

        if (bossNameText != null)
            bossNameText.text = bossName;

        if (boss.Health != null)
            boss.Health.Died += HandleBossDied;

        Refresh();
    }

    public void Hide()
    {
        UnbindBoss();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (boss == null || boss.Health == null)
            return;

        float current = boss.Health.CurrentHealth;
        float max = boss.Health.MaxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void HandleBossDied()
    {
        Hide();
    }

    private void UnbindBoss()
    {
        if (boss != null && boss.Health != null)
            boss.Health.Died -= HandleBossDied;

        boss = null;
    }
}