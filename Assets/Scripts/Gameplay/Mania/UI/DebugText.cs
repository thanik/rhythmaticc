using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class DebugText : MonoBehaviour
{
    private TMP_Text text;
    private ManiaGameController gameCtrl;
    private BeatOnsetManager bm;
    private StringBuilder sb;

    private AudioSource song;
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        gameCtrl = FindObjectOfType<ManiaGameController>();
        bm = FindObjectOfType<BeatOnsetManager>();
        sb = new StringBuilder();
        song = gameCtrl.song;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 60 == 0)
        {
            UpdateText();
        }

    }

    void UpdateText()
    {
        sb.Clear();
        sb.Append("fps: ");
        sb.Append((1 / Time.deltaTime).ToString("0.000"));
        sb.Append("\ngameTime: ");
        sb.Append(gameCtrl.gameTime.ToString("0.000"));
        sb.Append("\naudioTime: ");
        sb.Append((song.timeSamples / (float)song.clip.frequency).ToString("0.000"));
        sb.Append("\nanalysisTime: ");
        sb.Append(bm.processTime.ToString("0.000"));
        sb.Append("\navgBPM: ");
        sb.Append(bm.avgBPM.ToString("0.000"));
        sb.Append("\nhealth: ");
        sb.Append(gameCtrl.Health.ToString());
        text.text = sb.ToString();
    }
}
