using UnityEngine;
using UnityEngine.SceneManagement;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance;

    [Header("Vidas")]
    public int lives = 3;

    [Header("UI")]
    public BitmapScoreUI livesUI;

    [Header("Jugador")]
    public Transform player;
    public Transform respawnPoint;
    public string gameOverSceneName = "GameOver";

    private bool isInvulnerable = false;
    public float invulnerableTime = 3f;
    public float respawnBlinkInterval = 0.15f;

    public bool IsPlayerInvulnerable
    {
        get
        {
            if (isInvulnerable) return true;

            if (player == null) return false;

            PlayerController pc = player.GetComponent<PlayerController>();
            return pc != null && pc.isWaterBubbleRiding;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        lives = GameSession.CurrentLives;
        UpdateLivesUI();
    }

    public void LoseLife()
{
    if (IsPlayerInvulnerable) return;

    StartCoroutine(LoseLifeRoutine());
}

private System.Collections.IEnumerator LoseLifeRoutine()
{
    isInvulnerable = true;

    PlayerController pc = player.GetComponent<PlayerController>();

    if (pc != null)
    {
        pc.Die();
    }

    if (GameAudio.Instance != null)
    {
        GameAudio.Instance.PlayDead();
    }

    yield return new WaitForSeconds(0.6f);

    lives--;
    GameSession.Ensure().SetLives(lives);
    UpdateLivesUI();

    if (lives <= 0)
    {
        GoToGameOver();
        yield break;
    }

    yield return RespawnPlayerRoutine();

    isInvulnerable = false;
}





    private void UpdateLivesUI()
    {
        if (livesUI != null)
        {
            livesUI.score = lives;
            livesUI.SendMessage("UpdateScoreUI", SendMessageOptions.DontRequireReceiver);
        }
    }

    private System.Collections.IEnumerator RespawnPlayerRoutine()
{
    if (player == null || respawnPoint == null)
    {
        yield break;
    }

    player.position = respawnPoint.position;

    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
    RigidbodyConstraints2D originalConstraints = RigidbodyConstraints2D.None;
    if (rb != null)
    {
        originalConstraints = rb.constraints;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = originalConstraints | RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
    }

    PlayerController pc = player.GetComponent<PlayerController>();
    if (pc != null)
    {
        pc.isDead = false;
        pc.SetRespawning(true);
    }

    if (GameAudio.Instance != null)
    {
        GameAudio.Instance.PlayRespawn();
    }

    yield return BlinkPlayer(invulnerableTime);

    if (pc != null)
    {
        pc.SetRespawning(false);
    }

    if (rb != null)
    {
        rb.constraints = originalConstraints;
        rb.linearVelocity = Vector2.zero;
    }
}

private System.Collections.IEnumerator BlinkPlayer(float duration)
{
    SpriteRenderer[] renderers = player.GetComponentsInChildren<SpriteRenderer>();
    float timer = 0f;
    bool visible = true;

    while (timer < duration)
    {
        visible = !visible;
        SetPlayerRenderersEnabled(renderers, visible);

        float waitTime = Mathf.Min(respawnBlinkInterval, duration - timer);
        yield return new WaitForSeconds(waitTime);
        timer += waitTime;
    }

    SetPlayerRenderersEnabled(renderers, true);
}

private void SetPlayerRenderersEnabled(SpriteRenderer[] renderers, bool enabled)
{
    foreach (SpriteRenderer spriteRenderer in renderers)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = enabled;
        }
    }
}

    private System.Collections.IEnumerator Invulnerability()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerableTime);
        isInvulnerable = false;
    }

    private void GoToGameOver()
    {
        SceneManager.LoadScene(gameOverSceneName);
    }
}
