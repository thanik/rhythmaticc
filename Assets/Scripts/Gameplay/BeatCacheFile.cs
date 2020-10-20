using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BeatCacheFile
{
    public long fileSize;
    public List<float> beats;

    public BeatCacheFile(long fileSize, List<float> beats)
    {
        this.fileSize = fileSize;
        this.beats = beats;
    }
}
