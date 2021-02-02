using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class ToggleInvincibilityAction:RexAIAction 
	{
		public ToggleType toggleType;

		public enum ToggleType
		{
			Enable,
			Disable,
			Toggle
		}

		public override void Begin()
		{
			RexActor actor = aiRoutine.slots.controller.slots.actor;
			if(actor)
			{
				if(toggleType == ToggleType.Enable)
				{
					actor.invincibility.isAlwaysInvincible = true;
				}
				else if(toggleType == ToggleType.Disable)
				{
					actor.invincibility.isAlwaysInvincible = false;
				}
				else if(toggleType == ToggleType.Toggle)
				{
					if(!actor.invincibility.isAlwaysInvincible)
					{
						actor.invincibility.isAlwaysInvincible = true;
					}
					else
					{
						actor.invincibility.isAlwaysInvincible = false;
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
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.ToggleInvincibility;
		}

		public override string GetName()
		{
			return "Toggle Invincibility";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(ToggleInvincibilityAction))]
	public class ToggleInvincibilityActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

