using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class ToggleSequenceAction:RexAIAction 
	{
		public ToggleType toggleType;
		public RexAISequence sequence;

		public enum ToggleType
		{
			Play,
			Stop
		}

		public override void Begin()
		{
			if(sequence)
			{
				if(toggleType == ToggleType.Play)
				{
					sequence.Begin();
				}
				else if(toggleType == ToggleType.Stop)
				{
					sequence.Stop();
				}
			}

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);
			toggleType = (ToggleType)EditorGUILayout.EnumPopup("Toggle Type", toggleType);
			sequence = EditorGUILayout.ObjectField("Sequence", sequence, typeof(RexAISequence), true) as RexAISequence;
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.ToggleSequence;
		}

		public override string GetName()
		{
			return "Toggle Sequence";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(ToggleSequenceAction))]
	public class ToggleSequenceActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

