using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class GameConfigLoader : Singleton<GameConfigLoader>
{
    private const string USER_PATH = "UserData/";
    public GameConfig currentConfig;
    public InputActionAsset inputActions;
    public AudioMixer mixer;
    public GameObject agreementPanel;
    public GameConfig GetGameConfig() => currentConfig;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        loadConfig();
        checkAgreement();
    }

    void checkAgreement()
    {
        if (!currentConfig.agreementAccepted)
        {
            agreementPanel.SetActive(true);
        }
    }

    void loadConfig()
    {
        if (!File.Exists(USER_PATH + "config.json"))
        {
            initializeConfig();
        }

        if (!Directory.Exists(USER_PATH + "/BeatCache"))
        {
            Directory.CreateDirectory(USER_PATH + "/BeatCache");
        }

        StreamReader configFile = new StreamReader(USER_PATH + "config.json");
        currentConfig = JsonUtility.FromJson<GameConfig>(configFile.ReadToEnd());
        if (currentConfig != null)
        {
            applyCurrentSettings();
            LoadControlOverrides();
        }
    }

    void initializeConfig()
    {
        GameConfig newConfig = new GameConfig();
        newConfig.calibrationOffset = 0f;
        newConfig.fullScreenMode = (int)FullScreenMode.ExclusiveFullScreen;
        newConfig.verticalSync = true;
        newConfig.resolutionWidth = Screen.width;
        newConfig.resolutionHeight = Screen.height;
        newConfig.refreshRate = Screen.currentResolution.refreshRate;
        newConfig.keySoundVolume = 0.0001f;
        newConfig.musicVolume = 0.5f;
        newConfig.agreementAccepted = false;

        GamePreference newPref = new GamePreference();
        newPref.analyse = true;
        newPref.lastSongPath = "";
        newPref.lastChartPath = "";
        newPref.lastSongDirectory = "";
        newPref.middleTextMode = TextMode.OFF;
        newPref.topTextMode = TextMode.COMBO;
        newPref.speedMode = SpeedMode.FIXED;
        newPref.skinName = "default";
        newPref.earlyLateIndicator = true;
        newPref.speedMod = 1.0f;
        newPref.autoplay = false;

        newPref.lastExpAnswer = -1;

        //GenerationParam genParam = new GenerationParam();
        //genParam.beatSnappingDivider = 32;
        //genParam.beatSnappingErrorThreshold = 0.01f;
        //genParam.onsetThreshold = 1.00f;
        //genParam.seed = 0;
        //genParam.multipleLaneChance = 15;
        //genParam.repeatedLaneTimeThreshold = 4;
        //genParam.fourLanesMultipleLanesChance = new int[3] { 100, 10, 1 };
        //genParam.sixLanesMultipleLanesChance = new int[5] { 100, 100, 3, 2, 0 };
        GenerationParam genParam = new GenerationParam(
            0, //seed
            0.53f, // onset threshold
            2, // beat snapper divider
            0.002f, // beat snapping error threshold
            1, // multipleLaneChance
            3, // onBeatMultipleLaneChance
            1, // repeatedLaneTime
            new int[] { 100, 0, 0 }, // 4 lanes chance
            new int[] { 100, 0, 0, 0, 0 } // 6 lanes chance
            );

        newConfig.gamePreference = newPref;
        newConfig.genParam = genParam;

        File.WriteAllText(USER_PATH + "config.json", JsonUtility.ToJson(newConfig));
    }

    void applyCurrentSettings()
    {
        Screen.SetResolution(currentConfig.resolutionWidth, currentConfig.resolutionHeight, (FullScreenMode)currentConfig.fullScreenMode, currentConfig.refreshRate);
        QualitySettings.vSyncCount = (currentConfig.verticalSync ? 1 : 0);
        mixer.SetFloat("MusicVolume", Mathf.Log10(currentConfig.musicVolume) * 20);
        mixer.SetFloat("KeySoundVolume", Mathf.Log10(currentConfig.keySoundVolume) * 20);
    }

    public void saveConfig()
    {
        StoreControlOverrides();
        GamePreference gPre = currentConfig.gamePreference;
        gPre.lastChartPath = "";
        gPre.lastSongPath = "";
        gPre.autoplay = false;
        File.WriteAllText(USER_PATH + "config.json", JsonUtility.ToJson(currentConfig));
    }

    /// <summary>
    /// stores the active control overrides to player prefs
    /// </summary>
    public void StoreControlOverrides()
    {
        //saving
        currentConfig.keyBindings.Clear();
        foreach (var map in inputActions.actionMaps)
        {
            foreach (var binding in map.bindings)
            {
                if (!string.IsNullOrEmpty(binding.overridePath))
                {
                    currentConfig.keyBindings.Add(new BindingSerializable(binding.id.ToString(), binding.overridePath));
                }
            }
        }
    }

    /// <summary>
    /// Loads control overrides from playerprefs
    /// </summary>
    public void LoadControlOverrides()
    {

        //create a dictionary to easier check for existing overrides
        Dictionary<System.Guid, string> overrides = new Dictionary<System.Guid, string>();
        foreach (var item in currentConfig.keyBindings)
        {
            overrides.Add(new System.Guid(item.id), item.path);
        }

        //walk through action maps check dictionary for overrides
        foreach (var map in inputActions.actionMaps)
        {
            var bindings = map.bindings;
            for (var i = 0; i < bindings.Count; ++i)
            {
                if (overrides.TryGetValue(bindings[i].id, out string overridePath))
                {
                    //if there is an override apply it
                    map.ApplyBindingOverride(i, new InputBinding { overridePath = overridePath });
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
