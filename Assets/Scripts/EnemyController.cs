using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyBubble))]
[RequireComponent(typeof(EnemyFoodDrop))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyState { Walking, TrappedBubble, Angry, SpinningToFood }

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
    [HideInInspector] public EnemyBubble bubble;
    [HideInInspector] public EnemyFoodDrop foodDrop;

    [HideInInspector] public EnemyState currentState = EnemyState.Walking;
    [HideInInspector] public Vector2 currentBubbleDirection;
    [HideInInspector] public float originalGravity;

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
            bubble.ChangeBubbleDirection(other.name);
            return;
        }

        if ((currentState == EnemyState.TrappedBubble || currentState == EnemyState.SpinningToFood)
            && other.CompareTag("LevelLimit"))
        {
            bubble.BounceBubble(other);
            return;
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
