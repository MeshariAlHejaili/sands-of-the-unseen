using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuMusicController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameSessionController sessionController;

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private float volume = 0.35f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;

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
        if (state == GameSessionState.MainMenu)
        {
            PlayMenuMusic();
        }
        else
        {
            StopMenuMusic();
        }
    }

    private void PlayMenuMusic()
    {
        if (menuMusic == null)
        {
            return;
        }

        if (audioSource.clip != menuMusic)
        {
            audioSource.clip = menuMusic;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void StopMenuMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}