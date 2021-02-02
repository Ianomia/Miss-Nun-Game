using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class FacingTargetBranch:RexAIBranch 
	{
		public FindTargetData findTargetData;

		protected FindTargetMethods findTargetMethods;

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

			findTargetMethods.GetTarget(findTargetData);
		}

		public override string ID()
		{
			return "Facing Target";
		}

		public override bool Determine(RexAIRoutine _routine = null)
		{
			if(findTargetData.otherTransform == null)
			{
				findTargetData.otherTransform = GameManager.Instance.players[0].transform;
			}

			if(findTargetData.otherTransform == null)
			{
				return false;
			}

			bool isFacingTarget = false;

			isFacingTarget = (_routine.slots.controller.direction.horizontal == Direction.Horizontal.Left && findTargetData.otherTransform.position.x <= transform.position.x) || (_routine.slots.controller.direction.horizontal == Direction.Horizontal.Right && findTargetData.otherTransform.position.x >= transform.position.x);

			return isFacingTarget;
		}

		public override void DrawInspectorGUI()
		{
			#if UNITY_EDITOR
			if(findTargetData == null)
			{
				findTargetData = new FindTargetData();
			}

			findTargetData.DrawInspectorGUI();
			#endif
		}

		public override BranchType GetBranchType()
		{
			return BranchType.FacingTarget;
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(FacingTargetBranch))]
	public class FacingTargetBranchEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}
