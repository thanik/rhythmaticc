using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationNote : MonoBehaviour
{
	public Vector3 startPos;
	public Vector3 endPos;
	public float startTime = 0;
	public float endTime = 0;

	private CalibrationController gc;
	void Start()
	{
		gc = FindObjectOfType<CalibrationController>();
	}

	void Update()
	{
		startTime = endTime - gc.getTravellingTime();
		transform.localPosition = Vector3.LerpUnclamped(startPos, endPos, (gc.gameTime - startTime) / (endTime - startTime));

		//if (gc.gameTime > endTime + 1f)
		//{
		//	gameObject.SetActive(false);
		//}
	}
}
