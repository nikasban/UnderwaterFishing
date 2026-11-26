
using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    public Player player;
    public Transform underwaterSpawn;

    private bool isTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered || !other.CompareTag("Player"))
            return;

        if (player == null)
            player = other.GetComponentInParent<Player>();

        if (player == null || underwaterSpawn == null)
        {
            Debug.LogError(
                "SceneTrigger: не назначен Player или UnderwaterSpawn!"
            );
            return;
        }

        ScreenFade fade = ScreenFade.Instance;

        if (fade == null || !fade.CanStartTransition)
            return;

        if (!player.IsOnPier)
            return;

        isTriggered = true;

        fade.TransitionAction(() =>
        {
            player.SetPierMode(false);

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();

            if (body != null)
            {
                body.position = underwaterSpawn.position;
                body.linearVelocity = Vector2.zero;
            }
            else
            {
                player.transform.position = underwaterSpawn.position;
            }

            player.oxygen = player.maxoxygen;

            isTriggered = false;
        });
    }
}
