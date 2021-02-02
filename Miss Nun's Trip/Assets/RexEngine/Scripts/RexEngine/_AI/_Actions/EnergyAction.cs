using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class EnergyAction:RexAIAction 
	{
		public EnergyType energyType;
		public ValueType valueType;
		public int amount;
		public float barFillDuration = 2.5f;
		public TargetType targetType;
		public RexActor otherActor;

		public enum EnergyType
		{
			HP,
			MP
		}

		public enum TargetType
		{
			Self,
			Player,
			Other
		}

		public enum ValueType
		{
			Add,
			Subtract,
			SetToAmount,
			SetToMax,
			SetToZero,
			AnimateBarFill
		}

		public override void Begin()
		{
			RexActor actor = aiRoutine.slots.controller.slots.actor;
			if(targetType == TargetType.Player)
			{
				actor = GameManager.Instance.player;
			}
			else if(targetType == TargetType.Other)
			{
				actor = otherActor;
			}

			if(actor == null)
			{
				End();
				return;
			}

			if(energyType == EnergyType.MP && actor.mp == null)
			{
				End();
				return;
			}

			Energy energy = (energyType == EnergyType.HP) ? actor.hp : actor.mp;
			if(energy == null)
			{
				End();
				return;
			}

			if(valueType == ValueType.Add)
			{
				energy.Restore(amount);
			}
			else if(valueType == ValueType.Subtract)
			{
				if(energyType == EnergyType.HP)
				{
					actor.Damage(amount, false);
				}
				else if(energyType == EnergyType.MP)
				{
					energy.Decrement(amount);
				}
			}
			else if(valueType == ValueType.SetToAmount) 
			{
				energy.SetValue(amount);
			}
			else if(valueType == ValueType.SetToMax)
			{
				energy.SetToMax();
			}
			else if(valueType == ValueType.SetToZero)
			{
				if(energyType == EnergyType.HP)
				{
					actor.KillImmediately();
				}
				else if(energyType == EnergyType.MP)
				{
					energy.SetValue(0);
				}
			}
			else if(valueType == ValueType.AnimateBarFill)
			{
				energy.bar.FillBarFromEmpty(barFillDuration);
			}

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);

			energyType = (EnergyType)EditorGUILayout.EnumPopup("Energy Type", energyType);
			valueType = (ValueType)EditorGUILayout.EnumPopup("Value Type", valueType);

			if(valueType == ValueType.Add || valueType == ValueType.Subtract || valueType == ValueType.SetToAmount)
			{
				amount = EditorGUILayout.IntField("Amount", amount);
				if(valueType == ValueType.SetToAmount)
				{
					if(amount < 1)
					{
						amount = 1;
					}
				}

				if(valueType == ValueType.Add || valueType == ValueType.SetToAmount)
				{
					EditorGUILayout.LabelField("If this amount is greater than the maximum amount defined by the Energy component, it will be set to its maximum value.", EditorStyles.helpBox);
				}
			}
			else if(valueType == ValueType.AnimateBarFill)
			{
				barFillDuration = EditorGUILayout.FloatField("Bar Fill Duration", barFillDuration);
			}

			targetType = (TargetType)EditorGUILayout.EnumPopup("Target Type", targetType);

			if(targetType == TargetType.Other)
			{
				otherActor = EditorGUILayout.ObjectField("Other Actor", otherActor, typeof(RexActor), true) as RexActor;
			}
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.SetEnergy;
		}

		public override string GetName()
		{
			return "Energy";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(EnergyAction))]
	public class EnergyActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}
