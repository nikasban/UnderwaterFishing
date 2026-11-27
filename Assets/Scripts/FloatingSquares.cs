using UnityEngine;

public class FloatingSquare : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private float startY;
    private float surfaceY;

    private float startSize;
    private float endSize;
    private float speed;

    public void Initialize(
        float targetSurfaceY,
        float initialSize,
        float finalSize,
        float movementSpeed,
        float alpha)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        startY = transform.position.y;
        surfaceY = targetSurfaceY;

        startSize = initialSize;
        endSize = finalSize;
        speed = movementSpeed;

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        transform.localScale = Vector3.one * startSize;
    }

    void Update()
    {
        transform.position +=
            Vector3.up * speed * Time.deltaTime;

        float progress = Mathf.InverseLerp(
            startY,
            surfaceY,
            transform.position.y
        );

        float currentSize = Mathf.Lerp(
            startSize,
            endSize,
            progress
        );

        transform.localScale =
            Vector3.one * currentSize;

        if (transform.position.y >= surfaceY)
        {
            Destroy(gameObject);
        }
    }
}