using System;
using UnityEngine;

public class SurvivalTimer : MonoBehaviour
{
    [Header("Timer")]
    [Tooltip("Duration in seconds before the survival phase expires.")]
    [Min(1f)]
    [SerializeField] private float survivalDurationSeconds = 300f;

    private float remainingTime;
    private bool isRunning;

    public float RemainingTime => remainingTime;
    public float SurvivalDurationSeconds => survivalDurationSeconds;

    public event Action<float> TimerChanged;
    public event Action TimerExpired;

    private void Awake()
    {
        remainingTime = survivalDurationSeconds;
        TimerChanged?.Invoke(remainingTime);
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(0f, remainingTime);

        TimerChanged?.Invoke(remainingTime);

        if (remainingTime <= 0f)
        {
            isRunning = false;
            TimerExpired?.Invoke();
        }
    }

    public void BeginTimer()
    {
        remainingTime = survivalDurationSeconds;
        isRunning = true;
        TimerChanged?.Invoke(remainingTime);
    }

    public void StopTimer()
    {
        isRunning = false;
    }
}
