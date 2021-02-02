using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class TargetFacingBranch:RexAIBranch 
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
			return "Target Facing";
		}

		public override bool Determine(RexAIRoutine _routine = null)
		{
			RexActor targetActor = null;
			RexController targetController = null;
			if(findTargetData.otherTransform == null)
			{
				findTargetData.otherTransform = GameManager.Instance.players[0].transform;
			}

			if(findTargetData.otherTransform != null)
			{
				targetActor = findTargetData.otherTransform.GetComponent<RexActor>();
				if(targetActor != null)
				{
					targetController = targetActor.slots.controller;
					if(targetController == null)
					{
						targetController = targetActor.GetComponentInChildren<RexController>();
					}
				}
			}

			if(targetController == null)
			{
				return false;
			}

			bool isTargetFacingThis = (targetController.direction.horizontal == Direction.Horizontal.Left && transform.position.x <= targetController.transform.position.x) || (targetController.direction.horizontal == Direction.Horizontal.Right && transform.position.x >= targetController.transform.position.x);

			//Debug.Log("Is Player Facing Bazooka Rex: " + isTargetFacingThis);

			return isTargetFacingThis;
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
			return BranchType.TargetFacing;
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(TargetFacingBranch))]
	public class TargetFacingBranchEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}
