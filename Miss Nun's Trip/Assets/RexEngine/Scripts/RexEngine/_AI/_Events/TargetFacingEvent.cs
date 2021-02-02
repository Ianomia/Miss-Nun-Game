using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class TargetFacingEvent:RexAIEvent 
	{
		public RexController targetController;
		public TargetType targetType;
		public RequireFacing requireFacing;

		public enum TargetType
		{
			Player,
			Controller
		}

		public enum RequireFacing
		{
			TowardsThis,
			AwayFromThis
		}

		void Awake() 
		{

		}

		void Start() 
		{
			if(targetType == TargetType.Player)
			{
				targetController = GameManager.Instance.players[0].slots.controller;
			}
		}

		public override void Enable()
		{
			base.Enable();

			if(targetType == TargetType.Player)
			{
				targetController = GameManager.Instance.players[0].slots.controller;
			}
		}

		public override void Disable()
		{
			base.Disable();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			targetType = (TargetType)EditorGUILayout.EnumPopup("Target Type", targetType);
			if(targetType == TargetType.Controller)
			{
				targetController = (RexController)EditorGUILayout.ObjectField("Target Controller", targetController, typeof(RexController), true);
			}

			requireFacing = (RequireFacing)EditorGUILayout.EnumPopup("Require Facing", requireFacing);
		}
		#endif

		public override string GetName()
		{
			return "Target Facing";
		}

		public override void CheckEventStatus() 
		{
			if(targetController == null || RexSceneManager.Instance.isLoadingNewScene)
			{
				return;
			}

			bool isTargetFacingThis = (targetController.direction.horizontal == Direction.Horizontal.Left && transform.position.x <= targetController.transform.position.x) || (targetController.direction.horizontal == Direction.Horizontal.Right && transform.position.x >= targetController.transform.position.x);
			if((isTargetFacingThis && requireFacing == RequireFacing.TowardsThis) || (!isTargetFacingThis && requireFacing == RequireFacing.AwayFromThis))
			{
				OnEventActivated();
			}
		}

		public override EventType GetEventType()
		{
			return EventType.TargetFacing;
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(TargetFacingEvent))]
	public class TargetFacingEventEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}
