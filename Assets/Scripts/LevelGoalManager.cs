using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoalManager : MonoBehaviour
{
    public static LevelGoalManager Instance;
    private bool levelCompleted;

    [Header("Objetivo del nivel")]
    public int enemiesAlive;
    public int fruitsRemaining;

    [Header("Siguiente nivel")]
    public string nextSceneName = "Level2";

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        enemiesAlive = FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length;
        fruitsRemaining = GameObject.FindGameObjectsWithTag("Food").Length;
    }

    public void EnemyDefeated()
    {
        EnemyDefeated(null);
    }

    public void EnemyDefeated(EnemyController defeatedEnemy)
    {
        enemiesAlive--;
        MakeLastEnemyGroupAngry(defeatedEnemy);
        CheckLevelComplete();
    }

    public void FruitSpawned()
    {
        fruitsRemaining++;
    }

    public void FruitCollected()
    {
        fruitsRemaining--;
        CheckLevelComplete();
    }

    private void CheckLevelComplete()
    {
        if (!levelCompleted && enemiesAlive <= 0 && fruitsRemaining <= 0)
        {
            levelCompleted = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void MakeLastEnemyGroupAngry(EnemyController defeatedEnemy)
    {
        if (defeatedEnemy == null) return;

        string defeatedGroup = defeatedEnemy.EnemyGroup;
        EnemyController lastEnemyInGroup = null;
        int enemiesInGroup = 0;

        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (EnemyController enemy in enemies)
        {
            if (enemy == null ||
                enemy == defeatedEnemy ||
                enemy.currentState == EnemyController.EnemyState.SpinningToFood ||
                enemy.EnemyGroup != defeatedGroup)
            {
                continue;
            }

            enemiesInGroup++;
            lastEnemyInGroup = enemy;
        }

        if (enemiesInGroup == 1 && lastEnemyInGroup != null)
        {
            lastEnemyInGroup.BecomeAngry();
        }
    }
}
