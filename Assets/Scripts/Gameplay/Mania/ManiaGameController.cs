using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ManiaGameController : MonoBehaviour
{
    public float gameTime = 0f;
    public float beatNormalized = 0f;
    public bool isPlaying = false;
    public bool isAudioLoaded = false;
    public AudioSource song;
    public AudioSource hitSound;
    private int currentBeatIndex = 0;
    private int maxCombo = 0;
    private int perfectCount = 0;
    private int greatCount = 0;
    private int goodCount = 0;
    private int missCount = 0;
    private int holdGoodCount = 0;
    private int holdBadCount = 0;
    private float percentageFromMax = 100;
    private float percentageFromZero = 0;
    private int allNoteCount = 0;
    private List<JudgmentRecord> judgeRecords;
    private List<NotePoint> noteObjects;
    private float travellingTime;
    private float togglePauseTime = 0;

    private double trackStartTime;

    private List<float> musicDiff = new List<float>();
    private List<GenerationReportEntry> genReport = new List<GenerationReportEntry>();

    [Header("Level & Game Params")]
    public float estimatedBPM = 0f;
    public float speedMod = 1f;
    public float damagePerOnePercentOfNotes = 20;
    public float healthPerOnePercentOfNotes = 10;
    public float calibrationOffset = 0f;
    public bool autoplay = false;

    [Header("Other Objects")]
    public FFmpegCaller ffmpegCaller;
    public BeatOnsetManager bm;
    public GameUI gui;
    public PauseMenu pausePanel;
    public Dictionary<int, LaneController> laneCtrls = new Dictionary<int, LaneController>();
    public SkinManager sm;
    public SpriteAnimationPlayer judgmentText;
    public PlayerInput input;
    public SpriteRenderer earlyLateIndicator;
    public BeatGridController bgc;
    private GameConfig cfg;
    private GamePreference gPre;
    private GenerationParam genParam;

    [Header("Timing Values")]
    public float perfectWindow = 0.035f;
    public float greatWindow = 0.065f;
    public float goodWindow = 0.1f;
    public float missWindow = 0.18f;

    public float Health { get; set; } = 0;
    public int Combo { get; set; } = 0;
    public int Score { get; set; } = 0;

    void Awake()
    {
        ffmpegCaller = FindObjectOfType<FFmpegCaller>();
        bm = FindObjectOfType<BeatOnsetManager>();
        gui = FindObjectOfType<GameUI>();
        song = GetComponent<AudioSource>();
        sm = FindObjectOfType<SkinManager>();
        cfg = GameConfigLoader.Instance.GetGameConfig();
        input = GetComponent<PlayerInput>();
        input.onActionTriggered += Input_onActionTriggered;
    }

    private void Input_onActionTriggered(InputAction.CallbackContext ctx)
    {
        var control = ctx.control; // Grab control.
        
        // Can do control-specific checks.
        var button = control as ButtonControl;
        if (button != null)
        {
            string actionName = ctx.action.name;
            if (actionName.StartsWith("Lane") && !autoplay && isPlaying)
            {
                string laneNumber = ctx.action.name.Replace("Lane", "");
                int lane = int.Parse(laneNumber);
                if (ctx.action.phase == InputActionPhase.Started)
                {
                    laneCtrls[lane - 1].Press();
                }
                else if (ctx.action.phase == InputActionPhase.Canceled)
                {
                    laneCtrls[lane - 1].Release();
                }
            }
            else if (actionName.StartsWith("Speed") && ctx.action.phase == InputActionPhase.Started && isPlaying)
            {
                if (actionName == "SpeedIncrease")
                {
                    increaseSpeedMod();
                }
                else if (actionName == "SpeedDecrease")
                {
                    decreaseSpeedMod();
                }
            }
            else if (actionName.StartsWith("Pause") && ctx.action.phase == InputActionPhase.Started && isAudioLoaded && gameTime > 0f)
            {
                togglePause();
            }
        }
    }

    public void calculateTravellingTime()
    {
        travellingTime = 2 - ((speedMod / 10f) * 1.994f);
    }

    public void decreaseSpeedMod()
    {
        speedMod -= 0.25f;
        if (speedMod < 0.25f)
        {
            speedMod = 0.25f;
        }
        calculateTravellingTime();
        gui.updateSpeedModText(speedMod.ToString("0.00"));
    }

    public void increaseSpeedMod()
    {
        speedMod += 0.25f;
        if (speedMod > 10f)
        {
            speedMod = 10;
        }
        calculateTravellingTime();
        gui.updateSpeedModText(speedMod.ToString("0.00"));
    }

    public void togglePause()
    {
        if (isPlaying)
        {
            song.Pause();
            isPlaying = false;
            //input.SwitchCurrentActionMap("UI");
            pausePanel.gameObject.SetActive(true);
            pausePanel.ResetMenu();
            pausePanel.speedModText.text = speedMod.ToString("0.00");
        }
        else
        {
            //GamePreference gPre = cfg.gamePreference;
            //switch (gPre.keyMode)
            //{
            //    case KeyMode._4K:
            //        input.SwitchCurrentActionMap("4K");
            //        break;
            //    case KeyMode._6K:
            //        input.SwitchCurrentActionMap("6K");
            //        break;
            //    default:
            //        break;
            //}
            if (togglePauseTime <= 0)
            {
                StartCoroutine(continuePlaying());
                pausePanel.gameObject.SetActive(false);
            }
        }

    }

    IEnumerator continuePlaying()
    {
        gui.showCountdown();
        togglePauseTime = 3f;
        while(togglePauseTime > 0)
        {
            togglePauseTime -= Time.deltaTime;
            gui.updateCountdown(togglePauseTime + 1);
            yield return new WaitForEndOfFrame();

        }
        gui.hideCountdown();
        song.Play();
        isPlaying = true;
    }

    public void RestartLevel()
    {
        ResetParam();
        gui.ResetUI();
        UpdatePercentage();
        UpdateTopNumberText();
        UpdateMiddleNumberText();
        gui.updateScoreText(Score.ToString());
        if (gPre.miniJudgePanelEnabled)
        {
            gui.updateMiniJudgePanel(perfectCount, greatCount, goodCount, missCount, holdGoodCount, holdBadCount);
        }
        for (int i = 0; i < sm.lanes; i++)
        {
            laneCtrls[i].Restart();

        }
        pausePanel.gameObject.SetActive(false);
        song.Stop();
        startPlay();
    }

    void Start()
    {
        gPre = cfg.gamePreference;
        genParam = cfg.genParam;
        ResetParam();
        switch (gPre.keyMode)
        {
            case KeyMode._4K:
                sm.lanes = 4;
                input.SwitchCurrentActionMap("4K");
                break;
            case KeyMode._6K:
                sm.lanes = 6;
                input.SwitchCurrentActionMap("6K");
                break;
            default:
                break;
        }
        sm.loadSkin(gPre.skinName, false);
        Setup();
    }

    public void test()
    {
        StartCoroutine(testCorotine());
    }

    IEnumerator testCorotine()
    {
        gPre = cfg.gamePreference;
        ResetParam();
        switch (gPre.keyMode)
        {
            case KeyMode._4K:
                sm.lanes = 4;
                input.SwitchCurrentActionMap("4K");
                break;
            case KeyMode._6K:
                sm.lanes = 6;
                input.SwitchCurrentActionMap("6K");
                break;
            default:
                break;
        }
        gPre.skinName = "default";
        gPre.lastChartPath = "UserData\\Charts\\test.osu";
        sm.loadSkin(gPre.skinName, false);
        yield return new WaitForEndOfFrame();
        Setup();
        importBeatmap();
        song.clip = Resources.Load<AudioClip>("CalibrationMusic");
        startPlay();
    }

    public void importBeatmap()
    {
        noteObjects = new List<NotePoint>();
        try
        {
            StreamReader chartFile = new StreamReader(cfg.gamePreference.lastChartPath);
            string line;
            bool hitObjects = false;
            bool timingPoints = false;
            int index = 0;
            while ((line = chartFile.ReadLine()) != null)
            {
                if (line.StartsWith("[TimingPoints]"))
                {
                    timingPoints = true;
                    hitObjects = false;
                    continue;
                }

                if (line.StartsWith("[HitObjects]"))
                {
                    timingPoints = false;
                    hitObjects = true;
                    continue;
                }

                if (timingPoints)
                {

                }

                if (hitObjects)
                {
                    string[] splittedLine = line.Split(',');
                    if (splittedLine.Length == 6)
                    {
                        NotePoint newNotePoint = new NotePoint();
                        newNotePoint.lane = Mathf.FloorToInt(float.Parse(splittedLine[0]) * sm.lanes / 512);
                        newNotePoint.time = float.Parse(splittedLine[2]) / 1000;
                        newNotePoint.noteId = index;
                        string[] splittedColon = splittedLine[5].Split(':');
                        if (splittedColon[0] != "0")
                        {
                            newNotePoint.releaseTime = float.Parse(splittedColon[0]) / 1000;
                            allNoteCount++;
                        }

                        laneCtrls[newNotePoint.lane].timePoints.Add(newNotePoint);
                        index++;
                        allNoteCount++;
                        noteObjects.Add(newNotePoint);
                    }
                }
            }

            for (int i = 0; i < sm.lanes; i++)
            {
                laneCtrls[i].spawnNotes();

            }
            isAudioLoaded = true;
            startPlay();
            SceneManagement.Instance.showGameScreen();
            Debug.Log("allNoteCount: " + allNoteCount.ToString() + " noteObjs:" + noteObjects.Count.ToString());
        }
        catch(System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public int weightedRandom(int numsOfLanes, List<int> previousLanes, float currentOnset, float lastOnset, GenerationReportEntry currentReport)
    {
        if (lastOnset <= 0.001f)
        {
            return Random.Range(0, numsOfLanes);
        }
        else
        {
            float onsetDiff = currentOnset - lastOnset;
            var weights = new Dictionary<int, int>();
            for (int i = 0; i < numsOfLanes; i++)
            {
                int calculatedWeight = 100;
                if (previousLanes.Contains(i))
                {
                    float threshold = 60 / (bm.avgBPM * genParam.repeatedLaneTimeThreshold);
                    if (onsetDiff > threshold)
                    {
                        float percent = (onsetDiff - threshold) / threshold;
                        weights.Add(i, Mathf.FloorToInt(Mathf.Clamp01(percent) * 100f));
                        currentReport.weightedPercentage.Add(Mathf.FloorToInt(Mathf.Clamp01(percent) * 100f));
                    }
                    else
                    {
                        currentReport.weightedPercentage.Add(0);
                    }
                }
                else
                {
                    weights.Add(i, calculatedWeight);
                    currentReport.weightedPercentage.Add(calculatedWeight);
                }

            }

            // if all lane in previous onset are occupied
            if (weights.Count == 0)
            {
                return -1;
            }
            else
            {
                return WeightedRandomizer.From(weights).TakeOne();
            }
        }
        
    }

    public void generateNoteObjects()
    {
        if (genParam.seed == 0)
        {
            genParam.seed = System.Environment.TickCount;
        }

        Random.InitState(genParam.seed);

        var weights = new Dictionary<int, int>();
        if (sm.lanes == 4)
        {
            weights.Add(2, genParam.fourLanesMultipleLanesChance[0]);
            weights.Add(3, genParam.fourLanesMultipleLanesChance[1]);
            weights.Add(4, genParam.fourLanesMultipleLanesChance[2]);
        }
        else if(sm.lanes == 6)
        {
            weights.Add(2, genParam.sixLanesMultipleLanesChance[0]);
            weights.Add(3, genParam.sixLanesMultipleLanesChance[1]);
            weights.Add(4, genParam.sixLanesMultipleLanesChance[2]);
            weights.Add(5, genParam.sixLanesMultipleLanesChance[3]);
            weights.Add(6, genParam.sixLanesMultipleLanesChance[4]);
        }

        noteObjects = new List<NotePoint>();
        int index = 0;
        List<int> lastSpawnedLanes = new List<int>();
        List<int> currentSpawnedLanes = new List<int>();
        float lastOnset = 0;
        foreach (Onset onset in bm.onsets)
        {
            GenerationReportEntry currentReport = new GenerationReportEntry();
            currentReport.onsetTime = onset.time;

            if (onset.pitches.Length > 0 && Random.Range(0, 100) < (onset.beatSnappingValue < 0.01 ? genParam.onBeatMultipleLaneChance : genParam.multipleLaneChance))
            {
                currentReport.multipleLane = true;
                currentReport.onBeat = (onset.beatSnappingValue < 0.01);

                int noteCount = WeightedRandomizer.From(weights).TakeOne();
                currentReport.noteCount = noteCount;

                currentSpawnedLanes.Clear();

                float threshold = 60 / (bm.avgBPM * genParam.repeatedLaneTimeThreshold);
                if ((onset.time - lastOnset) < threshold)
                {
                    if (noteCount >= lastSpawnedLanes.Count)
                    {
                        noteCount = 2;
                    }
                }

                for (int i = 0; i < noteCount; i++)
                {
                    int randomLane;

                        //int retries = 0;
                    do
                        {
                        //    if (retries > 10)
                        //    {
                        //        StringBuilder prevLaneStr = new StringBuilder();
                        //        foreach(int prevLane in lastSpawnedLanes)
                        //        {
                        //            prevLaneStr.Append(prevLane);
                        //            prevLaneStr.Append(", ");
                        //        }
                        //        Debug.LogWarning("MAX RETRIES HIT!: " + prevLaneStr.ToString());
                        //        randomLane = Random.Range(0, sm.lanes);
                        //        Debug.Log("expected: " + randomLane + " got: " + noteObjects[index - (3 - randomLane) - currentSpawnedLanes.Count].lane);
                        //        noteObjects.RemoveAt(index - (3 - randomLane) - currentSpawnedLanes.Count);
                        //        index--;
                        //        break;
                        //    }
                        randomLane = weightedRandom(sm.lanes, lastSpawnedLanes, onset.time, lastOnset, currentReport);
                        //    retries++;
                    }
                    while (currentSpawnedLanes.Contains(randomLane) || randomLane == -1);
                    currentSpawnedLanes.Add(randomLane);
                    NotePoint newNotePoint = new NotePoint();
                    newNotePoint.lane = randomLane;
                    newNotePoint.time = onset.time;
                    newNotePoint.noteId = index;
                    laneCtrls[newNotePoint.lane].timePoints.Add(newNotePoint);
                    index++;
                    allNoteCount++;
                    noteObjects.Add(newNotePoint);
                }

                lastSpawnedLanes.Clear();
                foreach (int lane in currentSpawnedLanes)
                {
                    lastSpawnedLanes.Add(lane);
                    currentReport.result.Add(lane);
                }
            }
            else
            {
                currentReport.multipleLane = false;
                currentReport.onBeat = (onset.beatSnappingValue < 0.01);
                currentReport.noteCount = 1;

                NotePoint newNotePoint = new NotePoint();
                int newLane = weightedRandom(sm.lanes, lastSpawnedLanes, onset.time, lastOnset, currentReport);
                if (newLane == -1)
                {
                    newLane = Random.Range(0, sm.lanes);
                    StringBuilder prevLaneStr = new StringBuilder();
                    for(int i = 0; i < lastSpawnedLanes.Count; i++)
                    {
                        prevLaneStr.Append(lastSpawnedLanes[i]);
                        prevLaneStr.Append(", ");
                        if (lastSpawnedLanes[i] == newLane)
                        {
                            noteObjects.RemoveAt(index - (3 - i));
                            Debug.Log("expected: " + newLane + " got: " + noteObjects[index - (3 - i)].lane);
                            index--;
                        }
                    }
                    Debug.LogWarning("0% HIT!: " + prevLaneStr.ToString());
                }
                newNotePoint.lane = newLane;
                newNotePoint.time = onset.time;
                newNotePoint.noteId = index;
                //if (splittedColon[0] != "0")
                //{
                //    newNotePoint.releaseTime = float.Parse(splittedColon[0]) / 1000;
                //}
                //if (lastSpawnedLanes.Contains(newNotePoint.lane))
                //{
                //    Debug.Log("Jack! Lane: " + newNotePoint.lane + " Onset: " + onset.time.ToString("0.00") + " Diff: " + (onset.time - lastOnset).ToString("0.00"));
                //}

                laneCtrls[newNotePoint.lane].timePoints.Add(newNotePoint);
                index++;
                allNoteCount++;
                noteObjects.Add(newNotePoint);

                lastSpawnedLanes.Clear();
                lastSpawnedLanes.Add(newNotePoint.lane);
                currentReport.result.Add(newNotePoint.lane);
            }
            lastOnset = onset.time;

            genReport.Add(currentReport);
        }

        for (int i = 0; i < sm.lanes; i++)
        {
            laneCtrls[i].spawnNotes();

        }

        isAudioLoaded = true;
        gui.pressToStartPanel.SetActive(true);
        SceneManagement.Instance.showGameScreen();

        File.WriteAllText(@"UserData/genReport.log", JsonUtility.ToJson(new GenerationReport(genReport)));
    }

    public void startPlay()
    {
        gui.pressToStartPanel.SetActive(false);
        isPlaying = true;
        
        trackStartTime = AudioSettings.dspTime + 2.0f;
        song.PlayScheduled(trackStartTime);
        gameTime = -2;
    }

    void Setup()
    {
        
        earlyLateIndicator.gameObject.SetActive(gPre.earlyLateIndicator);
        speedMod = gPre.speedMod;
        autoplay = gPre.autoplay;
        calibrationOffset = cfg.calibrationOffset;
        calculateTravellingTime();
        // setup UI stuff

        gui.setTopTextMode(gPre.topTextMode);
        gui.setMiddleTextMode(gPre.middleTextMode);
        gui.setTextPositions();
        gui.updateScoreText(Score.ToString());
        gui.updateSpeedModText(gPre.speedMod.ToString("0.00"));
        gui.miniJudgePanel.gameObject.SetActive(gPre.miniJudgePanelEnabled);
        gui.setMiniJudgePanelPosition();
        sm.middleJudgmentZone.position = new Vector3(0, gPre.judgeHeightOffset, 0);

    }

    // Update is called once per frame
    void Update()
    {
        // press enter to start
        if (isAudioLoaded && !isPlaying && gameTime < 0.001f && (Keyboard.current.anyKey.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)))
        {

            startPlay();
        }

        if (isPlaying)
        {
            gameTime += Time.deltaTime;
            if (gameTime > 0 && Mathf.Abs(gameTime - (song.timeSamples / (float)song.clip.frequency)) > 0.04f)
            {
                musicDiff.Add(gameTime - (song.timeSamples / (float)song.clip.frequency));
                song.timeSamples = Mathf.RoundToInt(gameTime * song.clip.frequency);
            }
        }

        if (isPlaying && gameTime > song.clip.length / 2)
        {
            // song end
            isPlaying = false;
            song.Stop();
            SceneManagement.Instance.transitionToResult(gPre.analyse, gPre.autoplay, sm.lanes, judgeRecords, noteObjects, maxCombo, percentageFromZero, bm.processTime);

            StringBuilder sb = new StringBuilder();
            foreach (float diff in musicDiff)
            {
                sb.Append(diff.ToString("0.000"));
                sb.Append("\n");
            }
            File.WriteAllText(@"UserData/timeDiffLog.log", sb.ToString());

        }


        if (bm.beats.Count > 2)
        {
            if (gameTime > bm.beats[currentBeatIndex] && currentBeatIndex < bm.beats.Count - 1)
            {
                currentBeatIndex++;
            }

            if (currentBeatIndex > 1)
            {
                beatNormalized = (bm.beats[currentBeatIndex] - gameTime) / (bm.beats[currentBeatIndex] - bm.beats[currentBeatIndex - 1]);
                estimatedBPM = 60f / (bm.beats[currentBeatIndex] - bm.beats[currentBeatIndex - 1]);
            }
        }

    }
    
    public void RecordJudge(Judgement judge, bool isLate, float chartTime, float actionTime, int noteId)
    {
        switch (judge)
        {
            case Judgement.PERFECT:
                perfectCount++;
                Score += 100;
                AddCombo();
                judgmentText.playAnimation(sm.sprAnimClips["perfect"]);
                earlyLateIndicator.sprite = null;
                break;
            case Judgement.GREAT:
                greatCount++;
                Score += 50;
                AddCombo();
                judgmentText.playAnimation(sm.sprAnimClips["great"]);
                earlyLateIndicator.sprite = (isLate ? sm.sprites["late"] : sm.sprites["early"]);
                break;
            case Judgement.GOOD:
                goodCount++;
                Score += 25;
                AddCombo();
                judgmentText.playAnimation(sm.sprAnimClips["good"]);
                earlyLateIndicator.sprite = (isLate ? sm.sprites["late"] : sm.sprites["early"]);
                break;
            case Judgement.GOOD_RELEASED:
                holdGoodCount++;
                Score += 100;
                AddCombo();
                break;
            case Judgement.BAD_RELEASED:
                holdBadCount++;
                BreakCombo();
                judgmentText.playAnimation(sm.sprAnimClips["miss"]);
                earlyLateIndicator.sprite = (isLate ? sm.sprites["late"] : sm.sprites["early"]);
                break;
            case Judgement.MISS:
                missCount++;
                BreakCombo();
                judgmentText.playAnimation(sm.sprAnimClips["miss"]);
                earlyLateIndicator.sprite = (isLate ? sm.sprites["late"] : sm.sprites["early"]);
                break;
            case Judgement.UNPRESSED:
                missCount++;
                BreakCombo();
                judgmentText.playAnimation(sm.sprAnimClips["miss"]);
                earlyLateIndicator.sprite = null;
                break;

        }
        
        judgeRecords.Add(new JudgmentRecord(judge, noteId, chartTime, actionTime, actionTime - chartTime));
        UpdatePercentage();
        UpdateTopNumberText();
        UpdateMiddleNumberText();
        gui.updateScoreText(Score.ToString());
        if (gPre.miniJudgePanelEnabled)
        {
            gui.updateMiniJudgePanel(perfectCount, greatCount, goodCount, missCount, holdGoodCount, holdBadCount);
        }
    }

    void UpdatePercentage()
    {
        float sumFromMax = 0;
        float sumFromZero = 0;
        for(int i = 0; i < allNoteCount; i++)
        {
            if (i < judgeRecords.Count)
            {
                JudgmentRecord currentRecord = judgeRecords[i];
                if (currentRecord.judgement == Judgement.PERFECT || currentRecord.judgement == Judgement.GOOD_RELEASED)
                {
                    sumFromMax += 100;
                    sumFromZero += 100;
                }
                else if (currentRecord.judgement == Judgement.GREAT)
                {
                    sumFromMax += 100 - ((Mathf.Abs(currentRecord.offset) / greatWindow) * 25);
                    sumFromZero += 100 - ((Mathf.Abs(currentRecord.offset) / greatWindow) * 25);
                }
                else if (currentRecord.judgement == Judgement.GOOD)
                {
                    sumFromMax += 100 - ((Mathf.Abs(currentRecord.offset) / goodWindow) * 50);
                    sumFromZero += 100 - ((Mathf.Abs(currentRecord.offset) / goodWindow) * 50);
                }
            }
            else
            {
                sumFromMax += 100;
            }
        }
        percentageFromMax = sumFromMax / allNoteCount;
        percentageFromZero = sumFromZero / allNoteCount;
    }

    void UpdateTopNumberText()
    {
        switch (gPre.topTextMode)
        {
            case TextMode.COMBO:
                
                gui.updateTopNumberText(Combo > 0 ? Combo.ToString() : "");
                break;
            case TextMode.ACCURACY_FROM_MAX:
                gui.updateTopNumberText(percentageFromMax.ToString("0.00") + "%");
                break;
            case TextMode.ACCURACY_FROM_ZERO:
                gui.updateTopNumberText(percentageFromZero.ToString("0.00") + "%");
                break;
            case TextMode.OFF:
                break;
        }
        
    }

    void UpdateMiddleNumberText()
    {
        switch (gPre.middleTextMode)
        {
            case TextMode.COMBO:

                gui.updateMiddleNumberText(Combo.ToString());
                break;
            case TextMode.ACCURACY_FROM_MAX:
                gui.updateMiddleNumberText(percentageFromMax.ToString("0.00") + "%");
                break;
            case TextMode.ACCURACY_FROM_ZERO:
                gui.updateMiddleNumberText(percentageFromZero.ToString("0.00") + "%");
                break;
            case TextMode.OFF:
                break;
        }
    }

    public void AddCombo()
    {
        Combo++;
        if (Combo > maxCombo)
        {
            maxCombo = Combo;
        }
        
    }

    public void BreakCombo()
    {
        Combo = 0;
    }

    private void ResetParam()
    {
        Health = 100f;
        perfectCount = 0;
        greatCount = 0;
        goodCount = 0;
        missCount = 0;
        holdBadCount = 0;
        holdGoodCount = 0;
        Combo = 0;
        maxCombo = 0;
        gameTime = 0f;
        Score = 0;
        percentageFromMax = 100;
        percentageFromZero = 0;
        judgeRecords = new List<JudgmentRecord>();
    }

    public float getTravellingTime()
    {
        return travellingTime;
    }
}
