using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GearOverlayConfig
{
    public string spriteName;
    public float posX;
    public float posY;
    public bool resizeToPlayArea;
    public bool resizeIncludeBorder;
    public float height;
    public float width;
}

[System.Serializable]
public class AnimationClipsConfig
{
    public string name;
    public float fps;
    public bool loop;
    public float loopLength;
    public bool showLastFrame;
    public string filePrefix;
    public int fileNumberStart;
    public int fileNumberEnd;
    public string fileSurfix;
}

[System.Serializable]
public class SpriteConfig
{
    public string name;
    public string file;
    public float borderLeft;
    public float borderBottom;
    public float borderRight;
    public float borderTop;
}

[System.Serializable]
public class HealthBarFillConfig
{
    public string spriteName;
    public float posX;
    public float posY;
    public float height;
    public float width;
    public int layer;
}

[System.Serializable]
public class TrackProgressIndConfig
{
    public string spriteName;
    public float startPosX;
    public float startPosY;
    public float endPosX;
    public float endPosY;
    public float height;
    public float width;
    public int layer;
}

[System.Serializable]
public class SkinConfig
{
    public int version;
    public bool laneExpand;
    public float playAreaWidth;
    public float laneSize;
    public float laneXOffset;
    public float judgmentLineDefaultPosY;
    public float judgmentTextDefaultPosY;
    public float noteDefaultHeight;
    public float noteStartPosY;
    public float bgHeight;

    public float beamPosYOffset;
    public float buttonPosY;

    public float earlyLateIndicatorYOffset;
    public float laneSplitterPosY;

    public bool gearOverlayEnabled;

    public float scoreTextPosX;
    public float scoreTextPosY;

    public float speedModTextPosX;
    public float speedModTextPosY;

    public float miniJudgePanelPosX;
    public float miniJudgePanelPosY;

    public GearOverlayConfig[] gearOverlays;
    public AnimationClipsConfig[] animationClips;
    public SpriteConfig[] sprites;
    public HealthBarFillConfig healthbarFill;
    public TrackProgressIndConfig trackProgressIndicator;

}
