using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SurveyPagesController : MonoBehaviour
{
    public SurveyPage[] pages;
    public Button btnNext;
    public Button btnPrevious;
    public UploadDataPacker uploader;
    public SurveyData surveyData = new SurveyData();

    private int currentPageIndex = 0;

    public void updateAnswer(int questionNumber, int answer)
    {
        switch(questionNumber)
        {
            case 1:
                surveyData.experience = answer;
                break;
            case 3:
                surveyData.funLevel = answer;
                break;
            case 4:
                surveyData.musicMatchLevel = answer;
                break;
            case 5:
                surveyData.humanMadeLevel = answer;
                break;
        }
    }

    public void updateAnswer(int questionNumber, string answer)
    {
        switch(questionNumber)
        {
            case 2:
                surveyData.songGenres = answer;
                break;
            case 6:
                surveyData.levelImprovement = answer;
                break;
            case 7:
                surveyData.gameFeedback = answer;
                break;
        }
    }

    public void enableNextButton()
    {
        btnNext.interactable = true;
    }

    public void disableNextButton()
    {
        btnNext.interactable = false;
    }

    public void nextPage()
    {
        if (currentPageIndex == pages.Length - 1)
        {
            // send survey
            uploader.surveyData = surveyData;
            uploader.StartUpload();
        }
        else
        {
            foreach (SurveyPage page in pages)
            {
                page.gameObject.SetActive(false);
            }
            currentPageIndex++;
            pages[currentPageIndex].gameObject.SetActive(true);
        }
        updateButtonLabel();
    }

    public void previousPage()
    {
        if (currentPageIndex == 0)
        {
            SceneManagement.Instance.transitionToMainMenu(true);
        }
        else
        {
            foreach (SurveyPage page in pages)
            {
                page.gameObject.SetActive(false);
            }
            currentPageIndex--;
            pages[currentPageIndex].gameObject.SetActive(true);
        }
        updateButtonLabel();
        enableNextButton();
    }

    public void updateButtonLabel()
    {
        if (currentPageIndex == 0)
        {
            btnPrevious.GetComponentInChildren<TMP_Text>().text = "Skip";
        }
        else if (currentPageIndex == pages.Length - 1)
        {
            btnNext.GetComponentInChildren<TMP_Text>().text = "Send survey";
        }
        else if(currentPageIndex > 0)
        {
            btnPrevious.GetComponentInChildren<TMP_Text>().text = "Previous";
            btnNext.GetComponentInChildren<TMP_Text>().text = "Next";
        }
    }
}
