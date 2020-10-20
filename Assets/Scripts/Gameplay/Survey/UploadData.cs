using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class UploadData {
    public ResultPackData result;
    public SurveyData survey;

    public UploadData(ResultPackData result, SurveyData survey)
    {
        this.result = result;
        this.survey = survey;
    }
}
