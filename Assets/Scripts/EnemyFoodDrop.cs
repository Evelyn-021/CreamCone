using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyFoodDrop : MonoBehaviour
{
    [Header("Drops raros")]
    public GameObject[] rareFoodPrefabs;
    [Range(0f, 1f)] public float rareFoodChance = 0.18f;

    [Header("Diamantes")]
    public GameObject[] fallingDiamondPrefabs;
    [Range(0f, 1f)] public float fallingDiamondChance = 0.25f;
    public float diamondSpawnMinX = -4.6f;
    public float diamondSpawnMaxX = 5.8f;
    public float diamondSpawnY = 2.55f;

    [Header("Water Bubble")]
    public string waterBubbleSceneName = "Level2";
    public GameObject[] waterBubblePrefabs;
    [Range(0f, 1f)] public float waterBubbleChance = 0.12f;

    [Header("Plataformas de fruta")]
    public string enemy2FoodPlatformName = "SolidGround2";
    public string enemy3FoodPlatformName = "SolidGround3";

    private EnemyController enemyController;
    private float spinTimer;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
    }

    public void PopEnemy()
    {
        if (enemyController.currentState != EnemyController.EnemyState.TrappedBubble) return;

        enemyController.currentState = EnemyController.EnemyState.SpinningToFood;
        spinTimer = enemyController.spinTime;

        enemyController.currentBubbleDirection = new Vector2(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 1f)
        ).normalized;

        enemyController.rb.gravityScale = 0f;
        enemyController.rb.linearVelocity = Vector2.zero;

        enemyController.enemyCollider.isTrigger = true;

        enemyController.anim.SetInteger("stateAnim", 4);

        if (GameAudio.Instance != null)
        {
            GameAudio.Instance.PlayPopEnemy();
        }
    }

    public void UpdateSpinToFood()
    {
        spinTimer -= Time.fixedDeltaTime;

        enemyController.rb.linearVelocity =
            enemyController.currentBubbleDirection.normalized * enemyController.spinMoveSpeed;

        if (enemyController.visual != null)
        {
            enemyController.visual.Rotate(0f, 0f, enemyController.spinSpeed * Time.fixedDeltaTime);
        }

        if (spinTimer <= 0f)
        {
            TurnIntoFood();
        }
    }

    private void TurnIntoFood()
    {
        GameObject selectedFood = ChooseFoodDrop();

        if (selectedFood != null)
        {
            GameObject food = Instantiate(selectedFood, transform.position, Quaternion.identity);
            FoodDrop foodDrop = food.GetComponent<FoodDrop>();
            if (foodDrop != null)
            {
                foodDrop.requiredLandingPlatformName = GetFoodLandingPlatformName();
                foodDrop.StartGuidedDropToPlatform();
            }

            if (LevelGoalManager.Instance != null)
            {
                LevelGoalManager.Instance.FruitSpawned();
            }
        }

        TrySpawnFallingDiamond();
        TrySpawnWaterBubble();

        if (LevelGoalManager.Instance != null)
        {
            LevelGoalManager.Instance.EnemyDefeated(enemyController);
        }

        Destroy(gameObject);
    }

    private GameObject ChooseFoodDrop()
    {
        if (rareFoodPrefabs != null &&
            rareFoodPrefabs.Length > 0 &&
            Random.value < rareFoodChance)
        {
            return rareFoodPrefabs[Random.Range(0, rareFoodPrefabs.Length)];
        }

        if (enemyController.foodPrefabs == null || enemyController.foodPrefabs.Length == 0)
        {
            return null;
        }

        return enemyController.foodPrefabs[Random.Range(0, enemyController.foodPrefabs.Length)];
    }

    private string GetFoodLandingPlatformName()
    {
        string groupName = enemyController.EnemyGroup;
        string objectName = enemyController.gameObject.name;

        if (MatchesEnemyName(groupName, objectName, "Enemy2"))
        {
            return enemy2FoodPlatformName;
        }

        if (MatchesEnemyName(groupName, objectName, "Enemy3"))
        {
            return enemy3FoodPlatformName;
        }

        return "";
    }

    private bool MatchesEnemyName(string groupName, string objectName, string enemyName)
    {
        return groupName.IndexOf(enemyName, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               objectName.IndexOf(enemyName, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void TrySpawnFallingDiamond()
    {
        if (fallingDiamondPrefabs == null ||
            fallingDiamondPrefabs.Length == 0 ||
            Random.value >= fallingDiamondChance)
        {
            return;
        }

        float spawnX = Random.Range(diamondSpawnMinX, diamondSpawnMaxX);
        Vector3 spawnPosition = new Vector3(spawnX, diamondSpawnY, transform.position.z);
        GameObject selectedDiamond = fallingDiamondPrefabs[Random.Range(0, fallingDiamondPrefabs.Length)];

        Instantiate(selectedDiamond, spawnPosition, Quaternion.identity);

        if (LevelGoalManager.Instance != null)
        {
            LevelGoalManager.Instance.FruitSpawned();
        }
    }

    private void TrySpawnWaterBubble()
    {
        if (SceneManager.GetActiveScene().name != waterBubbleSceneName)
        {
            return;
        }

        if (waterBubblePrefabs == null ||
            waterBubblePrefabs.Length == 0 ||
            Random.value >= waterBubbleChance)
        {
            return;
        }

        float spawnX = Random.Range(diamondSpawnMinX, diamondSpawnMaxX);
        Vector3 spawnPosition = new Vector3(spawnX, diamondSpawnY, transform.position.z);
        GameObject selectedWaterBubble = waterBubblePrefabs[Random.Range(0, waterBubblePrefabs.Length)];

        Instantiate(selectedWaterBubble, spawnPosition, Quaternion.identity);
    }
}
