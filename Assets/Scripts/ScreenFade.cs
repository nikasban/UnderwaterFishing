using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    public static ScreenFade Instance;
    public float fadeDuration = 0.5f;
    public float waitBeforeLoad = 1f;

    private Image fadeImage;
    private bool isTransitioning;
    public bool IsTransitioning => isTransitioning;

    public bool CanStartTransition =>
        !isTransitioning && fadeImage != null;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateFadeCanvas();
    }

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    void CreateFadeCanvas()
    {
        GameObject canvasObject = new GameObject("FadeCanvas");
        canvasObject.transform.SetParent(transform);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;

        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvasObject.transform, false);

        fadeImage = imageObject.AddComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = true;

        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        SetAlpha(1f);
    }

    void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    IEnumerator FadeIn()
    {
        yield return Fade(1f, 0f);
        fadeImage.raycastTarget = false;
    }

    IEnumerator Fade(float from, float to)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / fadeDuration);
            SetAlpha(Mathf.Lerp(from, to, t));

            yield return null;
        }

        SetAlpha(to);
    }

    public void TransitionTo(string sceneName)
    {
        if (isTransitioning) return;

        StartCoroutine(Transition(sceneName));
    }

    IEnumerator Transition(string sceneName)
    {
        isTransitioning = true;

        fadeImage.raycastTarget = true;

        yield return Fade(0f, 1f);
        yield return new WaitForSecondsRealtime(waitBeforeLoad);

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
            yield return null;

        yield return null;

        yield return Fade(1f, 0f);

        fadeImage.raycastTarget = false;
        isTransitioning = false;
    }
    public void TransitionAction(Action action)
    {
        if (isTransitioning || fadeImage == null)
            return;

        StartCoroutine(TransitionActionCoroutine(action));
    }

    IEnumerator TransitionActionCoroutine(Action action)
    {
        isTransitioning = true;
        fadeImage.raycastTarget = true;

        yield return Fade(fadeImage.color.a, 1f);
        yield return new WaitForSecondsRealtime(waitBeforeLoad);

        action?.Invoke();

        yield return null;
        yield return Fade(1f, 0f);

        fadeImage.raycastTarget = false;
        isTransitioning = false;
    }

}
