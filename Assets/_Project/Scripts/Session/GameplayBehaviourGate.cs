using UnityEngine;

public class GameplayBehaviourGate : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Session controller whose state decides whether gameplay behaviours are enabled.")]
    [SerializeField] private GameSessionController sessionController;

    [Tooltip("Player GameObject containing movement, aim, and shooting behaviours.")]
    [SerializeField] private GameObject player;

    [Space]
    [Header("Extra Gameplay Systems")]
    [Tooltip("Additional gameplay-only behaviours enabled during active combat states.")]
    [SerializeField] private MonoBehaviour[] gameplayOnlyBehaviours;

    private PlayerMovement playerMovement;
    private PlayerAim playerAim;
    private PlayerShooting playerShooting;

    private void Awake()
    {
        if (sessionController == null)
        {
            sessionController = FindFirstObjectByType<GameSessionController>();
        }

        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            playerAim = player.GetComponent<PlayerAim>();
            playerShooting = player.GetComponent<PlayerShooting>();
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
        bool gameplayEnabled = state == GameSessionState.Playing || state == GameSessionState.BossPhase;

        if (playerMovement != null)
        {
            playerMovement.enabled = gameplayEnabled;
        }

        if (playerAim != null)
        {
            playerAim.enabled = gameplayEnabled;
        }

        if (playerShooting != null)
        {
            playerShooting.enabled = gameplayEnabled;
        }

        foreach (MonoBehaviour behaviour in gameplayOnlyBehaviours)
        {
            if (behaviour != null)
            {
                behaviour.enabled = gameplayEnabled;
            }
        }
    }
}
