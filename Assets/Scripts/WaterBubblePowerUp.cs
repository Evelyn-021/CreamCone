using UnityEngine;

public class WaterBubblePowerUp : MonoBehaviour
{
    public float fallSpeed = 0.9f;
    public float rideSpeed = 4.5f;
    public float rideDuration = 7f;
    public float playerFollowOffsetY = -0.05f;
    public Vector2 areaMin = new Vector2(-4.6f, -2.35f);
    public Vector2 areaMax = new Vector2(5.8f, 2.55f);

    private Rigidbody2D rb;
    private PlayerController player;
    private bool riding;
    private float rideTimer;
    private int cornerIndex;

    private readonly Vector2[] corners = new Vector2[4];

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        BuildCorners();
    }

    private void FixedUpdate()
    {
        if (!riding)
        {
            UpdateFalling();
            return;
        }

        UpdateRide();
    }

    private void UpdateFalling()
    {
        rb.linearVelocity = Vector2.down * fallSpeed;

        if (transform.position.y < areaMin.y - 1f)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateRide()
    {
        rideTimer -= Time.fixedDeltaTime;

        Vector2 target = corners[cornerIndex];
        Vector2 current = rb.position;
        Vector2 next = Vector2.MoveTowards(current, target, rideSpeed * Time.fixedDeltaTime);
        rb.MovePosition(next);

        if (Vector2.Distance(next, target) <= 0.05f)
        {
            cornerIndex = (cornerIndex + 1) % corners.Length;
        }

        if (player != null)
        {
            Vector2 playerPosition = next + Vector2.up * playerFollowOffsetY;
            player.rb.MovePosition(playerPosition);
            player.rb.linearVelocity = Vector2.zero;
        }

        if (rideTimer <= 0f)
        {
            EndRide();
        }
    }

    private void StartRide(PlayerController targetPlayer)
    {
        if (riding) return;

        riding = true;
        rideTimer = rideDuration;
        player = targetPlayer;

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        transform.position = ClampToArea(transform.position);
        cornerIndex = GetClosestCornerIndex(transform.position);

        player.SetWaterBubbleRide(true);
    }

    private void EndRide()
    {
        if (player != null)
        {
            player.SetWaterBubbleRide(false);
            player = null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!riding && other.CompareTag("Player"))
        {
            PlayerController targetPlayer = other.GetComponentInParent<PlayerController>();
            if (targetPlayer != null)
            {
                StartRide(targetPlayer);
            }

            return;
        }

        if (!riding) return;

        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (enemy != null)
        {
            enemy.DefeatByWater();
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.SetWaterBubbleRide(false);
        }
    }

    private void BuildCorners()
    {
        corners[0] = new Vector2(areaMin.x, areaMax.y);
        corners[1] = new Vector2(areaMax.x, areaMax.y);
        corners[2] = new Vector2(areaMax.x, areaMin.y);
        corners[3] = new Vector2(areaMin.x, areaMin.y);
    }

    private int GetClosestCornerIndex(Vector2 position)
    {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < corners.Length; i++)
        {
            float distance = Vector2.SqrMagnitude(position - corners[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private Vector3 ClampToArea(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, areaMin.x, areaMax.x);
        position.y = Mathf.Clamp(position.y, areaMin.y, areaMax.y);
        return position;
    }
}
