using UnityEngine;

public class GameScreenRouter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameSessionController sessionController;

    [Header("Screens")]
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject hudScreen;
    [SerializeField] private GameObject victoryScreen;
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
        SetScreen(hudScreen, state == GameSessionState.Playing);
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