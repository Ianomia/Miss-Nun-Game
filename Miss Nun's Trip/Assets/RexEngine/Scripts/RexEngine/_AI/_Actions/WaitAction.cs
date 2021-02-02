using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class WaitAction:RexAIAction 
	{
		public bool randomizeTime;
		public float seconds;
		public float lowSeconds;
		public float highSeconds;

		public override void Begin()
		{
			StartCoroutine("WaitCoroutine");
		}

		protected IEnumerator WaitCoroutine()
		{
			if(randomizeTime)
			{
				seconds = RexMath.RandomFloat(lowSeconds, highSeconds);
			}
			
			yield return new WaitForSeconds(seconds);

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);

			randomizeTime = EditorGUILayout.Toggle("Randomize Time", randomizeTime);

			if(!randomizeTime)
			{
				seconds = EditorGUILayout.FloatField("Seconds", seconds);
			}
			else
			{
				lowSeconds = EditorGUILayout.FloatField("Seconds (Lowest)", lowSeconds);
				highSeconds = EditorGUILayout.FloatField("Seconds (Highest)", highSeconds);
			}
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.Wait;
		}

		public override string GetName()
		{
			return "Wait";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(WaitAction))]
	public class WaitActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

