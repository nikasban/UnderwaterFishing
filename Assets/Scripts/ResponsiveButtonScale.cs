
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ResponsiveButtonScale : MonoBehaviour
{
    [Header("Scaling")]
    [Min(0.1f)]
    public float minScale = 0.75f;

    [Min(0.1f)]
    public float maxScale = 1.35f;

    private RectTransform rectTransform;
    private Canvas targetCanvas;
    private CanvasScaler canvasScaler;

    private Vector3 originalScale;

    private int lastScreenWidth;
    private int lastScreenHeight;
    private float lastCanvasScale = -1f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;

        targetCanvas = GetComponentInParent<Canvas>();

        if (targetCanvas != null)
            targetCanvas = targetCanvas.rootCanvas;

        if (targetCanvas != null)
            canvasScaler = targetCanvas.GetComponent<CanvasScaler>();
    }

    void Start()
    {
        UpdateButtonScale();
    }

    void LateUpdate()
    {
        if (Screen.width != lastScreenWidth ||
            Screen.height != lastScreenHeight ||
            (targetCanvas != null &&
            !Mathf.Approximately(
                targetCanvas.scaleFactor,
                lastCanvasScale)))
        {
            UpdateButtonScale();
        }
    }

    void UpdateButtonScale()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        if (targetCanvas == null ||
            canvasScaler == null ||
            canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            Debug.LogWarning(
                "ResponsiveButtonScale: Canvas Scaler не найден или настроен неправильно!"
            );
            return;
        }

        float referenceWidth = canvasScaler.referenceResolution.x;

        if (referenceWidth <= 0f)
            return;

        float canvasWidth =
            Screen.width / Mathf.Max(targetCanvas.scaleFactor, 0.0001f);

        float scaleMultiplier = canvasWidth / referenceWidth;

        scaleMultiplier = Mathf.Clamp(
            scaleMultiplier,
            minScale,
            maxScale
        );

        rectTransform.localScale = new Vector3(
            originalScale.x * scaleMultiplier,
            originalScale.y,
            originalScale.z
        );

        lastCanvasScale = targetCanvas.scaleFactor;
    }
}
