using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RexEngine;

public class CameraAutoscroller:MonoBehaviour 
{
	public float scrollSpeed = 5.0f;

	protected RexCamera rexCamera;

	void Awake() 
	{
		
	}

	void Start() 
	{
		rexCamera = Camera.main.GetComponent<RexCamera>();
		rexCamera.SetFocusObject(null);
		rexCamera.scrolling.willScrollHorizontally = false;
	}

	void Update()
	{
		rexCamera.transform.position = new Vector3(rexCamera.transform.position.x + (scrollSpeed * Time.deltaTime), rexCamera.transform.position.y, rexCamera.transform.position.z);
		if(GameManager.Instance.player.transform.position.x < CameraHelper.Instance.GetLeftEdgeOfCamera())
		{
			GameManager.Instance.player.SetPosition(new Vector2(CameraHelper.Instance.GetLeftEdgeOfCamera(), GameManager.Instance.player.transform.position.y));
		}
	}
}
