using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyBubble))]
[RequireComponent(typeof(EnemyFoodDrop))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyState { Walking, TrappedBubble, Angry, SpinningToFood }

    [Header("Grupo")]
    public string enemyGroup = "";

    [Header("Movimiento")]
    public float normalSpeed = 2f;
    public float angrySpeed = 3.5f;
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public bool movingRight = true;

    [Header("Deteccion de borde")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;
    public float groundCheckOffset = 0.5f;

    [Header("Visual")]
    public Transform visual;

    [Header("Burbuja")]
    public float trappedTime = 5f;
    public float bubbleSpeed = 1.2f;
    public Vector2 bubbleDirection = new Vector2(0.6f, 1f);

    [Header("Pop / Fruta")]
    public float spinTime = 1.5f;
    public float spinSpeed = 420f;
    public float spinMoveSpeed = 2.5f;
    public GameObject[] foodPrefabs;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Animator anim;
    [HideInInspector] public Collider2D enemyCollider;

    [HideInInspector] public EnemyMovement movement;
    [HideInInspector] public EnemyDiagonalMovement diagonalMovement;
    [HideInInspector] public EnemyVerticalPatrol verticalPatrol;
    [HideInInspector] public EnemyBubble bubble;
    [HideInInspector] public EnemyFoodDrop foodDrop;

    [HideInInspector] public EnemyState currentState = EnemyState.Walking;
    [HideInInspector] public Vector2 currentBubbleDirection;
    [HideInInspector] public float originalGravity;

    public string EnemyGroup
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(enemyGroup))
            {
                return enemyGroup;
            }

            int cloneSuffixIndex = gameObject.name.IndexOf(" (", System.StringComparison.Ordinal);
            return cloneSuffixIndex >= 0 ? gameObject.name.Substring(0, cloneSuffixIndex) : gameObject.name;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        originalGravity = rb.gravityScale;

        movement = GetComponent<EnemyMovement>();
        if (movement == null)
        {
            movement = gameObject.AddComponent<EnemyMovement>();
        }

        diagonalMovement = GetComponent<EnemyDiagonalMovement>();

        verticalPatrol = GetComponent<EnemyVerticalPatrol>();

        bubble = GetComponent<EnemyBubble>();
        if (bubble == null)
        {
            bubble = gameObject.AddComponent<EnemyBubble>();
        }

        foodDrop = GetComponent<EnemyFoodDrop>();
        if (foodDrop == null)
        {
            foodDrop = gameObject.AddComponent<EnemyFoodDrop>();
        }
    }

    private void Start()
    {
        currentSpeed = normalSpeed;
        anim.SetInteger("stateAnim", 1);
        movement.UpdateGroundCheckPosition();
        IgnoreCollisionsWithOtherEnemies();
    }

    private void FixedUpdate()
    {
        if (currentState == EnemyState.TrappedBubble)
        {
            bubble.UpdateBubble();
            return;
        }

        if (currentState == EnemyState.SpinningToFood)
        {
            foodDrop.UpdateSpinToFood();
            return;
        }

        if (verticalPatrol != null && verticalPatrol.OnUpdate())
        {
            return;
        }

        if (diagonalMovement != null && diagonalMovement.OnUpdate())
        {
            return;
        }

        movement.OnUpdate();
    }

    public void TrapEnemy()
    {
        bubble.TrapEnemy();
    }

    public void PopEnemy()
    {
        foodDrop.PopEnemy();
    }

    public void DefeatByWater()
    {
        if (currentState == EnemyState.SpinningToFood) return;

        currentState = EnemyState.TrappedBubble;
        currentSpeed = 0f;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        enemyCollider.isTrigger = true;

        foodDrop.PopEnemy();
    }

    public void BecomeAngry()
    {
        if (currentState == EnemyState.SpinningToFood) return;

        currentState = EnemyState.Angry;
        currentSpeed = angrySpeed;

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;

        enemyCollider.isTrigger = false;
        IgnoreCollisionsWithOtherEnemies();

        anim.SetInteger("stateAnim", 3);
    }

    public void IgnoreCollisionsWithOtherEnemies()
    {
        if (enemyCollider == null) return;

        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        foreach (EnemyController otherEnemy in enemies)
        {
            if (otherEnemy == this || otherEnemy.enemyCollider == null) continue;

            Physics2D.IgnoreCollision(enemyCollider, otherEnemy.enemyCollider, true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == EnemyState.TrappedBubble && other.CompareTag("Player"))
        {
            PopEnemy();
            return;
        }

        if ((currentState == EnemyState.TrappedBubble || currentState == EnemyState.SpinningToFood)
            && other.CompareTag("BubbleTurn"))
        {
            bubble.ChangeBubbleDirection(other);
            return;
        }

        if ((currentState == EnemyState.TrappedBubble || currentState == EnemyState.SpinningToFood)
            && other.CompareTag("LevelLimit"))
        {
            bubble.BounceBubble(other);
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        IgnoreEnemyCollision(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        IgnoreEnemyCollision(collision.collider);
    }

    private void IgnoreEnemyCollision(Collider2D otherCollider)
    {
        EnemyController otherEnemy = otherCollider.GetComponentInParent<EnemyController>();

        if (otherEnemy != null && otherEnemy != this && otherEnemy.enemyCollider != null)
        {
            Physics2D.IgnoreCollision(enemyCollider, otherEnemy.enemyCollider, true);
        }
    }

    public bool IsTrapped()
    {
        return currentState == EnemyState.TrappedBubble;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                groundCheck.position,
                groundCheck.position + Vector3.down * groundCheckDistance
            );
        }
    }
}
