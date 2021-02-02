using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class ChangeAIRoutineAction:RexAIAction 
	{
		public RexAIRoutine newAIRoutine;

		public override void Begin()
		{
			if(aiRoutine != null)
			{
				if(aiRoutine.slots.controller.slots.actor != null)
				{
					aiRoutine.slots.controller.slots.actor.SetAIRoutine(newAIRoutine);
				}
			}

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);
			newAIRoutine = EditorGUILayout.ObjectField("New Rex AI Routine", newAIRoutine, typeof(RexAIRoutine), true) as RexAIRoutine;
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.ChangeAIRoutine;
		}

		public override string GetName()
		{
			return "Change AI Routine";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(ChangeAIRoutineAction))]
	public class ChangeAIRoutineActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

