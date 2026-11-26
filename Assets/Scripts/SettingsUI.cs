using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider volumeSlider;
    public TMP_Text volumeValueText;
    public TMP_Dropdown screenModeDropdown;
    public GameObject settingsPanel;

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
        SetupScreenModeDropdown();

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

    void SetupScreenModeDropdown()
    {
        if (screenModeDropdown == null)
        {
            Debug.LogWarning("SettingsUI: не назначен Screen Mode Dropdown!");
            return;
        }

        screenModeDropdown.onValueChanged.RemoveListener(SetScreenMode);

        screenModeDropdown.ClearOptions();

        screenModeDropdown.AddOptions(
            new List<string>
            {
                "Полноэкранный",
                "Оконный"
            }
        );

        screenModeDropdown.value = settings.screenMode;
        screenModeDropdown.RefreshShownValue();

        screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
    }

    public void SetScreenMode(int mode)
    {
        settings.screenMode = Mathf.Clamp(mode, 0, 1);

        ApplyScreenMode(settings.screenMode);

        SaveSettings();
    }

    void ApplyScreenMode(int mode)
    {
        if (mode == 0)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
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
            return;
        }

        try
        {
            string json = File.ReadAllText(SettingsFilePath);

            SettingsData loadedSettings =
                JsonUtility.FromJson<SettingsData>(json);

            if (loadedSettings != null)
            {
                settings = loadedSettings;
            }
            else
            {
                settings = new SettingsData();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                "Не удалось загрузить настройки. Используются значения по умолчанию. " +
                e.Message
            );

            settings = new SettingsData();
        }

        settings.volume = Mathf.Clamp01(settings.volume);
        settings.screenMode = Mathf.Clamp(settings.screenMode, 0, 1);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        SaveSettings();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
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
