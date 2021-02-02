using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class FaceTargetAction:RexAIAction 
	{
		public FindTargetData findTargetData;

		protected FindTargetMethods findTargetMethods;

		[HideInInspector]
		public string targetName = "";

		void Awake()
		{
			findTargetMethods = gameObject.AddComponent<FindTargetMethods>();
		}

		void Start()
		{
			if(findTargetData.targetType != FindTargetData.TargetType.Transform)
			{
				findTargetData.otherTransform = null;
			}
		}

		public override void Begin()
		{
			if(findTargetData.otherTransform == null)
			{
				findTargetMethods.GetTarget(findTargetData);
			}

			if(findTargetData.otherTransform == null)
			{
				End();
				return;
			}

			Direction.Horizontal direction = Direction.Horizontal.Right;
			if(findTargetData.otherTransform.position.x <= transform.position.x)
			{
				direction = Direction.Horizontal.Left;
			}

			aiRoutine.slots.controller.FaceDirection(direction);

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			if(findTargetData == null)
			{
				findTargetData = new FindTargetData();
			}

			findTargetData.DrawInspectorGUI();
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.FaceTarget;
		}

		public override string GetName()
		{
			return "Face Target Action";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(FaceTargetAction))]
	public class FaceTargetActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

