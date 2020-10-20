using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatGridController : MonoBehaviour
{
    public GameObject prefab;
    public List<NoteObject> gridlines;
    public SkinManager sm;
    public ManiaGameController gc;
    public BeatOnsetManager bm;
    public Sprite gridSprite;
    public int gridlinesCount;
    public int beatIndex = 0;
    public int currentObjectIndex = 0;

    private void Awake()
    {
        gc = FindObjectOfType<ManiaGameController>();
        sm = FindObjectOfType<SkinManager>();
        bm = FindObjectOfType<BeatOnsetManager>();
        for (int i = 0; i < gridlinesCount; i++)
        {
            GameObject newGridline = Instantiate(prefab, transform);
            gridlines.Add(newGridline.GetComponent<NoteObject>());
        }
    }

    void Start()
    {
        
    }

    public void ResetObjects()
    {
        beatIndex = 0;
        foreach(NoteObject grid in gridlines)
        {
            grid.GetComponent<SpriteRenderer>().sprite = gridSprite;
            grid.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.025f);
            grid.gameObject.SetActive(false);
        }
    }

    public void spawnNotes(float endTime)
    {
        NoteObject no = gridlines[currentObjectIndex];
        SpriteRenderer sr = no.GetComponent<SpriteRenderer>();
        //sr.sprite = gridSprite;
        sr.size = new Vector2(sm.laneSize * sm.lanes, gridSprite.bounds.size.y);
        no.transform.position = new Vector3(0f, sm.GetSkinConfig().noteStartPosY, 0f);
        no.startPos = new Vector3(0f, sm.GetSkinConfig().noteStartPosY, 0f);
        no.endPos = new Vector3(0f, sm.GetSkinConfig().judgmentLineDefaultPosY, 0f);
        no.endTime = endTime;
        no.releaseTime = 0f;
        no.noteId = -1;
        no.gameObject.SetActive(true);
        if (currentObjectIndex < gridlinesCount - 1)
        {
            currentObjectIndex++;
        }
        else
        {
            currentObjectIndex = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (beatIndex < bm.beats.Count && gc.gameTime + gc.getTravellingTime() > bm.beats[beatIndex])
        {
            spawnNotes(bm.beats[beatIndex]);
            beatIndex++;
        }
    }
}
