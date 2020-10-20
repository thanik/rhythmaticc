using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public SkinManager sm;
    public Text topNumberText;
    public Animator topNumberAnimator;
    public Image topNumberDescription;

    public TMP_Text scoreText;
    public TMP_Text speedText;
    public TMP_Text speedTextTitle;

    public TMP_Text middleNumberText;

    public RectTransform miniJudgePanel;
    public TMP_Text perfectCount;
    public TMP_Text greatCount;
    public TMP_Text goodCount;
    public TMP_Text missCount;
    public TMP_Text holdGoodCount;
    public TMP_Text holdBadCount;

    public TMP_Text txtPauseCountdown;

    public GameObject pressToStartPanel;

    void Start()
    {
        sm = FindObjectOfType<SkinManager>();
    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    public void updateMiniJudgePanel(int perfect, int great, int good, int miss, int holdGood, int holdBad)
    {
        perfectCount.text = perfect.ToString();
        greatCount.text = great.ToString();
        goodCount.text = good.ToString();
        missCount.text = miss.ToString();
        holdGoodCount.text = holdGood.ToString();
        holdBadCount.text = holdBad.ToString();
    }

    public void setTextPositions()
    {
        scoreText.rectTransform.position = Camera.main.WorldToScreenPoint(new Vector3(sm.GetSkinConfig().scoreTextPosX, sm.GetSkinConfig().scoreTextPosY));
        speedText.rectTransform.position = Camera.main.WorldToScreenPoint(new Vector3(sm.GetSkinConfig().speedModTextPosX, sm.GetSkinConfig().speedModTextPosY));
        speedTextTitle.rectTransform.position = Camera.main.WorldToScreenPoint(new Vector3(sm.GetSkinConfig().speedModTextPosX, sm.GetSkinConfig().speedModTextPosY + 0.2f));
    }

    public void setMiniJudgePanelPosition()
    {
        miniJudgePanel.position = Camera.main.WorldToScreenPoint(new Vector3(sm.GetSkinConfig().miniJudgePanelPosX, sm.GetSkinConfig().miniJudgePanelPosY));
    }

    public void setTopTextMode(TextMode mode)
    {
        switch (mode)
        {
            case TextMode.OFF:
                topNumberText.gameObject.SetActive(false);
                topNumberDescription.gameObject.SetActive(false);
                break;
            case TextMode.ACCURACY_FROM_MAX:
            case TextMode.ACCURACY_FROM_ZERO:
                topNumberText.gameObject.SetActive(true);
                topNumberDescription.gameObject.SetActive(true);
                topNumberDescription.sprite = sm.sprites["accuracyText"];
                break;
            case TextMode.COMBO:
                topNumberText.gameObject.SetActive(true);
                topNumberDescription.gameObject.SetActive(true);
                topNumberDescription.sprite = sm.sprites["comboText"];
                break;
        }
        topNumberDescription.SetNativeSize();
    }

    public void updateSpeedModText(string number)
    {
        speedText.text = number;
    }

    public void updateScoreText(string number)
    {
        scoreText.text = number;
    }

    public void updateTopNumberText(string number)
    {
        topNumberText.text = number;
        topNumberAnimator.Play("FadeIn", -1, 0f);
    }

    public void setMiddleTextMode(TextMode mode)
    {
        middleNumberText.gameObject.SetActive(mode != TextMode.OFF);
    }

    public void updateMiddleNumberText(string number)
    {
        middleNumberText.text = number;
    }

    public void ResetUI()
    {
        topNumberText.text = "";
        scoreText.text = "0";
    }

    public void showCountdown()
    {
        txtPauseCountdown.gameObject.SetActive(true);
    }

    public void updateCountdown(float time)
    {
        txtPauseCountdown.text = Mathf.FloorToInt(time).ToString();

    }

    public void hideCountdown()
    {
        txtPauseCountdown.gameObject.SetActive(false);
    }
}
