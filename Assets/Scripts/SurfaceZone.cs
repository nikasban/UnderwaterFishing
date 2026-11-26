using UnityEngine;

public class SurfaceZone : MonoBehaviour
{
    public GameObject exitButton;
    public Transform player;
    public float surfaceY = 3f;

    private bool isVisible = false;

    void Start()
    {
        if (exitButton != null)
        {
            exitButton.SetActive(false);
            isVisible = false;
        }
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null || exitButton == null) return;

        bool shouldShow = player.position.y >= surfaceY;

        if (shouldShow != isVisible)
        {
            isVisible = shouldShow;
            exitButton.SetActive(shouldShow);
            Debug.Log($"SurfaceZone: кнопка {(shouldShow ? "появилась" : "скрылась")} (Y игрока = {player.position.y:F2})");
        }
    }
}