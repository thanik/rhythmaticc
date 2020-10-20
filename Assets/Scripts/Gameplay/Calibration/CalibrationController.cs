using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class CalibrationController : MonoBehaviour
{
    public float gameTime = 0f;
    public float beatNormalized = 0f;
    public bool isPlaying = false;
    private float travellingTime;
    public float speedMod = 1f;
    public List<CalibrationNote> laneNotes = new List<CalibrationNote>();
    public List<NotePoint> timePoints = new List<NotePoint>();
    private float pressedTime;
    public int noteIndex;
    public int noteSpawningIndex;
    public bool isPressedInWindow = false;
    public bool isCurrentNoteJudged = false;
    public GameObject notePrefab;
    public float currentOffset = 0f;
    public float range = 0.3f;

    public Color[] judgeColor;
    public Sprite[] earlyLateSprite;
    public SpriteAnimationClip hitAnimation;

    [Header("Objects")]
    public TMP_InputField txtCurrentOffset;
    public TMP_Text txtEarlyCount;
    public TMP_Text txtLateCount;
    public TMP_Text txtJudgment;
    public TMP_Text txtPressedOffset;
    public TMP_Text txtSpeedMod;
    public TMP_Text txtRange;
    public Image imgEarlyLate;
    public SpriteAnimationPlayer hitAnim;
    public TMP_Text txtAverageOffset;

    [Header("Timing Values")]
    public float perfectWindow = 0.035f;
    public float greatWindow = 0.065f;
    public float goodWindow = 0.1f;
    public float missWindow = 0.18f;

    private GameConfigLoader cfgLoader;
    private GameConfig cfg;
    private int earlyCount = 0;
    private int lateCount = 0;
    private Color blue = new Color(0.35f, 0.35f, 1f);
    private Color red = new Color(1f, 0.18f, 0.18f);
    private Color white = new Color(1f, 1f, 1f);
    private AudioSource song;

    private Queue<float> lastOffset = new Queue<float>();
   

    private void Awake()
    {
        cfgLoader = GameConfigLoader.Instance;
        song = GetComponent<AudioSource>();
        cfg = cfgLoader.GetGameConfig();
        currentOffset = cfg.calibrationOffset;
    }

    public void Press()
    {
        isPressedInWindow = judgeNote();
        if (isPressedInWindow)
        {
            hitAnim.playAnimation(hitAnimation);
            laneNotes[noteIndex].transform.localPosition = new Vector3(0f, 7f, 0f);
            laneNotes[noteIndex].gameObject.SetActive(false);

            passToNextNote();
        }
    }
    private void passToNextNote()
    {
        isPressedInWindow = false;
        if (noteIndex < laneNotes.Count - 1)
        {
            noteIndex++; /* judge next note */
            isCurrentNoteJudged = false;
        }
        else
        {
            isCurrentNoteJudged = true;
        }
    }

    public bool judgeNote()
    {
        pressedTime = gameTime;

        float noteOffset = pressedTime - laneNotes[noteIndex].endTime + currentOffset;
        bool isLate = noteOffset > 0;
        float absNoteOffset = Mathf.Abs(noteOffset);

        txtPressedOffset.text = noteOffset.ToString("0.000");
        txtPressedOffset.color = isLate ? red : blue;

        if (absNoteOffset > perfectWindow && absNoteOffset < missWindow)
        {
            if (isLate)
            {
                lateCount++;
            }
            else
            {
                earlyCount++;
            }
            imgEarlyLate.gameObject.SetActive(true);
            imgEarlyLate.sprite = earlyLateSprite[isLate ? 1 : 0];
        }

        if (absNoteOffset < missWindow)
        {
            lastOffset.Enqueue(noteOffset);
            if (lastOffset.Count >= timePoints.Count)
            {
                lastOffset.Dequeue();
            }
            calculateAvgOffset();
        }

        txtEarlyCount.text = earlyCount.ToString();
        txtLateCount.text = lateCount.ToString();
        if (absNoteOffset > goodWindow && absNoteOffset <= missWindow)
        {
            txtJudgment.text = "miss";
            txtJudgment.color = judgeColor[3];
            
            return true;
        }
        else if (absNoteOffset > greatWindow && absNoteOffset <= goodWindow)
        {
            txtJudgment.text = "good";
            txtJudgment.color = judgeColor[2];
            return true;
        }
        else if (absNoteOffset > perfectWindow && absNoteOffset <= greatWindow)
        {
            txtJudgment.text = "great";
            txtJudgment.color = judgeColor[1];
            return true;
        }
        else if (absNoteOffset <= perfectWindow)
        {
            txtJudgment.text = "perfect";
            txtJudgment.color = judgeColor[0];
            imgEarlyLate.gameObject.SetActive(false);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void calculateAvgOffset()
    {
        float avg = lastOffset.Average();
        txtAverageOffset.text = avg.ToString("0.000");
        txtAverageOffset.color = avg > 0 ? red : blue;
    }

    public void resetStat()
    {
        earlyCount = 0;
        lateCount = 0;
        lastOffset.Clear();

        txtEarlyCount.text = earlyCount.ToString();
        txtLateCount.text = lateCount.ToString();

        txtAverageOffset.text = "0.000";
        txtAverageOffset.color = white;
        txtPressedOffset.text = "0.000";
        txtPressedOffset.color = white;
        imgEarlyLate.gameObject.SetActive(false);
        txtJudgment.text = "";
    }

    public void startPlay()
    {
        isPlaying = true;
        song.PlayScheduled(AudioSettings.dspTime + 2f);
        gameTime = -2f;
    }

    public void spawnNotes()
    {
        for (int i = 0; i < timePoints.Count; i++)
        {
            GameObject newObj = Instantiate(notePrefab, transform);
            SpriteRenderer sr = newObj.GetComponent<SpriteRenderer>();
            newObj.transform.position = new Vector3(0f, 7f, 0f);
            CalibrationNote no = newObj.GetComponent<CalibrationNote>();
            no.startPos = new Vector3(0f, 7f, 0f);
            no.endPos = new Vector3(0f, 0f, 0f);
            no.endTime = timePoints[i].time;
            laneNotes.Add(no);
            newObj.SetActive(false);
        }
    }

    void Start()
    {
        calculateTravellingTime();
        timePoints.Add(new NotePoint{ lane = 0, noteId = 0, releaseTime = 0, time = 8f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 1, releaseTime = 0, time = 8.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 2, releaseTime = 0, time = 9.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 3, releaseTime = 0, time = 9.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 4, releaseTime = 0, time = 10.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 5, releaseTime = 0, time = 10.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 6, releaseTime = 0, time = 11.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 7, releaseTime = 0, time = 11.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 8, releaseTime = 0, time = 12.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 9, releaseTime = 0, time = 12.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 10, releaseTime = 0, time = 13.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 11, releaseTime = 0, time = 13.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 12, releaseTime = 0, time = 14.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 13, releaseTime = 0, time = 14.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 14, releaseTime = 0, time = 15.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 15, releaseTime = 0, time = 15.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 16, releaseTime = 0, time = 16.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 17, releaseTime = 0, time = 16.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 18, releaseTime = 0, time = 17.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 19, releaseTime = 0, time = 17.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 20, releaseTime = 0, time = 18.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 21, releaseTime = 0, time = 18.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 22, releaseTime = 0, time = 19.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 23, releaseTime = 0, time = 19.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 24, releaseTime = 0, time = 20.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 25, releaseTime = 0, time = 20.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 26, releaseTime = 0, time = 21.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 27, releaseTime = 0, time = 21.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 28, releaseTime = 0, time = 22.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 29, releaseTime = 0, time = 22.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 30, releaseTime = 0, time = 23.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 31, releaseTime = 0, time = 23.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 32, releaseTime = 0, time = 24.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 33, releaseTime = 0, time = 24.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 34, releaseTime = 0, time = 25.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 35, releaseTime = 0, time = 25.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 36, releaseTime = 0, time = 26.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 37, releaseTime = 0, time = 26.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 38, releaseTime = 0, time = 27.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 39, releaseTime = 0, time = 27.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 40, releaseTime = 0, time = 28.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 41, releaseTime = 0, time = 28.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 42, releaseTime = 0, time = 29.0f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 43, releaseTime = 0, time = 29.5f });
        timePoints.Add(new NotePoint{ lane = 0, noteId = 44, releaseTime = 0, time = 30.0f });
        timePoints.Add(new NotePoint { lane = 0, noteId = 45, releaseTime = 0, time = 30.5f });
        timePoints.Add(new NotePoint { lane = 0, noteId = 46, releaseTime = 0, time = 31.0f });
        timePoints.Add(new NotePoint { lane = 0, noteId = 47, releaseTime = 0, time = 31.5f });
        spawnNotes();
        StartCoroutine(startPlaying());
        txtCurrentOffset.text = currentOffset.ToString("0.000");
        txtRange.text = $"Range: {-range:0.000} to {range:0.000}";
    }
    
    IEnumerator startPlaying()
    {
        yield return new WaitForSeconds(0.5f);
        startPlay();
    }

    // Update is called once per frame
    void Update()
    {
        Keyboard k = Keyboard.current;
        if (k.spaceKey.wasPressedThisFrame)
        {
            Press();
        }

        if (k.f1Key.wasPressedThisFrame)
        {
            increaseOffset();
            resetStat();
        }

        if (k.f2Key.wasPressedThisFrame)
        {
            decreaseOffset();
            resetStat();
        }
        
        if (k.f11Key.wasPressedThisFrame)
        {
            resetStat();
        }

        if (k.f12Key.wasPressedThisFrame)
        {
            currentOffset = 0f;
            txtCurrentOffset.text = currentOffset.ToString("0.000");

            resetStat();
        }

        if (k.escapeKey.wasPressedThisFrame)
        {
            cfg.calibrationOffset = currentOffset;
            cfgLoader.saveConfig();
            SceneManagement.Instance.transitionToMainMenu(false);
        }

        if(k.upArrowKey.wasPressedThisFrame)
        {
            increaseSpeedMod();
        }

        if(k.downArrowKey.wasPressedThisFrame)
        {
            decreaseSpeedMod();
        }

        if (isPlaying)
        {
            gameTime += Time.deltaTime;
            if (gameTime > 0 && Mathf.Abs(gameTime - (song.timeSamples / (float)song.clip.frequency)) > 0.04f)
            {
                song.timeSamples = Mathf.RoundToInt(gameTime * song.clip.frequency);
            }

            if (gameTime > song.clip.length)
            {
                noteIndex = 0;
                noteSpawningIndex = 0;
                isCurrentNoteJudged = false;
                foreach (CalibrationNote noteObj in laneNotes)
                {
                    noteObj.gameObject.SetActive(false);
                }
                song.Stop();
                song.PlayScheduled(AudioSettings.dspTime + 0.5f);
                gameTime = -0.5f;
            }

            if (noteSpawningIndex < timePoints.Count && gameTime + 2f > timePoints[noteSpawningIndex].time)
            {
                CalibrationNote no = laneNotes[noteSpawningIndex];
                no.gameObject.SetActive(true);
                noteSpawningIndex++;
            }

            if (gameTime > laneNotes[noteIndex].endTime + missWindow + currentOffset && !isCurrentNoteJudged)
            {
                txtJudgment.text = "miss";
                txtJudgment.color = judgeColor[3];
                passToNextNote();
            }
        }


    }

    public void decreaseSpeedMod()
    {
        speedMod -= 0.25f;
        if (speedMod < 0.25f)
        {
            speedMod = 0.25f;
        }
        calculateTravellingTime();
        txtSpeedMod.text = speedMod.ToString("0.00");
    }

    public void increaseSpeedMod()
    {
        speedMod += 0.25f;
        if (speedMod > 10f)
        {
            speedMod = 10;
        }
        calculateTravellingTime();
        txtSpeedMod.text = speedMod.ToString("0.00");
    }

    public void setOffset(string value)
    {
        float floatVal = float.Parse(value);
        if (floatVal > range)
        {
            currentOffset = range;
        }
        else if (floatVal < -range)
        {
            currentOffset = -range;
        }
        else
        {
            currentOffset = floatVal;
        }
        txtCurrentOffset.text = currentOffset.ToString("0.000");
        resetStat();
    }

    public void increaseOffset()
    {
        currentOffset += 0.01f;
        if (currentOffset > range)
        {
            currentOffset = range;
        }
        txtCurrentOffset.text = currentOffset.ToString("0.000");
    }

    public void decreaseOffset()
    {
        currentOffset -= 0.01f;
        if (currentOffset < -range)
        {
            currentOffset = -range;
        }
        txtCurrentOffset.text = currentOffset.ToString("0.000");
    }

    void calculateTravellingTime()
    {
        travellingTime = 2 - ((speedMod / 10f) * 1.994f);
    }

    public float getTravellingTime()
    {
        return travellingTime;
    }
}
