using UnityEngine;
using UnityEngine.UI;

public class StatusBarsUI : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public Image healthFill;
    public Image inventoryFill;

    void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();
    }

    void Update()
    {
        if (player != null && healthFill != null)
        {
            float healthPercent = player.maxHealth > 0f
                ? player.health / player.maxHealth
                : 0f;

            healthFill.fillAmount = Mathf.Clamp01(healthPercent);
        }
        GameManager gm = GameManager.Instance;

        if (gm != null && inventoryFill != null)
        {
            float inventoryPercent = gm.CurrentMaxWeight > 0f
                ? gm.TotalWeight / gm.CurrentMaxWeight
                : 0f;

            inventoryFill.fillAmount = Mathf.Clamp01(inventoryPercent);
        }
    }
}