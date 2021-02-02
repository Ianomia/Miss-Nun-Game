using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class SetRexControllerWorldAction:WorldAction 
	{
		public RexController controller;
		public string controllerName = "";
		public ReferenceType referenceType;

		public override void TriggerEffect() 
		{
			if(referenceType == ReferenceType.Name)
			{
				controller = null;
			}
			else if(referenceType == ReferenceType.Slot)
			{
				controllerName = "";
			}

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

			if(controller == null)
			{
				Transform controllerHolderTransform = actor.transform.Find("Controllers");
				if(controllerHolderTransform != null)
				{
					Transform controllerTransform = controllerHolderTransform.Find(controllerName);
					if(controllerTransform != null)
					{
						controller = controllerTransform.GetComponent<RexController>();
					}
				}
				else
				{
					Transform controllerTransform = actor.transform.Find(controllerName);
					if(controllerTransform != null)
					{
						controller = controllerTransform.GetComponent<RexController>();
					}
				}
			}

			if(controller != null)
			{
				actor.SetController(controller);
			}
		}

		public override string GetName()
		{
			return "Set Rex Controller";
		}

		public override EffectType GetEffectType()
		{
			return EffectType.SetRexController;
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

			referenceType = (ReferenceType)EditorGUILayout.EnumPopup("Reference By", referenceType);

			if(referenceType == ReferenceType.Name)
			{
				controllerName = EditorGUILayout.TextField("Rex Controller Name", controllerName);
			}
			else if(referenceType == ReferenceType.Slot)
			{
				controller = EditorGUILayout.ObjectField("Rex Controller", controller, typeof(RexController), true) as RexController;
			}
		}
		#endif
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(SetRexControllerWorldAction))]
	public class SetRexControllerWorldActionEditor:Editor 
	{
		public override void OnInspectorGUI()
		{
		}
	}
	#endif
}