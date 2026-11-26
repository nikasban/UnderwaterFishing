using System.Collections;
using UnityEngine;

public class Player1 : MonoBehaviour
{
    public float speed = 5f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public float doubleClickTime = 0.25f;

    private float targetX;
    private bool isMoving;
    private bool isDashing;
    private bool canDash = true;
    private float lastClickTime = -1f;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        targetX = transform.position.x;
    }

    void Update()
    {
        if (isDashing) return;

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (lastClickTime > 0 && Time.time - lastClickTime < doubleClickTime && canDash)
            {
                targetX = p.x;
                lastClickTime = -1f;
                StartCoroutine(Dash());
            }
            else
            {
                targetX = p.x;
                isMoving = true;
                lastClickTime = Time.time;
            }
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        if (isMoving)
        {
            float dx = targetX - transform.position.x;
            float step = speed * Time.fixedDeltaTime;

            if (Mathf.Abs(dx) <= step)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                isMoving = false;
            }
            else
            {
                rb.linearVelocity = new Vector2(Mathf.Sign(dx) * speed, rb.linearVelocity.y);
            }
        }
        else
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.05f)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        isMoving = false;

        float dir = Mathf.Sign(targetX - transform.position.x);
        float t = 0f;

        while (t < dashDuration)
        {
            if (Mathf.Abs(targetX - transform.position.x) < 0.05f)
                break;

            rb.linearVelocity = new Vector2(dir * dashSpeed, rb.linearVelocity.y);

            t += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}