using UnityEngine;

public class EnemyDiagonalMovement : MonoBehaviour
{
    public Vector2 startDirection = new Vector2(1f, 1f);
    public LayerMask bounceMask;
    public float bounceCooldown = 0.05f;

    private EnemyController enemyController;
    private Vector2 moveDirection;
    private float lastBounceTime;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        moveDirection = startDirection.sqrMagnitude > 0f
            ? startDirection.normalized
            : new Vector2(1f, 1f).normalized;
    }

    public bool OnUpdate()
    {
        enemyController.rb.linearVelocity = moveDirection * enemyController.currentSpeed;
        UpdateVisualFacing();
        return true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Bounce(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Bounce(collision);
    }

    private void Bounce(Collision2D collision)
    {
        if (!ShouldBounce(collision.collider)) return;
        if (Time.time < lastBounceTime + bounceCooldown) return;

        Vector2 bestNormal = Vector2.zero;
        float strongestHit = 0f;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;
            float hitStrength = -Vector2.Dot(moveDirection, normal);

            if (hitStrength > strongestHit)
            {
                strongestHit = hitStrength;
                bestNormal = normal;
            }
        }

        if (strongestHit <= 0.05f) return;

        moveDirection = Vector2.Reflect(moveDirection, bestNormal).normalized;
        lastBounceTime = Time.time;
    }

    private bool ShouldBounce(Collider2D other)
    {
        if (other.GetComponentInParent<EnemyController>() != null) return false;
        if (bounceMask.value == 0) return true;

        return (bounceMask.value & (1 << other.gameObject.layer)) != 0;
    }

    private void UpdateVisualFacing()
    {
        if (enemyController.visual == null) return;

        Vector3 scale = enemyController.visual.localScale;
        float xMagnitude = Mathf.Abs(scale.x);
        scale.x = moveDirection.x < 0f ? -xMagnitude : xMagnitude;
        enemyController.visual.localScale = scale;
    }
}
