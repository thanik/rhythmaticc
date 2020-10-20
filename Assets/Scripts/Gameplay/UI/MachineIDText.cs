using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MachineIDText : MonoBehaviour
{
    public string firstText;
    public string secondText;
    private TMP_Text text;
    void Start()
    {
        text = GetComponent<TMP_Text>();
        GameConfig currentCfg = GameConfigLoader.Instance.GetGameConfig();
        text.text = firstText;
        if (!string.IsNullOrEmpty(currentCfg.machineID))
        {
            text.text += "\n" + secondText + currentCfg.machineID;
        }
    }
}
