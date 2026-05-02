using UnityEngine;

public class GameScreenRouter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Session controller whose state drives screen visibility.")]
    [SerializeField] private GameSessionController sessionController;

    [Space]
    [Header("Screens")]
    [Tooltip("Screen shown while the session is waiting at the main menu.")]
    [SerializeField] private GameObject mainMenuScreen;

    [Tooltip("HUD shown during active combat states.")]
    [SerializeField] private GameObject hudScreen;

    [Tooltip("Screen shown during the pre-combat countdown.")]
    [SerializeField] private GameObject countdownScreen;

    [Tooltip("Screen shown while upgrade options are available and gameplay is paused.")]
    [SerializeField] private GameObject upgradeSelectionScreen;

    [Tooltip("Overlay shown while the boss phase is active.")]
    [SerializeField] private GameObject bossOverlayScreen;

    [Tooltip("Screen shown after the player wins.")]
    [SerializeField] private GameObject victoryScreen;

    [Tooltip("Screen shown after the player is defeated.")]
    [SerializeField] private GameObject defeatScreen;

    private void Awake()
    {
        if (sessionController == null)
        {
            sessionController = FindFirstObjectByType<GameSessionController>();
        }
    }

    private void OnEnable()
    {
        if (sessionController != null)
        {
            sessionController.StateChanged += HandleStateChanged;
            HandleStateChanged(sessionController.CurrentState);
        }
    }

    private void OnDisable()
    {
        if (sessionController != null)
        {
            sessionController.StateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameSessionState state)
    {
        SetScreen(mainMenuScreen, state == GameSessionState.MainMenu);
        SetScreen(hudScreen, state == GameSessionState.Playing || state == GameSessionState.BossPhase);
        SetScreen(countdownScreen, state == GameSessionState.Countdown);
        SetScreen(upgradeSelectionScreen, state == GameSessionState.UpgradeSelection);
        SetScreen(bossOverlayScreen, state == GameSessionState.BossPhase);
        SetScreen(victoryScreen, state == GameSessionState.Victory);
        SetScreen(defeatScreen, state == GameSessionState.Defeat);
    }

    private void SetScreen(GameObject screen, bool isActive)
    {
        if (screen != null)
        {
            screen.SetActive(isActive);
        }
    }
}
