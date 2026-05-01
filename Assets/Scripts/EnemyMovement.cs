using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float wallCheckDistance = 0.3f;
    public LayerMask wallMask;
    public bool visualFacesRight = true;
    public float angryTurnDeadZone = 0.35f;

    private EnemyController enemyController;
    private Transform player;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
    }

    public void OnUpdate()
    {
        UpdateAngryDirection();
        CheckEdge();
        Move();
    }

    private void Move()
    {
        float dir = enemyController.movingRight ? 1f : -1f;
        enemyController.rb.linearVelocity = new Vector2(
            dir * enemyController.currentSpeed,
            enemyController.rb.linearVelocity.y
        );
    }

    private void UpdateAngryDirection()
    {
        if (enemyController.currentState != EnemyController.EnemyState.Angry) return;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player == null) return;

        float distanceToPlayer = player.position.x - transform.position.x;
        if (Mathf.Abs(distanceToPlayer) <= angryTurnDeadZone) return;

        bool shouldMoveRight = distanceToPlayer > 0f;
        if (enemyController.movingRight != shouldMoveRight)
        {
            enemyController.movingRight = shouldMoveRight;
            UpdateGroundCheckPosition();
        }
    }

    private void CheckEdge()
    {
        if (enemyController.groundCheck == null) return;

        RaycastHit2D groundHit = Physics2D.Raycast(
            enemyController.groundCheck.position,
            Vector2.down,
            enemyController.groundCheckDistance,
            enemyController.groundMask
        );

        bool foundWall = false;
        if (wallMask.value != 0)
        {
            Vector2 wallDirection = enemyController.movingRight ? Vector2.right : Vector2.left;
            Vector2 wallOrigin = GetWallCheckOrigin(wallDirection);

            RaycastHit2D wallHit = Physics2D.Raycast(
                wallOrigin,
                wallDirection,
                wallCheckDistance,
                wallMask
            );

            foundWall = wallHit.collider != null;
        }

        if (groundHit.collider == null || foundWall)
        {
            Flip();
        }
    }

    private Vector2 GetWallCheckOrigin(Vector2 wallDirection)
    {
        if (enemyController.enemyCollider == null) return transform.position;

        Bounds bounds = enemyController.enemyCollider.bounds;
        float x = wallDirection.x > 0f ? bounds.max.x : bounds.min.x;

        return new Vector2(x, bounds.center.y);
    }

    private void Flip()
    {
        enemyController.movingRight = !enemyController.movingRight;
        UpdateGroundCheckPosition();
        if (enemyController.verticalPatrol != null)
        {
            enemyController.verticalPatrol.RegisterHorizontalFlip();
        }
    }

    public void UpdateGroundCheckPosition()
    {
        if (enemyController.groundCheck == null) return;

        float dir = enemyController.movingRight ? 1f : -1f;

        Vector3 pos = enemyController.groundCheck.localPosition;
        pos.x = Mathf.Abs(enemyController.groundCheckOffset) * dir;
        enemyController.groundCheck.localPosition = pos;

        UpdateVisualFacing();
    }

    private void UpdateVisualFacing()
    {
        if (enemyController.visual == null) return;

        Vector3 scale = enemyController.visual.localScale;
        float xMagnitude = Mathf.Abs(scale.x);
        bool shouldFlipVisual = enemyController.movingRight != visualFacesRight;

        scale.x = shouldFlipVisual ? -xMagnitude : xMagnitude;
        enemyController.visual.localScale = scale;
    }
}
