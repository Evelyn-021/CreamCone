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

    private bool isInvulnerable = false;
    public float invulnerableTime = 1.5f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateLivesUI();
    }

    public void LoseLife()
{
    if (isInvulnerable) return;

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

    yield return new WaitForSeconds(0.6f);

    lives--;
    UpdateLivesUI();

    if (lives <= 0)
    {
        RestartLevel();
        yield break;
    }

    RespawnPlayer();

    yield return new WaitForSeconds(invulnerableTime);

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

    private void RespawnPlayer()
{
    if (player != null && respawnPoint != null)
    {
        player.position = respawnPoint.position;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.isDead = false;
        }
    }
}

    private System.Collections.IEnumerator Invulnerability()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerableTime);
        isInvulnerable = false;
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}