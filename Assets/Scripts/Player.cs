using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public float doubleClickTime = 0.25f;

    [Header("Stats")]
    public float oxygen;
    public float maxoxygen = 100f;
    public float health = 100f;
    public float maxHealth = 100f;

    [Header("Water")]
    [Min(0f)]
    public float surfaceMargin = 0.25f;
    public float waterSurfaceY;

    [Header("Upgrade Stats")]
    public float harpoonRange;
    public float viewRange;

    [Range(0f, 0.8f)]
    public float armorReduction;

    [Header("Pier")]
    public Transform pierSpawn;
    public float pierGravityScale = 1f;
    [Header("Overweight")]
    public float overweightSpeedMultiplier = 0.5f;

    [Min(0.1f)]
    public float dropInterval = 1f;

    [Range(0f, 100f)]
    public float overweightDropChancePerKg = 10f;
    private Rigidbody2D body;
    private Harpoon harpoon;
    private PlayerBounds playerBounds;
    private Vector3 targetPoint;
    private bool isMoving;
    private bool isDashing;
    private bool canDash = true;
    private bool isOnPier = true;
    public bool gameplayEnabled = false;
    private float lastClickTime = -1f;
    private float overweightDropTimer;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        harpoon = FindFirstObjectByType<Harpoon>();
        playerBounds = GetComponent<PlayerBounds>();
        ApplyUpgradeStats();

        oxygen = maxoxygen;

        maxHealth = 100f;
        health = 100f;

        if (pierSpawn != null)
        {
            transform.position = pierSpawn.position;
            Debug.Log("Начальная позиция игрока: " + pierSpawn.position);
        }
        else
        {
            Debug.LogError("PierSpawn НЕ назначен в Player!");
        }

        SetPierMode(true);
    }

    void Update()
    {
        if (!gameplayEnabled || Time.timeScale == 0f)
            return;

        if (isOnPier)
        {
            HandlePierInput();
            return;
        }

        oxygen -= Time.deltaTime;

        if (oxygen <= 0f)
        {
            oxygen = 0f;
            Die();
            return;
        }

        HandleOverweightDrop();

        if (isDashing)
            return;

        HandleUnderwaterInput();
    }

    void FixedUpdate()
    {
        if (!gameplayEnabled || !isOnPier || body == null)
            return;

        if (!isMoving)
        {
            body.linearVelocity = new Vector2(
                0f,
                body.linearVelocity.y
            );

            return;
        }

        float difference = targetPoint.x - transform.position.x;

        if (Mathf.Abs(difference) < 0.08f)
        {
            body.linearVelocity = new Vector2(
                0f,
                body.linearVelocity.y
            );

            return;
        }

        float direction = Mathf.Sign(difference);

        body.linearVelocity = new Vector2(
            direction * CurrentSpeed(),
            body.linearVelocity.y
        );
    }
    void LateUpdate()
    {
        if (!gameplayEnabled || isOnPier)
            return;

        float maxY = waterSurfaceY - surfaceMargin;

        if (body == null)
            body = GetComponent<Rigidbody2D>();

        Vector2 currentPosition = body != null
            ? body.position
            : (Vector2)transform.position;

        if (currentPosition.y > maxY)
        {
            currentPosition.y = maxY;

            if (body != null)
            {
                body.position = currentPosition;

                if (body.linearVelocity.y > 0f)
                {
                    body.linearVelocity = new Vector2(
                        body.linearVelocity.x,
                        0f
                    );
                }
            }
            else
            {
                transform.position = new Vector3(
                    currentPosition.x,
                    currentPosition.y,
                    transform.position.z
                );
            }
        }
    }
    void HandlePierInput()
    {
        if (Input.GetMouseButton(1))
        {
            targetPoint = GetMouseWorldPosition();
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    void HandleUnderwaterInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = GetMouseWorldPosition();

            if (lastClickTime > 0f &&
                Time.time - lastClickTime < doubleClickTime &&
                canDash)
            {
                targetPoint = mousePosition;
                lastClickTime = -1f;
                StartCoroutine(Dash());
                return;
            }

            lastClickTime = Time.time;
        }

        if (Input.GetMouseButton(1))
        {
            targetPoint = GetMouseWorldPosition();
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        if (isMoving)
        {
            Vector3 nextPosition = Vector3.MoveTowards(
                transform.position,
                targetPoint,
                CurrentSpeed() * Time.deltaTime
            );

            if (playerBounds != null)
            {
                Vector2 clamped = playerBounds.ClampPosition(nextPosition);
                nextPosition.x = clamped.x;
                nextPosition.y = clamped.y;
            }

            transform.position = ClampUnderwaterPosition(nextPosition);
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        if (Camera.main == null)
            return transform.position;

        Vector3 position =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        position.z = 0f;

        return position;
    }

    public void SetPierMode(bool value)
    {
        if (isOnPier && !value)
        {
            maxHealth = 100f;
            health = 100f;
        }
        isOnPier = value;

        isMoving = false;
        isDashing = false;
        lastClickTime = -1f;

        if (body == null)
            body = GetComponent<Rigidbody2D>();

        if (body != null)
        {
            body.gravityScale = value ? pierGravityScale : 0f;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        if (harpoon == null)
            harpoon = FindFirstObjectByType<Harpoon>();

        if (harpoon != null)
            harpoon.SetAvailable(!value);
    }
    public void SetGameplayEnabled(bool value)
    {
        gameplayEnabled = value;

        if (body == null)
            body = GetComponent<Rigidbody2D>();

        if (body != null)
        {
            body.simulated = value;

            if (!value)
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }
        }

        isMoving = false;
        isDashing = false;
        lastClickTime = -1f;
    }

    public bool IsOnPier => isOnPier;
    public bool IsGameplayEnabled => gameplayEnabled;
    public void ApplyUpgradeStats()
    {
        if (GameManager.Instance == null)
            return;

        GameManager gm = GameManager.Instance;

        int harpoonLevel =
            gm.GetUpgradeLevel(UpgradeType.HarpoonRange);

        int viewLevel =
            gm.GetUpgradeLevel(UpgradeType.ViewRange);

        int oxygenLevel =
            gm.GetUpgradeLevel(UpgradeType.OxygenCapacity);

        int armorLevel =
            gm.GetUpgradeLevel(UpgradeType.Armor);

        maxoxygen = 30f +
            oxygenLevel * gm.oxygenPerLevel;

        harpoonRange =
            harpoonLevel * gm.harpoonRangePerLevel;

        viewRange =
            viewLevel * gm.viewSizePerLevel;

        armorReduction = Mathf.Clamp(
            armorLevel * gm.armorReductionPerLevel,
            0f,
            gm.maxArmorReduction
        );

        oxygen = Mathf.Min(oxygen, maxoxygen);
    }

    float CurrentSpeed()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsOverweight)
        {
            return speed * overweightSpeedMultiplier;
        }

        return speed;
    }

    void HandleOverweightDrop()
    {
        GameManager gm = GameManager.Instance;

        if (gm == null)
            return;

        if (!gm.IsOverweight)
        {
            overweightDropTimer = 0f;
            return;
        }

        if (!isMoving && !isDashing)
        {
            overweightDropTimer = 0f;
            return;
        }

        overweightDropTimer += Time.deltaTime;

        float interval = Mathf.Max(0.1f, dropInterval);

        if (overweightDropTimer < interval)
            return;

        overweightDropTimer = 0f;
        float chance = Mathf.Clamp(
            gm.ExcessWeight * overweightDropChancePerKg,
            0f,
            100f
        );
        if (chance <= 0f)
            return;

        if (chance < 100f &&
            Random.value >= chance / 100f)
            return;
        int droppedCount = gm.DropNewestExcessFish();
    }

    public void TakeDamage(float dmg)
    {
        if (isOnPier)
            return;

        float finalDamage = dmg * (1f - armorReduction);

        health -= finalDamage;

        if (health <= 0f)
        {
            health = 0f;
            Die();
        }
    }

    void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoseAllFish();

        if (pierSpawn == null)
        {
            Debug.LogError("Player: не назначен Pier Spawn!");
            return;
        }

        if (ScreenFade.Instance != null)
        {
            ScreenFade.Instance.TransitionAction(() =>
            {
                TeleportToPier();
            });
        }
        else
        {
            TeleportToPier();
        }
    }

    void TeleportToPier()
    {
        SetPierMode(true);

        if (body != null)
        {
            body.position = new Vector2(
                pierSpawn.position.x,
                pierSpawn.position.y
            );

            body.linearVelocity = Vector2.zero;
        }
        else
        {
            transform.position = pierSpawn.position;
        }

        oxygen = maxoxygen;
        health = maxHealth;
        if (GameManager.Instance != null)
            GameManager.Instance.SaveGame();
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        isMoving = false;

        float t = 0f;

        while (t < dashDuration && !isOnPier)
        {
            Vector3 nextPosition = Vector3.MoveTowards(
                transform.position,
                targetPoint,
                dashSpeed * Time.deltaTime
            );

            if (playerBounds != null)
            {
                Vector2 clamped = playerBounds.ClampPosition(nextPosition);
                nextPosition.x = clamped.x;
                nextPosition.y = clamped.y;
            }

            transform.position = ClampUnderwaterPosition(nextPosition);

            if (Vector3.Distance(transform.position, targetPoint) < 0.05f)
                break;

            t += Time.deltaTime;
            yield return null;
        }

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }
    Vector3 ClampUnderwaterPosition(Vector3 position)
    {
        position.y = Mathf.Min(
            position.y,
            waterSurfaceY - surfaceMargin
        );

        return position;
    }
    public void ApplyPoisonDamage(
        float totalDamage,
        float duration,
        float tickInterval)
    {
        StartCoroutine(PoisonDamageCoroutine(
            totalDamage,
            duration,
            tickInterval
        ));
    }

    private IEnumerator PoisonDamageCoroutine(
        float totalDamage,
        float duration,
        float tickInterval)
    {
        duration = Mathf.Max(0.01f, duration);
        tickInterval = Mathf.Max(0.01f, tickInterval);

        float elapsed = 0f;

        while (elapsed < duration && !isOnPier)
        {
            float currentTick = Mathf.Min(
                tickInterval,
                duration - elapsed
            );

            yield return new WaitForSeconds(currentTick);

            if (isOnPier)
                yield break;

            float damage = totalDamage * currentTick / duration;

            TakeDamage(damage);

            elapsed += currentTick;
        }
    }
}
