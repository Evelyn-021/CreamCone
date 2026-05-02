using UnityEngine;

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

    [Header("Volume")]
    [Range(0f, 1f)] public float volume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
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
}
