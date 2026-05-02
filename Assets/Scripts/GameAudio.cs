using UnityEngine;
using UnityEngine.SceneManagement;

public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Header("Clips")]
    public AudioClip coinClip;
    public AudioClip attackClip;
    public AudioClip deadClip;
    public AudioClip jumpClip;
    public AudioClip popEnemyClip;
    public AudioClip powerUpClip;
    public AudioClip respawnClip;

    [Header("Music")]
    public AudioClip menuMusicClip;
    public AudioClip gameMusicClip;
    public string menuSceneName = "MainMenu";
    public string firstLevelSceneName = "SampleScene";
    public string secondLevelSceneName = "Level2";

    [Header("Volume")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.45f;

    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureMusicSource();
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMusicForScene(scene.name);
    }

    public void PlayCoin()
    {
        Play(coinClip);
    }

    public void PlayAttack()
    {
        Play(attackClip);
    }

    public void PlayDead()
    {
        Play(deadClip);
    }

    public void PlayJump()
    {
        Play(jumpClip);
    }

    public void PlayPopEnemy()
    {
        Play(popEnemyClip);
    }

    public void PlayPowerUp()
    {
        Play(powerUpClip);
    }

    public void PlayRespawn()
    {
        Play(respawnClip);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null) return;

        Vector3 position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    private void EnsureMusicSource()
    {
        if (musicSource != null) return;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;
    }

    private void UpdateMusicForScene(string sceneName)
    {
        EnsureMusicSource();
        musicSource.volume = musicVolume;

        if (sceneName == menuSceneName)
        {
            PlayMusic(menuMusicClip);
        }
        else if (sceneName == firstLevelSceneName || sceneName == secondLevelSceneName)
        {
            PlayMusic(gameMusicClip);
        }
        else
        {
            StopMusic();
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            StopMusic();
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.Play();
    }

    private void StopMusic()
    {
        if (musicSource == null) return;

        musicSource.Stop();
        musicSource.clip = null;
    }
}
