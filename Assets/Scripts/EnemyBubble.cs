using UnityEngine;

public class EnemyBubble : MonoBehaviour
{
    private EnemyController enemyController;
    private float trappedTimer;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
    }

    public void UpdateBubble()
    {
        trappedTimer -= Time.fixedDeltaTime;

        float wave = Mathf.Sin(Time.time * 2f) * 0.25f;

        Vector2 moveDirection = new Vector2(
            enemyController.currentBubbleDirection.x,
            enemyController.currentBubbleDirection.y + wave
        ).normalized;

        enemyController.rb.linearVelocity = moveDirection * enemyController.bubbleSpeed;

        if (trappedTimer <= 0f)
        {
            ReleaseEnemy();
        }
    }

    public void TrapEnemy()
    {
        if (enemyController.currentState == EnemyController.EnemyState.TrappedBubble ||
            enemyController.currentState == EnemyController.EnemyState.SpinningToFood) return;

        enemyController.currentState = EnemyController.EnemyState.TrappedBubble;
        enemyController.currentSpeed = 0f;
        trappedTimer = enemyController.trappedTime;
        enemyController.verticalPatrol.ResetVerticalPatrol();

        enemyController.currentBubbleDirection = enemyController.bubbleDirection;
        enemyController.currentBubbleDirection.x = Mathf.Abs(enemyController.currentBubbleDirection.x) *
            (enemyController.movingRight ? 1f : -1f);

        enemyController.rb.gravityScale = 0f;
        enemyController.rb.linearVelocity = Vector2.zero;

        enemyController.enemyCollider.isTrigger = true;

        enemyController.anim.SetInteger("stateAnim", 2);
    }

    private void ReleaseEnemy()
    {
        enemyController.currentState = EnemyController.EnemyState.Angry;
        enemyController.currentSpeed = enemyController.angrySpeed;

        enemyController.rb.gravityScale = enemyController.originalGravity;
        enemyController.rb.linearVelocity = Vector2.zero;

        enemyController.enemyCollider.isTrigger = false;
        enemyController.IgnoreCollisionsWithOtherEnemies();

        enemyController.anim.SetInteger("stateAnim", 3);
    }

    public void ChangeBubbleDirection(string zoneName)
    {
        if (zoneName.Contains("Left"))
        {
            enemyController.currentBubbleDirection = new Vector2(1f, 0.15f).normalized;
        }
        else if (zoneName.Contains("Right"))
        {
            enemyController.currentBubbleDirection = new Vector2(-1f, 0.15f).normalized;
        }
        else if (zoneName.Contains("Top"))
        {
            enemyController.currentBubbleDirection =
                new Vector2(enemyController.currentBubbleDirection.x, -0.4f).normalized;
        }
        else if (zoneName.Contains("Bottom"))
        {
            enemyController.currentBubbleDirection =
                new Vector2(enemyController.currentBubbleDirection.x, 0.4f).normalized;
        }
    }

    public void BounceBubble(Collider2D limit)
    {
        Vector2 directionToEnemy = transform.position - limit.transform.position;

        if (Mathf.Abs(directionToEnemy.x) > Mathf.Abs(directionToEnemy.y))
        {
            enemyController.currentBubbleDirection.x *= -1f;
        }
        else
        {
            enemyController.currentBubbleDirection.y *= -1f;
        }

        enemyController.currentBubbleDirection.Normalize();
    }
}
