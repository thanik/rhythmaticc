using System.Collections;
using System.Collections.Generic;

public enum SpeedMode
{
    FIXED,
    BPM
}

public enum TextMode
{
    OFF,
    COMBO,
    ACCURACY_FROM_ZERO,
    ACCURACY_FROM_MAX
}

public enum KeyMode
{
    _4K,
    _6K,
    BMS
}

[System.Serializable]
public class GamePreference
{
    public string lastSongPath;
    public string lastChartPath;
    public string lastSongDirectory;
    public bool analyse;
    public int difficulty;
    public string skinName;
    public SpeedMode speedMode;
    public TextMode topTextMode;
    public TextMode middleTextMode;
    public bool earlyLateIndicator;
    public float speedMod;
    public KeyMode keyMode;
    public bool autoplay;
    public float judgeHeightOffset;
    public bool miniJudgePanelEnabled;

    public int lastExpAnswer;
    
}

[System.Serializable]
public class GenerationParam
{
    public int seed;
    public float onsetThreshold;
    public int beatSnappingDivider;
    public float beatSnappingErrorThreshold;
    public int multipleLaneChance;
    public int onBeatMultipleLaneChance;
    public float repeatedLaneTimeThreshold;
    public int[] fourLanesMultipleLanesChance;
    public int[] sixLanesMultipleLanesChance;

    public GenerationParam(int seed, float onsetThreshold, int beatSnappingDivider, float beatSnappingErrorThreshold, int multipleLaneChance, 
        int onBeatMultipleLaneChance, float repeatedLaneTimeThreshold, int[] fourLanesMultipleLanesChance, int[] sixLanesMultipleLanesChance)
    {
        this.seed = seed;
        this.onsetThreshold = onsetThreshold;
        this.beatSnappingDivider = beatSnappingDivider;
        this.beatSnappingErrorThreshold = beatSnappingErrorThreshold;
        this.multipleLaneChance = multipleLaneChance;
        this.onBeatMultipleLaneChance = onBeatMultipleLaneChance;
        this.repeatedLaneTimeThreshold = repeatedLaneTimeThreshold;
        this.fourLanesMultipleLanesChance = fourLanesMultipleLanesChance;
        this.sixLanesMultipleLanesChance = sixLanesMultipleLanesChance;

    }
}

[System.Serializable]
public class GameConfig
{
    public int fullScreenMode;
    public int resolutionWidth;
    public int resolutionHeight;
    public int refreshRate;
    public bool verticalSync;
    public float calibrationOffset;
    public float musicVolume;
    public float keySoundVolume;
    public bool agreementAccepted;
    public string machineID;
    public List<BindingSerializable> keyBindings;
    public GamePreference gamePreference;
    public GenerationParam genParam;
}

[System.Serializable]
public struct BindingSerializable
{
    public string id;
    public string path;

    public BindingSerializable(string bindingId, string bindingPath)
    {
        id = bindingId;
        path = bindingPath;
    }
}
