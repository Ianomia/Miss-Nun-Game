using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class SystemAction:RexAIAction 
	{
		public SystemType systemType;
		public EnableType enableType;
		public GameObject objectToEnable;
		public string levelToLoad;

		public enum SystemType
		{
			EnableGameObject,
			EnablePlayerControl,
			ShakeScreen,
			LoadLevel
		}

		public enum EnableType
		{
			Enable,
			Disable
		}

		public override void Begin()
		{
			if(systemType == SystemType.EnableGameObject)
			{
				if(enableType == EnableType.Enable)
				{
					if(objectToEnable)
					{
						objectToEnable.SetActive(true);
					}
				}
				else if(enableType == EnableType.Disable)
				{
					if(objectToEnable)
					{
						objectToEnable.SetActive(false);
					}
				}
			}
			else if(systemType == SystemType.EnablePlayerControl)
			{
				if(enableType == EnableType.Enable)
				{
					GameManager.Instance.players[0].RegainControl();
				}
				else if(enableType == EnableType.Disable)
				{
					GameManager.Instance.players[0].RemoveControl();
				}
			}
			else if(systemType == SystemType.ShakeScreen)
			{
				ScreenShake.Instance.Shake();
			}
			else if(systemType == SystemType.LoadLevel)
			{
				RexSceneManager.Instance.LoadSceneWithFadeOut(levelToLoad);
			}

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);

			systemType = (SystemType)EditorGUILayout.EnumPopup("Action Type", systemType);

			if(systemType == SystemType.EnableGameObject || systemType == SystemType.EnablePlayerControl)
			{
				enableType = (EnableType)EditorGUILayout.EnumPopup("Enable Type", enableType);
			}

			if(systemType == SystemType.EnableGameObject)
			{
				objectToEnable = EditorGUILayout.ObjectField("GameObject", objectToEnable, typeof(GameObject), true) as GameObject;
			}

			if(systemType == SystemType.LoadLevel)
			{
				levelToLoad = EditorGUILayout.TextField("Level", levelToLoad);
			}
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.System;
		}

		public override string GetName()
		{
			return "System";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(SystemAction))]
	public class SystemActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}
