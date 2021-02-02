using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class FaceDirectionAction:RexAIAction 
	{
		public Direction.Horizontal direction;
		public ToggleType toggleType;

		public enum ToggleType
		{
			Explicit,
			Toggle
		}

		public override void Begin()
		{
			Direction.Horizontal newDirection = direction;
			if(toggleType == ToggleType.Toggle && aiRoutine.slots.controller != null)
			{
				newDirection = (aiRoutine.slots.controller.direction.horizontal == Direction.Horizontal.Left) ? Direction.Horizontal.Right : Direction.Horizontal.Left;
			}

			aiRoutine.slots.controller.FaceDirection(newDirection);

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			toggleType = (ToggleType)EditorGUILayout.EnumPopup("Toggle Type", toggleType);
			if(toggleType == ToggleType.Explicit)
			{
				direction = (Direction.Horizontal)EditorGUILayout.EnumPopup("Direction", direction);
			}
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.FaceDirection;
		}

		public override string GetName()
		{
			return "Face Direction Action";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(FaceDirectionAction))]
	public class FaceDirectionActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

