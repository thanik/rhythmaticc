using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agreement : MonoBehaviour
{

    public void accept()
    {
        GameConfigLoader.Instance.currentConfig.agreementAccepted = true;
        GameConfigLoader.Instance.saveConfig();
        gameObject.SetActive(false);
    }

    public void decline()
    {
        Application.Quit();
    }
}
