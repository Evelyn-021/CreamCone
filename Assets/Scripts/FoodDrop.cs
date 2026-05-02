using UnityEngine;
using UnityEngine.SceneManagement;

public class FoodDrop : MonoBehaviour
{
    public int points = 100;
    public float fallSpeedLimit = -2f;
    public float descendSpeed = 0f;
    public float autoDestroyY = -999f;

    [Header("Spawn guiado")]
    public float guidedDropSpeed = 1.2f;
    public float platformSearchHorizontalRange = 6f;
    public float platformSearchStep = 0.25f;
    public float platformSearchDownDistance = 8f;
    public float platformLandingOffset = 0.35f;
    public float minimumGuidedDropDistance = 1.2f;
    public float minimumWalkableSurfaceWidth = 1.5f;
    public float topLimitClearance = 1.6f;
    public string strictLandingSceneName = "Level2";
    public float minimumSideClearance = 1.1f;
    public string requiredLandingPlatformName = "";
    public LayerMask platformMask;

    private Rigidbody2D rb;
    private bool collected;
    private bool guidingToPlatform;
    private Vector2 platformTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (guidingToPlatform)
        {
            MoveToPlatformTarget();
            return;
        }

        if (descendSpeed > 0f)
        {
            rb.linearVelocity = new Vector2(0f, -descendSpeed);
        }

        if (rb.linearVelocity.y < fallSpeedLimit)
        {
            rb.linearVelocity = new Vector2(0f, fallSpeedLimit);
        }

        if (!collected && transform.position.y <= autoDestroyY)
        {
            collected = true;

            if (LevelGoalManager.Instance != null)
            {
                LevelGoalManager.Instance.FruitCollected();
            }

            Destroy(gameObject);
        }
    }

    public void StartGuidedDropToPlatform()
    {
        if (!TryFindPlatformTarget(out platformTarget)) return;

        guidingToPlatform = true;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void MoveToPlatformTarget()
    {
        if (rb == null) return;

        Vector2 nextPosition = Vector2.MoveTowards(
            rb.position,
            platformTarget,
            guidedDropSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);
        rb.linearVelocity = Vector2.zero;

        if (Vector2.Distance(nextPosition, platformTarget) <= 0.02f)
        {
            guidingToPlatform = false;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private bool TryFindPlatformTarget(out Vector2 target)
    {
        target = transform.position;
        EnsurePlatformMask();

        if (platformMask.value == 0) return false;

        if (!TryFindBestPlatform(true, out RaycastHit2D bestHit) &&
            !TryFindBestPlatform(false, out bestHit))
        {
            return false;
        }

        target = bestHit.point + Vector2.up * platformLandingOffset;
        return true;
    }

    private bool TryFindBestPlatform(bool onlyAccessiblePlatforms, out RaycastHit2D bestHit)
    {
        bestHit = new RaycastHit2D();
        float bestScore = float.MaxValue;
        int steps = Mathf.CeilToInt(platformSearchHorizontalRange / platformSearchStep);

        for (int i = 0; i <= steps; i++)
        {
            float distance = i * platformSearchStep;
            CheckPlatformCandidate(distance, onlyAccessiblePlatforms, ref bestHit, ref bestScore);

            if (distance > 0f)
            {
                CheckPlatformCandidate(-distance, onlyAccessiblePlatforms, ref bestHit, ref bestScore);
            }
        }

        return bestHit.collider != null;
    }

    private void CheckPlatformCandidate(
        float xOffset,
        bool onlyAccessiblePlatforms,
        ref RaycastHit2D bestHit,
        ref float bestScore)
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(xOffset, 0.2f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, platformSearchDownDistance, platformMask);

        if (hit.collider == null) return;
        if (!IsAllowedPlatformHit(hit)) return;
        if (onlyAccessiblePlatforms && !IsAccessiblePlatformHit(hit)) return;
        if (!onlyAccessiblePlatforms && !IsReasonableFallbackPlatformHit(hit)) return;

        float horizontalDistance = Mathf.Abs(xOffset);
        float verticalDistance = Mathf.Max(0f, origin.y - hit.point.y);
        float score = horizontalDistance * 1.5f + verticalDistance;

        if (score < bestScore)
        {
            bestScore = score;
            bestHit = hit;
        }
    }

    private bool IsAccessiblePlatformHit(RaycastHit2D hit)
    {
        float dropDistance = transform.position.y - hit.point.y;
        if (dropDistance < minimumGuidedDropDistance)
        {
            return false;
        }

        if (IsTooCloseToTopLimit(hit.point.y))
        {
            return false;
        }

        if (IsBlockedByNearbySideWall(hit.point))
        {
            return false;
        }

        return HasWalkableSurfaceAround(hit.point);
    }

    private bool IsReasonableFallbackPlatformHit(RaycastHit2D hit)
    {
        float dropDistance = transform.position.y - hit.point.y;
        return dropDistance >= minimumGuidedDropDistance &&
               !IsTooCloseToTopLimit(hit.point.y) &&
               !IsBlockedByNearbySideWall(hit.point);
    }

    private bool IsTooCloseToTopLimit(float platformY)
    {
        GameObject[] limits = GameObject.FindGameObjectsWithTag("LevelLimit");
        if (limits.Length == 0) return false;

        float topLimitY = float.MinValue;
        foreach (GameObject limit in limits)
        {
            if (limit.name.Contains("Top"))
            {
                topLimitY = Mathf.Max(topLimitY, limit.transform.position.y);
            }
        }

        if (topLimitY == float.MinValue)
        {
            foreach (GameObject limit in limits)
            {
                topLimitY = Mathf.Max(topLimitY, limit.transform.position.y);
            }
        }

        return platformY > topLimitY - topLimitClearance;
    }

    private bool HasWalkableSurfaceAround(Vector2 point)
    {
        float halfWidth = minimumWalkableSurfaceWidth * 0.5f;
        return HasPlatformBelow(point + Vector2.left * halfWidth) &&
               HasPlatformBelow(point) &&
               HasPlatformBelow(point + Vector2.right * halfWidth);
    }

    private bool IsBlockedByNearbySideWall(Vector2 point)
    {
        if (!string.IsNullOrWhiteSpace(requiredLandingPlatformName))
        {
            return false;
        }

        if (SceneManager.GetActiveScene().name != strictLandingSceneName)
        {
            return false;
        }

        Vector2 origin = point + Vector2.up * platformLandingOffset;
        return HasSideWall(origin, Vector2.left) || HasSideWall(origin, Vector2.right);
    }

    private bool HasSideWall(Vector2 origin, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, minimumSideClearance, platformMask);
        return hit.collider != null;
    }

    private bool HasPlatformBelow(Vector2 point)
    {
        Vector2 origin = point + Vector2.up * 0.25f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.6f, platformMask);
        return hit.collider != null && IsAllowedPlatformHit(hit);
    }

    private bool IsAllowedPlatformHit(RaycastHit2D hit)
    {
        if (string.IsNullOrWhiteSpace(requiredLandingPlatformName))
        {
            return true;
        }

        Transform current = hit.collider.transform;
        while (current != null)
        {
            if (current.name.IndexOf(requiredLandingPlatformName, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void EnsurePlatformMask()
    {
        if (platformMask.value != 0) return;

        int groundLayer = LayerMask.NameToLayer("Ground");
        int oneWayLayer = LayerMask.NameToLayer("OneWayPlatforms");

        if (groundLayer >= 0)
        {
            platformMask |= 1 << groundLayer;
        }

        if (oneWayLayer >= 0)
        {
            platformMask |= 1 << oneWayLayer;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (collected) return;
            collected = true;

            if (BitmapScoreUI.Instance != null)
            {
                BitmapScoreUI.Instance.AddScore(points);
            }

            if (GameAudio.Instance != null)
            {
                GameAudio.Instance.PlayCoin();
            }

            if (LevelGoalManager.Instance != null)
            {
                LevelGoalManager.Instance.FruitCollected();
            }

            Destroy(gameObject);
        }
    }
}
