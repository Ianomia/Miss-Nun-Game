using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace RexEngine
{
	public class FollowTargetMovement:RexAIMovement 
	{
		[Tooltip("When moving towards a Transform, this actor will attempt to maintain this amount of space between them.")]
		public float buffer = 0.5f;

		public UseDimensions useDimensions;

		void Awake()
		{
			findTargetMethods = gameObject.AddComponent<FindTargetMethods>();
		}

		void Start()
		{
			turn.onLedgeContact = false;
			turn.onWallContact = false;
			turn.onCeilingFloorContact = false;
		}

		public override void OnBegin()
		{
			ResetTimer();

			if(findTargetData.otherTransform == null)
			{
				findTargetMethods.GetTarget(findTargetData);
			}

			base.OnBegin();
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
			if(mode == Mode.Wait)
			{
				return;
			}

			if(findTargetData.otherTransform == null)
			{
				findTargetMethods.GetTarget(findTargetData);
			}

			durationInThisDirection += Time.deltaTime;

			MoveTowards();
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

			EditorGUILayout.LabelField("");

			buffer = EditorGUILayout.FloatField(new GUIContent("Maintain Distance", "Here is a tooltip"), buffer);
			useDimensions = (UseDimensions)EditorGUILayout.EnumPopup("Use Dimensions", useDimensions);

			EditorGUILayout.LabelField("");
			InspectorHelper.DrawLine();
			EditorGUILayout.LabelField("");

			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("TARGET", EditorStyles.boldLabel);

			DrawFaceTargetGUI();

			EditorGUILayout.EndVertical();

			DisplayJumpOptions();

			if(jump.onLedgeContact || turn.onLedgeContact)
			{
				DrawSlopesGUI();
			}
		}
		#endif

		protected void MoveTowards()
		{
			RexController controller = aiRoutine.slots.controller;
			if(controller == null)
			{
				return;
			}

			bool useX = (useDimensions == UseDimensions.XAndY || useDimensions == UseDimensions.X);
			bool useY = (useDimensions == UseDimensions.XAndY || useDimensions == UseDimensions.Y);

			if(useX && findTargetData.otherTransform != null)
			{
				if(findTargetData.otherTransform)
				{
					if(findTargetData.otherTransform.position.x > transform.position.x + buffer)
					{
						controller.SetAxis(new Vector2(1.0f, controller.axis.y));
					}
					else if(findTargetData.otherTransform.position.x < transform.position.x - buffer)
					{
						controller.SetAxis(new Vector2(-1.0f, controller.axis.y));
					}
					else
					{
						controller.SetAxis(new Vector2(0.0f, controller.axis.y));
					}

					if((findTargetData.otherTransform.position.x > transform.position.x && controller.slots.physicsObject.CheckForWallContact(Direction.Horizontal.Right)) || (findTargetData.otherTransform.position.x < transform.position.x && controller.slots.physicsObject.CheckForWallContact(Direction.Horizontal.Left)))
					{
						controller.SetAxis(new Vector2(0.0f, controller.axis.y));
					}
				}
			}

			if(useY && findTargetData.otherTransform != null)
			{
				LadderState ladderState = controller.GetComponent<LadderState>();
				if(controller.StateID() == LadderState.idString)
				{
					if(findTargetData.otherTransform.position.y > transform.position.y + buffer || (ladderState.GetDistanceFromTop() < 1.5f && controller.axis.y > 0.0f))
					{
						controller.SetAxis(new Vector2(controller.axis.x, 1.0f));
					}
					else if(findTargetData.otherTransform.position.y < transform.position.y - buffer || (ladderState.GetDistanceFromBottom() < 1.5f && controller.axis.y < 0.0f))
					{
						controller.SetAxis(new Vector2(controller.axis.x, -1.0f));
					}
					else
					{
						if(ladderState.GetDistanceFromTop() > 1.5f && ladderState.GetDistanceFromBottom() > 1.5f)
						{
							controller.SetAxis(new Vector2(controller.axis.x, 0.0f));
						}
					}
				}
				else
				{
					if(findTargetData.otherTransform.position.y > transform.position.y + buffer)
					{
						controller.SetAxis(new Vector2(controller.axis.x, 1.0f));
					}
					else if(findTargetData.otherTransform.position.y < transform.position.y - buffer)
					{
						controller.SetAxis(new Vector2(controller.axis.x, -1.0f));
					}
					else
					{
						controller.SetAxis(new Vector2(controller.axis.x, 0.0f));
					}
				}
			}
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(FollowTargetMovement))]
	public class MoveTowardsTargetMovementEditor:Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			(target as FollowTargetMovement).DrawInspectorGUI();

			if(!Application.isPlaying && EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}
	}
	#endif
}
