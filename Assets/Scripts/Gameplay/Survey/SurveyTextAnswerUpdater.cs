using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SurveyTextAnswerUpdater : MonoBehaviour
{
    public int questionNumber;
    private TMP_InputField text;
    private SurveyPagesController surveyCtrl;
    void Start()
    {
        surveyCtrl = GetComponentInParent<SurveyPagesController>();
        text = GetComponent<TMP_InputField>();
        text.onValueChanged.AddListener(updateAnswer);
    }

    void updateAnswer(string ans)
    {
        surveyCtrl.updateAnswer(questionNumber, ans);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
