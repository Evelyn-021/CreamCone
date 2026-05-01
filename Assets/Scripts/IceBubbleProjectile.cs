using UnityEngine;

public class IceBubbleProjectile : MonoBehaviour
{
    public float speed = 5f;
    public float maxTravelDistance = 2.5f;
    public float projectileLifeTime = 2f;
    public float floatingLifeTime = 4f;
    public float floatRiseSpeed = 0.35f;
    public float floatWaveAmplitude = 0.15f;
    public float floatWaveFrequency = 2f;
    public float popAnimationTime = 0.45f;
    public string floatingLayerName = "OneWayPlatforms";

    private Vector2 direction = Vector2.right;
    private Vector2 startPosition;
    private Vector2 floatingStartPosition;
    private float stateTimer;
    private float floatingTimer;
    private bool isFloating;
    private bool isPopping;

    private Animator anim;
    private Rigidbody2D rb;
    private Collider2D bubbleCollider;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bubbleCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        startPosition = transform.position;
        stateTimer = projectileLifeTime;

        if (anim != null)
        {
            anim.enabled = false;
        }
    }

    void Update()
    {
        if (isPopping) return;

        stateTimer -= Time.deltaTime;

        if (!isFloating)
        {
            transform.Translate(direction * speed * Time.deltaTime);

            if (stateTimer <= 0f || Vector2.Distance(startPosition, transform.position) >= maxTravelDistance)
            {
                StartFloating();
            }

            return;
        }

        floatingTimer += Time.deltaTime;
        float waveOffset = Mathf.Sin(floatingTimer * floatWaveFrequency) * floatWaveAmplitude;
        transform.position = new Vector3(
            floatingStartPosition.x + waveOffset,
            floatingStartPosition.y + floatingTimer * floatRiseSpeed,
            transform.position.z
        );

        if (stateTimer <= 0f)
        {
            PopAndDestroy();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isFloating || isPopping || other.CompareTag("Player"))
            return;

        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (enemy != null)
        {
            enemy.TrapEnemy();

            Destroy(gameObject);
            return;
        }

        if (other.isTrigger)
            return;

        PopAndDestroy();
    }

    private void StartFloating()
    {
        isFloating = true;
        stateTimer = floatingLifeTime;
        floatingTimer = 0f;
        floatingStartPosition = transform.position;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
        }

        if (bubbleCollider != null)
        {
            bubbleCollider.isTrigger = false;
        }

        int floatingLayer = LayerMask.NameToLayer(floatingLayerName);
        if (floatingLayer >= 0)
        {
            gameObject.layer = floatingLayer;
        }

        if (anim != null)
        {
            anim.enabled = true;
            anim.Play("burbujaFlotando_Anim", 0, 0f);
        }
    }

    private void PopAndDestroy()
    {
        if (isPopping) return;

        isPopping = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
        }

        if (bubbleCollider != null)
        {
            bubbleCollider.enabled = false;
        }

        if (anim != null)
        {
            anim.enabled = true;
            anim.Play("burbujaExplotando_Anim", 0, 0f);
        }

        Destroy(gameObject, popAnimationTime);
    }
}
