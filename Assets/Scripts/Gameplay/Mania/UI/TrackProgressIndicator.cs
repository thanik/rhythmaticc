using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackProgressIndicator : MonoBehaviour
{
    private ManiaGameController gc;
    private SkinManager sm;
    TrackProgressIndConfig cfg;

    Vector3 startPos;
    Vector3 endPos;
    float songLength = 0;
    void Start()
    {
        gc = FindObjectOfType<ManiaGameController>();
        sm = FindObjectOfType<SkinManager>();
        cfg = sm.GetSkinConfig().trackProgressIndicator;
        startPos = new Vector3(cfg.startPosX, cfg.startPosY, 0f);
        endPos = new Vector3(cfg.endPosX, cfg.endPosY, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (gc.isAudioLoaded && songLength == 0)
        {
            songLength = gc.song.clip.length / 2;
        }

        if (gc.isPlaying)
        {
            transform.position = Vector3.Lerp(startPos, endPos, (gc.gameTime / songLength));
        }
    }
}
