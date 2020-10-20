using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GenerationReportEntry
{
    public float onsetTime;
    public bool multipleLane;
    public bool onBeat;
    public int noteCount;
    public List<int> weightedPercentage = new List<int>();
    public List<int> result = new List<int>();
}

[System.Serializable]
public class GenerationReport
{
    public List<GenerationReportEntry> entries;
    public GenerationReport(List<GenerationReportEntry> entries)
    {
        this.entries = entries;
    }
}
