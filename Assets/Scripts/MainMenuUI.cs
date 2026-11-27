using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Player")]
    [SerializeField] private Player player;
    [Header("Gameplay UI")]
    [SerializeField] private GameObject gameplayUI;
    private void Awake()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        else
            Debug.LogError("MainMenuUI: не назначен MainMenuPanel!");

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        else
            Debug.LogError("MainMenuUI: не назначен SettingsPanel!");

        if (player != null)
            player.SetGameplayEnabled(false);
        else
            Debug.LogError("MainMenuUI: Player не найден!");
    }

    public void PlayGame()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (player != null)
            player.SetGameplayEnabled(true);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (player != null)
            player.SetGameplayEnabled(false);
    }
}