using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using System;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Globalization;

[System.Serializable]
public class Onset
{
    public float time;
    public int[] pitches;
    public float beatSnappingValue;

    public Onset(float time, List<int> pitches)
    {
        this.time = time;
        this.pitches = pitches.ToArray();
        this.beatSnappingValue = -1;
    }
}

public class BeatOnsetManager : MonoBehaviour
{
    //public GameObject analysingPanel;
    public List<float> beats = new List<float>();
    public List<Onset> onsets = new List<Onset>();
    public float processTime;
    public bool isDone = false;
    public bool hasError = false;
    public float avgBPM = 0;
    public bool analyse;
    private bool analyseBeat = false;

    private string beatCacheDirectory;
    private string md5Hash;
    private long fileLength;

    NumberFormatInfo nfi = new NumberFormatInfo();

    private Process analysisProcess;
    void Start()
    {
        beatCacheDirectory = "UserData/BeatCache";
        nfi.NumberDecimalSeparator = ".";
    }

    // Update is called once per frame
    void Update()
    {
        if (isDone)
        {
            GenerationParam genParam = GameConfigLoader.Instance.GetGameConfig().genParam;
            filterOnset(genParam.beatSnappingDivider, genParam.beatSnappingErrorThreshold);
            isDone = false;
            SceneManagement.Instance.DoneAnalyse();
        }
        else
        {
            if (analysisProcess != null && analysisProcess.HasExited && (beats.Count == 0 || onsets.Count == 0) && !hasError)
            {
                Debug.Log("Analysis process exited unexpectedly.");
                SceneManagement.Instance.loadingScreen.FinishProgressbar();
                SceneManagement.Instance.ShowErrorDialog("An error occurred while analysing the song.");
                hasError = true;
            }
        }
    }

    void calculateSongData()
    {
        float sum = 0;
        for (int i = 1;i < beats.Count; i++)
        {
            sum += 60f / (beats[i] - beats[i - 1]);
        }
        avgBPM = sum / beats.Count;

        foreach(Onset onset in onsets)
        {
            for(int i = 0; i < beats.Count - 2; i++)
            {
                if (beats[i] <= onset.time && onset.time <= beats[i+1])
                {
                    onset.beatSnappingValue = (onset.time - beats[i]) / (beats[i + 1] - beats[i]);
                }
            }
        }
        isDone = true;
    }

    public void filterOnset(int divider, float error)
    {
        // snapping beat
        if (divider != 0)
        {
            List<Onset> onsetToBeFiltered = new List<Onset>();
            float snappingValue = 1 / (float)divider;
            bool snapped = false;
            foreach(Onset onset in onsets)
            {
                snapped = false;
                float currentSnappingValue = 0;
                for (int i = 0; i < divider; i++)
                {
                    if (Mathf.Abs(currentSnappingValue - onset.beatSnappingValue) < error)
                    {
                        snapped = true;
                        break;
                    }
                    currentSnappingValue += snappingValue;
                }

                if (!snapped)
                {
                    onsetToBeFiltered.Add(onset);
                }
            }
            Debug.Log("Onset filtered from snapping: " + onsetToBeFiltered.Count + " from total " + onsets.Count);
            foreach(Onset onset in onsetToBeFiltered)
            {
                onsets.Remove(onset);
            }
            Debug.Log("Onsets now: " + onsets.Count);
        }
    }
    
    void saveBeatCache()
    {
        
        BeatCacheFile cacheFile = new BeatCacheFile(fileLength, beats);

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(beatCacheDirectory + "/" + md5Hash + ".beat");
        bf.Serialize(file, cacheFile);
        file.Close();
    }

    public void loadBeatsAndOnsets(string filename, float onsetThreshold, float songLength)
    {
        if (analyse)
        {
            //analysingPanel.SetActive(true);
            analyseBeat = false;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    fileLength = stream.Length;
                    var hash = md5.ComputeHash(stream);
                    md5Hash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    
                    if(!Directory.Exists(beatCacheDirectory))
                    {
                        Directory.CreateDirectory(beatCacheDirectory);
                    }

                    if(File.Exists(beatCacheDirectory + "/" + md5Hash + ".beat"))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        FileStream file = File.Open(beatCacheDirectory + "/" + md5Hash + ".beat", FileMode.Open);
                        try
                        {
                            BeatCacheFile cacheFile = (BeatCacheFile)bf.Deserialize(file);
                            file.Close();

                            if (cacheFile.fileSize == stream.Length)
                            {
                                beats = cacheFile.beats;
                                analyseBeat = false;
                            }
                            else
                            {
                                analyseBeat = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                            analyseBeat = false;
                        }
                    }
                    else
                    {
                        analyseBeat = true;
                    }
                }
            }

            analysisProcess = new Process();
            analysisProcess.StartInfo.FileName = Application.streamingAssetsPath + "/analyse.exe";
            if (analyseBeat)
            {
                analysisProcess.StartInfo.Arguments = "\"" + filename + "\" -ot " + onsetThreshold.ToString("0.00", nfi);
                SceneManagement.Instance.loadingScreen.StartProgressbar(songLength / 3);
            }
            else
            {
                analysisProcess.StartInfo.Arguments = "-o \"" + filename + "\" -ot " + onsetThreshold.ToString("0.00",  nfi);
                SceneManagement.Instance.loadingScreen.StartProgressbar(songLength / 12);
            }
            analysisProcess.StartInfo.WorkingDirectory = Application.streamingAssetsPath;
            analysisProcess.StartInfo.UseShellExecute = false;
            analysisProcess.StartInfo.CreateNoWindow = true;
            analysisProcess.StartInfo.RedirectStandardOutput = true;
            analysisProcess.StartInfo.RedirectStandardError = true;
            //* Set your output and error (asynchronous) handlers
            analysisProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            analysisProcess.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            //* Start process and handlers
            analysisProcess.Start();
            analysisProcess.BeginOutputReadLine();
            analysisProcess.BeginErrorReadLine();
        }
        else
        {
            try
            {
                using (var reader = new StreamReader(filename + ".beats"))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        string[] split = line.Split(',');
                        if (float.TryParse(split[0], out float result))
                        {
                            beats.Add(result);
                        }
                    }
                }

                //using (var reader = new StreamReader(filename + ".onsets"))
                //{
                //    while (!reader.EndOfStream)
                //    {
                //        var line = reader.ReadLine();
                //        if (float.TryParse(line, out float result))
                //        {
                //            onsets.Add(result);
                //        }
                //    }
                //}
                calculateSongData();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        
    }


    void OutputHandler(object sender, DataReceivedEventArgs outLine)
    {
        try
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                string[] lines = outLine.Data.Split('\n');
                foreach (string line in lines)
                {

                    if (line.StartsWith("b"))
                    {
                       
                        string[] split = outLine.Data.Split(':');
                        if (split.Length > 1)
                        {
                            Debug.Log("get beat data");
                            string[] times = split[1].Split(',');
                            foreach (string time in times)
                            {
                                if (float.TryParse(time, NumberStyles.Float, nfi, out float result))
                                {
                                    beats.Add(result);
                                }
                            }
                        }
                    }
                    else if (line.StartsWith("o"))
                    {
                        string[] split = outLine.Data.Split(':');
                        if (split.Length > 1)
                        {
                            Debug.Log("get onset data");
                            string[] onsetsString = split[1].Split(',');
                            List<int> pitches = new List<int>();
                            foreach (string onset in onsetsString)
                            {
                                pitches.Clear();
                                string[] splittedOnset = onset.Split(';');
                                if (splittedOnset[0] != "")
                                {
                                    if (splittedOnset.Length > 1)
                                    {
                                        foreach (string splittedPitch in splittedOnset[1].Split('+'))
                                        {
                                            if (splittedPitch != "")
                                            {
                                                pitches.Add(int.Parse(splittedPitch));
                                            }
                                        }
                                    }
                                    onsets.Add(new Onset(float.Parse(splittedOnset[0], NumberStyles.Float, nfi), pitches));
                                }
                            }
                        }
                    }
                    else if (line.StartsWith("t"))
                    {
                        Debug.Log(outLine.Data);
                        string[] split = outLine.Data.Split(':');
                        if (split.Length > 1)
                        {
                            float.TryParse(split[1], NumberStyles.Float, nfi, out processTime);
                        }

                        if (analyseBeat)
                        {
                            saveBeatCache();
                        }
                        calculateSongData();
                    }
                    else
                    {
                        Debug.Log(outLine.Data);
                    }
                }
            }
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
