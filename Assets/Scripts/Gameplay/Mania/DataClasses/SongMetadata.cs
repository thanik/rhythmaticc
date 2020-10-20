using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SongMetadata
{
    public string songName;
    public string songArtist;
    public string songFilename;
    public float songLength;
    public Difficulty difficulty;
    public GenerationParam genParam;
    public float avgBPM;

    public SongMetadata(string songName, string songArtist, float songLength, string songFilename, Difficulty difficulty, GenerationParam genParam, float avgBPM)
    {
        this.songName = songName;
        this.songArtist = songArtist;
        this.songLength = songLength;
        this.songFilename = songFilename;
        this.difficulty = difficulty;
        this.genParam = genParam;
        this.avgBPM = avgBPM;
    }
}
