using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class AbilityAction:RexAIAction 
	{
		public RexState ability;
		public AbilityType abilityType;

		public enum AbilityType
		{
			Crouch,
			Dash,
			Glide,
			GroundPound,
			Jump,
			Knockback,
			Landing,
			Other
		}

		#if UNITY_EDITOR
		protected InspectorHelper.AIInspectorStyles styles;
		#endif

		public override void Begin()
		{
			ability.Begin();

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);
			abilityType = (AbilityType)EditorGUILayout.EnumPopup("Ability Type", abilityType);
			string className = "RexEngine." + abilityType.ToString() + "State";
			System.Type classType = System.Type.GetType(className);

			if(abilityType != AbilityType.Other)
			{
				if(ability == null || (ability.GetType() != classType))
				{
					if(aiRoutine.slots.controller.GetComponent(classType) != null)
					{
						ability = aiRoutine.slots.controller.GetComponent(classType) as RexState;
					}
					else
					{
						EditorGUILayout.LabelField("WARNING: The RexController associated with this RexAIRoutine has no " + abilityType.ToString() + "State. No ability will be performed when this action plays.", styles.helpTextStyle);
					}
				}
			}
			else
			{
				ability = EditorGUILayout.ObjectField("Ability", ability, typeof(RexState), true) as RexState;
				EditorGUILayout.LabelField("Some abilities (such as Stair Climbing) are contextual to the environment the character is in. As such, they may not execute properly if called here.", styles.boxStyle);
			}
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.Ability;
		}

		public override string GetName()
		{
			return "Ability";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(AbilityAction))]
	public class AbilityActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

