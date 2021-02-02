/* Copyright Sky Tyrannosaur */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace RexEngine
{
	public class CircularMovingPlatform:MovingPlatform
	{
		public CircularMovementDirection circularMovementDirection;
		public float duration = 5.0f;
		public float radius = 2.5f;
		public float startingAngle;

		[HideInInspector]
		public float currentAngle = 0;

		public enum CircularMovementDirection
		{
			Clockwise,
			CounterClockwise
		}

		protected Vector2 startingPosition;

		void Awake() 
		{
			startingPosition = transform.position;
		}

		void Start()
		{
			float radiansIntoDegrees = 360.0f / RadiansPerRotation();
			currentAngle = startingAngle / radiansIntoDegrees;

			SetToStartingPosition();

			if(!willStartWhenPlayerIsOnTop && isMovementEnabled)
			{
				StartMoving();
			}
		}

		void FixedUpdate()
		{
			if(isMoving && isMovementEnabled)
			{
				MoveInCircle();
			}

			if(willStartWhenPlayerIsOnTop && hasPlayerOnTop && !isMoving && isMovementEnabled)
			{
				StartMoving();
			}
		}

		public override void MovePhysics(RexPhysics _physicsObject)
		{
			if(_physicsObject.IsOnSurface())
			{
				_physicsObject.ApplyDirectTranslation(physicsObject.GetPositionChangeFromLastFrame());
			}
		}

		public override Vector2 GetVelocity()
		{
			float x = 0.0f;
			float y = 0.0f;

			if(physicsObject == null)
			{
				return Vector2.zero;
			}

			Vector2 positionChange = physicsObject.GetPositionChangeFromLastFrame();
			if(positionChange.x < 0.0f)
			{
				x = -1.0f;
			}
			else if(positionChange.x > 0.0f)
			{
				x = 1.0f;
			}

			if(positionChange.y < 0.0f)
			{
				y = -1.0f;
			}
			else if(positionChange.y > 0.0f)
			{
				y = 1.0f;
			}

			return new Vector2(x, y);
		}

		protected override void StartMoving()
		{
			isMoving = true;
		}

		protected void SetToStartingPosition()
		{
			physicsObject.previousFrameProperties.position = new Vector2(startingPosition.x + Mathf.Cos(currentAngle) * radius, startingPosition.y + Mathf.Sin(currentAngle) * radius);
			physicsObject.properties.position = new Vector2(startingPosition.x + Mathf.Cos(currentAngle) * radius, startingPosition.y + Mathf.Sin(currentAngle) * radius);
			transform.position = new Vector2(startingPosition.x + Mathf.Cos(currentAngle) * radius, startingPosition.y + Mathf.Sin(currentAngle) * radius);
		}

		protected void MoveInCircle()
		{
			float speed = RadiansPerRotation() / duration;
			if(circularMovementDirection == CircularMovementDirection.Clockwise)
			{
				speed *= -1;
			}

			currentAngle += speed * Time.fixedDeltaTime;

			SetToPosition(new Vector2(startingPosition.x + Mathf.Cos(currentAngle) * radius, startingPosition.y + Mathf.Sin(currentAngle) * radius));
		}

		public float RadiansPerRotation()
		{
			return 2 * Mathf.PI;
		}

		protected void SetToPosition(Vector2 _position)
		{
			Vector2 difference = _position - physicsObject.properties.position;
			physicsObject.ApplyDirectTranslation(difference);
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(CircularMovingPlatform))]
	public class CircularMovingPlatformEditor:Editor 
	{
		protected InspectorHelper.AIInspectorStyles styles;

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			CircularMovingPlatform _target = target as CircularMovingPlatform;
			_target.isMovementEnabled = EditorGUILayout.Toggle("Is Movement Enabled", _target.isMovementEnabled);
			_target.willStartWhenPlayerIsOnTop = EditorGUILayout.Toggle("Only Start When Player is On Top", _target.willStartWhenPlayerIsOnTop);

			_target.circularMovementDirection = (CircularMovingPlatform.CircularMovementDirection)EditorGUILayout.EnumPopup("Movement Direction", _target.circularMovementDirection);

			_target.duration = EditorGUILayout.FloatField("Full Rotation Duration", _target.duration);
			_target.radius = EditorGUILayout.FloatField("Radius", _target.radius);
			_target.startingAngle = EditorGUILayout.FloatField("Starting Angle", _target.startingAngle);

			while(_target.startingAngle > 360.0f)
			{
				_target.startingAngle -= 360.0f;
			}

			while(_target.startingAngle < 0.0f)
			{
				_target.startingAngle += 360.0f;
			}

			if(_target.physicsObject == null)
			{
				_target.physicsObject = _target.GetComponent<RexPhysics>();
			}

			if(!Application.isPlaying && EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}

		void OnSceneGUI()		
		{
			if(Application.isPlaying)
			{
				return;
			}

			CircularMovingPlatform _target = target as CircularMovingPlatform;

			Handles.color = new Color(0.75f, 0.75f, 1.0f, 0.75f);
			Handles.DrawWireDisc(_target.transform.position, new Vector3(0, 0, 1), _target.radius);

			float radiansIntoDegrees = 360.0f / _target.RadiansPerRotation();
			float angle = _target.startingAngle / radiansIntoDegrees;

			Handles.color = new Color(1.0f, 1.0f, 0.5f, 1.0f);
			Vector3 handlePosition = new Vector3(_target.transform.position.x + Mathf.Cos(angle) * _target.radius, _target.transform.position.y + Mathf.Sin(angle) * _target.radius);
			Handles.DotHandleCap(0, handlePosition, _target.transform.rotation * Quaternion.LookRotation(Vector3.forward), 0.2f, EventType.Repaint);
		}
	}
	#endif
}
