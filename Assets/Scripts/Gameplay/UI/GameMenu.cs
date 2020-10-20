using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System;
using System.IO;
using System.Linq;

public class GameMenu : MonoBehaviour
{
    public TMP_Text lblMusicFileName;
    public Button btnChooseMusicFile;
    public TMP_Dropdown drpKeyMode;
    public Toggle chkAnalyse;
    public TMP_InputField txtSeed;
    public TMP_Dropdown drpDifficultyPreset;
    public TMP_Dropdown drpSkin;
    public TMP_Dropdown drpScrollSpeedMode;
    public Slider sldSpeedMod;
    public TMP_InputField txtSpeedMod;
    public Slider sldJudgeHeightOffset;
    public TMP_InputField txtJudgeHeightOffset;
    public Toggle chkEarlyLateInd;
    public TMP_Dropdown drpTopTextMode;
    public TMP_Dropdown drpMiddleTextMode;
    public Button btnStart;
    public TMP_Text lblChartFileName;
    public Button btnChooseChartFile;
    public Toggle chkMiniJudgePanelEnabled;
    public Toggle chkAutoplay;
    public Button btnCustomize;

    private GamePreference gp;
    private GenerationParam genParam;
    private bool isUpdating = false;

    private void OnEnable()
    {
        GameConfig currentCfg = GameConfigLoader.Instance.GetGameConfig();
        gp = currentCfg.gamePreference;
        genParam = currentCfg.genParam;
        genParam.seed = 0;
        updateUIValues();
        ValidateValues();
    }

    public void updateUIValues()
    {
        isUpdating = true;
        lblMusicFileName.text = gp.lastSongPath;
        lblChartFileName.text = gp.lastChartPath;
        drpSkin.RefreshShownValue();
        if (genParam.seed != 0)
        {
            txtSeed.text = genParam.seed.ToString();
        }

        switch (gp.speedMode)
        {
            case SpeedMode.FIXED:
                drpScrollSpeedMode.value = 0;
                break;
            case SpeedMode.BPM:
                drpScrollSpeedMode.value = 1;
                break;
        }
        drpScrollSpeedMode.RefreshShownValue();

        sldSpeedMod.value = gp.speedMod;
        txtSpeedMod.text = gp.speedMod.ToString("0.00");
        sldJudgeHeightOffset.value = gp.judgeHeightOffset;
        txtJudgeHeightOffset.text = gp.judgeHeightOffset.ToString("0.00");
        chkEarlyLateInd.isOn = gp.earlyLateIndicator;
        drpTopTextMode.value = (int)gp.topTextMode;
        drpMiddleTextMode.value = (int)gp.middleTextMode;
        drpKeyMode.value = (int)gp.keyMode;
        drpDifficultyPreset.value = gp.difficulty;
        chkMiniJudgePanelEnabled.isOn = gp.miniJudgePanelEnabled;
        chkAutoplay.isOn = gp.autoplay;
        isUpdating = false;
    }

    void Start()
    {
        //GameConfig currentCfg = GameConfigLoader.Instance.GetGameConfig();
        //gp = currentCfg.gamePreference;
        
        // load list of skins
        List<string> skinList = GetDirectories("UserData/Skins/", "*");
        skinList = skinList.Where(item => File.Exists("UserData/Skins/" + item + "/config.json")).ToList();
        for (int i = 0; i < skinList.Count; i++)
        {
            if (gp.skinName == skinList[i])
            {
                drpSkin.value = i;
            }
        }
        drpSkin.AddOptions(skinList);
        updateUIValues();
    }

    List<string> GetDirectories(string path, string searchPattern)
    {
        try
        {
            DirectoryInfo dInfo = new DirectoryInfo(path);
            DirectoryInfo[] directories = dInfo.GetDirectories();
            List<string> dirNames = new List<string>();
            foreach(DirectoryInfo folder in directories)
            {
                dirNames.Add(folder.Name);
            }
            return dirNames;
        }
        catch (UnauthorizedAccessException)
        {
            return new List<string>();
        }
    }

    public void ValidateValues()
    {
        if (isUpdating) return;
        if (string.IsNullOrEmpty(txtSeed.text))
        {
            genParam.seed = 0;
        }
        else
        {
            genParam.seed = int.Parse(txtSeed.text);
        }

        // load list of skins
        if (drpSkin.options.Count > 0)
        {
            gp.skinName = drpSkin.options[drpSkin.value].text;
            drpSkin.RefreshShownValue();
        }

        gp.speedMode = (SpeedMode)drpScrollSpeedMode.value;

        gp.earlyLateIndicator = chkEarlyLateInd.isOn;
        gp.topTextMode = (TextMode) drpTopTextMode.value;
        gp.middleTextMode = (TextMode)drpMiddleTextMode.value;
        gp.analyse = chkAnalyse.isOn;
        gp.autoplay = chkAutoplay.isOn;
        gp.miniJudgePanelEnabled = chkMiniJudgePanelEnabled.isOn;

        if (gp.analyse)
        {
            if (gp.lastSongPath != "")
            {
                btnStart.interactable = true;
            }
            else
            {
                btnStart.interactable = false;
            }
        }
        else
        {
            if (gp.lastSongPath != "" && gp.lastChartPath != "")
            {
                btnStart.interactable = true;
            }
            else
            {
                btnStart.interactable = false;
            }
        }
        gp.keyMode = (KeyMode)drpKeyMode.value;
    }

    public void PreviewMusicFile()
    {
        if (!string.IsNullOrEmpty(gp.lastSongPath))
        {
            System.Diagnostics.Process.Start(gp.lastSongPath);
        }
    }

    public void ChooseMusicFile()
    {
        StartCoroutine(ShowLoadMusicDialogCoroutine());
        gp.lastChartPath = "";
        lblChartFileName.text = "";
    }

    public IEnumerator ShowLoadMusicDialogCoroutine()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Music file", ".wav", ".mp3", ".flac", ".m4a", ".aiff", ".ogg", ".aac"));
        FileBrowser.AddQuickLink("My Music", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), null);
        FileBrowser.AddQuickLink("osu default installation", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\osu!\\Songs", null);
        
        FileBrowser.SetDefaultFilter(".mp3");

        if (!Directory.Exists(gp.lastSongDirectory))
        {
            gp.lastSongDirectory = "";
        }

        yield return FileBrowser.WaitForLoadDialog(false, false, (string.IsNullOrEmpty(gp.lastSongDirectory)) ? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) : gp.lastSongDirectory, "Select Music", "Select");
        if (FileBrowser.Success)
        {
            //SceneManager.LoadScene("ManiaGame", LoadSceneMode.Additive);
            //byte[] SoundFile = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
            //yield return SoundFile;
            //FindObjectOfType<BeatOnsetManager>().loadBeatsAndOnsets(FileBrowser.Result[0]);
            //StartCoroutine(ffmpegCaller.LoadAudio(FileBrowser.Result[0], FindObjectOfType<ManiaGameController>().song, delegate ()
            //{
            //    //isAudioLoaded = true;
            //    GameObject.Find("MainMenu").SetActive(false);
            //}));
            gp.lastSongPath = FileBrowser.Result[0];
            lblMusicFileName.text = FileBrowser.Result[0];
            gp.lastSongDirectory = Path.GetDirectoryName(FileBrowser.Result[0]);
            ValidateValues();
        }
    }

    public void ChooseChartFile()
    {
        StartCoroutine(ShowLoadChartDialogCoroutine());
    }

    public IEnumerator ShowLoadChartDialogCoroutine()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("osu! level file", ".osu"));
        FileBrowser.AddQuickLink("osu default installation", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\osu!\\Songs", null);
        yield return FileBrowser.WaitForLoadDialog(false, false, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Select Chart", "Select");
        if (FileBrowser.Success)
        {
            //SceneManager.LoadScene("ManiaGame", LoadSceneMode.Additive);
            //byte[] SoundFile = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
            //yield return SoundFile;
            //FindObjectOfType<BeatOnsetManager>().loadBeatsAndOnsets(FileBrowser.Result[0]);
            //StartCoroutine(ffmpegCaller.LoadAudio(FileBrowser.Result[0], FindObjectOfType<ManiaGameController>().song, delegate ()
            //{
            //    //isAudioLoaded = true;
            //    GameObject.Find("MainMenu").SetActive(false);
            //}));
            gp.lastChartPath = FileBrowser.Result[0];
            lblChartFileName.text = FileBrowser.Result[0];
            ValidateValues();
        }
    }

    public void SetSpeedMod(float value)
    {
        txtSpeedMod.text = value.ToString("0.00");
        gp.speedMod = value;
    }
    public void SetSpeedMod(string value)
    {
        float floatValue = float.Parse(value);
        if (floatValue > sldSpeedMod.maxValue)
        {
            gp.speedMod = sldSpeedMod.maxValue;
        }
        else if (floatValue < sldSpeedMod.minValue)
        {
            gp.speedMod = sldSpeedMod.minValue;
        }
        else
        {
            gp.speedMod = floatValue;
        }
        sldSpeedMod.value = gp.speedMod;
    }

    public void SetJudgeHeightOffset(string value)
    {
        float floatValue = float.Parse(value);
        if (floatValue > sldJudgeHeightOffset.maxValue)
        {
            gp.judgeHeightOffset = sldJudgeHeightOffset.maxValue;
        }
        else if (floatValue < sldJudgeHeightOffset.minValue)
        {
            gp.judgeHeightOffset = sldJudgeHeightOffset.minValue;
        }
        else
        {
            gp.judgeHeightOffset = floatValue;
        }
        sldJudgeHeightOffset.value = gp.judgeHeightOffset;
    }

    public void SetJudgeHeightOffset(float value)
    {
        txtJudgeHeightOffset.text = value.ToString("0.00");
        gp.judgeHeightOffset = value;
    }

    public void SetDifficulty(int diff)
    {
        if (diff != 4)
        {
            //btnCustomize.interactable = false;
            GenerationParam preset = DiffcultyPresets.GetPreset(diff);
            genParam.beatSnappingDivider = preset.beatSnappingDivider;
            genParam.beatSnappingErrorThreshold = preset.beatSnappingErrorThreshold;
            genParam.multipleLaneChance = preset.multipleLaneChance;
            genParam.onBeatMultipleLaneChance = preset.onBeatMultipleLaneChance;
            genParam.onsetThreshold = preset.onsetThreshold;
            genParam.repeatedLaneTimeThreshold = preset.repeatedLaneTimeThreshold;

            for(int i=0; i < genParam.fourLanesMultipleLanesChance.Length; i++)
            {
                genParam.fourLanesMultipleLanesChance[i] = preset.fourLanesMultipleLanesChance[i];
            }

            for (int i = 0; i < genParam.sixLanesMultipleLanesChance.Length; i++)
            {
                genParam.sixLanesMultipleLanesChance[i] = preset.sixLanesMultipleLanesChance[i];
            }

        }
        gp.difficulty = diff;
    }

    public void startGame()
    {
        btnStart.interactable = false;
        btnCustomize.interactable = false;
        btnChooseChartFile.interactable = false;
        btnChooseMusicFile.interactable = false;
        SceneManagement.Instance.transitionToGame();
    }
}
