using UnityEngine;

public class EnemyFoodDrop : MonoBehaviour
{
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
        if (enemyController.foodPrefabs != null && enemyController.foodPrefabs.Length > 0)
        {
            int index = Random.Range(0, enemyController.foodPrefabs.Length);
            GameObject selectedFood = enemyController.foodPrefabs[index];

            Instantiate(selectedFood, transform.position, Quaternion.identity);

            if (LevelGoalManager.Instance != null)
            {
                LevelGoalManager.Instance.FruitSpawned();
            }
        }

        if (LevelGoalManager.Instance != null)
        {
            LevelGoalManager.Instance.EnemyDefeated();
        }

        Destroy(gameObject);
    }
}
