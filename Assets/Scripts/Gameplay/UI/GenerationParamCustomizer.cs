using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.Utilities;

public class GenerationParamCustomizer : MonoBehaviour
{
    public Slider sldOnsetThreshold;
    public TMP_InputField txtOnsetThreshold;
    public Slider sldBeatSnapping;
    public TMP_InputField txtBeatSnapping;
    public Slider sldBeatSnappingErrorThreshold;
    public TMP_InputField txtBeatSnappingErrorThreshold;
    public Slider sldMultipleLaneChance;
    public TMP_InputField txtMultipleLaneChance;
    public Slider sldOnBeatMultipleLaneChance;
    public TMP_InputField txtOnBeatMultipleLaneChance;
    public Slider sldMulLanesTimeThreshold;
    public TMP_InputField txtMulLanesTimeThreshold;
    public Slider[] sld4KLanesChance;
    public TMP_InputField[] txt4KLanesChance;
    public Slider[] sld6KLanesChance;
    public TMP_InputField[] txt6KLanesChance;

    private GamePreference gp;
    private GenerationParam genParam;
    private bool isUpdating = false;
    private void OnEnable()
    {
        GameConfig currentCfg = GameConfigLoader.Instance.GetGameConfig();
        gp = currentCfg.gamePreference;
        genParam = currentCfg.genParam;
        updateUIValues();
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void changeDifficultyToCustom()
    {
        gp.difficulty = 4;
        FindObjectOfType<GameMenu>().updateUIValues();
    }

    public void ValidateValues()
    {
        if (isUpdating) return;
        for (int i = 0; i < genParam.fourLanesMultipleLanesChance.Length; i++)
        {
            genParam.fourLanesMultipleLanesChance[i] = Mathf.RoundToInt(sld4KLanesChance[i].value);
        }

        for (int i = 0; i < genParam.sixLanesMultipleLanesChance.Length; i++)
        {
            genParam.sixLanesMultipleLanesChance[i] = Mathf.RoundToInt(sld6KLanesChance[i].value);
        }
        changeDifficultyToCustom();
        updateUIValues();
    }

    void updateUIValues()
    {
        isUpdating = true;
        sldOnsetThreshold.value = genParam.onsetThreshold;
        txtOnsetThreshold.text = genParam.onsetThreshold.ToString("0.00");

        switch (genParam.beatSnappingDivider)
        {
            case 0:
                sldBeatSnapping.value = 0;
                txtBeatSnapping.text = "Off";
                break;
            case 1:
                sldBeatSnapping.value = 1;
                txtBeatSnapping.text = "1/1";
                break;
            case 2:
                sldBeatSnapping.value = 2;
                txtBeatSnapping.text = "1/2";
                break;
            case 4:
                sldBeatSnapping.value = 3;
                txtBeatSnapping.text = "1/4";
                break;
            case 8:
                sldBeatSnapping.value = 4;
                txtBeatSnapping.text = "1/8";
                break;
            case 16:
                sldBeatSnapping.value = 5;
                txtBeatSnapping.text = "1/16";
                break;
            case 32:
                sldBeatSnapping.value = 6;
                txtBeatSnapping.text = "1/32";
                break;
            default:
                break;
        }

        sldBeatSnappingErrorThreshold.value = genParam.beatSnappingErrorThreshold;
        txtBeatSnappingErrorThreshold.text = genParam.beatSnappingErrorThreshold.ToString("0.000");

        sldMultipleLaneChance.value = genParam.multipleLaneChance;
        txtMultipleLaneChance.text = genParam.multipleLaneChance.ToString();
        sldOnBeatMultipleLaneChance.value = genParam.onBeatMultipleLaneChance;
        txtOnBeatMultipleLaneChance.text = genParam.onBeatMultipleLaneChance.ToString();
        switch (genParam.repeatedLaneTimeThreshold)
        {
            case 0.5f:
                sldMulLanesTimeThreshold.value = 0;
                txtMulLanesTimeThreshold.text = "2/1";
                break;
            case 1:
                sldMulLanesTimeThreshold.value = 1;
                txtMulLanesTimeThreshold.text = "1/1";
                break;
            case 2:
                sldMulLanesTimeThreshold.value = 2;
                txtMulLanesTimeThreshold.text = "1/2";
                break;
            case 4:
                sldMulLanesTimeThreshold.value = 3;
                txtMulLanesTimeThreshold.text = "1/4";
                break;
            case 8:
                sldMulLanesTimeThreshold.value = 4;
                txtMulLanesTimeThreshold.text = "1/8";
                break;
            case 16:
                sldMulLanesTimeThreshold.value = 5;
                txtMulLanesTimeThreshold.text = "1/16";
                break;
            default:
                break;
        }

        for (int i=0; i < sld4KLanesChance.Length; i++)
        {
            sld4KLanesChance[i].value = genParam.fourLanesMultipleLanesChance[i];
        }

        for (int i = 0; i < sld6KLanesChance.Length; i++)
        {
            sld6KLanesChance[i].value = genParam.sixLanesMultipleLanesChance[i];
        }

        for (int i = 0; i < txt4KLanesChance.Length; i++)
        {
            txt4KLanesChance[i].text = genParam.fourLanesMultipleLanesChance[i].ToString();
        }

        for (int i = 0; i < txt6KLanesChance.Length; i++)
        {
            txt6KLanesChance[i].text = genParam.sixLanesMultipleLanesChance[i].ToString();
        }

        isUpdating = false;
    }

    public void SetOnsetThreshold(float value)
    {
        txtOnsetThreshold.text = value.ToString("0.00");
        genParam.onsetThreshold = value;
        if(!isUpdating) changeDifficultyToCustom();
    }

    public void SetOnsetThreshold(string value)
    {
        float floatValue = float.Parse(value);
        if (floatValue > sldOnsetThreshold.maxValue)
        {
            genParam.onsetThreshold = sldOnsetThreshold.maxValue;
        }
        else if (floatValue < sldOnsetThreshold.minValue)
        {
            genParam.onsetThreshold = sldOnsetThreshold.minValue;
        }
        else
        {
            genParam.onsetThreshold = floatValue;
        }
        sldOnsetThreshold.value = genParam.onsetThreshold;
        if (!isUpdating) changeDifficultyToCustom();
    }

    public void SetBeatSnappingErrorThreshold(float value)
    {
        txtBeatSnappingErrorThreshold.text = value.ToString("0.000");
        genParam.beatSnappingErrorThreshold = value;
        if (!isUpdating) changeDifficultyToCustom();
    }

    public void SetBeatSnappingErrorThreshold(string value)
    {
        float floatValue = float.Parse(value);
        if (floatValue > sldBeatSnappingErrorThreshold.maxValue)
        {
            genParam.beatSnappingErrorThreshold = sldBeatSnappingErrorThreshold.maxValue;
        }
        else if (floatValue < sldBeatSnappingErrorThreshold.minValue)
        {
            genParam.beatSnappingErrorThreshold = sldBeatSnappingErrorThreshold.minValue;
        }
        else
        {
            genParam.beatSnappingErrorThreshold = floatValue;
        }
        sldBeatSnappingErrorThreshold.value = genParam.beatSnappingErrorThreshold;
        if (!isUpdating) changeDifficultyToCustom();
    }

    public void SetBeatSnapping(float value)
    {
        switch (Mathf.RoundToInt(value))
        {
            case 0:
                genParam.beatSnappingDivider = 0;
                txtBeatSnapping.text = "Off";
                break;
            case 1:
                genParam.beatSnappingDivider = 1;
                txtBeatSnapping.text = "1/1";
                break;
            case 2:
                genParam.beatSnappingDivider = 2;
                txtBeatSnapping.text = "1/2";
                break;
            case 3:
                genParam.beatSnappingDivider = 4;
                txtBeatSnapping.text = "1/4";
                break;
            case 4:
                genParam.beatSnappingDivider = 8;
                txtBeatSnapping.text = "1/8";
                break;
            case 5:
                genParam.beatSnappingDivider = 16;
                txtBeatSnapping.text = "1/16";
                break;
            case 6:
                genParam.beatSnappingDivider = 32;
                txtBeatSnapping.text = "1/32";
                break;
            default:
                break;
        }
        if (!isUpdating) changeDifficultyToCustom();
    }

    public void SetMultipleLaneChance(float value)
    {
        int intVal = Mathf.RoundToInt(value);
        txtMultipleLaneChance.text = intVal.ToString("0");
        genParam.multipleLaneChance = intVal;
        if (!isUpdating) changeDifficultyToCustom();
    }

    public void SetMultipleLaneChance(string value)
    {
        int intVal = int.Parse(value);
        if (intVal > sldMultipleLaneChance.maxValue)
        {
            genParam.multipleLaneChance = Mathf.RoundToInt(sldMultipleLaneChance.maxValue);
        }
        else if (intVal < sldMultipleLaneChance.minValue)
        {
            genParam.multipleLaneChance = Mathf.RoundToInt(sldMultipleLaneChance.minValue);
        }
        else
        {
            genParam.multipleLaneChance = intVal;
        }
        sldMultipleLaneChance.value = genParam.multipleLaneChance;
        if (!isUpdating) changeDifficultyToCustom();
    }

    public void SetOnBeatMultipleLaneChance(float value)
    {
        int intVal = Mathf.RoundToInt(value);
        txtOnBeatMultipleLaneChance.text = intVal.ToString("0");
        genParam.onBeatMultipleLaneChance = intVal;
        if (!isUpdating) changeDifficultyToCustom();
    }

    public void SetOnBeatMultipleLaneChance(string value)
    {
        int intVal = int.Parse(value);
        if (intVal > sldOnBeatMultipleLaneChance.maxValue)
        {
            genParam.onBeatMultipleLaneChance = Mathf.RoundToInt(sldOnBeatMultipleLaneChance.maxValue);
        }
        else if (intVal < sldOnBeatMultipleLaneChance.minValue)
        {
            genParam.onBeatMultipleLaneChance = Mathf.RoundToInt(sldOnBeatMultipleLaneChance.minValue);
        }
        else
        {
            genParam.onBeatMultipleLaneChance = intVal;
        }
        sldOnBeatMultipleLaneChance.value = genParam.onBeatMultipleLaneChance;
        if (!isUpdating) changeDifficultyToCustom();
    }

    public void SetMulLanesTimeThreshold(float value)
    {
        switch (value)
        {
            case 0f:
                genParam.repeatedLaneTimeThreshold = 0.5f;
                txtMulLanesTimeThreshold.text = "2/1";
                break;
            case 1:
                genParam.repeatedLaneTimeThreshold = 1;
                txtMulLanesTimeThreshold.text = "1/1";
                break;
            case 2:
                genParam.repeatedLaneTimeThreshold = 2;
                txtMulLanesTimeThreshold.text = "1/2";
                break;
            case 3:
                genParam.repeatedLaneTimeThreshold = 4;
                txtMulLanesTimeThreshold.text = "1/4";
                break;
            case 4:
                genParam.repeatedLaneTimeThreshold = 8;
                txtMulLanesTimeThreshold.text = "1/8";
                break;
            case 5:
                genParam.repeatedLaneTimeThreshold = 16;
                txtMulLanesTimeThreshold.text = "1/16";
                break;
            default:
                break;
        }
        if (!isUpdating) changeDifficultyToCustom();
    }
}
