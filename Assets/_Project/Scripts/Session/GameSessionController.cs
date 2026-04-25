using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SurvivalTimer survivalTimer;

    [Header("Startup")]
    [SerializeField] private bool startAtMainMenu = true;

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

        if (CurrentState == GameSessionState.Playing)
        {
            StartGameplay();
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
        StartGameplay();
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

    private void StartGameplay()
    {
        Time.timeScale = 1f;

        SetState(GameSessionState.Playing);

        if (survivalTimer != null)
        {
            survivalTimer.BeginTimer();
        }
    }

    private void HandlePlayerDied()
    {
        if (CurrentState != GameSessionState.Playing)
        {
            return;
        }

        Time.timeScale = 0f;
        SetState(GameSessionState.Defeat);
    }

    private void HandleTimerExpired()
    {
        if (CurrentState != GameSessionState.Playing)
        {
            return;
        }

        Time.timeScale = 0f;
        SetState(GameSessionState.Victory);
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
