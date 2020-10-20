using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LikertScaleSelector : MonoBehaviour
{
    public int questionNumber;
    public Toggle[] toggles;
    public int value = -1;
    private SurveyPagesController surveyCtrl;

    private void OnEnable()
    {
        surveyCtrl = GetComponentInParent<SurveyPagesController>();
        updateValue(false);
        
    }

    void Start()
    {
        foreach(Toggle toggle in toggles)
        {
            toggle.onValueChanged.AddListener(updateValue);
        }

        int oldAns = GameConfigLoader.Instance.GetGameConfig().gamePreference.lastExpAnswer;
        if (questionNumber == 1 && oldAns > -1)
        {
            toggles[oldAns].isOn = true;
            updateValue(false);
        }

    }

    void updateValue(bool boolVal)
    {
        value = -1;
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn)
            {
                value = i;
                break;
            }
        }

        if (value == -1)
        {
            surveyCtrl.disableNextButton();
        }
        else
        {
            surveyCtrl.enableNextButton();
        }

        surveyCtrl.updateAnswer(questionNumber, value);
    }
}
