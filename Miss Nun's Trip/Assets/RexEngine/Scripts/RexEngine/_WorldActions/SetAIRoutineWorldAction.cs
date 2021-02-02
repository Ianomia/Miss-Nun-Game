using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class SetAIRoutineWorldAction:WorldAction 
	{
		public RexAIRoutine aiRoutine; 
		public string aiRoutineName;

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

			if(aiRoutine == null)
			{
				Transform aiHolderTransform = actor.transform.Find("AI");
				Transform aiTransform = actor.transform.Find(aiRoutineName);
				if(aiHolderTransform != null || aiTransform != null)
				{
					if(aiTransform == null)
					{
						aiTransform = aiHolderTransform.transform.Find(aiRoutineName);
					}

					if(aiTransform != null)
					{
						aiRoutine = aiTransform.GetComponent<RexAIRoutine>();
					}
				}
			}

			if(aiRoutine != null)
			{
				actor.SetAIRoutine(aiRoutine);
			}
		}

		public override string GetName()
		{
			return "Set AI Routine";
		}

		public override EffectType GetEffectType()
		{
			return EffectType.SetAIRoutine;
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

			aiRoutine = EditorGUILayout.ObjectField("New Rex AI Routine",aiRoutine, typeof(RexAIRoutine), true) as RexAIRoutine;
		}
		#endif
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(SetAIRoutineWorldAction))]
	public class SetAIRoutineEffectWorldAction:Editor 
	{
		public override void OnInspectorGUI()
		{
		}
	}
	#endif
}

