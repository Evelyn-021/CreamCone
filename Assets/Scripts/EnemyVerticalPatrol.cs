using UnityEngine;

public class EnemyVerticalPatrol : MonoBehaviour
{
    [Header("Patrulla vertical")]
    public bool useVerticalPatrol = true;
    public int flipsBeforeVerticalMove = 4;
    public float verticalSearchDistance = 4f;
    public float verticalPatrolSpeed = 3f;
    public float verticalStopDistance = 0.05f;
    public float platformSearchExtraDistance = 0.8f;
    public float verticalMoveCooldown = 1.5f;
    public float verticalCastWidthMultiplier = 2.5f;
    public float horizontalLandingSearchDistance = 3f;
    public float horizontalLandingSearchStep = 0.5f;
    public float oppositeDirectionLandingPenalty = 1.5f;
    public LayerMask verticalPatrolPlatformMask;
    public bool debugVerticalPatrol;

    private enum MovementMode
    {
        HorizontalPatrol,
        MovingVertical
    }

    private struct TargetCandidate
    {
        public bool Found;
        public float Score;
        public float PlatformTop;
        public float X;
    }

    private EnemyController enemyController;
    private MovementMode movementMode = MovementMode.HorizontalPatrol;

    private float targetY;
    private float targetX;
    private bool originalColliderTrigger;
    private int flipCounter;
    private int nextVerticalDirection = -1; // -1 = bajar, 1 = subir
    private int horizontalSearchFirstSign = 1;
    private float nextVerticalMoveTime;

    private const float PlatformTopProbeHeight = 1.2f;
    private const float PlatformThicknessFallback = 0.7f;
    private const float PlatformTopTolerance = 0.08f;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        horizontalSearchFirstSign = transform.position.x >= 0f ? -1 : 1;
    }

    public bool OnUpdate()
    {
        if (!useVerticalPatrol) return false;
        if (movementMode != MovementMode.MovingVertical) return false;

        MoveVertical();
        return true;
    }

    public void RegisterHorizontalFlip()
    {
        if (!useVerticalPatrol) return;
        if (movementMode != MovementMode.HorizontalPatrol) return;
        if (Time.time < nextVerticalMoveTime) return;

        flipCounter++;

        if (flipCounter < flipsBeforeVerticalMove) return;

        flipCounter = 0;
        StartVerticalMove();
    }

    private bool StartVerticalMove()
    {
        int direction = nextVerticalDirection;

        if (!TrySetTarget(direction))
        {
            LogVertical($"No encontro plataforma para {(direction < 0 ? "bajar" : "subir")}. Prueba la direccion contraria.");
            direction *= -1;

            if (!TrySetTarget(direction))
            {
                LogVertical("No encontro plataforma vertical en ninguna direccion.");
                return false;
            }
        }

        originalColliderTrigger = enemyController.enemyCollider.isTrigger;
        movementMode = MovementMode.MovingVertical;

        enemyController.rb.gravityScale = 0f;
        enemyController.rb.linearVelocity = Vector2.zero;
        enemyController.enemyCollider.isTrigger = true;

        nextVerticalDirection = direction < 0 ? 1 : -1;
        horizontalSearchFirstSign *= -1;
        LogVertical($"Empieza a {(direction < 0 ? "bajar" : "subir")} hacia targetY={targetY:F2}.");
        return true;
    }

    private bool TrySetTarget(int direction)
    {
        if (enemyController.groundCheck == null) return false;

        LayerMask platformMask = GetVerticalPlatformMask();
        float groundClearance = GetGroundClearance();
        float groundCheckOffsetY = enemyController.groundCheck.position.y - enemyController.rb.position.y;
        float currentPlatformTop = enemyController.rb.position.y + groundCheckOffsetY - groundClearance;
        float searchDistance = verticalSearchDistance + platformSearchExtraDistance + groundClearance;
        string directionName = direction < 0 ? "bajar" : "subir";

        RaycastHit2D[] hits = CastForPlatforms(
            enemyController.rb.position,
            direction,
            searchDistance,
            platformMask
        );

        LogVertical($"Busca {directionName}: hits={hits.Length}, search={searchDistance:F2}.");

        TargetCandidate best = new TargetCandidate
        {
            Score = float.MaxValue,
            X = enemyController.rb.position.x
        };

        CheckHitsForTarget(
            hits,
            direction,
            directionName,
            enemyController.rb.position.x,
            0f,
            0,
            currentPlatformTop,
            platformMask,
            ref best
        );

        if (!best.Found)
        {
            TryFindNearbyLanding(
                direction,
                directionName,
                searchDistance,
                currentPlatformTop,
                platformMask,
                ref best
            );
        }

        if (!best.Found)
        {
            LogVertical($"No hay candidata valida para {directionName}.");
            return false;
        }

        targetY = best.PlatformTop + groundClearance - groundCheckOffsetY;
        targetX = best.X;
        LogVertical(
            $"Target elegido para {directionName}: platformTop={best.PlatformTop:F2}, " +
            $"target=({targetX:F2}, {targetY:F2})."
        );
        return true;
    }

    private void TryFindNearbyLanding(
        int direction,
        string directionName,
        float searchDistance,
        float currentPlatformTop,
        LayerMask platformMask,
        ref TargetCandidate best
    )
    {
        float step = Mathf.Max(0.1f, horizontalLandingSearchStep);
        int steps = Mathf.CeilToInt(horizontalLandingSearchDistance / step);

        for (int radiusIndex = 1; radiusIndex <= steps; radiusIndex++)
        {
            float offsetDistance = radiusIndex * step;

            for (int signIndex = 0; signIndex < 2; signIndex++)
            {
                int sign = signIndex == 0 ? horizontalSearchFirstSign : -horizontalSearchFirstSign;
                float offsetX = offsetDistance * sign;
                Vector2 castOrigin = enemyController.rb.position + Vector2.right * offsetX;
                RaycastHit2D[] hits = CastForPlatforms(
                    castOrigin,
                    direction,
                    searchDistance,
                    platformMask
                );

                CheckHitsForTarget(
                    hits,
                    direction,
                    directionName,
                    castOrigin.x,
                    Mathf.Abs(offsetX),
                    sign,
                    currentPlatformTop,
                    platformMask,
                    ref best
                );

                if (best.Found)
                {
                    return;
                }
            }
        }
    }

    private void CheckHitsForTarget(
        RaycastHit2D[] hits,
        int direction,
        string directionName,
        float candidateX,
        float horizontalDistance,
        int horizontalSign,
        float currentPlatformTop,
        LayerMask platformMask,
        ref TargetCandidate best
    )
    {
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;

            if (hit.distance <= enemyController.groundCheckDistance + 0.15f)
            {
                continue;
            }

            float platformTop = direction > 0
                ? GetPlatformTop(hit, platformMask)
                : hit.point.y;

            bool isValidTarget = direction > 0
                ? platformTop > currentPlatformTop + PlatformTopTolerance
                : platformTop < currentPlatformTop - PlatformTopTolerance;

            if (!isValidTarget)
            {
                continue;
            }

            int preferredSign = enemyController.movingRight ? 1 : -1;
            float directionPenalty = direction < 0 && horizontalSign != 0 && horizontalSign != preferredSign
                ? oppositeDirectionLandingPenalty
                : 0f;
            float score = horizontalDistance + directionPenalty + hit.distance * 0.05f;
            if (score < best.Score)
            {
                best.Found = true;
                best.Score = score;
                best.PlatformTop = platformTop;
                best.X = candidateX;
            }
        }
    }

    private void MoveVertical()
    {
        Vector2 position = enemyController.rb.position;
        float newX = Mathf.MoveTowards(
            position.x,
            targetX,
            verticalPatrolSpeed * Time.fixedDeltaTime
        );
        float newY = Mathf.MoveTowards(
            position.y,
            targetY,
            verticalPatrolSpeed * Time.fixedDeltaTime
        );

        enemyController.rb.linearVelocity = Vector2.zero;
        enemyController.rb.MovePosition(new Vector2(newX, newY));

        if (Vector2.Distance(new Vector2(newX, newY), new Vector2(targetX, targetY)) <= verticalStopDistance)
        {
            FinishVerticalMove();
        }
    }

    private void FinishVerticalMove()
    {
        movementMode = MovementMode.HorizontalPatrol;

        enemyController.rb.gravityScale = enemyController.originalGravity;
        enemyController.rb.linearVelocity = Vector2.zero;
        enemyController.enemyCollider.isTrigger = originalColliderTrigger;
        enemyController.IgnoreCollisionsWithOtherEnemies();

        flipCounter = 0;
        nextVerticalMoveTime = Time.time + verticalMoveCooldown;
        LogVertical($"Termina movimiento vertical. Cooldown hasta t={nextVerticalMoveTime:F2}.");
    }

    private float GetGroundClearance()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            enemyController.groundCheck.position,
            Vector2.down,
            enemyController.groundCheckDistance + 0.5f,
            enemyController.groundMask
        );

        return hit.collider != null ? hit.distance : enemyController.groundCheckDistance;
    }

    private RaycastHit2D[] CastForPlatforms(
        Vector2 origin,
        int direction,
        float distance,
        LayerMask platformMask
    )
    {
        return Physics2D.BoxCastAll(
            origin,
            GetVerticalCastSize(direction),
            0f,
            direction > 0 ? Vector2.up : Vector2.down,
            distance,
            platformMask
        );
    }

    private Vector2 GetVerticalCastSize(int direction)
    {
        if (enemyController.enemyCollider == null)
        {
            return new Vector2(0.8f, 0.1f);
        }

        Bounds bounds = enemyController.enemyCollider.bounds;
        float widthMultiplier = direction < 0 ? verticalCastWidthMultiplier : 0.85f;
        return new Vector2(Mathf.Max(0.1f, bounds.size.x * widthMultiplier), 0.1f);
    }

    private float GetPlatformTop(RaycastHit2D platformHit, LayerMask platformMask)
    {
        Vector2 probeOrigin = platformHit.point + Vector2.up * PlatformTopProbeHeight;

        RaycastHit2D topHit = Physics2D.Raycast(
            probeOrigin,
            Vector2.down,
            PlatformTopProbeHeight + 0.2f,
            platformMask
        );

        return topHit.collider != null
            ? topHit.point.y
            : platformHit.point.y + PlatformThicknessFallback;
    }

    private LayerMask GetVerticalPlatformMask()
    {
        if (verticalPatrolPlatformMask.value != 0)
        {
            return verticalPatrolPlatformMask;
        }

        int oneWayPlatformsMask = LayerMask.GetMask("OneWayPlatforms");
        return oneWayPlatformsMask != 0 ? oneWayPlatformsMask : enemyController.groundMask;
    }

    private void LogVertical(string message)
    {
        if (!debugVerticalPatrol) return;

        Debug.Log($"[EnemyVerticalPatrol] {name}: {message}", this);
    }

    public void ResetVerticalPatrol()
    {
        movementMode = MovementMode.HorizontalPatrol;
        flipCounter = 0;
    }
}
