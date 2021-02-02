using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class FacingDirectionBranch:RexAIBranch  
	{
		public Direction.Horizontal direction;

		public override string ID()
		{
			return "Facing Direction";
		}

		public override bool Determine(RexAIRoutine _routine = null)
		{
			Direction.Horizontal actorDirection = _routine.slots.controller.direction.horizontal;
			bool isFacingDirection = (actorDirection == direction);

			Debug.Log("Is facing " + direction + ": " + isFacingDirection);

			return isFacingDirection;
		}

		public override void DrawInspectorGUI()
		{
			#if UNITY_EDITOR
			direction = (Direction.Horizontal)EditorGUILayout.EnumPopup("Direction", direction);
			#endif
		}

		public override BranchType GetBranchType()
		{
			return BranchType.FacingDirection;
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(FacingDirectionBranch))]
	public class FacingDirectionBranchEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}
