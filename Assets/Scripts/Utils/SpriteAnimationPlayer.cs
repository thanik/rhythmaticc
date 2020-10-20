using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimationPlayer : MonoBehaviour
{
    public SpriteAnimationClip currentClip;
    private bool isLoop;
    private bool isPlaying = false;
    public int frame;
    
    public float deltaTime;
    public float nextFrameTime;
    public float currentTime;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentClip.sprites != null && isPlaying)
        {
            deltaTime += Time.deltaTime;
            currentTime += Time.deltaTime;
            if (deltaTime > nextFrameTime)
            {
                if (frame < currentClip.sprites.Count - 1)
                {
                    frame++;
                    sr.sprite = currentClip.sprites[frame];
                }
                else if (isLoop)
                {
                    frame = 0;
                    sr.sprite = currentClip.sprites[frame];
                    if (currentClip.length > 0 && currentTime > currentClip.length)
                    {
                        endAnimation();
                    }
                }
                else
                {
                    isPlaying = false;
                    if(!currentClip.showLastFrame)
                        sr.sprite = null;
                }

                deltaTime = 0f;
            }
        }

    }

    public void playAnimation(SpriteAnimationClip clip)
    {
        currentClip = clip;
        isLoop = clip.isLoop;
        frame = 0;
        deltaTime = 0;
        currentTime = 0;
        sr.sprite = currentClip.sprites[frame];
        nextFrameTime = (float)1 / clip.fps;
        isPlaying = true;
    }
        public void endAnimation()
    {
        isLoop = false;
    }

    public void stopAnimation()
    {
        frame = 0;
        isPlaying = false;
        sr.sprite = currentClip.sprites[frame];
    }
}
