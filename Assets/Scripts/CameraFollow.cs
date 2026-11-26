using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera")]
    public Transform player;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public Camera targetCamera;

    [Header("Depth View")]
    [Tooltip("На сколько уменьшается размер обзора за одну единицу глубины")]
    [Min(0f)]
    public float depthViewReductionPerUnit = 0.1f;

    [Tooltip("Минимальный размер обзора на большой глубине")]
    [Min(0.1f)]
    public float minOrthographicSize = 2f;

    [Header("Smoothing")]
    public float viewSizeSmooth = 4f;

    private float baseOrthographicSize;
    private Player playerScript;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                player = p.transform;
        }

        if (player == null)
        {
            Debug.LogError("CameraFollow: Player не найден!");
            enabled = false;
            return;
        }

        playerScript = player.GetComponent<Player>();

        if (playerScript == null)
        {
            Debug.LogError("CameraFollow: на Player нет скрипта Player!");
            enabled = false;
            return;
        }

        if (targetCamera != null && targetCamera.orthographic)
            baseOrthographicSize = targetCamera.orthographicSize;
    }

    void LateUpdate()
    {
        if (player == null)
            return;

        transform.position = player.position + offset;

        UpdateViewRange();
    }

    void UpdateViewRange()
    {
        if (targetCamera == null || !targetCamera.orthographic)
            return;

        float targetSize = baseOrthographicSize;

        GameManager gm = GameManager.Instance;

        if (!playerScript.IsOnPier && gm != null)
        {
            int level = gm.GetUpgradeLevel(
                UpgradeType.ViewRange
            );

            float depth = Mathf.Max(
                0f,
                playerScript.waterSurfaceY - player.position.y
            );

            float upgradeBonus =
                level * gm.viewSizePerLevel;

            float depthPenalty =
                depth * depthViewReductionPerUnit;

            targetSize =
                baseOrthographicSize +
                upgradeBonus -
                depthPenalty;

            float minimumSize = Mathf.Min(
                minOrthographicSize,
                baseOrthographicSize
            );

            targetSize = Mathf.Max(
                minimumSize,
                targetSize
            );
        }

        targetCamera.orthographicSize = Mathf.Lerp(
            targetCamera.orthographicSize,
            targetSize,
            viewSizeSmooth * Time.deltaTime
        );
    }
}