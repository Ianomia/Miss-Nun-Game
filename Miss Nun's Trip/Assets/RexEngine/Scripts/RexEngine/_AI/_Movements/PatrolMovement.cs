using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace RexEngine
{
	public class PatrolMovement:RexAIMovement 
	{
		[System.Serializable]
		public class StartingMovement
		{
			[Tooltip("Whether this actor initially moves left or right.")]
			public Direction.Horizontal horizontal;
			[Tooltip("Whether this actor initially moves up or down, assuming vertical movement is enabled.")]
			public Direction.Vertical vertical;
			[Tooltip("If True, this actor will attempt to face and move towards the player on initialization, regardless of their above Startming Movement settings.")]
			public bool willFacePlayerAtStart = true;
		}

		public StartingDirectionType startingDirectionType;

		[Tooltip("Who doesn't love medusa heads?")]
		public bool moveInWave;

		public float sineWaveAmount = 0.2f;

		protected bool hasStartedMoving = false;
		protected float fixedTime;

		public enum StartingDirectionType
		{
			AlreadyFacing,
			Explicit,
			Target
		}

		public StartingMovement startingMovement;

		public override void OnBegin()
		{
			ResetTimer();

			if(aiRoutine.slots.controller)
			{
				Direction currentDirection = aiRoutine.slots.controller.direction;
				if(startingDirectionType == StartingDirectionType.AlreadyFacing)
				{
					hasStartedMoving = true;
					aiRoutine.slots.controller.SetAxis(new Vector2((int)currentDirection.horizontal, (int)currentDirection.vertical));
				}
				else if(startingDirectionType == StartingDirectionType.Explicit)
				{
					hasStartedMoving = true;
					aiRoutine.slots.controller.SetAxis(new Vector2((int)startingMovement.horizontal, (int)startingMovement.vertical));
				}
				else if(startingDirectionType == StartingDirectionType.Target)
				{
					findTargetMethods.GetTarget(findTargetData);
				}
			}

			base.OnBegin();
		}

		protected override void OnModeSetToActive()
		{
			Direction currentDirection = aiRoutine.slots.controller.direction;
			aiRoutine.slots.controller.SetAxis(new Vector2((int)currentDirection.horizontal, (int)currentDirection.vertical));
		}

		public override void OnEnded()
		{
			if(aiRoutine.slots.controller)
			{
				aiRoutine.slots.controller.SetAxis(Vector2.zero);
			}
		}

		public override void UpdateAI()
		{
			if(!findTargetData.hasFoundTarget || mode == Mode.Wait)
			{
				return;
			}

			if(!hasStartedMoving)
			{
				hasStartedMoving = true;
				Direction.Horizontal newDirection = (findTargetData.otherTransform.position.x < aiRoutine.transform.position.x) ? Direction.Horizontal.Left : Direction.Horizontal.Right;
				aiRoutine.slots.controller.SetAxis(new Vector2((int)newDirection, (int)aiRoutine.slots.controller.direction.vertical));
			}

			if(moveInWave)
			{
				fixedTime += Time.fixedDeltaTime;
				aiRoutine.slots.controller.SetAxis(new Vector2(aiRoutine.slots.controller.axis.x, Mathf.Sin(fixedTime) * sineWaveAmount));
			}

			durationInThisDirection += Time.deltaTime; 

			CheckTurnsAndJumps();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			mode = (Mode)EditorGUILayout.EnumPopup("Mode", mode);

			//STARTING MOVEMENT
			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("STARTING MOVEMENT", EditorStyles.boldLabel);

			startingDirectionType = (StartingDirectionType)EditorGUILayout.EnumPopup("Starting Direction", startingDirectionType);

			if(startingDirectionType == StartingDirectionType.Explicit)
			{
				startingMovement.horizontal = (Direction.Horizontal)EditorGUILayout.EnumPopup("Horizontal", startingMovement.horizontal);

				if(!moveInWave)
				{
					startingMovement.vertical = (Direction.Vertical)EditorGUILayout.EnumPopup("Vertical", startingMovement.vertical);
				}
			}

			//If it's set to "Target," show options for targets
			if(startingDirectionType == StartingDirectionType.Target)
			{
				if(findTargetData == null)
				{
					findTargetData = new FindTargetData();
				}

				DrawFaceTargetGUI();
			}

			EditorGUILayout.EndVertical();

			//TURN
			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("TURN", EditorStyles.boldLabel);			
			turn.onWallContact = EditorGUILayout.Toggle("On Wall Contact", turn.onWallContact);
			turn.onCeilingFloorContact = EditorGUILayout.Toggle("On Ceiling/Floor Contact", turn.onCeilingFloorContact);
			turn.onLedgeContact = EditorGUILayout.Toggle("On Ledge Contact", turn.onLedgeContact);

			if(turn.onLedgeContact)
			{
				ShowSlopeWarning();
			}

			if(turn.atTimerIntervals)
			{
				EditorGUILayout.BeginVertical(styles.actionStyle);
			}

			turn.atTimerIntervals = EditorGUILayout.Toggle("At Timer Intervals", turn.atTimerIntervals);
			if(turn.atTimerIntervals)
			{
				turn.useRandomInterval = EditorGUILayout.Toggle("Use Random Interval", turn.useRandomInterval);
				if(turn.useRandomInterval)
				{
					turn.intervalLow = EditorGUILayout.FloatField("Low", turn.intervalLow);
					turn.intervalHigh = EditorGUILayout.FloatField("High", turn.intervalHigh);
				}
				else
				{
					turn.interval = EditorGUILayout.FloatField("Interval", turn.interval);
				}
			}

			if(turn.atTimerIntervals)
			{
				EditorGUILayout.EndVertical();
			}

			EditorGUILayout.EndVertical();

			//WAVE
			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("WAVE", EditorStyles.boldLabel);		

			moveInWave = EditorGUILayout.Toggle("Move in Wave", moveInWave);
			if(moveInWave)
			{
				sineWaveAmount = EditorGUILayout.FloatField("Amount", sineWaveAmount);
				EditorGUILayout.LabelField("'Can Move Vertically' on the MovingState component of the corresponding RexController must be enabled for wave movement to work.", styles.helpTextStyle);		
			}

			EditorGUILayout.EndVertical();

			//JUMP
			DisplayJumpOptions();

			if(jump.onLedgeContact || turn.onLedgeContact)
			{
				DrawSlopesGUI();
			}
		}
		#endif
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(PatrolMovement))]
	public class PatrolMovementEditor:Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			(target as PatrolMovement).DrawInspectorGUI();

			if(!Application.isPlaying && EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}
	}
	#endif
}
