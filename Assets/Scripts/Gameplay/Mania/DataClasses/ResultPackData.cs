using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ResultPackData
{
    public bool generated;
    public bool autoplay;
    public int keyCount;
    public SongMetadata metadata;
    public List<NotePoint> levelNotes;
    public List<JudgmentRecord> judgeRecords;
    public int perfectCount;
    public int greatCount;
    public int goodCount;
    public int missCount;
    public int earlyCount;
    public int lateCount;
    public int maxCombo;
    public int goodHoldCount;
    public int badHoldCount;

    public float averageTime;
    public float stdDevTime;
    public float maxEarly;
    public float maxLate;

    public float accuracyPercentage;
    public int score;

    public float analysisTime;
    public string reportGeneratedTime;
}
