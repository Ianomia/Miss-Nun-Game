using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class RexCameraBase:MonoBehaviour 
	{
		public bool usePlayerAsTarget = true;

		//[HideInInspector]
		public Vector2 boundariesMin; //These are the adjusted camera boundary positions, which are calculated using boundaryObjectPosition as well as camera size

		//[HideInInspector]
		public Vector2 boundariesMax; 

		//[HideInInspector]
		public Vector2 boundaryObjectPositionMin; //These are the raw positions of the actual SceneBoundary objects, with no adjustment for camera size

		//[HideInInspector]
		public Vector2 boundaryObjectPositionMax;

		[System.NonSerialized]
		public Vector2 shakeOffset; //The offset that ScreenShake is giving the camera; should not be set directly

		protected Transform focusObject; //The object the camera will focus on and follow

		void Start() 
		{
			ScreenShake.Instance.SetCamera(this);
		}

		public virtual void SetFocusObject(Transform _focusObject)
		{
			focusObject = _focusObject;
		}

		public virtual void NotifyOfShake()
		{
			
		}

		public virtual void CenterOnPlayer()
		{
			
		}

		public virtual void SetCameraBoundary(SceneBoundary.Edge _edge, float _value)
		{
			switch(_edge)
			{
				case SceneBoundary.Edge.Left:
					boundaryObjectPositionMin.x = _value;
					break;
				case SceneBoundary.Edge.Right:
					boundaryObjectPositionMax.x = _value;
					break;
				case SceneBoundary.Edge.Bottom:
					boundaryObjectPositionMin.y = _value;
					break;
				case SceneBoundary.Edge.Top:
					boundaryObjectPositionMax.y = _value;
					break;
			}
		}

		public void UpdateCameraBoundaries()
		{
			Vector2 halfScreenSize = CameraHelper.Instance.GetScreenSizeInUnits() * 0.5f;

			boundariesMin.x = boundaryObjectPositionMin.x + halfScreenSize.x;
			boundariesMax.x = boundaryObjectPositionMax.x - halfScreenSize.x;
			boundariesMin.y = boundaryObjectPositionMin.y + halfScreenSize.y;
			boundariesMax.y = boundaryObjectPositionMax.y - halfScreenSize.y;
		}

		/*public virtual void SetCameraBoundary(SceneBoundary.Edge _edge, float _value)
		{
			switch(_edge)
			{
				case SceneBoundary.Edge.Left:
					boundariesMin.x = _value + CameraHelper.Instance.GetScreenSizeInUnits().x * 0.5f;
					break;
				case SceneBoundary.Edge.Right:
					boundariesMax.x = _value - CameraHelper.Instance.GetScreenSizeInUnits().x * 0.5f;
					break;
				case SceneBoundary.Edge.Bottom:
					boundariesMin.y = _value + CameraHelper.Instance.GetScreenSizeInUnits().y * 0.5f;
					break;
				case SceneBoundary.Edge.Top:
					boundariesMax.y = _value - CameraHelper.Instance.GetScreenSizeInUnits().y * 0.5f;
					break;
			}
		}*/
	}
}
