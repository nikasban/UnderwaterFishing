using UnityEngine;
using TMPro;

public class HealthUI : MonoBehaviour
{
    public TMP_Text healthText;
    public Player player;

    void Awake()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (healthText != null)
            healthText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (healthText == null)
            return;

        bool shouldShow = player != null && !player.IsOnPier;

        if (healthText.gameObject.activeSelf != shouldShow)
            healthText.gameObject.SetActive(shouldShow);

        if (!shouldShow)
            return;

        healthText.text =
            $"HP: {Mathf.CeilToInt(player.health)}/{Mathf.CeilToInt(player.maxHealth)}";
    }
}