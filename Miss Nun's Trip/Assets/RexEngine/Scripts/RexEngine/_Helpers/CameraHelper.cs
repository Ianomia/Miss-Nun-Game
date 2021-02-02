/* Copyright Sky Tyrannosaur */

using UnityEngine;
using System.Collections;

public class CameraHelper:MonoBehaviour 
{
	protected Vector3 screenSizeInUnits;
	protected float leftEdgeOfCamera;
	protected float rightEdgeOfCamera;
	protected float bottomEdgeOfCamera;
	protected float topEdgeOfCamera;

	protected static Camera mainCamera;

	private static CameraHelper instance = null;
	public static CameraHelper Instance 
	{ 
		get 
		{
			if(instance == null)
			{
				GameObject go = new GameObject();
				instance = go.AddComponent<CameraHelper>();
				go.name = "CameraHelper";
			}

			return instance; 
		} 
	}

	void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
	}

	void Update()
	{
		SetValues();
	}

	public bool CameraContainsPoint(Vector3 point, float buffer = 0.0f)
	{
		Rect rect = new Rect();
		rect.xMin = leftEdgeOfCamera - buffer;
		rect.xMax = rightEdgeOfCamera + buffer;
		rect.yMin = bottomEdgeOfCamera - buffer;
		rect.yMax = topEdgeOfCamera + buffer;
	
		if(rect.Contains(point))
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	
	public Vector3 GetScreenSizeInUnits(Camera _camera = null)
	{
		return screenSizeInUnits;
	}

	public float GetLeftEdgeOfCamera()
	{
		return leftEdgeOfCamera;
	}
	
	public float GetRightEdgeOfCamera()
	{
		return rightEdgeOfCamera;
	}

	public float GetTopEdgeOfCamera()
	{
		return topEdgeOfCamera;
	}

	public float GetBottomEdgeOfCamera()
	{
		return bottomEdgeOfCamera;
	}

	public void SetValues(Camera _camera = null)
	{
		if(mainCamera == null)
		{
			mainCamera = Camera.main;
		}

		Vector3 screenSize = new Vector3(Screen.width, Screen.height, 0.0f);

		if(!mainCamera.orthographic)
		{
			screenSizeInUnits = new Vector3(mainCamera.WorldToViewportPoint(screenSize * 0.5f).x, mainCamera.WorldToViewportPoint(screenSize * 0.5f).y, mainCamera.WorldToViewportPoint(screenSize * 0.5f).z);
		}
		else
		{
			float height = mainCamera.orthographicSize * 2;
			float width = height * screenSize.x / screenSize.y;
			screenSizeInUnits = new Vector3(width, height, 0.0f);
		}

		leftEdgeOfCamera = mainCamera.transform.position.x - GetScreenSizeInUnits().x * 0.5f;
		rightEdgeOfCamera = mainCamera.transform.position.x + GetScreenSizeInUnits().x * 0.5f;
		topEdgeOfCamera = mainCamera.transform.position.y + GetScreenSizeInUnits().y * 0.5f;
		bottomEdgeOfCamera = mainCamera.transform.position.y - GetScreenSizeInUnits().y * 0.5f;
	}
}
