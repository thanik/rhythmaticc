using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpriteAnimationClip
{
    public string name;
    public List<Sprite> sprites;
    public float fps;
    public bool isLoop;
    // clear sprite after animation ends
    public bool showLastFrame;
    // loop until secs
    public float length;

    public SpriteAnimationClip(string name, float fps, bool isLoop, bool showLastFrame, float length)
    {
        sprites = new List<Sprite>();
        this.name = name;
        this.fps = fps;
        this.isLoop = isLoop;
        this.showLastFrame = showLastFrame;
        this.length = length;
    }

    public void LoadSprites(int start, int end, string prefix, string surfix)
    {
        Vector4 defaultVec4 = new Vector4();
        for (int i = start; i <= end; i++)
        {
            Debug.Log("Loading sprite: " + prefix + i + surfix);
            sprites.Add(IMG2Sprite.LoadNewSprite(prefix + i + surfix, defaultVec4, 100f, SpriteMeshType.FullRect));
        }
    }
}
