using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class JudgmentRecord
{
    public Judgement judgement;
    public int noteId;
    public float chartTime;
    public float actionTime;
    public float offset;

    public JudgmentRecord(Judgement judge, int noteId, float chartTime, float actionTime, float offset)
    {
        this.judgement = judge;
        this.noteId = noteId;
        this.chartTime = chartTime;
        this.actionTime = actionTime;
        this.offset = offset;
    }
}
