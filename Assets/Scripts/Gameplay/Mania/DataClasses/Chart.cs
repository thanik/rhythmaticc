using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class NotePoint
{
    public int noteId;
    public float time;
    public float releaseTime;
    public int lane;
}

[Serializable]
public class Chart
{
    public string songName;
    public string artistName;
    public string diffculty;
    public string chartAuthor;
    public string songChecksum;
    public List<NotePoint> notes;
    public List<float> beats;
}
