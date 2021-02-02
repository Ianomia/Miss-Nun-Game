using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class EnableDisableObjectWorldAction:WorldAction 
	{
		public ReferenceType referenceType;
		public GameObject objectToEnable;
		public string objectName = "";
		public EnableType enableType;

		public override void TriggerEffect() 
		{
			if(referenceType == ReferenceType.Name)
			{
				objectToEnable = null;
			}
			else if(referenceType == ReferenceType.Slot)
			{
				objectName = "";
			}

			bool willEnable = (enableType == EnableType.Enable);
			if(objectToEnable)
			{
				objectToEnable.SetActive(willEnable);
			}
			else if(objectName != "")
			{
				GameObject objectToFind = GameObject.Find(objectName);
				if(objectToFind != null)
				{
					objectToFind.SetActive(willEnable);
				}
			}
		}

		public override string GetName()
		{
			return "Enable or Disable Object";
		}

		public override EffectType GetEffectType()
		{
			return EffectType.EnableOrDisableObject;
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);

			referenceType = (ReferenceType)EditorGUILayout.EnumPopup("Reference By", referenceType);

			enableType = (EnableType)EditorGUILayout.EnumPopup("Enable Type", enableType);

			if(referenceType == ReferenceType.Name)
			{
				objectName = EditorGUILayout.TextField("GameObject Name", objectName);
			}
			else if(referenceType == ReferenceType.Slot)
			{
				objectToEnable = EditorGUILayout.ObjectField("GameObject", objectToEnable, typeof(GameObject), true) as GameObject;
			}
		}
		#endif
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(EnableDisableObjectWorldAction))]
	public class EnableDisableObjectWorldActionEditor:Editor 
	{
		public override void OnInspectorGUI()
		{
		}
	}
	#endif
}
