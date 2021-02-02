using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class ToggleEventAction:RexAIAction 
	{
		public ToggleType toggleType;
		public RexAIActionEventSet eventToToggle;

		public enum ToggleType
		{
			Enable,
			Disable,
			Toggle
		}

		public override void Begin()
		{
			if(eventToToggle)
			{
				if(toggleType == ToggleType.Enable)
				{
					eventToToggle.Enable();
				}

				else if(toggleType == ToggleType.Disable)
				{
					eventToToggle.Disable();
				}
				else if(toggleType == ToggleType.Toggle)
				{
					if(!eventToToggle.isEnabled)
					{
						eventToToggle.Enable();
					}
					else
					{
						eventToToggle.Disable();
					}
				}
			}

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);
			toggleType = (ToggleType)EditorGUILayout.EnumPopup("Toggle Type", toggleType);
			eventToToggle = EditorGUILayout.ObjectField("Event", eventToToggle, typeof(RexAIActionEventSet), true) as RexAIActionEventSet;
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.ToggleEvent;
		}

		public override string GetName()
		{
			return "Toggle Event";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(ToggleEventAction))]
	public class ToggleEventActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

