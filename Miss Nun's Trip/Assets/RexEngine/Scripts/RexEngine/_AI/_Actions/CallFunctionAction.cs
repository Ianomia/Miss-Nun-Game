using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class CallFunctionAction:RexAIAction 
	{
		public MonoBehaviour callFunctionOnScript;
		public string methodName = "";

		public override void Begin()
		{
			if(callFunctionOnScript && methodName != "")
			{
				callFunctionOnScript.Invoke(methodName, 0.0f);
			}

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);
			callFunctionOnScript = EditorGUILayout.ObjectField("Call Function on Script", callFunctionOnScript, typeof(MonoBehaviour), true) as MonoBehaviour;
			methodName = EditorGUILayout.TextField("Method Name", methodName);
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.CallFunction;
		}

		public override string GetName()
		{
			return "Call Function";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(CallFunctionAction))]
	public class CallFunctionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}
