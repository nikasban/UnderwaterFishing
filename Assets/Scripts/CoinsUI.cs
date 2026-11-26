using UnityEngine;
using TMPro;

public class CoinsUI : MonoBehaviour
{
    public TMP_Text coinsText;

    void Update()
    {
        if (GameManager.Instance == null || coinsText == null) return;
        coinsText.text = $"Монеты: {GameManager.Instance.coins}";
    }
}