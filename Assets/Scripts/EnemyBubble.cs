using UnityEngine;

public class EnemyBubble : MonoBehaviour
{
    [Header("Zonas de direccion")]
    public string bubbleTurnZoneFilter = "";

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
        if (enemyController.verticalPatrol != null)
        {
            enemyController.verticalPatrol.ResetVerticalPatrol();
        }

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
        enemyController.BecomeAngry();
    }

    public void ChangeBubbleDirection(Collider2D zone)
    {
        if (!CanUseBubbleTurnZone(zone)) return;

        ChangeBubbleDirection(zone.name);
    }

    private bool CanUseBubbleTurnZone(Collider2D zone)
    {
        if (string.IsNullOrWhiteSpace(bubbleTurnZoneFilter)) return true;

        Transform current = zone.transform;

        while (current != null)
        {
            if (current.name.Contains(bubbleTurnZoneFilter))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void ChangeBubbleDirection(string zoneName)
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
