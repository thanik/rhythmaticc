using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ResultPanel : MonoBehaviour
{
    public TMP_Text perfectCount;
    public TMP_Text greatCount;
    public TMP_Text goodCount;
    public TMP_Text missCount;

    public TMP_Text goodHoldCount;
    public TMP_Text badHoldCount;

    public TMP_Text earlyCount;
    public TMP_Text lateCount;

    public TMP_Text maxCombo;
    
    public TMP_Text averageTime;
    public TMP_Text stdDivTime;
    public TMP_Text maxEarlyTime;
    public TMP_Text maxLateTime;

    public TMP_InputField levelSeed;
    public TMP_Text numberOfNotes;

    public TMP_Text songName;
    public TMP_Text difficultyName;

    public TMP_Text accuracyPercentage;
    public TMP_Text score;

    public ResultPackData resultData;
    public GameObject surveyPanel;
    public UploadDataPacker uploader;
    void Start()
    {
        GameConfigLoader.Instance.saveConfig();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void showResult(ResultPackData resultData)
    {
        perfectCount.text = resultData.perfectCount.ToString();
        greatCount.text = resultData.greatCount.ToString();
        goodCount.text = resultData.goodCount.ToString();
        missCount.text = resultData.missCount.ToString();
        goodHoldCount.text = resultData.goodHoldCount.ToString();
        badHoldCount.text = resultData.badHoldCount.ToString();
        earlyCount.text = resultData.earlyCount.ToString();
        lateCount.text = resultData.lateCount.ToString();

        maxCombo.text = resultData.maxCombo.ToString();
        averageTime.text = resultData.averageTime.ToString("0.000") + "s";
        stdDivTime.text = resultData.stdDevTime.ToString("0.000") + "s";
        maxEarlyTime.text = resultData.maxEarly.ToString("0.000") + "s";
        maxLateTime.text = resultData.maxLate.ToString("0.000") + "s";
        levelSeed.text = resultData.generated ? resultData.metadata.genParam.seed.ToString() : "-";
        songName.text = resultData.metadata.songName;
        accuracyPercentage.text = resultData.accuracyPercentage.ToString("0.00") + "%";
        score.text = resultData.score.ToString();
        numberOfNotes.text = resultData.levelNotes.Count.ToString();

        songName.text = resultData.metadata.songName;

        switch (resultData.metadata.difficulty)
        {
            case Difficulty.EASY:
                difficultyName.text = "easy";
                break;
            case Difficulty.MEDIUM:
                difficultyName.text = "medium";
                break;
            case Difficulty.HARD:
                difficultyName.text = "hard";
                break;
            case Difficulty.INSANE:
                difficultyName.text = "insane";
                break;
            case Difficulty.CUSTOM:
                difficultyName.text = "custom";
                break;
        }

        this.resultData = resultData;
        uploader.resultData = resultData;
    }

    public void showSurvey()
    {
        //if (resultData.generated && !resultData.autoplay)
        //{
        //    surveyPanel.SetActive(true);
        //    gameObject.SetActive(false);
        //}
        //else
        //{
            SceneManagement.Instance.transitionToMainMenu(true);
        //}
    }
}
