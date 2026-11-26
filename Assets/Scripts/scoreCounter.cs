using UnityEngine;
using TMPro;
public class scoreCounter : MonoBehaviour
{
    public TMP_Text counterText;
    private int score = 0;
    void Start()
    {
        UpdateUI();
    }
    public void AddScore()
    {
        score += 1;
        UpdateUI();
    }
    private void UpdateUI()
    {
        counterText.text = $"{score}";
    }
}
