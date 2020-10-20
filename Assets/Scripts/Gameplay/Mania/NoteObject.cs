using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
	public int noteId;
	public Vector3 startPos;
	public Vector3 endPos;
	public float startTime = 0;
	public float endTime = 0;
	public float releaseTime = 0;

	private float oldSpeed = 0;
	private ManiaGameController gc;
	private SpriteRenderer sr;
	private SkinManager sm;
	void Start()
	{
		gc = FindObjectOfType<ManiaGameController>();
		sr = GetComponent<SpriteRenderer>();
		sm = FindObjectOfType<SkinManager>();
	}

	void Update()
	{
		startTime = endTime - gc.getTravellingTime();
		if (releaseTime > 0)
		{
			float timeLength = releaseTime - endTime;
			float spriteSize = ((sm.GetSkinConfig().noteStartPosY - sm.GetSkinConfig().judgmentLineDefaultPosY) / gc.getTravellingTime()) * timeLength;
			Vector2 oldSize = sr.size;
			oldSize.y = spriteSize + (sr.sprite.border.x / 100f) + (sr.sprite.border.w / 100f) + (sm.GetSkinConfig().noteDefaultHeight / 2);
			sr.size = oldSize;
			if (oldSpeed != gc.speedMod)
			{
				startPos.y = sm.GetSkinConfig().noteStartPosY + (spriteSize / 2);
				endPos.y = sm.GetSkinConfig().judgmentLineDefaultPosY - (spriteSize / 2);
				oldSpeed = gc.speedMod;
			}
			transform.localPosition = Vector3.LerpUnclamped(startPos, endPos, (gc.gameTime - startTime) / (releaseTime - startTime));
		}
		else
		{
			transform.localPosition = Vector3.LerpUnclamped(startPos, endPos, (gc.gameTime - startTime) / (endTime - startTime));
		}

		if (gc.gameTime > (releaseTime == 0 ? endTime : releaseTime) + 0.2f)
        {
            gameObject.SetActive(false);
        }
    }
}
