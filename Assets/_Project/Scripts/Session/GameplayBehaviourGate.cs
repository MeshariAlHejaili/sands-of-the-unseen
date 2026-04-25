using UnityEngine;

public class GameplayBehaviourGate : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameSessionController sessionController;
    [SerializeField] private GameObject player;

    [Header("Extra Gameplay Systems")]
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
        bool gameplayEnabled = state == GameSessionState.Playing;

        if (playerMovement != null)
            playerMovement.enabled = gameplayEnabled;

        if (playerAim != null)
            playerAim.enabled = gameplayEnabled;

        if (playerShooting != null)
            playerShooting.enabled = gameplayEnabled;

        foreach (MonoBehaviour behaviour in gameplayOnlyBehaviours)
        {
            if (behaviour != null)
            {
                behaviour.enabled = gameplayEnabled;
            }
        }
    }
}