using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class APIResponse
{
    public bool success;
    public string error;
    public string machineID;
}

public class UploadDataPacker : MonoBehaviour
{
    public ResultPackData resultData;
    public SurveyData surveyData;

    private GameConfigLoader cfgLoader;
    private GamePreference gPre;

    void Start()
    {
        cfgLoader = GameConfigLoader.Instance;
        gPre = cfgLoader.GetGameConfig().gamePreference;

    }

    public void StartUpload()
    {
        if (resultData != null && surveyData != null)
        {
            UploadData uploadData = new UploadData(resultData, surveyData);
            string postData = JsonUtility.ToJson(uploadData);

            gPre.lastExpAnswer = surveyData.experience;
            cfgLoader.saveConfig();

            File.WriteAllText(@"UserData\latestSurvey.json", postData);
            SceneManagement.Instance.uploadSurveyData(postData);
        }
        
    }

}
