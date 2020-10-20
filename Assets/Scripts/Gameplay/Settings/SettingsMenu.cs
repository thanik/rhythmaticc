using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{

    public AudioMixer mixer;
    Resolution[] resolutions;

    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fullScreenModeDropdown;
    public Slider musicVolumeSlider;
    public Slider keySoundVolumeSlider;
    public Toggle vsyncToggle;
    public TMP_Text latencyText;

    GameConfig currentConfig;

    private void OnEnable()
    {
        currentConfig = GameConfigLoader.Instance.GetGameConfig();
        updateUI();
    }

    void updateUI()
    {
        
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height + "@" + resolutions[i].refreshRate + "Hz";
            options.Add(option);

            //if (resolutions[i].width == Screen.currentResolution.width &&
            //    resolutions[i].height == Screen.currentResolution.height &&
            //    resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
            if (resolutions[i].width == currentConfig.resolutionWidth &&
                resolutions[i].height == currentConfig.resolutionHeight &&
                resolutions[i].refreshRate == currentConfig.refreshRate)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();

        vsyncToggle.isOn = QualitySettings.vSyncCount > 0;

        musicVolumeSlider.value = currentConfig.musicVolume;
        keySoundVolumeSlider.value = currentConfig.keySoundVolume;
        fullScreenModeDropdown.value = currentConfig.fullScreenMode;
        fullScreenModeDropdown.RefreshShownValue();

        latencyText.text = "Current Setting: " + (currentConfig.calibrationOffset * 1000).ToString("0") + "ms";
    }

    public void btnSave()
    {
        GameConfigLoader.Instance.saveConfig();
    }

    public void SetMusicVolume(float volume)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        currentConfig.musicVolume = volume;
    }

    public void SetKeySoundVolume(float volume)
    {
        mixer.SetFloat("KeySoundVolume", Mathf.Log10(volume) * 20);
        currentConfig.keySoundVolume = volume;
    }

    public void SetFullscreenMode(int mode)
    {
        switch(mode)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                currentConfig.fullScreenMode = (int)FullScreenMode.ExclusiveFullScreen;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                currentConfig.fullScreenMode = (int)FullScreenMode.FullScreenWindow;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                currentConfig.fullScreenMode = (int)FullScreenMode.Windowed;
                break;
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution res = resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRate);
        currentConfig.resolutionHeight = res.height;
        currentConfig.resolutionWidth = res.width;
        currentConfig.refreshRate = res.refreshRate;
    }

    public void SetVsync(bool isVsync)
    {
        if (isVsync)
        {
            QualitySettings.vSyncCount = 1;
            currentConfig.verticalSync = true;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            currentConfig.verticalSync = false;
        }
    }

    public void calibrate()
    {
        SceneManagement.Instance.transitionToCalibration();
    }
}
