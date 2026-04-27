using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private enum EnemyState { Walking, TrappedBubble, Angry, SpinningToFood }

    [Header("Movimiento")]
    public float normalSpeed = 2f;
    public float angrySpeed = 3.5f;
    private float currentSpeed;
    private bool movingRight = true;

    [Header("Detección de borde")]
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

    private float trappedTimer;
    private float spinTimer;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D enemyCollider;

    private EnemyState currentState = EnemyState.Walking;
    private Vector2 currentBubbleDirection;
    private float originalGravity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        originalGravity = rb.gravityScale;
    }

    private void Start()
    {
        currentSpeed = normalSpeed;
        anim.SetInteger("stateAnim", 1);
        UpdateGroundCheckPosition();
    }

    private void FixedUpdate()
    {
        if (currentState == EnemyState.TrappedBubble)
        {
            UpdateBubble();
            return;
        }

        if (currentState == EnemyState.SpinningToFood)
        {
            UpdateSpinToFood();
            return;
        }

        Move();
        CheckEdge();
    }

    private void Move()
    {
        float dir = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * currentSpeed, rb.linearVelocity.y);
    }

    private void UpdateBubble()
    {
        trappedTimer -= Time.fixedDeltaTime;

        float wave = Mathf.Sin(Time.time * 2f) * 0.25f;

        Vector2 moveDirection = new Vector2(
            currentBubbleDirection.x,
            currentBubbleDirection.y + wave
        ).normalized;

        rb.linearVelocity = moveDirection * bubbleSpeed;

        if (trappedTimer <= 0f)
        {
            ReleaseEnemy();
        }
    }

    private void UpdateSpinToFood()
    {
        spinTimer -= Time.fixedDeltaTime;

        rb.linearVelocity = currentBubbleDirection.normalized * spinMoveSpeed;

        if (visual != null)
        {
            visual.Rotate(0f, 0f, spinSpeed * Time.fixedDeltaTime);
        }

        if (spinTimer <= 0f)
        {
            TurnIntoFood();
        }
    }

    private void CheckEdge()
    {
        if (groundCheck == null) return;

        RaycastHit2D hit = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundCheckDistance,
            groundMask
        );

        if (hit.collider == null)
        {
            Flip();
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        UpdateGroundCheckPosition();

        if (visual != null)
        {
            Vector3 scale = visual.localScale;
            scale.x *= -1;
            visual.localScale = scale;
        }
    }

    private void UpdateGroundCheckPosition()
    {
        if (groundCheck == null) return;

        float dir = movingRight ? 1f : -1f;

        Vector3 pos = groundCheck.localPosition;
        pos.x = Mathf.Abs(groundCheckOffset) * dir;
        groundCheck.localPosition = pos;
    }

    public void TrapEnemy()
    {
        if (currentState == EnemyState.TrappedBubble || currentState == EnemyState.SpinningToFood) return;

        currentState = EnemyState.TrappedBubble;
        currentSpeed = 0f;
        trappedTimer = trappedTime;

        currentBubbleDirection = bubbleDirection;
        currentBubbleDirection.x = Mathf.Abs(currentBubbleDirection.x) * (movingRight ? 1f : -1f);

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        enemyCollider.isTrigger = true;

        anim.SetInteger("stateAnim", 2);
    }

    private void ReleaseEnemy()
    {
        currentState = EnemyState.Angry;
        currentSpeed = angrySpeed;

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;

        enemyCollider.isTrigger = false;

        anim.SetInteger("stateAnim", 3);
    }

    public void PopEnemy()
    {
        if (currentState != EnemyState.TrappedBubble) return;

        currentState = EnemyState.SpinningToFood;
        spinTimer = spinTime;

        currentBubbleDirection = new Vector2(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 1f)
        ).normalized;

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        enemyCollider.isTrigger = true;

        anim.SetInteger("stateAnim", 4);
    }

    private void TurnIntoFood()
{
    if (foodPrefabs != null && foodPrefabs.Length > 0)
    {
        int index = Random.Range(0, foodPrefabs.Length);
        GameObject selectedFood = foodPrefabs[index];

        Instantiate(selectedFood, transform.position, Quaternion.identity);
    }

    Destroy(gameObject);
}

   private void OnTriggerEnter2D(Collider2D other)
{
    // Player revienta la burbuja
    if (currentState == EnemyState.TrappedBubble && other.CompareTag("Player"))
    {
        PopEnemy();
        return;
    }

    // Zonas invisibles que guían la dirección
    if ((currentState == EnemyState.TrappedBubble || currentState == EnemyState.SpinningToFood)
        && other.CompareTag("BubbleTurn"))
    {
        ChangeBubbleDirection(other.name);
        return;
    }

    // Límites del nivel como sistema de seguridad
    if ((currentState == EnemyState.TrappedBubble || currentState == EnemyState.SpinningToFood)
        && other.CompareTag("LevelLimit"))
    {
        BounceBubble(other);
        return;
    }
}

private void ChangeBubbleDirection(string zoneName)
{
    if (zoneName.Contains("Left"))
    {
        currentBubbleDirection = new Vector2(1f, 0.15f).normalized;
    }
    else if (zoneName.Contains("Right"))
    {
        currentBubbleDirection = new Vector2(-1f, 0.15f).normalized;
    }
    else if (zoneName.Contains("Top"))
    {
        currentBubbleDirection = new Vector2(currentBubbleDirection.x, -0.4f).normalized;
    }
    else if (zoneName.Contains("Bottom"))
    {
        currentBubbleDirection = new Vector2(currentBubbleDirection.x, 0.4f).normalized;
    }
}


private void BounceBubble(Collider2D limit)
{
    Vector2 directionToEnemy = transform.position - limit.transform.position;

    if (Mathf.Abs(directionToEnemy.x) > Mathf.Abs(directionToEnemy.y))
    {
        currentBubbleDirection.x *= -1f;
    }
    else
    {
        currentBubbleDirection.y *= -1f;
    }

    currentBubbleDirection.Normalize();
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
