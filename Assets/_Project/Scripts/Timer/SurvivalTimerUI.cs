using TMPro;
using UnityEngine;

public class SurvivalTimerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SurvivalTimer survivalTimer;
    [SerializeField] private TMP_Text timerText;

    private void Awake()
    {
        if (timerText == null)
        {
            timerText = GetComponent<TMP_Text>();
        }

        if (survivalTimer == null)
        {
            survivalTimer = FindFirstObjectByType<SurvivalTimer>();
        }
    }

    private void OnEnable()
    {
        if (survivalTimer != null)
        {
            survivalTimer.TimerChanged += HandleTimerChanged;
            HandleTimerChanged(survivalTimer.RemainingTime);
        }
    }

    private void OnDisable()
    {
        if (survivalTimer != null)
        {
            survivalTimer.TimerChanged -= HandleTimerChanged;
        }
    }

    private void HandleTimerChanged(float remainingSeconds)
    {
        int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
        int seconds = Mathf.FloorToInt(remainingSeconds % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}