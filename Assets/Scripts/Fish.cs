
using UnityEngine;

public class Fish : MonoBehaviour
{
    [Header("Fish")]
    public FishType fishType;

    public float baseSpeed = 2f;
    public float pickupSpeed = 8f;

    [Header("Movement")]
    public float steeringAcceleration = 12f;

    [Header("Timid Fish Movement")]
    public float timidSteering = 10f;
    public float timidExitExtraRange = 0.75f;

    private bool timidIsFleeing;
    private Vector2 timidEscapeDirection;
    [Header("Behavior")]
    public float reactionHysteresis = 0.5f;

    private Transform harp;
    private Transform player;

    private Player playerScript;
    private Rigidbody2D body;
    [SerializeField]
    private float actualWeight;

    public float ActualWeight => actualWeight;
    private SpriteRenderer[] harpoonSprites;

    private bool iscatched;
    private bool isReactingToThreat;

    private Vector2 wanderDir;
    private float wanderTimer;

    private Vector2 velocity;

    private float waterSurfaceY = 3f;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        if (fishType != null)
        {
            float averageWeight = Mathf.Max(0.01f, fishType.weight);

            actualWeight = Random.Range(
                averageWeight * 0.75f,
                averageWeight * 1.25f
            );
        }
    }

    void Start()
    {
        GameObject h = GameObject.Find("Harpoon");
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (h != null)
        {
            harp = h.transform;

            harpoonSprites =
                h.GetComponentsInChildren<SpriteRenderer>(true);
        }

        if (p != null)
        {
            player = p.transform;
            playerScript = p.GetComponent<Player>();

            if (playerScript != null)
                waterSurfaceY = playerScript.waterSurfaceY;
        }

        PickWanderDir();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance == null || fishType == null)
            return;

        if (player != null &&
            Vector2.Distance(GetPosition(), player.position) > 50f)
        {
            Destroy(gameObject);
            return;
        }

        if (iscatched)
        {
            UpdateCaughtMovement();
            return;
        }

        Vector2 currentPosition = GetPosition();

        Vector2 desiredVelocity = GetDesiredVelocity(currentPosition);

        if (currentPosition.y >= waterSurfaceY - 0.001f &&
            desiredVelocity.y > 0f)
        {
            desiredVelocity.y = 0f;
        }

        float currentSteering =
            fishType.behavior == FishBehavior.Timid && timidIsFleeing
            ? timidSteering
            : steeringAcceleration;

        velocity = Vector2.MoveTowards(
            velocity,
            desiredVelocity,
            currentSteering * Time.fixedDeltaTime
        );

        Vector2 nextPosition =
            currentPosition + velocity * Time.fixedDeltaTime;

        if (nextPosition.y > waterSurfaceY)
        {
            nextPosition.y = waterSurfaceY;

            if (velocity.y > 0f)
                velocity.y = 0f;
        }

        MoveFish(nextPosition);

        if (CanBeHarpoonCaught() &&
            harp != null &&
            Vector2.Distance(harp.position, nextPosition) < 0.5f)
        {
            if (GameManager.Instance.CanAddFish(fishType))
            {
                iscatched = true;
                velocity = Vector2.zero;
            }
        }

        if (player != null &&
            Vector2.Distance(player.position, nextPosition) < 0.5f)
        {
            if (fishType.behavior == FishBehavior.Aggressive)
            {
                if (fishType.lethal)
                    KillPlayer();
                else
                    DamagePlayer(fishType.damage);

                Destroy(gameObject);
                return;
            }

            if (GameManager.Instance.AddFish(fishType, actualWeight))
                Destroy(gameObject);
        }
    }

    Vector2 GetPosition()
    {
        if (body != null)
            return body.position;

        return transform.position;
    }

    void MoveFish(Vector2 position)
    {
        if (body != null)
        {
            body.MovePosition(position);
        }
        else
        {
            transform.position = new Vector3(
                position.x,
                position.y,
                transform.position.z
            );
        }
    }

    void UpdateCaughtMovement()
    {
        velocity = Vector2.zero;

        Vector2 currentPosition = GetPosition();

        if (harp != null)
        {
            Vector2 nextPosition = Vector2.MoveTowards(
                currentPosition,
                harp.position,
                pickupSpeed * 2f * Time.fixedDeltaTime
            );

            MoveFish(nextPosition);
            currentPosition = nextPosition;
        }

        if (player != null &&
            Vector2.Distance(player.position, currentPosition) < 0.5f)
        {
            GameManager.Instance.AddFish(fishType, actualWeight);
            Destroy(gameObject);
        }
    }

    bool CanBeHarpoonCaught()
    {
        return fishType.behavior != FishBehavior.Aggressive;
    }

    void PickWanderDir()
    {
        wanderDir = Random.insideUnitCircle.normalized;

        if (wanderDir.sqrMagnitude < 0.001f)
            wanderDir = Vector2.right;

        wanderTimer = Random.Range(1f, 3f);
    }

    Vector2 Wander()
    {
        wanderTimer -= Time.fixedDeltaTime;

        if (wanderTimer <= 0f)
            PickWanderDir();

        return wanderDir * baseSpeed;
    }

    bool UpdateReactionState(float distance)
    {
        float enterRange = Mathf.Max(0f, fishType.detectRange);

        float exitRange = enterRange +
            Mathf.Max(0.1f, reactionHysteresis);

        if (!isReactingToThreat && distance <= enterRange)
            isReactingToThreat = true;

        else if (isReactingToThreat && distance >= exitRange)
            isReactingToThreat = false;

        return isReactingToThreat;
    }

    bool IsHarpoonVisible()
    {
        if (harp == null || harpoonSprites == null)
            return false;

        foreach (SpriteRenderer sr in harpoonSprites)
        {
            if (sr != null &&
                sr.enabled &&
                sr.gameObject.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    Vector2 GetDirectionAway(Vector2 threatPosition)
    {
        Vector2 direction =
            GetPosition() - threatPosition;

        if (direction.sqrMagnitude < 0.001f)
            return wanderDir;

        return direction.normalized;
    }

    Vector2 GetDirectionTowards(Vector2 targetPosition)
    {
        Vector2 direction =
            targetPosition - GetPosition();

        if (direction.sqrMagnitude < 0.001f)
            return Vector2.zero;

        return direction.normalized;
    }

    Vector2 GetDesiredVelocity(Vector2 currentPosition)
    {
        switch (fishType.behavior)
        {
            case FishBehavior.Friendly:
                isReactingToThreat = false;
                return Wander();

            case FishBehavior.Neutral:
                if (!IsHarpoonVisible())
                {
                    isReactingToThreat = false;
                    return Wander();
                }

                if (UpdateReactionState(
                    Vector2.Distance(currentPosition, harp.position)))
                {
                    return GetDirectionAway(harp.position) *
                        fishType.fleeSpeed;
                }

                return Wander();

            case FishBehavior.Timid:
                return GetTimidVelocity(currentPosition);

            case FishBehavior.Aggressive:

                if (player != null &&
                    UpdateReactionState(
                        Vector2.Distance(currentPosition, player.position)))
                {
                    return GetDirectionTowards(player.position) *
                        fishType.chaseSpeed;
                }

                return Wander();
        }

        return Wander();
    }
    Vector2 GetTimidVelocity(Vector2 currentPosition)
    {
        if (player == null)
        {
            timidIsFleeing = false;
            return Wander();
        }

        float distance = Vector2.Distance(
            currentPosition,
            player.position
        );

        float enterRange = Mathf.Max(0f, fishType.detectRange);

        float exitRange = enterRange +
            Mathf.Max(0.1f, timidExitExtraRange);

        if (!timidIsFleeing && distance <= enterRange)
        {
            timidIsFleeing = true;
        }

        if (timidIsFleeing && distance >= exitRange)
        {
            timidIsFleeing = false;
            timidEscapeDirection = Vector2.zero;
            return Wander();
        }

        if (!timidIsFleeing)
            return Wander();
        Vector2 away = currentPosition - (Vector2)player.position;

        if (away.sqrMagnitude > 0.04f)
        {
            Vector2 newDirection = away.normalized;

            if (timidEscapeDirection.sqrMagnitude < 0.01f)
            {
                timidEscapeDirection = newDirection;
            }
            else
            {
                Vector2 blended = Vector2.Lerp(
                    timidEscapeDirection,
                    newDirection,
                    0.15f
                );

                if (blended.sqrMagnitude > 0.001f)
                    timidEscapeDirection = blended.normalized;
            }
        }

        return timidEscapeDirection * fishType.fleeSpeed;
    }

    void ChooseTimidEscapeDirection(bool smooth)
    {
        Vector2 away =
            (Vector2)transform.position -
            (Vector2)player.position;

        if (away.sqrMagnitude < 0.04f)
        {
            if (timidEscapeDirection.sqrMagnitude > 0.01f)
                return;

            away = velocity.sqrMagnitude > 0.01f
                ? velocity
                : wanderDir;
        }

        Vector2 newDirection = away.normalized;

        if (smooth &&
            timidEscapeDirection.sqrMagnitude > 0.01f)
        {
            Vector2 blendedDirection = Vector2.Lerp(
                timidEscapeDirection,
                newDirection,
                0.35f
            );

            if (blendedDirection.sqrMagnitude > 0.001f)
                timidEscapeDirection = blendedDirection.normalized;
            else
                timidEscapeDirection = newDirection;
        }
        else
        {
            timidEscapeDirection = newDirection;
        }
    }
    void KillPlayer()
    {
        if (playerScript == null)
        {
            Debug.LogError(
                "Fish: не найден Player для смертельной атаки!"
            );
            return;
        }
        float damage = playerScript.health /
            Mathf.Max(0.0001f, 1f - playerScript.armorReduction) + 1f;

        playerScript.TakeDamage(damage);
    }

    void DamagePlayer(float damage)
    {
        if (playerScript != null)
            playerScript.TakeDamage(damage);
    }
}
