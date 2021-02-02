using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	[System.Serializable]
	public class FindTargetData
	{
		public TargetType targetType;
		public Transform otherTransform;
		public string targetName = "";

		[HideInInspector]
		public bool hasFoundTarget = true; //Used primarily when the target is the Player to ensure that the player is fully positioned before this moves

		public enum TargetType 
		{
			Player,
			Name,
			Transform
		}

		public void DrawInspectorGUI()
		{
			#if UNITY_EDITOR
			targetType = (FindTargetData.TargetType)EditorGUILayout.EnumPopup("Target Type", targetType);
			if(targetType == FindTargetData.TargetType.Player)
			{
				otherTransform = null;
			}
			else if(targetType == FindTargetData.TargetType.Name)
			{
				targetName = EditorGUILayout.TextField("Target Name", targetName);
			}
			else if(targetType == FindTargetData.TargetType.Transform)
			{
				otherTransform = (Transform)EditorGUILayout.ObjectField("Transform", otherTransform, typeof(Transform), true);
			}
			#endif
		}
	}

	public class FindTargetMethods:MonoBehaviour 
	{
		protected FindTargetData findTargetData;

		public void GetTarget(FindTargetData _findTargetData)
		{
			findTargetData = _findTargetData;

			if(findTargetData.targetType != FindTargetData.TargetType.Player || (findTargetData.targetType == FindTargetData.TargetType.Player && !RexSceneManager.Instance.isLoadingNewScene))
			{
				if(findTargetData.targetType == FindTargetData.TargetType.Player)
				{
					GetPlayer();
				}
				else if(findTargetData.targetType == FindTargetData.TargetType.Name)
				{
					GameObject otherGameObject = GameObject.Find(findTargetData.targetName);
					if(otherGameObject != null)
					{
						findTargetData.otherTransform = otherGameObject.transform;
					}
				}
			}
			else
			{
				RexSceneManager.Instance.OnPlayerPositioned += OnPlayerPositioned;
				findTargetData.hasFoundTarget = false;
			}
		}

		protected void GetPlayer()
		{
			findTargetData.otherTransform = GameManager.Instance.players[0].transform;
		}

		protected void OnPlayerPositioned()
		{
			RexSceneManager.Instance.OnPlayerPositioned -= OnPlayerPositioned;

			GetPlayer();
			findTargetData.hasFoundTarget = true;
		}
	}
}
