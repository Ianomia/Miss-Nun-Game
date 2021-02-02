using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class CallFunctionWorldAction:WorldAction
	{
		public MonoBehaviour callFunctionOnScript;
		public string methodName = "";

		public override void TriggerEffect() 
		{
			if(callFunctionOnScript && methodName != "")
			{
				callFunctionOnScript.Invoke(methodName, 0.0f);
			}
		}

		public override string GetName()
		{
			return "Call Function";
		}

		public override EffectType GetEffectType()
		{
			return EffectType.CallFunction;
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);
			callFunctionOnScript = EditorGUILayout.ObjectField("Call Function on Script", callFunctionOnScript, typeof(MonoBehaviour), true) as MonoBehaviour;
			methodName = EditorGUILayout.TextField("Method Name", methodName);
		}
		#endif
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(CallFunctionWorldAction))]
	public class CallFunctionWorldActionEditor:Editor 
	{
		public override void OnInspectorGUI()
		{
		}
	}
	#endif
}