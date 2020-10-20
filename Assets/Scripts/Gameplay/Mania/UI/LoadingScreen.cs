using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingScreen : MonoBehaviour
{
    public TMP_Text[] texts;
    public Image progressbar;
    public Image progressbarFill;

    private float maxTime;
    private float currentTime = 0;
    private bool loading = false;

    public void updateText(bool analyse)
    {
        if (analyse)
        {
            texts[0].text = "analysing...";
            texts[1].text = "The music analysis process may takes up to 2-3 minutes depends on your processing power and length of your music file.";
        }
        else
        {
            texts[0].text = "loading...";
            texts[1].text = "";
        }
    }

    public void StartFadeIn()
    {
        foreach(TMP_Text text in texts)
        {
            text.DOFade(1f, 1f);
        }
        progressbar.DOFade(1f, 1f);
        progressbarFill.DOFade(1f, 1f);
    }

    public void StartFadeOut()
    {
        foreach (TMP_Text text in texts)
        {
            text.DOFade(0f, 1f);
        }
        progressbar.DOFade(0f, 1f);
        progressbarFill.DOFade(0f, 1f);
    }

    public void StartProgressbar(float maxTime)
    {
        loading = true;
        currentTime = 0f;
        this.maxTime = maxTime;
    }

    public void FinishProgressbar()
    {
        loading = false;
        progressbarFill.fillAmount = 1f;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (loading && currentTime < maxTime)
        {
            currentTime += Time.deltaTime;
            progressbarFill.fillAmount = ((currentTime / maxTime) * 0.95f);
        }
    }
}
