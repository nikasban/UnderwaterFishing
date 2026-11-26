
using UnityEngine;
using UnityEngine.EventSystems;

public class Harpoon : MonoBehaviour
{
    public Transform player;

    public enum state
    {
        Following,
        toTarget,
        Returning
    }

    [Header("Harpoon")]
    public float speed = 10f;
    public float baseRange = 10f;

    private state cur = state.Following;
    private Vector3 target;

    private SpriteRenderer[] spriteRenderers;
    private Player playerScript;

    private bool isAvailable;

    void Awake()
    {
        spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        SetVisual(false);
    }

    void Start()
    {
        FindPlayer();

        if (player == null)
        {
            Debug.LogError("Harpoon: Player не найден!");
            return;
        }

        transform.position = player.position;

        if (playerScript != null)
            SetAvailable(!playerScript.IsOnPier);
        else
            SetAvailable(false);

        if (spriteRenderers.Length == 0)
        {
            Debug.LogError(
                "Harpoon: SpriteRenderer не найден на объекте " +
                "или его дочерних объектах!"
            );
        }
    }

    void FindPlayer()
    {
        if (player == null)
        {
            GameObject found =
                GameObject.FindGameObjectWithTag("Player");

            if (found != null)
                player = found.transform;
        }

        if (player != null)
            playerScript = player.GetComponent<Player>();
    }

    float CurrentRange()
    {
        if (GameManager.Instance == null)
            return baseRange;

        int level =
            GameManager.Instance.GetUpgradeLevel(
                UpgradeType.HarpoonRange
            );

        return baseRange +
            level * GameManager.Instance.harpoonRangePerLevel;
    }

    public void SetAvailable(bool value)
    {
        FindPlayer();
        if (value && playerScript != null &&
            playerScript.IsOnPier)
        {
            value = false;
        }

        isAvailable = value;
        cur = state.Following;

        if (player != null)
            transform.position = player.position;

        SetVisual(false);

        Debug.Log("Harpoon доступен: " + isAvailable);
    }

    void SetVisual(bool visible)
    {
        if (spriteRenderers == null)
            return;

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
                sr.enabled = visible;
        }
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();

            if (player == null)
                return;
        }

        if (playerScript != null &&
            playerScript.IsOnPier &&
            isAvailable)
        {
            SetAvailable(false);
        }

        if (!isAvailable)
        {
            transform.position = player.position;
            return;
        }

        Camera cam = Camera.main;

        if (cam == null)
            return;
        if (Input.GetMouseButtonDown(0) &&
            cur != state.Returning)
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector3 mouseScreen = Input.mousePosition;

            float distanceToPlane = Mathf.Abs(
                player.position.z - cam.transform.position.z
            );

            mouseScreen.z = distanceToPlane;

            Vector3 mouseWorld =
                cam.ScreenToWorldPoint(mouseScreen);

            mouseWorld.z = player.position.z;

            float distance = Vector2.Distance(
                mouseWorld,
                player.position
            );

            if (distance <= CurrentRange())
            {
                target = mouseWorld;
                cur = state.toTarget;

                transform.position = player.position;
                SetVisual(true);

                Debug.Log("Harpoon: запущен!");
            }
            else
            {
                Debug.Log(
                    "Harpoon: цель слишком далеко. Дистанция: " +
                    distance + ", дальность: " + CurrentRange()
                );
            }
        }

        switch (cur)
        {
            case state.Following:

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    player.position,
                    speed * Time.deltaTime
                );

                SetVisual(false);
                break;

            case state.toTarget:

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    speed * Time.deltaTime
                );

                if (Vector3.Distance(
                    transform.position,
                    target) < 0.1f)
                {
                    cur = state.Returning;
                }

                break;

            case state.Returning:

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    player.position,
                    speed * Time.deltaTime
                );

                if (Vector3.Distance(
                    transform.position,
                    player.position) < 0.1f)
                {
                    transform.position = player.position;
                    cur = state.Following;
                    SetVisual(false);
                }

                break;
        }
    }
}
