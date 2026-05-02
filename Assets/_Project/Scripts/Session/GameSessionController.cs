using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player health component that raises the defeat event when the player dies.")]
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("Survival timer that controls the wave survival phase duration in seconds.")]
    [SerializeField] private SurvivalTimer survivalTimer;

    [Space]
    [Header("Startup")]
    [Tooltip("When enabled, the session starts on the main menu instead of immediately entering gameplay.")]
    [SerializeField] private bool startAtMainMenu = true;

    private bool hasStartedSurvivalTimer;
    private GameSessionState stateBeforeUpgradeSelection = GameSessionState.Playing;

    public GameSessionState CurrentState { get; private set; }

    public event Action<GameSessionState> StateChanged;

    private void Awake()
    {
        SetState(startAtMainMenu ? GameSessionState.MainMenu : GameSessionState.Playing);
    }

    private void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (survivalTimer == null)
        {
            survivalTimer = FindFirstObjectByType<SurvivalTimer>();
        }

        if (playerHealth != null)
        {
            playerHealth.Died += HandlePlayerDied;
        }

        if (survivalTimer != null)
        {
            survivalTimer.TimerExpired += HandleTimerExpired;
        }

        if (!startAtMainMenu)
        {
            EnterPlaying();
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDied;
        }

        if (survivalTimer != null)
        {
            survivalTimer.TimerExpired -= HandleTimerExpired;
        }
    }

    public void StartGame()
    {
        EnterPlaying();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void EnterCountdown()
    {
        Time.timeScale = 0f;

        if (survivalTimer != null)
        {
            survivalTimer.StopTimer();
        }

        SetState(GameSessionState.Countdown);
    }

    public void EnterPlaying()
    {
        Time.timeScale = 1f;
        SetState(GameSessionState.Playing);
        StartSurvivalTimerIfNeeded();
    }

    public void EnterBossPhase()
    {
        Time.timeScale = 1f;

        if (survivalTimer != null)
        {
            survivalTimer.StopTimer();
        }

        SetState(GameSessionState.BossPhase);
        Debug.Log("BossPhase entered", this);
    }

    public void EnterUpgradeSelection()
    {
        if (CurrentState != GameSessionState.Playing && CurrentState != GameSessionState.BossPhase)
        {
            return;
        }

        Time.timeScale = 0f;
        stateBeforeUpgradeSelection = CurrentState;
        SetState(GameSessionState.UpgradeSelection);
    }

    public void ExitUpgradeSelection()
    {
        Time.timeScale = 1f;
        SetState(stateBeforeUpgradeSelection);
    }

    public void TriggerVictory()
    {
        Time.timeScale = 0f;

        if (survivalTimer != null)
        {
            survivalTimer.StopTimer();
        }

        SetState(GameSessionState.Victory);
    }

    public void TriggerDefeat()
    {
        Time.timeScale = 0f;

        if (survivalTimer != null)
        {
            survivalTimer.StopTimer();
        }

        SetState(GameSessionState.Defeat);
    }

    private void HandlePlayerDied()
    {
        if (CurrentState != GameSessionState.Playing && CurrentState != GameSessionState.BossPhase)
        {
            return;
        }

        TriggerDefeat();
    }

    private void HandleTimerExpired()
    {
        if (CurrentState != GameSessionState.Playing)
        {
            return;
        }

        EnterBossPhase();
    }

    private void StartSurvivalTimerIfNeeded()
    {
        if (hasStartedSurvivalTimer || survivalTimer == null)
        {
            return;
        }

        hasStartedSurvivalTimer = true;
        survivalTimer.BeginTimer();
    }

    private void SetState(GameSessionState newState)
    {
        if (CurrentState == newState)
        {
            return;
        }

        CurrentState = newState;
        StateChanged?.Invoke(CurrentState);
    }
}
