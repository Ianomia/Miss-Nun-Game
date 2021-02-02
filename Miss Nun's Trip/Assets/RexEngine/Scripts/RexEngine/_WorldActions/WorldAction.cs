using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class WorldAction:MonoBehaviour 
	{
		public EffectType effectType;
		public FindTargetData findTargetData;

		#if UNITY_EDITOR
		protected InspectorHelper.AIInspectorStyles styles;
		#endif

		public enum ReferenceType
		{
			Name,
			Slot
		}

		public enum EnableType
		{
			Enable,
			Disable
		}

		public enum EffectType
		{
			EnableOrDisableObject,
			ToggleAbility,
			ToggleAttack,
			SetRexController,
			SetAIRoutine,
			CallFunction
		}

		protected FindTargetMethods findTargetMethods;

		void Awake()
		{
			findTargetMethods = gameObject.AddComponent<FindTargetMethods>();
		}

		public virtual EffectType GetEffectType()
		{
			return EffectType.EnableOrDisableObject;
		}

		public virtual void TriggerEffect() 
		{
			
		}

		public virtual void DrawInspectorGUI()
		{

		}

		public virtual string GetName()
		{
			return "World Action";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(WorldAction))]
	public class WorldActionEditor:Editor 
	{
		public override void OnInspectorGUI()
		{
		}
	}
	#endif
}