using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace RexEngine
{
	public class WorldActionSequenceSet:MonoBehaviour 
	{
		public List<WorldActionSequence> worldActionSequences = new List<WorldActionSequence>();
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(WorldActionSequenceSet))]
	public class WorldActionSequenceSetEditor:Editor 
	{
		public InspectorHelper.AIInspectorStyles styles;

		public override void OnInspectorGUI()
		{
			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			WorldActionSequenceSet _target = (target as WorldActionSequenceSet);

			for(int i = _target.worldActionSequences.Count - 1; i >= 0; i --)
			{
				if(_target.worldActionSequences[i] == null)
				{
					_target.worldActionSequences.RemoveAt(i);
				}
			}

			for(int i = 0; i < _target.worldActionSequences.Count; i ++)
			{
				string actionName = "Sequence " + (i + 1).ToString();
				_target.worldActionSequences[i].isFoldoutActive = EditorGUILayout.Foldout(_target.worldActionSequences[i].isFoldoutActive, actionName);
				if(_target.worldActionSequences[i].isFoldoutActive)
				{
					EditorGUILayout.BeginVertical(styles.eventBoxStyle);

					_target.worldActionSequences[i].DrawInspectorGUI();

					EditorGUILayout.EndVertical();

					if(_target.worldActionSequences.Count > 1)
					{
						ShowSequenceOptions(i);
					}

					EditorGUILayout.LabelField("");
				}
			}

			EditorGUILayout.LabelField("");

			InspectorHelper.DrawLine();

			EditorGUILayout.LabelField("");

			DisplayAddEffectMenu();

			EditorGUILayout.LabelField("");
		}

		protected void ShowSequenceOptions(int index)
		{
			WorldActionSequenceSet _target = (target as WorldActionSequenceSet);

			if(GUILayout.Button("Sequence Options", EditorStyles.miniButton)) 
			{
				GenericMenu menu = new GenericMenu();

				if(index != 0)
				{
					menu.AddItem(new GUIContent("Shift (Up)"), false, ShiftItemUp, (object)((int)index));
				}

				if(index < _target.worldActionSequences.Count - 1 && _target.worldActionSequences.Count > 1)
				{
					menu.AddItem(new GUIContent("Shift (Down)"), false, ShiftItemDown, (object)((int)index));
				}

				if(_target.worldActionSequences.Count > 1)
				{
					menu.AddItem(new GUIContent("Remove Item"), false, RemoveItem, (object)((int)index));
				}

				menu.ShowAsContext();
			}
		}

		protected void ShiftItemUp(object indexObject)
		{
			WorldActionSequenceSet _target = (target as WorldActionSequenceSet);

			int index = (int)indexObject;

			if(index <= 0)
			{
				return;
			}

			WorldActionSequence effect = _target.worldActionSequences[index];

			_target.worldActionSequences.RemoveAt(index);
			_target.worldActionSequences.Insert(index - 1, effect);

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

		protected void ShiftItemDown(object indexObject)
		{
			WorldActionSequenceSet _target = (target as WorldActionSequenceSet);

			int index = (int)indexObject;

			if(index > _target.worldActionSequences.Count - 1)
			{
				return;
			}

			WorldActionSequence effect = _target.worldActionSequences[index];

			_target.worldActionSequences.RemoveAt(index);
			_target.worldActionSequences.Insert(index + 1, effect);

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

		protected void RemoveItem(object indexObject)
		{
			WorldActionSequenceSet _target = (target as WorldActionSequenceSet);

			int index = (int)indexObject;

			WorldActionSequence effect = _target.worldActionSequences[index];

			DestroyImmediate(effect.gameObject);

			_target.worldActionSequences.RemoveAt(index);

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

		protected void DisplayAddEffectMenu()
		{
			if(GUILayout.Button("Add New World Action Sequence")) 
			{
				GenericMenu menu = new GenericMenu();

				int totalEvents = System.Enum.GetValues(typeof(WorldAction.EffectType)).Length;
				for(int i = 0; i < totalEvents; i ++)
				{
					menu.AddItem(new GUIContent("Starting Action: " + InspectorHelper.AddSpacesToString(((WorldAction.EffectType)i).ToString())), false, AddworldActionsequence, (object)((int)i));
				}

				menu.ShowAsContext();
			}
		}

		protected void AddworldActionsequence(object indexObject)
		{
			WorldActionSequenceSet _target = (target as WorldActionSequenceSet);

			Transform worldActionsTransform = _target.transform.Find("WorldActionSequences");
			GameObject worldActionsParentObject = null;
			if(worldActionsTransform != null)
			{
				worldActionsParentObject = worldActionsTransform.gameObject;
			}

			if(worldActionsParentObject == null)
			{
				worldActionsParentObject = new GameObject();
				worldActionsParentObject.name = "WorldActionSequences";
				worldActionsParentObject.transform.parent = _target.gameObject.transform;
				worldActionsParentObject.transform.localScale = Vector3.one;
				worldActionsParentObject.transform.localPosition = Vector3.zero;
			}

			GameObject worldActionSequenceGameObject = new GameObject();
			worldActionSequenceGameObject.transform.parent = worldActionsParentObject.transform;
			worldActionSequenceGameObject.transform.localScale = Vector3.one;
			worldActionSequenceGameObject.transform.localPosition = Vector3.zero;

			WorldActionSequence newWorldActionSequence = worldActionSequenceGameObject.AddComponent<WorldActionSequence>();
			newWorldActionSequence.worldActions = new List<WorldAction>();

			GameObject dataGameObject = InspectorHelper.DataGameObject(worldActionSequenceGameObject.transform);

			int index = (int)indexObject;
			WorldAction effect = WorldActionSequence.AddAction(index, dataGameObject);
			newWorldActionSequence.worldActions.Add(effect);
			newWorldActionSequence.isFoldoutActive = true;

			_target.worldActionSequences.Add(newWorldActionSequence);

			worldActionSequenceGameObject.name = "WorldActionSequence";

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				EditorGUIUtility.PingObject(worldActionSequenceGameObject);
			}
		}
	}
	#endif
}
