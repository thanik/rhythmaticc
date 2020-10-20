using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkinEditor : MonoBehaviour
{
    public TMP_InputField txtLanes;
    public TMP_InputField txtLaneSize;
    public TMP_InputField txtLaneXOffset;
    public TMP_InputField txtJudgeLineY;
    public TMP_InputField txtJudgeFontY;
    public TMP_InputField txtGearY;
    public TMP_InputField txtHealthBarX;
    public TMP_InputField txtHealthBarY;
    public TMP_InputField txtLeftTextX;
    public TMP_InputField txtRightTextX;
    public TMP_InputField txtTopInTextY;
    public TMP_InputField txtScoreTextX;

    public SkinManager sm;
    void Start()
    {
        sm = FindObjectOfType<SkinManager>();
        sm.loadSkin("defaultv2", true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updateTextFieldValue()
    {
        txtLanes.text = sm.lanes.ToString("0.###");
        //txtLaneSize.text = sm.laneSize.ToString("0.###");
        //txtLaneXOffset.text = sm.laneXOffset.ToString("0.###");
        //txtJudgeLineY.text = sm.judgeLineY.ToString("0.###");
        //txtJudgeFontY.text = sm.judgeTextY.ToString("0.###");
        //txtGearY.text = sm.gearY.ToString("0.###");
        //txtHealthBarX.text = sm.healthBarX.ToString("0.###");
        //txtHealthBarY.text = sm.healthBarY.ToString("0.###");
        //txtLeftTextX.text = sm.leftTextX.ToString("0.###");
        //txtRightTextX.text = sm.rightTextX.ToString("0.###");
        //txtTopInTextY.text = sm.topInTextY.ToString("0.###");
        //txtScoreTextX.text = sm.scoreTextX.ToString("0.###");
    }

    public void updateSkinValue()
    {
        sm.lanes = int.Parse(txtLanes.text);
        //sm.laneSize = float.Parse(txtLaneSize.text);
        //sm.laneXOffset = float.Parse(txtLaneXOffset.text);
        //sm.judgeLineY = float.Parse(txtJudgeLineY.text);
        //sm.judgeTextY = float.Parse(txtJudgeFontY.text);
        //sm.gearY = float.Parse(txtGearY.text);
        //sm.healthBarX = float.Parse(txtHealthBarX.text);
        //sm.healthBarY = float.Parse(txtHealthBarY.text);
        //sm.leftTextX = float.Parse(txtLeftTextX.text);
        //sm.rightTextX = float.Parse(txtRightTextX.text);
        //sm.topInTextY = float.Parse(txtTopInTextY.text);
        //sm.scoreTextX = float.Parse(txtScoreTextX.text);
        //sm.updateSkinParamPreview();
    }
}
