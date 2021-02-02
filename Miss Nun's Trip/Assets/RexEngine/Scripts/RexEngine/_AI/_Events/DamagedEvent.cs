using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class DamagedEvent:RexAIEvent 
	{
		public HPType hpType;
		public DamageOperator damageOperator;
		public int hpThreshold;
		public float hpPercentage;

		protected RexActor actor;

		void Awake() 
		{

		}

		void Start() 
		{
			
		}

		public override void Enable()
		{
			base.Enable();

			actor = aiRoutine.slots.controller.slots.actor;
			if(actor != null)
			{
				actor.OnDamageTaken += this.OnParentDamaged;
			}
		}

		public override void Disable()
		{
			base.Disable();

			if(actor != null)
			{
				actor.OnDamageTaken -= this.OnParentDamaged;
			}
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
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
		}
		#endif

		public override string GetName()
		{
			return "Damaged";
		}

		public override EventType GetEventType()
		{
			return EventType.Damaged;
		}

		public void OnParentDamaged(int amount, int unadjustedAmount, int currentHP, int totalHP)
		{
			bool didActivate = HPHelpers.CheckHP(damageOperator, hpType, actor, hpThreshold, hpPercentage, currentHP);

			if(didActivate)
			{
				actor.OnDamageTaken -= this.OnParentDamaged;
				OnEventActivated();
			}
		}

		void OnDestroy()
		{
			if(actor)
			{
				actor.OnDamageTaken -= this.OnParentDamaged;
			}
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(DamagedEvent))]
	public class DamagedEventEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

