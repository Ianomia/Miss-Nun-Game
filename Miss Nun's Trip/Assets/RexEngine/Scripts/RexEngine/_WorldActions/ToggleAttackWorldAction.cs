using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class ToggleAttackWorldAction:WorldAction
	{
		public string attackName;
		public bool willEnableAttack;

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

			Transform attackHolderTransform = actor.transform.Find("Attacks");
			Transform attackTransform = actor.transform.Find(attackName);
			if(attackHolderTransform != null || attackTransform != null)
			{
				if(attackTransform == null)
				{
					attackTransform = attackHolderTransform.transform.Find(attackName);
				}

				if(attackTransform != null)
				{
					Attack attack = attackTransform.GetComponent<Attack>();
					if(attack != null)
					{
						attack.isEnabled = willEnableAttack;
					}
					else
					{
						ComboChain comboChain = attackTransform.GetComponent<ComboChain>();
						if(comboChain != null)
						{
							comboChain.SetEnabled(willEnableAttack);
						}
					}
				}
			}
		}

		public override string GetName()
		{
			return "Toggle Attack";
		}

		public override EffectType GetEffectType()
		{
			return EffectType.ToggleAttack;
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

			attackName = EditorGUILayout.TextField("Attack Name", attackName);
			willEnableAttack = EditorGUILayout.Toggle("Enable", willEnableAttack);
		}
		#endif
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(ToggleAttackWorldAction))]
	public class ToggleAttackWorldActionEditor:Editor 
	{
		public override void OnInspectorGUI()
		{
		}
	}
	#endif
}

