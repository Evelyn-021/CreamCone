using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public int score;
    public int lives = 3;

    public static int CurrentScore => Instance != null ? Instance.score : 0;
    public static int CurrentLives => Instance != null ? Instance.lives : 3;

    public static GameSession Ensure()
    {
        if (Instance != null) return Instance;

        GameObject sessionObject = new GameObject("GameSession");
        return sessionObject.AddComponent<GameSession>();
    }

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

    public void ResetRun(int startingLives)
    {
        score = 0;
        lives = startingLives;
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void SetLives(int value)
    {
        lives = Mathf.Max(0, value);
    }
}
