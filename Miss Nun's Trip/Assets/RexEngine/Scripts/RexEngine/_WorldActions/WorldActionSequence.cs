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
	[System.Serializable]
	public class WorldActionSequence:MonoBehaviour 
	{
		public List<WorldAction> worldActions;

		#if UNITY_EDITOR
		public InspectorHelper.AIInspectorStyles styles;
		public bool isFoldoutActive;

		public void DrawInspectorGUI()
		{
			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			if(worldActions == null)
			{
				worldActions = new List<WorldAction>();
			}

			for(int i = worldActions.Count - 1; i >= 0; i --)
			{
				if(worldActions[i] == null)
				{
					worldActions.RemoveAt(i);
				}
			}

			for(int i = 0; i < worldActions.Count; i ++)
			{
				DisplayEffect(i);
			}

			EditorGUILayout.LabelField("");

			InspectorHelper.DrawLine();

			EditorGUILayout.LabelField("");

			EditorGUILayout.BeginHorizontal();

			if(GUILayout.Button("Add World Action to This Sequence"))
			{
				AddAction();
			}

			EditorGUILayout.EndHorizontal();
		}

		protected void AddAction()
		{
			Transform dataGameObjectTransform = transform.Find("Data");
			GameObject dataGameObject = null;
			if(dataGameObjectTransform != null)
			{
				dataGameObject = dataGameObjectTransform.gameObject;
			}

			if(dataGameObject == null)
			{
				dataGameObject = InspectorHelper.DataGameObject(transform);
			}

			EnableDisableObjectWorldAction newEffect = dataGameObject.AddComponent<EnableDisableObjectWorldAction>();

			worldActions.Add(newEffect);

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

		protected void DisplayEffect(int index)
		{
			string prefix = "";

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField("<i>" + prefix + "</i>", styles.labelStyle);

			DisplayEffectOptionsMenu(index);

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginVertical(styles.actionStyle);

			worldActions[index].DrawInspectorGUI();

			EditorGUILayout.EndVertical();

			EditorGUILayout.LabelField("");
			EditorGUILayout.LabelField("");

			if(EditorGUI.EndChangeCheck())
			{
				if(!Application.isPlaying)
				{
					EditorUtility.SetDirty(worldActions[index]);
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}
			}
		}

		protected void DisplayEffectOptionsMenu(int index)
		{
			if(GUILayout.Button("Action Options", EditorStyles.miniButton, GUILayout.Width(100))) 
			{
				GenericMenu menu = new GenericMenu();

				if(index != 0)
				{
					menu.AddItem(new GUIContent("Shift (Up)"), false, ShiftItemUp, (object)((int)index));
				}

				if(index < worldActions.Count - 1 && worldActions.Count > 1)
				{
					menu.AddItem(new GUIContent("Shift (Down)"), false, ShiftItemDown, (object)((int)index));
				}

				DisplayActionsInMenu(menu, index, worldActions);

				if(worldActions.Count > 1)
				{
					menu.AddItem(new GUIContent("Remove Item"), false, RemoveItem, (object)((int)index));
				}

				menu.ShowAsContext();
			}
		}

		protected void ShiftItemUp(object indexObject)
		{
			int index = (int)indexObject;

			if(index <= 0)
			{
				return;
			}

			WorldAction effect = worldActions[index];

			worldActions.RemoveAt(index);
			worldActions.Insert(index - 1, effect);

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

		protected void ShiftItemDown(object indexObject)
		{
			int index = (int)indexObject;

			if(index > worldActions.Count - 1)
			{
				return;
			}

			WorldAction effect = worldActions[index];

			worldActions.RemoveAt(index);
			worldActions.Insert(index + 1, effect);

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

		protected void RemoveItem(object indexObject)
		{
			int index = (int)indexObject;

			WorldAction effect = worldActions[index];

			DestroyImmediate(effect);

			worldActions.RemoveAt(index);

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

		protected void DisplayActionsInMenu(GenericMenu menu, int index, List<WorldAction> effects = null)
		{
			int effectType = (int)effects[index].GetEffectType();
			int totalEffects = System.Enum.GetValues(typeof(WorldAction.EffectType)).Length - 1;

			for(int i = 0; i <= totalEffects; i ++)
			{
				Hashtable hashtable = new Hashtable();
				hashtable[0] = index;
				hashtable[1] = i;
				hashtable[2] = effects;

				if(effectType != i) menu.AddItem(new GUIContent("Change Action Type/To: " + InspectorHelper.AddSpacesToString(((WorldAction.EffectType)i).ToString())), false, ChangeEffect, (object)(hashtable));
			}
		}

		protected void ChangeEffect(object parameters)
		{
			Hashtable hashtable = (Hashtable)parameters;

			List<WorldAction> effects = (List<WorldAction>)hashtable[2];

			int indexOfItemToChange = (int)hashtable[0];
			int newActionType = (int)hashtable[1];

			GameObject actionGameObject = effects[indexOfItemToChange].gameObject;

			WorldAction oldAction = effects[indexOfItemToChange];
			DestroyImmediate(oldAction);

			WorldAction newAction = WorldActionSequence.AddAction(newActionType, actionGameObject);

			if(effects != null)
			{
				effects[indexOfItemToChange] = newAction;
			}

			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}

		public static WorldAction AddAction(int index, GameObject _gameObject)
		{
			WorldAction effect = null;
			switch(index)
			{
				case 0:
					effect = _gameObject.AddComponent<EnableDisableObjectWorldAction>();
					break;
				case 1:
					effect = _gameObject.AddComponent<ToggleAbilityWorldAction>();
					break;
				case 2:
					effect = _gameObject.AddComponent<ToggleAttackWorldAction>();
					break;
				case 3:
					effect = _gameObject.AddComponent<SetRexControllerWorldAction>();
					break;
				case 4:
					effect = _gameObject.AddComponent<SetAIRoutineWorldAction>();
					break;
				case 5:
					effect = _gameObject.AddComponent<CallFunctionWorldAction>();
					break;
			}

			return effect;
		}
		#endif
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(WorldActionSequence))]
	public class WorldActionSequenceEditor:Editor 
	{
		public override void OnInspectorGUI()
		{
			WorldActionSequence _target = (target as WorldActionSequence);
			_target.DrawInspectorGUI();
		}
	}
	#endif
}
