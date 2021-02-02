using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace RexEngine
{
	public class RexAIMovement:MonoBehaviour 
	{
		public RexAIRoutine aiRoutine;
		public List<RexAIEvent> events;

		[Tooltip("Options for when this actor changes direction.")]
		public Turn turn;

		public Slopes slopes;

		[Tooltip("Options for when this actor jumps.")]
		public Jump jump;

		protected float durationInThisDirection = 0.0f;
		public FindTargetData findTargetData;

		public Mode mode;

		#if UNITY_EDITOR
		protected InspectorHelper.AIInspectorStyles styles;
		#endif

		protected FindTargetMethods findTargetMethods;

		public enum Mode
		{
			Active,
			Wait
		}

		[System.Serializable]
		public class Turn
		{
			[Tooltip("If True, this actor will turn around when it runs into a wall.")]
			public bool onWallContact = true;
			[Tooltip("If True, this actor will change its vertical movement direction when it contacts the ceiling or floor.")]
			public bool onCeilingFloorContact = false;
			[Tooltip("If True, this actor will turn around when it encounters a ledge at its feet.")]
			public bool onLedgeContact = true;

			public bool atTimerIntervals;
			public bool useRandomInterval;
			public float interval = 2.5f; 
			public float intervalLow = 2.5f;
			public float intervalHigh = 5.0f;
		}

		[System.Serializable]
		public class Slopes
		{
			public AllowSlopeType allowSlopesOfSize = AllowSlopeType.Manual;
			public float ledgeDetectionRaycastLength = 0.1f; 
		}

		[System.Serializable]
		public class Jump
		{
			[Tooltip("If True, this actor will jump when it runs into a wall.")]
			public bool onWallContact = true;
			[Tooltip("If True, this actor will jump around when it encounters a ledge at its feet.")]
			public bool onLedgeContact = true;
		}

		public enum AllowSlopeType
		{
			None,
			Moderate,
			Severe,
			Manual
		}

		void Awake() 
		{
			SetTurnRaycastLength();
			findTargetMethods = gameObject.AddComponent<FindTargetMethods>();
		}

		void Start() 
		{

		}

		public virtual void DrawInspectorGUI(){}
		public virtual void UpdateAI(){}

		public virtual void OnBegin()
		{
			for(int i = 0; i < events.Count; i ++)
			{
				events[i].Enable();
			}

			if(mode == Mode.Wait)
			{
				SetMode(Mode.Wait);
			}
		}

		public virtual void OnEnded()
		{
			for(int i = 0; i < events.Count; i ++)
			{
				events[i].Disable();
			}
		}

		public virtual void NotifyOfSequenceEnd(RexAISequence sequence){}

		public void SetMode(Mode _mode)
		{
			if(_mode == Mode.Wait)
			{
				mode = _mode;
				OnModeSetToWait();
			}
			else if(_mode == Mode.Active)
			{
				mode = _mode;
				OnModeSetToActive();
			}
		}

		public void ToggleMode()
		{
			if(mode == Mode.Active)
			{
				SetMode(Mode.Wait);
			}
			else if(mode == Mode.Wait)
			{
				SetMode(Mode.Active);
			}
		}

		protected virtual void OnModeSetToWait()
		{
			aiRoutine.slots.controller.SetAxis(Vector2.zero);
		}

		protected virtual void OnModeSetToActive(){}


		#if UNITY_EDITOR
		protected void DisplayJumpOptions()
		{
			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("JUMP", EditorStyles.boldLabel);
			jump.onWallContact = EditorGUILayout.Toggle("On Wall Contact", jump.onWallContact);

			if(jump.onLedgeContact)
			{
				EditorGUILayout.BeginVertical(styles.actionStyle);
			}

			jump.onLedgeContact = EditorGUILayout.Toggle("On Ledge Contact", jump.onLedgeContact);

			if(jump.onLedgeContact)
			{
				ShowSlopeWarning();

				EditorGUILayout.EndVertical();
			}

			EditorGUILayout.EndVertical();
		}
		#endif

		protected void CheckTurnsAndJumps()
		{
			bool willTurn = false;
			bool willJump = false;

			RexController controller = aiRoutine.slots.controller; 
			if(turn.atTimerIntervals && durationInThisDirection >= turn.interval)
			{
				if(controller)
				{
					ResetTimer();
					willTurn = true;
				}
			}
			else
			{
				if(turn.onWallContact)
				{
					if(CheckForWallContact())
					{
						willTurn = true;
					}
				}

				if(turn.onLedgeContact)
				{
					if(CheckForLedges())
					{
						willTurn = true;
					}
				}
			}

			if(jump.onWallContact)
			{
				if(CheckForWallContact())
				{
					willJump = true;
				}
			}

			if(controller)
			{
				if(turn.onCeilingFloorContact && CheckForFloorCeilingContact())
				{
					ResetTimer();
					controller.SetAxis(new Vector2(controller.axis.x, controller.axis.y * -1.0f));
				}

				if(willTurn)
				{
					ResetTimer();
					controller.Turn();
				}

				if(jump.onLedgeContact && !controller.slots.physicsObject.IsOnSurface())
				{
					JumpState jumpState = controller.GetComponent<JumpState>();
					if(jumpState)
					{
						if(jumpState.graceFrames < 1)
						{
							jumpState.graceFrames = 1;
						}

						willJump = true;
					}
				}

				if(willJump)
				{
					JumpState jumpState = controller.GetComponent<JumpState>();
					if(jumpState)
					{
						jumpState.Begin();
					}
				}
			}
		}

		protected bool CheckForLedges()
		{
			RexController controller = aiRoutine.slots.controller;
			if(controller)
			{
				float raycastLength = slopes.ledgeDetectionRaycastLength;
				//TODO: Gravity scale for the raycastHelper ledge detection? 
				if(!controller.slots.physicsObject.IsAgainstEitherWall() && controller.axis.x != 0.0f && controller.slots.physicsObject.IsOnSurface() && RaycastHelper.IsNextToLedge((Direction.Horizontal)controller.axis.x, (Direction.Vertical)(controller.GravityScaleMultiplier() * -1.0f), controller.slots.actor.GetComponent<BoxCollider2D>(), 1.0f, raycastLength)) //TODO: Slot this box collider in somewhere so you aren't using GetComponent every frame
				{
					return true;
				}
			}

			return false;
		}

		protected bool CheckForWallContact()
		{
			RexController controller = aiRoutine.slots.controller;
			if(controller)
			{
				if(controller.StateID() == LadderState.idString)
				{
					return false;
				}

				RexPhysics physicsObject = controller.slots.physicsObject;
				if((physicsObject.properties.isAgainstLeftWall || physicsObject.DidHitLeftWallThisFrame()) || (physicsObject.properties.isAgainstRightWall || physicsObject.DidHitRightWallThisFrame()))
				{
					return true;
				}

				return false;
			}

			return false;
		}

		protected bool CheckForFloorCeilingContact()
		{
			RexController controller = aiRoutine.slots.controller;
			if(controller)
			{
				if(controller.StateID() == LadderState.idString)
				{
					return false;
				}

				if(((controller.axis.y < 0.0f && controller.slots.physicsObject.properties.isGrounded) || (controller.axis.y > 0.0f && controller.slots.physicsObject.properties.isAgainstCeiling)))
				{
					return true;
				}

				return false;
			}

			return false;
		}

		protected void ResetTimer()
		{
			durationInThisDirection = 0.0f;
			if(turn.useRandomInterval)
			{
				turn.interval = RexMath.RandomFloat(turn.intervalLow, turn.intervalHigh);
			}
		}

		protected void SetTurnRaycastLength()
		{
			switch(slopes.allowSlopesOfSize)
			{
				case AllowSlopeType.Moderate:
					slopes.ledgeDetectionRaycastLength = 1.5f * GlobalValues.tileSize;
					break;
				case AllowSlopeType.Severe:
					slopes.ledgeDetectionRaycastLength = 2.0f * GlobalValues.tileSize; //TODO: Test
					break;
				case AllowSlopeType.None:
					slopes.ledgeDetectionRaycastLength = 0.0f;
					break;
			}
		}

		public void DrawFaceTargetGUI()
		{
			#if UNITY_EDITOR
			findTargetData.DrawInspectorGUI();
			#endif
		}

		public void DrawSlopesGUI()
		{
			#if UNITY_EDITOR
			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("SLOPES", EditorStyles.boldLabel);

			slopes.allowSlopesOfSize = (AllowSlopeType)EditorGUILayout.EnumPopup("Slope Threshold", slopes.allowSlopesOfSize);
			if(slopes.allowSlopesOfSize == AllowSlopeType.Manual)
			{
				slopes.ledgeDetectionRaycastLength = EditorGUILayout.FloatField("Raycast Length", slopes.ledgeDetectionRaycastLength);
			}

			EditorGUILayout.LabelField("\"Slope Threshold\" is the highest grade of slopes allowed before they register as ledges when descended.", styles.boxStyle);
			EditorGUILayout.EndVertical();
			#endif
		}

		public void ShowSlopeWarning()
		{
			#if UNITY_EDITOR
			EditorGUILayout.LabelField("To ensure this actor will properly detect ledges and slopes, double-check the settings under the SLOPES heading below.", styles.helpTextStyle);
			#endif
		}
	}
}
