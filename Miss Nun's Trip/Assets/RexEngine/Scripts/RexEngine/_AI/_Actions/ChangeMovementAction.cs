using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class ChangeMovementAction:RexAIAction 
	{
		public ChangeType changeType;
		public RexAIMovement newMovement;

		public enum ChangeType
		{
			StartNew,
			StopCurrent,
			SetModeToActive,
			SetModeToWait,
			ToggleMode
		}

		public override void Begin()
		{
			if(changeType == ChangeType.StartNew)
			{
				if(newMovement)
				{
					aiRoutine.ChangeMovement(newMovement);
				}
			}
			else if(changeType == ChangeType.StopCurrent)
			{
				aiRoutine.StopMovement();
			}
			else if(changeType == ChangeType.ToggleMode)
			{
				if(aiRoutine.aiMovement)
				{
					aiRoutine.aiMovement.ToggleMode();
				}
			}
			else if(changeType == ChangeType.SetModeToActive)
			{
				if(aiRoutine.aiMovement)
				{
					aiRoutine.aiMovement.SetMode(RexAIMovement.Mode.Active);
				}
			}
			else if(changeType == ChangeType.SetModeToWait)
			{
				if(aiRoutine.aiMovement)
				{
					aiRoutine.aiMovement.SetMode(RexAIMovement.Mode.Wait);
				}
			}

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);

			changeType = (ChangeType)EditorGUILayout.EnumPopup("Change Type", changeType);
			if(changeType == ChangeType.StartNew)
			{
				newMovement = EditorGUILayout.ObjectField("New Movement", newMovement, typeof(RexAIMovement), true) as RexAIMovement;
			}
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.ChangeMovement;
		}

		public override string GetName()
		{
			return "Change Movement";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(ChangeMovementAction))]
	public class ChangeMovementActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

