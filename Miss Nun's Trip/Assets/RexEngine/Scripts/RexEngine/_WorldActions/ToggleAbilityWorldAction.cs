using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class ToggleAbilityWorldAction:WorldAction 
	{
		public RexState ability;
		public AbilityType abilityType;
		public bool willEnableAbility;

		public enum AbilityType //TODO: AbilityAction.cs also uses this; put this somewhere so they can both use it
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

		public override void TriggerEffect() 
		{
			if(findTargetData == null)
			{
				findTargetData = new FindTargetData();
			}

			if(findTargetData.otherTransform == null)
			{
				findTargetMethods.GetTarget(findTargetData);
			}

			if(findTargetData.otherTransform == null)
			{
				return;
			}

			RexActor actor = findTargetData.otherTransform.GetComponent<RexActor>();
			if(actor == null)
			{
				return;
			}

			string className = "RexEngine." + abilityType.ToString() + "State";
			System.Type classType = System.Type.GetType(className);

			if(abilityType != AbilityType.Other)
			{
				if(ability == null || (ability.GetType() != classType))
				{
					if(actor.slots.controller.GetComponent(classType) != null)
					{
						ability = actor.slots.controller.GetComponent(classType) as RexState;
					}
				}
			}


			if(ability != null)
			{
				ability.isEnabled = willEnableAbility;
			}
		}

		public override string GetName()
		{
			return "Toggle Ability";
		}

		public override EffectType GetEffectType()
		{
			return EffectType.ToggleAbility;
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);

			if(findTargetData == null)
			{
				findTargetData = new FindTargetData();
			}

			findTargetData.DrawInspectorGUI();

			abilityType = (AbilityType)EditorGUILayout.EnumPopup("Ability Type", abilityType);

			if(abilityType == AbilityType.Other)
			{
				ability = EditorGUILayout.ObjectField("Ability", ability, typeof(RexState), true) as RexState;
			}

			willEnableAbility = EditorGUILayout.Toggle("Enable", willEnableAbility);
		}
		#endif
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(ToggleAbilityWorldAction))]
	public class ToggleAbilityWorldActionEditor:Editor 
	{
		public override void OnInspectorGUI()
		{
		}
	}
	#endif
}

