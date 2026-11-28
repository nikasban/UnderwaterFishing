using System;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider volumeSlider;
    public TMP_Text volumeValueText;

    [Header("Screen Mode Switch")]
    public Toggle screenModeToggle;
    public Image screenModeTrack;
    public RectTransform screenModeHandle;

    [Header("Screen Switch Colors")]
    [SerializeField]
    private Color fullscreenColor =
        new Color(0.2f, 0.55f, 0.95f, 1f);

    [SerializeField]
    private Color windowedColor =
        new Color(0.55f, 0.55f, 0.55f, 1f);

    [Header("Panels")]
    public GameObject settingsPanel;

    [Header("Navigation")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pausePanel;

    private GameObject previousPanel;

    private const string SettingsFileName = "settings.json";

    [Serializable]
    private class SettingsData
    {
        public float volume = 1f;

        public int screenMode = 0;
    }

    private SettingsData settings = new SettingsData();

    private string SettingsFilePath =>
        Path.Combine(Application.persistentDataPath, SettingsFileName);

    void Start()
    {
        LoadSettings();

        AudioListener.volume = settings.volume;

        SetupVolumeSlider();
        SetupScreenModeToggle();

        UpdateVolumeText();
        ApplyScreenMode(settings.screenMode);
    }

    void SetupVolumeSlider()
    {
        if (volumeSlider == null)
        {
            Debug.LogWarning("SettingsUI: не назначен Volume Slider!");
            return;
        }

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.wholeNumbers = false;

        volumeSlider.onValueChanged.RemoveListener(SetVolume);

        volumeSlider.value = settings.volume;

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        settings.volume = Mathf.Clamp01(value);

        AudioListener.volume = settings.volume;

        UpdateVolumeText();
        SaveSettings();
    }

    void UpdateVolumeText()
    {
        if (volumeValueText == null)
            return;

        int percent = Mathf.RoundToInt(settings.volume * 100f);

        volumeValueText.text = percent + "%";
    }

    void SetupScreenModeToggle()
    {
        if (screenModeToggle == null)
        {
            Debug.LogWarning("SettingsUI: не назначен Screen Mode Toggle!");
            return;
        }

        screenModeToggle.onValueChanged.RemoveListener(SetScreenMode);

        screenModeToggle.transition = Selectable.Transition.None;

        if (screenModeTrack != null)
            screenModeToggle.targetGraphic = screenModeTrack;

        screenModeToggle.graphic = null;

        bool isFullscreen = settings.screenMode == 0;

        screenModeToggle.isOn = isFullscreen;

        screenModeToggle.onValueChanged.AddListener(SetScreenMode);

        UpdateScreenModeSwitchVisual(isFullscreen);
    }

    public void SetScreenMode(bool isFullscreen)
    {
        settings.screenMode = isFullscreen ? 0 : 1;

        ApplyScreenMode(settings.screenMode);

        UpdateScreenModeSwitchVisual(isFullscreen);

        SaveSettings();
    }

    void ApplyScreenMode(int mode)
    {
        bool isFullscreen = mode == 0;

        Screen.fullScreenMode = isFullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        Screen.fullScreen = isFullscreen;
    }

    void UpdateScreenModeSwitchVisual(bool isFullscreen)
    {
        if (screenModeTrack != null)
        {
            screenModeTrack.color = isFullscreen
                ? fullscreenColor
                : windowedColor;
        }

        if (screenModeTrack == null || screenModeHandle == null)
            return;

        float trackWidth =
            screenModeTrack.rectTransform.rect.width;

        float handleWidth =
            screenModeHandle.rect.width;

        float offset = Mathf.Max(
            0f,
            trackWidth / 2f - handleWidth / 2f - 3f
        );

        screenModeHandle.anchoredPosition = new Vector2(
            isFullscreen ? offset : -offset,
            0f
        );
    }

    void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(settings, true);

            Directory.CreateDirectory(Application.persistentDataPath);

            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("Не удалось сохранить настройки: " + e.Message);
        }
    }


    void LoadSettings()
    {
        if (!File.Exists(SettingsFilePath))
        {
            settings = new SettingsData();
            SaveSettings();
            return;
        }

        try
        {
            string json = File.ReadAllText(SettingsFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                ResetSettings("Файл настроек пустой.");
                return;
            }
            bool validVolumeType = Regex.IsMatch(
                json,
                "\"volume\"\\s*:\\s*-?(?:\\d+(?:\\.\\d*)?|\\.\\d+)(?:[eE][+-]?\\d+)?\\s*[,}]"
            );

            bool validScreenModeType = Regex.IsMatch(
                json,
                "\"screenMode\"\\s*:\\s*-?\\d+\\s*[,}]"
            );

            if (!validVolumeType || !validScreenModeType)
            {
                ResetSettings("Неправильные типы данных.");
                return;
            }

            SettingsData loadedSettings =
                JsonUtility.FromJson<SettingsData>(json);

            if (loadedSettings == null ||
                float.IsNaN(loadedSettings.volume) ||
                float.IsInfinity(loadedSettings.volume) ||
                loadedSettings.volume < 0f ||
                loadedSettings.volume > 1f ||
                loadedSettings.screenMode < 0 ||
                loadedSettings.screenMode > 1)
            {
                ResetSettings("Недопустимые значения настроек.");
                return;
            }

            settings = loadedSettings;

            Debug.Log("SettingsUI: настройки успешно загружены.");
        }
        catch (Exception e)
        {
            ResetSettings("Ошибка чтения JSON: " + e.Message);
        }
    }

    private void ResetSettings(string reason)
    {
        Debug.LogWarning(
            "SettingsUI: настройки сброшены. Причина: " + reason
        );

        settings = new SettingsData();

        SaveSettings();

        Debug.Log("SettingsUI: восстановлены настройки по умолчанию.");
    }


    public void OpenSettingsFromMainMenu()
    {
        OpenSettings(mainMenuPanel);
    }

    public void OpenSettingsFromPause()
    {
        OpenSettings(pausePanel);
    }

    private void OpenSettings(GameObject sourcePanel)
    {
        if (settingsPanel == null)
            return;

        previousPanel = sourcePanel;

        if (previousPanel != null)
            previousPanel.SetActive(false);

        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        SaveSettings();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (previousPanel != null)
            previousPanel.SetActive(true);

        previousPanel = null;
    }

    void OnApplicationQuit()
    {
        SaveSettings();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveSettings();
    }
}