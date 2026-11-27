using UnityEngine;

public class PauseUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private MainMenuUI mainMenuUI;
    private void Awake()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void PauseGame()
    {
        if (pausePanel == null)
            return;

        pausePanel.SetActive(true);

        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        AudioListener.pause = false;
        Time.timeScale = 1f;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (mainMenuUI != null)
            mainMenuUI.ReturnToMainMenu();
        else
            Debug.LogError("PauseUI: MainMenuUI не назначен!");
    }
}