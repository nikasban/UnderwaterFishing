using UnityEngine;
using UnityEngine.UI;

public class FillBar : MonoBehaviour
{
    public Player player;
    private Image fillBar;

    private void Start()
    {
        fillBar = GetComponent<Image>();
    }

    private void Update()
    {
        if (player != null && player.maxoxygen > 0)
        {
            fillBar.fillAmount = player.oxygen / player.maxoxygen;
        }
    }
}
