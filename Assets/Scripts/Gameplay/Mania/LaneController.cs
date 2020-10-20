using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class LaneController : MonoBehaviour
{
    public List<NoteObject> laneNotes = new List<NoteObject>();
	public List<NotePoint> timePoints = new List<NotePoint>(); 
    private float pressedTime;
	private float releasedTime;
    public int noteIndex;
    public int noteSpawningIndex;
	public bool isPressedInWindow = false;
	public bool isCurrentNoteJudged = false;
	public GameObject notePrefab;
	public ManiaGameController gc;
	public SkinManager sm;
    public int lane;
	public Transform notesTransform;
	public Vector3 startPos;
	public Sprite noteSprite;
	public SpriteRenderer keyBeam;
	public SpriteRenderer button;
	public SpriteAnimationPlayer hitAnim;

	private AudioSource hitSound;

    void Start()
	{
		gc = FindObjectOfType<ManiaGameController>();
		sm = FindObjectOfType<SkinManager>();
		hitSound = GetComponent<AudioSource>();
		noteIndex = 0;
		noteSpawningIndex = 0;
		gc.laneCtrls.Add(lane, this);
	}

	public void Restart()
    {
		noteIndex = 0;
		noteSpawningIndex = 0;
		isCurrentNoteJudged = false;
		hitAnim.endAnimation();
		foreach(NoteObject noteObj in laneNotes)
        {
			noteObj.GetComponent<SpriteRenderer>().enabled = true;
			noteObj.gameObject.SetActive(false);
        }
	}

	public void Press()
    {
		isPressedInWindow = judgeNote();
		if (isPressedInWindow)
        {
			if (laneNotes[noteIndex].releaseTime > 0)
            {
				isCurrentNoteJudged = true;
				hitAnim.playAnimation(sm.sprAnimClips["hold"]);
            }
			else
            {
				hitAnim.playAnimation(sm.sprAnimClips["hit"]);
				laneNotes[noteIndex].GetComponent<SpriteRenderer>().enabled = false;
				passToNextNote();
            }
			

		}
		else
        {
			//Debug.Log("PRESS MISS");
		}
		
        releasedTime = 0f;
		keyBeam.enabled = true;
		button.sprite = sm.sprites["btnPressed"];
	}

	public void Release()
    {
		releasedTime = gc.gameTime;
		if (laneNotes[noteIndex].releaseTime > 0 && isPressedInWindow)
        {
			if (releasedTime + gc.calibrationOffset < laneNotes[noteIndex].releaseTime - 0.2f)
			{
				gc.RecordJudge(Judgement.BAD_RELEASED, false, laneNotes[noteIndex].releaseTime, releasedTime + gc.calibrationOffset, laneNotes[noteIndex].noteId);
				//Debug.Log("HOLD MISS");
			}
			else
            {
				gc.RecordJudge(Judgement.GOOD_RELEASED, false, laneNotes[noteIndex].releaseTime, releasedTime + gc.calibrationOffset, laneNotes[noteIndex].noteId);
				//laneNotes[noteIndex].GetComponent<SpriteRenderer>().enabled = false;

			}
			hitAnim.endAnimation();
			passToNextNote();
		}
		isPressedInWindow = false;
		keyBeam.enabled = false;
		button.sprite = sm.sprites["btnUnpressed"];
	}

	void Update()
	{

		if (gc.isPlaying && laneNotes.Count > 0)
		{
			if (gc.autoplay)
			{
				if (gc.gameTime > laneNotes[noteIndex].endTime && !isCurrentNoteJudged)
				{
					if (laneNotes[noteIndex].releaseTime > 0)
					{
						isCurrentNoteJudged = true;
						hitAnim.playAnimation(sm.sprAnimClips["hold"]);
					}
					else
					{
						
						hitAnim.playAnimation(sm.sprAnimClips["hit"]);
						laneNotes[noteIndex].GetComponent<SpriteRenderer>().enabled = false;
						passToNextNote();
					}
					gc.RecordJudge(Judgement.PERFECT, false, laneNotes[noteIndex].endTime, laneNotes[noteIndex].endTime, laneNotes[noteIndex].noteId);
					hitSound.Play();
					
				}

				if (laneNotes[noteIndex].releaseTime > 0 && gc.gameTime > laneNotes[noteIndex].releaseTime)
				{
					gc.RecordJudge(Judgement.GOOD_RELEASED, false, laneNotes[noteIndex].releaseTime, releasedTime + gc.calibrationOffset, laneNotes[noteIndex].noteId);
					hitAnim.endAnimation();
					passToNextNote();
				}
			}
			else
			{
				/* if the player doesn't press in time */
				if (gc.gameTime > laneNotes[noteIndex].endTime + gc.missWindow + gc.calibrationOffset && !isCurrentNoteJudged)
				{

					gc.RecordJudge(Judgement.UNPRESSED, true, laneNotes[noteIndex].endTime, 0f, laneNotes[noteIndex].noteId);
					passToNextNote();
					//Debug.Log("NORMAL MISS");
				}

				if (laneNotes[noteIndex].releaseTime > 0 && gc.gameTime > laneNotes[noteIndex].releaseTime + gc.missWindow + gc.calibrationOffset && isCurrentNoteJudged && noteIndex < laneNotes.Count - 1)
				{
					gc.RecordJudge(Judgement.BAD_RELEASED, false, laneNotes[noteIndex].releaseTime, -1, laneNotes[noteIndex].noteId);
					hitAnim.endAnimation();
					passToNextNote();
				}
			}

			if (noteSpawningIndex < timePoints.Count && gc.gameTime + 2f > timePoints[noteSpawningIndex].time)
			{
				NoteObject no = laneNotes[noteSpawningIndex];
				//no.startTime = timePoints[noteSpawningIndex].time - 1f;
				no.gameObject.SetActive(true);
				noteSpawningIndex++;
			}
		}
    }

	private void passToNextNote()
    {
		isPressedInWindow = false;
		if (noteIndex < laneNotes.Count - 1)
		{
			noteIndex++; /* judge next note */
			isCurrentNoteJudged = false;
		}
		else
        {
			isCurrentNoteJudged = true;
        }
	}

    public void spawnNotes()
    {
        for (int i = 0; i < timePoints.Count; i++)
        {
            GameObject newObj = Instantiate(notePrefab, notesTransform);
            SpriteRenderer sr = newObj.GetComponent<SpriteRenderer>();
            sr.sprite = noteSprite;
            sr.size = new Vector2(sm.laneSize, sm.GetSkinConfig().noteDefaultHeight);
            newObj.transform.position = new Vector3(0f, sm.GetSkinConfig().noteStartPosY, 0f);
            NoteObject no = newObj.GetComponent<NoteObject>();
            no.startPos = new Vector3(0f, sm.GetSkinConfig().noteStartPosY, 0f);
            no.endPos = new Vector3(0f, sm.GetSkinConfig().judgmentLineDefaultPosY, 0f);
            no.endTime = timePoints[i].time;
            no.releaseTime = timePoints[i].releaseTime;
            no.noteId = timePoints[i].noteId;
            laneNotes.Add(no);
            newObj.SetActive(false);
        }
    }

    public bool isFloatPositive(float value)
	{
		return value >= 0;
	}

	/* this function will return true if the player can press the note in time even if it's judged as near */
	public bool judgeNote()
	{
		pressedTime = gc.gameTime;

		float noteOffset = pressedTime - laneNotes[noteIndex].endTime + gc.calibrationOffset;
		bool isLate = isFloatPositive(noteOffset);
		float absNoteOffset = Mathf.Abs(noteOffset);

		if (absNoteOffset > gc.goodWindow && absNoteOffset <= gc.missWindow)
		{
			gc.RecordJudge(Judgement.MISS, isLate, laneNotes[noteIndex].endTime, pressedTime + gc.calibrationOffset, laneNotes[noteIndex].noteId);
			return true;
		}
		else if (absNoteOffset > gc.greatWindow && absNoteOffset <= gc.goodWindow)
		{
			gc.RecordJudge(Judgement.GOOD, isLate, laneNotes[noteIndex].endTime, pressedTime + gc.calibrationOffset, laneNotes[noteIndex].noteId);
			hitSound.Play();
			return true;
		}
		else if (absNoteOffset > gc.perfectWindow && absNoteOffset <= gc.greatWindow)
		{
			gc.RecordJudge(Judgement.GREAT, isLate, laneNotes[noteIndex].endTime, pressedTime + gc.calibrationOffset, laneNotes[noteIndex].noteId);
			hitSound.Play();
			return true;
		}
		else if (absNoteOffset <= gc.perfectWindow)
		{
			gc.RecordJudge(Judgement.PERFECT, isLate, laneNotes[noteIndex].endTime, pressedTime + gc.calibrationOffset, laneNotes[noteIndex].noteId);
			hitSound.Play();
			return true;
		}
		else
		{
			return false;
		}
	}
}
