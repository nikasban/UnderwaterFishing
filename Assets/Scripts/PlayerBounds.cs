
using UnityEngine;

public class PlayerBounds : MonoBehaviour
{
    [Header("Underwater Bounds")]
    public float minX = -30f;
    public float maxX = 30f;
    public float minY = -50f;
    public float maxY = 5f;

    [Header("Pier Bounds")]
    public float pierMinX = 970f;
    public float pierMaxX = 1030f;
    public float pierMinY = -10f;
    public float pierMaxY = 10f;

    private Player player;
    private Rigidbody2D body;

    void Awake()
    {
        player = GetComponent<Player>();
        body = GetComponent<Rigidbody2D>();
    }

    public Vector2 ClampPosition(Vector2 position)
    {
        float currentMinX = minX;
        float currentMaxX = maxX;
        float currentMinY = minY;
        float currentMaxY = maxY;

        if (player != null && player.IsOnPier)
        {
            currentMinX = pierMinX;
            currentMaxX = pierMaxX;
            currentMinY = pierMinY;
            currentMaxY = pierMaxY;
        }

        return new Vector2(
            Mathf.Clamp(position.x, currentMinX, currentMaxX),
            Mathf.Clamp(position.y, currentMinY, currentMaxY)
        );
    }

    void FixedUpdate()
    {
        Vector2 position = body != null
            ? body.position
            : (Vector2)transform.position;

        Vector2 clampedPosition = ClampPosition(position);

        if ((clampedPosition - position).sqrMagnitude > 0.000001f)
        {
            if (body != null)
            {
                body.position = clampedPosition;

                Vector2 velocity = body.linearVelocity;

                if (position.x != clampedPosition.x)
                    velocity.x = 0f;

                if (position.y != clampedPosition.y)
                    velocity.y = 0f;

                body.linearVelocity = velocity;
            }
            else
            {
                transform.position = new Vector3(
                    clampedPosition.x,
                    clampedPosition.y,
                    transform.position.z
                );
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        DrawBounds(minX, maxX, minY, maxY, Color.cyan);
        DrawBounds(pierMinX, pierMaxX, pierMinY, pierMaxY, Color.green);
    }

    void DrawBounds(
        float left,
        float right,
        float bottom,
        float top,
        Color color)
    {
        Gizmos.color = color;

        Vector3 center = new Vector3(
            (left + right) / 2f,
            (bottom + top) / 2f,
            0f
        );

        Vector3 size = new Vector3(
            right - left,
            top - bottom,
            0.1f
        );

        Gizmos.DrawWireCube(center, size);
    }
}
