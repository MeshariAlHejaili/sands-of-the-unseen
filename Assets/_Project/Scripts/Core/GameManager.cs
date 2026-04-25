using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("Player health component observed for death and scene restart.")]
    [SerializeField] private PlayerHealth playerHealth;

    [Space]
    [Header("Restart")]
    [Tooltip("Delay in seconds between player death and scene reload.")]
    [Min(0f)]
    [SerializeField] private float restartDelay = 1.25f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
            playerHealth.Died += OnPlayerDied;
        else
            Debug.LogWarning("GameManager: no PlayerHealth found in scene.", this);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.Died -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        StartCoroutine(RestartSceneRoutine());
    }

    private IEnumerator RestartSceneRoutine()
    {
        if (restartDelay > 0f)
            yield return new WaitForSeconds(restartDelay);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
