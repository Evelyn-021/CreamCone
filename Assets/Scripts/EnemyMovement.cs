using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private EnemyController enemyController;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
    }

    public void OnUpdate()
    {
        Move();
        CheckEdge();
    }

    private void Move()
    {
        float dir = enemyController.movingRight ? 1f : -1f;
        enemyController.rb.linearVelocity = new Vector2(
            dir * enemyController.currentSpeed,
            enemyController.rb.linearVelocity.y
        );
    }

    private void CheckEdge()
    {
        if (enemyController.groundCheck == null) return;

        RaycastHit2D hit = Physics2D.Raycast(
            enemyController.groundCheck.position,
            Vector2.down,
            enemyController.groundCheckDistance,
            enemyController.groundMask
        );

        if (hit.collider == null)
        {
            Flip();
        }
    }

    private void Flip()
    {
        enemyController.movingRight = !enemyController.movingRight;
        UpdateGroundCheckPosition();

        if (enemyController.visual != null)
        {
            Vector3 scale = enemyController.visual.localScale;
            scale.x *= -1;
            enemyController.visual.localScale = scale;
        }
    }

    public void UpdateGroundCheckPosition()
    {
        if (enemyController.groundCheck == null) return;

        float dir = enemyController.movingRight ? 1f : -1f;

        Vector3 pos = enemyController.groundCheck.localPosition;
        pos.x = Mathf.Abs(enemyController.groundCheckOffset) * dir;
        enemyController.groundCheck.localPosition = pos;
    }
}
