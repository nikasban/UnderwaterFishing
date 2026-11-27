
using UnityEngine;

public class ExitWaterButton : MonoBehaviour
{
    public Player player;
    public Transform pierSpawn;

    public void ExitWater()
    {
        ScreenFade fade = ScreenFade.Instance;

        if (fade == null || !fade.CanStartTransition)
            return;

        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (player == null || pierSpawn == null)
        {
            Debug.LogError(
                "ExitWaterButton: не назначен Player или PierSpawn!"
            );
            return;
        }

        if (player.IsOnPier)
            return;

        fade.TransitionAction(() =>
        {
            player.SetPierMode(true);

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();

            if (body != null)
            {
                body.position = pierSpawn.position;
                body.linearVelocity = Vector2.zero;
            }
            else
            {
                player.transform.position = pierSpawn.position;
            }

            player.oxygen = player.maxoxygen;
            if (GameManager.Instance != null)
                GameManager.Instance.SaveGame();
        });
    }
}
