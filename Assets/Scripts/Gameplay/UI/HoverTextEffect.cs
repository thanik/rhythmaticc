using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine;

public class HoverTextEffect : MonoBehaviour
{
    public Color hoverText;
    public Color normalText;
    public float animatingTime;
    private TMP_Text text;
    void Start()
    {
        text = GetComponentInChildren<TMP_Text>();
    }

    public void mouseEnter()
    {
        text.DOColor(hoverText, animatingTime);
    }

    public void mouseExit()
    {
        text.DOColor(normalText, animatingTime);
    }

    private void OnDisable()
    {
        text.color = normalText;
    }
}
