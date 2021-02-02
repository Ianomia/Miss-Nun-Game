using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class HPBranch:RexAIBranch 
	{
		public HPType hpType;
		public DamageOperator damageOperator;
		public int hpThreshold;
		public float hpPercentage;

		public override string ID()
		{
			return "HP";
		}

		public override bool Determine(RexAIRoutine _routine = null)
		{
			if(_routine == null)
			{
				return false;
			}

			RexActor actor = _routine.slots.controller.slots.actor;

			if(actor == null)
			{
				return false;
			}

			return HPHelpers.CheckHP(damageOperator, hpType, actor, hpThreshold, hpPercentage, actor.hp.current);
		}

		public override void DrawInspectorGUI()
		{
			#if UNITY_EDITOR
			hpType = (HPType)EditorGUILayout.EnumPopup("HP Type", hpType);
			damageOperator = (DamageOperator)EditorGUILayout.EnumPopup("Operator", damageOperator);

			if(hpType == HPType.Amount)
			{
				hpThreshold = EditorGUILayout.IntField("HP Threshold", hpThreshold);
			}
			else if(hpType == HPType.Percentage)
			{
				hpPercentage = EditorGUILayout.FloatField("HP Percentage Threshold", Mathf.Clamp01(hpPercentage));
			}
			#endif
		}

		public override BranchType GetBranchType()
		{
			return BranchType.HP;
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(HPBranch))]
	public class HPBranchEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

