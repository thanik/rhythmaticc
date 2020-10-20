using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkinManager : MonoBehaviour
{
    SkinConfig currentConfig;
    public Dictionary<string, SpriteAnimationClip> sprAnimClips;
    public Dictionary<string, Sprite> sprites;
    //public List<Sprite> noteSprites;

    public int lanes;
    public string skinName;

    public GameObject spriteRendererPrefab;
    public GameObject judgmentTextPrefab;
    public GameObject laneControllerPrefab;
    public GameObject trackIndicatorPrefab;

    public SpriteRenderer background;
    public List<SpriteRenderer> laneSpitters;
    public List<LaneSkinPreview> lanePreviews;
    public List<LaneController> laneControllers;
    public List<SpriteRenderer> gearOverlays;
    public SpriteRenderer judgmentLine;
    public SpriteRenderer judgmentText;
    public SpriteRenderer elIndicator;
    public SpriteRenderer healthbarFill;
    public SpriteRenderer trackInd;
    public Transform middleJudgmentZone;

    private ManiaGameController gm;
    public float laneSize;

    private string SKIN_PATH = "UserData/Skins/";
    void Awake()
    {
        sprAnimClips = new Dictionary<string, SpriteAnimationClip>();
        sprites = new Dictionary<string, Sprite>();
        gm = FindObjectOfType<ManiaGameController>();
    }
    void Start()
    {
        
        //loadSkin("default");
    }

    public SkinConfig GetSkinConfig()
    {
        return currentConfig;
    }

    public void loadSkin(string name, bool editor)
    {
        try
        {
            StreamReader configFile = new StreamReader(SKIN_PATH + name + "/config.json");
            currentConfig = JsonUtility.FromJson<SkinConfig>(configFile.ReadToEnd());
            if (currentConfig != null)
            {
                foreach (SpriteConfig sprConfig in currentConfig.sprites)
                {
                    Debug.Log("Loading sprite: " + SKIN_PATH + name + "/" + sprConfig.file);
                    sprites.Add(sprConfig.name, IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + sprConfig.file, new Vector4(sprConfig.borderLeft, sprConfig.borderBottom, sprConfig.borderRight, sprConfig.borderTop), 100f, SpriteMeshType.FullRect));
                }

                foreach (AnimationClipsConfig aniClipConfig in currentConfig.animationClips)
                {
                    Debug.Log("Loading sprite animation clip: " + aniClipConfig.name);
                    SpriteAnimationClip newClip = new SpriteAnimationClip(aniClipConfig.name, aniClipConfig.fps, aniClipConfig.loop, aniClipConfig.showLastFrame, aniClipConfig.loopLength);
                    newClip.LoadSprites(aniClipConfig.fileNumberStart, aniClipConfig.fileNumberEnd, SKIN_PATH + name + "/" + aniClipConfig.filePrefix, aniClipConfig.fileSurfix);
                    sprAnimClips.Add(aniClipConfig.name, newClip);
                }
            }
            configFile.Close();
            skinName = name;

            if (editor)
            {
                FindObjectOfType<SkinEditor>().updateTextFieldValue();
            }
            spawnSkin(editor);

        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void spawnSkin(bool editor)
    {
        if (!currentConfig.laneExpand)
        {
            laneSize = currentConfig.playAreaWidth / lanes;
        }
        // spawn bg
        background = Instantiate(spriteRendererPrefab, transform).GetComponent<SpriteRenderer>();
        background.name = "Background";
        background.sortingLayerName = "Background";
        background.sprite = sprites["background"];

        Vector2 newSize;
        // spawn gear overlay
        if (currentConfig.gearOverlayEnabled)
        {
            for (int i = 0; i < currentConfig.gearOverlays.Length; i++)
            {
                GearOverlayConfig overlayCfg = currentConfig.gearOverlays[i];
                SpriteRenderer gear = Instantiate(spriteRendererPrefab, transform).GetComponent<SpriteRenderer>();
                gear.sortingLayerName = "Gear";
                gear.sortingOrder = i;
                gear.sprite = sprites[overlayCfg.spriteName];
                gear.name = "GearOverlay " + i;
                if (overlayCfg.resizeToPlayArea)
                {
                    if (overlayCfg.resizeIncludeBorder)
                    {
                        gear.size = new Vector2(laneSize * lanes + (sprites[overlayCfg.spriteName].border.x / 100f) + (sprites[overlayCfg.spriteName].border.z / 100f), overlayCfg.height);
                    }
                    else
                    {
                        gear.size = new Vector2(laneSize * lanes, overlayCfg.height);

                    }
                }
                else
                {
                    newSize = new Vector2();

                    if (overlayCfg.width == 0)
                    {
                        newSize.x = sprites[overlayCfg.spriteName].bounds.size.x;
                    }
                    else
                    {
                        newSize.x = overlayCfg.width;
                    }

                    if (overlayCfg.height == 0)
                    {
                        newSize.y = sprites[overlayCfg.spriteName].bounds.size.y;
                    }
                    else
                    {
                        newSize.y = overlayCfg.height;
                    }

                    gear.size = newSize;
                }
                gear.transform.position = new Vector3(overlayCfg.posX, overlayCfg.posY, 0f);
            }
        }

        // spawn judgement line
        judgmentLine = Instantiate(spriteRendererPrefab, transform).GetComponent<SpriteRenderer>();
        judgmentLine.sortingLayerName = "Background";
        judgmentLine.sortingOrder = 2;
        judgmentLine.sprite = sprites["judgmentLine"];
        judgmentLine.name = "Judgement Line";

        // spawn judgement text
        judgmentText = Instantiate(judgmentTextPrefab, middleJudgmentZone).GetComponent<SpriteRenderer>();
        judgmentText.sortingLayerName = "JudgmentText";
        judgmentText.sortingOrder = 0;
        judgmentText.name = "Judgement Text";
        gm.judgmentText = judgmentText.GetComponent<SpriteAnimationPlayer>();

        elIndicator = Instantiate(spriteRendererPrefab, middleJudgmentZone).GetComponent<SpriteRenderer>();
        elIndicator.sortingLayerName = "JudgmentText";
        elIndicator.sortingOrder = 0;
        elIndicator.name = "EarlyLate Indicator";
        gm.earlyLateIndicator = elIndicator;

        // spawn healthbar fill
        healthbarFill = Instantiate(spriteRendererPrefab, transform).GetComponent<SpriteRenderer>();
        healthbarFill.sortingLayerName = "Gear";
        healthbarFill.sortingOrder = currentConfig.healthbarFill.layer;
        healthbarFill.sprite = sprites[currentConfig.healthbarFill.spriteName];
        healthbarFill.name = "Healthbar Fill";
        newSize = new Vector2();
        if (currentConfig.healthbarFill.width == 0)
        {
            newSize.x = sprites[currentConfig.healthbarFill.spriteName].bounds.size.x;
        }
        else
        {
            newSize.x = currentConfig.healthbarFill.width;
        }

        if (currentConfig.healthbarFill.height == 0)
        {
            newSize.y = sprites[currentConfig.healthbarFill.spriteName].bounds.size.y;
        }
        else
        {
            newSize.y = currentConfig.healthbarFill.height;
        }

        healthbarFill.size = newSize;
        healthbarFill.transform.position = new Vector3(currentConfig.healthbarFill.posX, currentConfig.healthbarFill.posY, 0f);

        // spawn track ind
        trackInd = Instantiate(trackIndicatorPrefab, transform).GetComponent<SpriteRenderer>();
        trackInd.sortingLayerName = "Gear";
        trackInd.sortingOrder = currentConfig.trackProgressIndicator.layer;
        trackInd.sprite = sprites[currentConfig.trackProgressIndicator.spriteName];
        trackInd.name = "Track Progress Indicator";
        newSize = new Vector2();

        if (currentConfig.trackProgressIndicator.width == 0)
        {
            newSize.x = sprites[currentConfig.trackProgressIndicator.spriteName].bounds.size.x;
        }
        else
        {
            newSize.x = currentConfig.trackProgressIndicator.width;
        }

        if (currentConfig.trackProgressIndicator.height == 0)
        {
            newSize.y = sprites[currentConfig.trackProgressIndicator.spriteName].bounds.size.y;
        }
        else
        {
            newSize.y = currentConfig.trackProgressIndicator.height;
        }

        trackInd.size = newSize;
        trackInd.transform.position = new Vector3(currentConfig.trackProgressIndicator.startPosX, currentConfig.trackProgressIndicator.startPosY, 0f);

        if (editor)
        {
            //updateSkinParamPreview();
        }
        else
        {
            updateSkinParam();
        }
    }

    //public void loadSkinold(string name, bool editor)
    //{
    //    try
    //    {
    //        StreamReader configFile = new StreamReader(SKIN_PATH + name + "/config.txt");
    //        string line;
    //        while ((line = configFile.ReadLine()) != null)
    //        {
    //            string[] splittedLine = line.Split(':');
    //            if (splittedLine.Length > 1 && splittedLine[1].Length > 0)
    //            {
    //                switch (splittedLine[0])
    //                {
    //                    case "laneExpandOrDivideMode":
    //                        if (int.Parse(splittedLine[1]) > 0)
    //                        {
    //                            laneExpandOrDivideMode = true;
    //                        }
    //                        else
    //                        {
    //                            laneExpandOrDivideMode = false;
    //                        }
    //                        break;
    //                    case "playAreaWidth":
    //                        playAreaWidth = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "laneSize":
    //                        laneSize = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "laneXOffset":
    //                        laneXOffset = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "judgmentLineDefaultPosY":
    //                        judgeLineY = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "judgmentTextDefaultPosY":
    //                        judgeTextY = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "healthBarPosX":
    //                        healthBarX = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "healthBarPosY":
    //                        healthBarY = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "noteDefaultWidth":
    //                        noteDefaultWidth = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "noteDefaultHeight":
    //                        noteDefaultHeight = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "noteBorderLeft":
    //                        noteBorder.x = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "noteBorderBottom":
    //                        noteBorder.y = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "noteBorderRight":
    //                        noteBorder.z = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "noteBorderTop":
    //                        noteBorder.w = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "noteStartPosY":
    //                        noteStartPosY = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "leftTextX":
    //                        leftTextX = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "rightTextX":
    //                        rightTextX = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "topInTextY":
    //                        topInTextY = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "scoreTextX":
    //                        scoreTextX = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "bgBorderLeft":
    //                        bgBorder.x = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "bgBorderBottom":
    //                        bgBorder.y = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "bgBorderRight":
    //                        bgBorder.z = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "bgBorderTop":
    //                        bgBorder.w = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "bgFile":
    //                        bgFileName = splittedLine[1];
    //                        break;
    //                    case "bgHeight":
    //                        bgHeight = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "noteFile":
    //                        noteFileNames.Add(splittedLine[1]);
    //                        break;
    //                    case "perfectFile":
    //                        perfectTextFileName = splittedLine[1];
    //                        break;
    //                    case "greatFile":
    //                        greatTextFileName = splittedLine[1];
    //                        break;
    //                    case "goodFile":
    //                        goodTextFileName = splittedLine[1];
    //                        break;
    //                    case "nearFile":
    //                        nearTextFileName = splittedLine[1];
    //                        break;
    //                    case "missFile":
    //                        missTextFileName = splittedLine[1];
    //                        break;
    //                    case "beamFile":
    //                        beamFileName = splittedLine[1];
    //                        break;
    //                    case "beamPosOffsetY":
    //                        beamPosOffsetY = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "judgmentLineFile":
    //                        judgmentLineFileName = splittedLine[1];
    //                        break;
    //                    case "laneSplitterFile":
    //                        laneSplitterFileName = splittedLine[1];
    //                        break;
    //                    case "laneSplitterPosY":
    //                        laneSplitterPosY = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "animationFramePerSecond":
    //                        animationFramePerSecond = int.Parse(splittedLine[1]);
    //                        break;
    //                    case "hitAnimationPrefix":
    //                        hitAnimationPrefix = splittedLine[1];
    //                        break;
    //                    case "hitAnimationStart":
    //                        hitAnimationStart = int.Parse(splittedLine[1]);
    //                        break;
    //                    case "hitAnimationEnd":
    //                        hitAnimationEnd = int.Parse(splittedLine[1]);
    //                        break;
    //                    case "hitAnimationSurfix":
    //                        hitAnimationSurfix = splittedLine[1];
    //                        break;
    //                    case "hitHoldAnimationPrefix":
    //                        hitHoldAnimationPrefix = splittedLine[1];
    //                        break;
    //                    case "hitHoldAnimationStart":
    //                        hitHoldAnimationStart = int.Parse(splittedLine[1]);
    //                        break;
    //                    case "hitHoldAnimationEnd":
    //                        hitHoldAnimationEnd = int.Parse(splittedLine[1]);
    //                        break;
    //                    case "hitHoldAnimationSurfix":
    //                        hitHoldAnimationSurfix = splittedLine[1];
    //                        break;
    //                    case "gearFile":
    //                        gearFileName = splittedLine[1];
    //                        break;
    //                    case "gearDefaultPosY":
    //                        gearY = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "gearOverlayEnabled":
    //                        if (int.Parse(splittedLine[1]) > 0)
    //                        {
    //                            gearOverlayEnabled = true;
    //                        }
    //                        else
    //                        {
    //                            gearOverlayEnabled = false;
    //                        }
    //                        break;
    //                    case "gearOverlayFile":
    //                        gearOverlayFilenames.Add(splittedLine[1]);
    //                        break;
    //                    case "gearOverlayPosX":
    //                        gearOverlayPosX.Add(float.Parse(splittedLine[1]));
    //                        break;
    //                    case "gearOverlayPosY":
    //                        gearOverlayPosY.Add(float.Parse(splittedLine[1]));
    //                        break;
    //                    case "gearOverlayResizeToPlayArea":
    //                        if (int.Parse(splittedLine[1]) > 0)
    //                        {
    //                            gearOverlayResizeToPlayArea.Add(true);
    //                        }
    //                        else
    //                        {
    //                            gearOverlayResizeToPlayArea.Add(false);
    //                        }
    //                        break;
    //                    case "gearOverlayResizeIncludeBorder":
    //                        if (int.Parse(splittedLine[1]) > 0)
    //                        {
    //                            gearOverlayResizeIncludeBorder.Add(true);
    //                        }
    //                        else
    //                        {
    //                            gearOverlayResizeIncludeBorder.Add(false);
    //                        }
    //                        break;
    //                    case "gearOverlayBorderLeft":
    //                        gearOverlayBorderLeft.Add(float.Parse(splittedLine[1]));
    //                        break;
    //                    case "gearOverlayBorderBottom":
    //                        gearOverlayBorderBottom.Add(float.Parse(splittedLine[1]));
    //                        break;
    //                    case "gearOverlayBorderRight":
    //                        gearOverlayBorderRight.Add(float.Parse(splittedLine[1]));
    //                        break;
    //                    case "gearOverlayBorderTop":
    //                        gearOverlayBorderTop.Add(float.Parse(splittedLine[1]));
    //                        break;
    //                    case "gearOverlayHeight":
    //                        gearOverlayHeight.Add(float.Parse(splittedLine[1]));
    //                        break;
    //                    case "gearOverlayWidth":
    //                        gearOverlayWidth.Add(float.Parse(splittedLine[1]));
    //                        break;
    //                    case "buttonPressedFile":
    //                        buttonPressedFilename = splittedLine[1];
    //                        break;
    //                    case "buttonUnpressedFile":
    //                        buttonUnpressedFilename = splittedLine[1];
    //                        break;
    //                    case "buttonPosY":
    //                        buttonPosY = float.Parse(splittedLine[1]);
    //                        break;
    //                    case "judgeEarlyFilename":
    //                        judgeEarlyFilename = splittedLine[1];
    //                        break;
    //                    case "judgeLateFilename":
    //                        judgeLateFilename = splittedLine[1];
    //                        break;
    //                    case "earlyLateIndicatorOffsetY":
    //                        earlyLateIndicatorOffsetY = float.Parse(splittedLine[1]);
    //                        break;
    //                }
    //            }
    //        }

    //        Vector4 defaultVec4 = new Vector4();
    //        backgroundSprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + bgFileName, bgBorder, 100f, SpriteMeshType.FullRect);
    //        //gearSprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + gearFileName, defaultVec4, 100f, SpriteMeshType.FullRect);
    //        judgmentLineSprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + judgmentLineFileName, defaultVec4, 100f, SpriteMeshType.FullRect);
    //        keybeamSprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + beamFileName, defaultVec4, new Vector2(0.5f, 0f), 100f, SpriteMeshType.FullRect);
    //        foreach(string noteFileName in noteFileNames)
    //        {
    //            noteSprites.Add(IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + noteFileName, noteBorder, 100f, SpriteMeshType.FullRect));
    //        }
    //        laneSpitterSprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + laneSplitterFileName, defaultVec4, 100f, SpriteMeshType.FullRect);
    //        perfectSprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + perfectTextFileName, defaultVec4, 100f, SpriteMeshType.FullRect);
    //        greatSprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + greatTextFileName, defaultVec4, 100f, SpriteMeshType.FullRect);
    //        goodSprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + goodTextFileName, defaultVec4, 100f, SpriteMeshType.FullRect);
    //        nearSprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + nearTextFileName, defaultVec4, 100f, SpriteMeshType.FullRect);
    //        missSprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + missTextFileName, defaultVec4, 100f, SpriteMeshType.FullRect);
    //        buttonPressedSprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + buttonPressedFilename, defaultVec4, 100f, SpriteMeshType.FullRect);
    //        buttonUnpressedSprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + buttonUnpressedFilename, defaultVec4, 100f, SpriteMeshType.FullRect);
    //        earlySprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + judgeEarlyFilename, defaultVec4, 100f, SpriteMeshType.FullRect);
    //        lateSprite = IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + judgeLateFilename, defaultVec4, 100f, SpriteMeshType.FullRect);

    //        for (int i = hitAnimationStart; i <= hitAnimationEnd; i++)
    //        {
    //            hitSprites.Add(IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + hitAnimationPrefix + i + hitAnimationSurfix, defaultVec4, 100f, SpriteMeshType.FullRect));
    //        }

    //        for (int i = hitHoldAnimationStart; i <= hitHoldAnimationEnd; i++)
    //        {
    //            hitHoldSprites.Add(IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + hitHoldAnimationPrefix + i + hitHoldAnimationSurfix, defaultVec4, 100f, SpriteMeshType.FullRect));
    //        }

    //        if (gearOverlayEnabled)
    //        {
    //            for (int i=0; i < gearOverlayFilenames.Count; i++)
    //            {
    //                if (i < gearOverlayBorderBottom.Count && i < gearOverlayBorderLeft.Count && i < gearOverlayBorderRight.Count && i < gearOverlayBorderTop.Count)
    //                {
    //                    Vector4 border = new Vector4(gearOverlayBorderLeft[i], gearOverlayBorderBottom[i], gearOverlayBorderRight[i], gearOverlayBorderTop[i]);
    //                    gearOverlaySprites.Add(IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + gearOverlayFilenames[i], border, 100f, SpriteMeshType.FullRect));

    //                }
    //                else
    //                {
    //                    gearOverlaySprites.Add(IMG2Sprite.LoadNewSprite(SKIN_PATH + name + "/" + gearOverlayFilenames[i], defaultVec4, 100f, SpriteMeshType.FullRect));

    //                }
    //            }
    //        }

    //        configFile.Close();
    //        skinName = name;

    //        if (editor)
    //        {

    //            FindObjectOfType<SkinEditor>().updateTextFieldValue();
    //        }
    //        spawnSkin(editor);
    //    }
    //    catch(System.Exception ex)
    //    {
    //        Debug.LogException(ex);
    //    }

    //}

    public void saveSkin()
    {

    }

    //public void spawnSkinold(bool editor)
    //{
    //    if (!laneExpandOrDivideMode)
    //    {
    //        laneSize = playAreaWidth / lanes;
    //    }
    //    // spawn bg
    //    background = Instantiate(spriteRendererPrefab, transform).GetComponent<SpriteRenderer>();
    //    background.name = "Background";
    //    background.sortingLayerName = "Background";
    //    background.sprite = backgroundSprite;

    //    // spawn gear overlay
    //    if (gearOverlayEnabled)
    //    {
    //        for (int i = 0; i < gearOverlaySprites.Count; i++)
    //        {
    //            gear = Instantiate(spriteRendererPrefab, transform).GetComponent<SpriteRenderer>();
    //            gear.sortingLayerName = "Gear";
    //            gear.sortingOrder = i;
    //            gear.sprite = gearOverlaySprites[i];
    //            gear.name = "GearOverlay " + i;
    //            if (gearOverlayResizeToPlayArea[i])
    //            {
    //                if (gearOverlayResizeIncludeBorder[i])
    //                {
    //                    gear.size = new Vector2(laneSize * lanes + (gearOverlayBorderLeft[i] / 100f) + (gearOverlayBorderRight[i] / 100f), gearOverlayHeight[i]);
    //                }
    //                else
    //                {
    //                    gear.size = new Vector2(laneSize * lanes, gearOverlayHeight[i]);

    //                }
    //            }
    //            else
    //            {
    //                Vector2 newSize = new Vector2();

    //                if (gearOverlayWidth[i] == 0)
    //                {
    //                    newSize.x = gearOverlaySprites[i].bounds.size.x;
    //                }
    //                else
    //                {
    //                    newSize.x = gearOverlayWidth[i];
    //                }

    //                if (gearOverlayHeight[i] == 0)
    //                {
    //                    newSize.y = gearOverlaySprites[i].bounds.size.y;
    //                }
    //                else
    //                {
    //                    newSize.y = gearOverlayHeight[i];
    //                }

    //                gear.size = newSize;
    //            }
    //            gear.transform.position = new Vector3(gearOverlayPosX[i], gearOverlayPosY[i], 0f);
    //        }
    //    }

    //    // spawn judgement line
    //    judgmentLine = Instantiate(spriteRendererPrefab, transform).GetComponent<SpriteRenderer>();
    //    judgmentLine.sortingLayerName = "Background";
    //    judgmentLine.sortingOrder = 2;
    //    judgmentLine.sprite = judgmentLineSprite;
    //    judgmentLine.name = "Judgement Line";

    //    // spawn judgement text
    //    judgmentText = Instantiate(judgmentTextPrefab, transform).GetComponent<SpriteRenderer>();
    //    judgmentText.sortingLayerName = "JudgmentText";
    //    judgmentText.sortingOrder = 0;
    //    judgmentText.name = "Judgement Text";
    //    gm.judgmentText = judgmentText;

    //    elIndicator = Instantiate(spriteRendererPrefab, transform).GetComponent<SpriteRenderer>();
    //    elIndicator.sortingLayerName = "JudgmentText";
    //    elIndicator.sortingOrder = 0;
    //    elIndicator.name = "EarlyLate Indicator";
    //    gm.earlyLateIndicator = elIndicator;

    //    if (editor)
    //    {
    //        updateSkinParamPreview();
    //    }
    //    else
    //    {
    //        updateSkinParam();
    //    }
    //}

    //public void updateSkinParamPreview()
    //{
    //    background.size = new Vector2(laneSize * lanes + (bgBorder.x / 100f) + (bgBorder.z / 100f), bgHeight);
    //    judgmentLine.size = new Vector2(laneSize * lanes, judgmentLineSprite.bounds.size.y - 0.01f);
    //    judgmentLine.transform.position = new Vector3(laneXOffset, judgeLineY, 0f);
    //    judgmentText.transform.position = new Vector3(0f, judgeTextY, 0f);
    //    elIndicator.transform.position = new Vector3(0f, judgeTextY + earlyLateIndicatorOffsetY, 0f);


    //    foreach (LaneSkinPreview lc in lanePreviews)
    //    {
    //        Destroy(lc.gameObject);
    //    }
    //    lanePreviews.Clear();

    //    foreach (SpriteRenderer s in laneSpitters)
    //    {
    //        Destroy(s.gameObject);
    //    }
    //    laneSpitters.Clear();

    //    float currentPosX = 0f;
    //    if (lanes % 2 == 0)
    //    {
    //        currentPosX = -(laneSize / 2) - ((lanes / 2) - 1) * laneSize + laneXOffset;
    //    }
    //    else
    //    {
    //        currentPosX = -(lanes / 2) * laneSize + laneXOffset;
    //    }

    //    for (int i = 0; i < lanes; i++)
    //    {
    //        LaneSkinPreview lc = Instantiate(laneControllerPrefab, transform).GetComponent<LaneSkinPreview>();
    //        lc.name = "Lane Preview " + i;
    //        lc.transform.position = new Vector3(currentPosX, 0f, 0f);
    //        lc.keyBeam.sprite = keybeamSprite;
    //        lc.keyBeam.size = new Vector2(laneSize, keybeamSprite.bounds.size.y - 0.01f);
    //        lc.keyBeam.transform.localPosition = new Vector3(0f, judgeLineY, 0f);
    //        lc.note.sprite = noteSprites[0];
    //        lc.note.size = new Vector2(laneSize, noteDefaultHeight);
    //        lanePreviews.Add(lc);
    //        if (i < lanes - 1)
    //        { 
    //        SpriteRenderer splitter = Instantiate(spriteRendererPrefab, transform).GetComponent<SpriteRenderer>();
    //        splitter.name = "Splitter " + i;
    //        splitter.transform.position = new Vector3(currentPosX + (laneSize / 2), laneSplitterPosY, 0f);
    //        splitter.sprite = laneSpitterSprite;
    //        splitter.sortingLayerName = "Background";
    //        splitter.sortingOrder = 1;
    //        laneSpitters.Add(splitter);
    //        }
    //        currentPosX += laneSize;
    //    }
    //}

    public void updateSkinParam()
    {
        background.size = new Vector2(laneSize * lanes + (sprites["background"].border.x / 100f) + (sprites["background"].border.z / 100f), currentConfig.bgHeight);
        //gear.transform.position = new Vector3(0f, gearY, 0f);
        judgmentLine.size = new Vector2(laneSize * lanes, sprites["judgmentLine"].bounds.size.y - 0.01f);
        judgmentLine.transform.position = new Vector3(currentConfig.laneXOffset, currentConfig.judgmentLineDefaultPosY, 0f);
        judgmentText.transform.localPosition = new Vector3(0f, currentConfig.judgmentTextDefaultPosY, 0f);
        elIndicator.transform.localPosition = new Vector3(0f, currentConfig.judgmentTextDefaultPosY + currentConfig.earlyLateIndicatorYOffset, 0f);

        foreach (SpriteRenderer s in laneSpitters)
        {
            Destroy(s.gameObject);
        }
        laneSpitters.Clear();

        float currentPosX = 0f;
        if (lanes % 2 == 0)
        {
            currentPosX = -(laneSize / 2) - ((lanes / 2) - 1) * laneSize + currentConfig.laneXOffset;
        }
        else
        {
            currentPosX = -(lanes / 2) * laneSize + currentConfig.laneXOffset;
        }

        for (int i = 0; i < lanes; i++)
        {
            int noteSprite = 1;
            if (lanes == 4)
            {
                noteSprite = (i % 2 == 0 ? 1 : 2);
            }
            else if (lanes == 6 && (i == 1 || i == 4))
            {
                noteSprite = 2;
            }

            LaneController lc = Instantiate(laneControllerPrefab, gm.transform).GetComponent<LaneController>();
            lc.name = "Lane Controller " + i;
            lc.transform.position = new Vector3(currentPosX, 0f, 0f);
            lc.startPos = new Vector3(0f, currentConfig.noteStartPosY, 0f);
            lc.keyBeam.sprite = sprites["keybeam"];
            lc.keyBeam.size = new Vector2(laneSize, sprites["keybeam"].bounds.size.y - 0.01f);
            lc.keyBeam.transform.localPosition = new Vector3(0f, (sprites["keybeam"].bounds.size.y / 2) + currentConfig.judgmentLineDefaultPosY + currentConfig.beamPosYOffset, 0f);
            lc.noteSprite = sprites["note" + noteSprite];
            lc.hitAnim.transform.localPosition = new Vector3(0f, currentConfig.judgmentLineDefaultPosY, 0f);
            lc.button.sprite = sprites["btnUnpressed"];
            lc.button.transform.localPosition = new Vector3(0f, currentConfig.buttonPosY, 0f);
            lc.button.sortingOrder = currentConfig.gearOverlays.Length;
            float sizePercentage = (laneSize / sprites["btnUnpressed"].bounds.size.y);
            lc.button.size = new Vector2(sprites["btnUnpressed"].bounds.size.x * sizePercentage, sprites["btnUnpressed"].bounds.size.y * sizePercentage);
            lc.lane = i;
            laneControllers.Add(lc);
            if (i < lanes - 1)
            {
                SpriteRenderer splitter = Instantiate(spriteRendererPrefab, transform).GetComponent<SpriteRenderer>();
                splitter.name = "Splitter " + i;
                splitter.transform.position = new Vector3(currentPosX + (laneSize / 2), currentConfig.laneSplitterPosY, 0f);
                splitter.sprite = sprites["laneSpitter"];
                splitter.sortingLayerName = "Background";
                splitter.sortingOrder = 1;
                laneSpitters.Add(splitter);
            }
            currentPosX += laneSize;
        }

        gm.bgc.gridSprite = sprites["beatGrid"];
        gm.bgc.ResetObjects();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
