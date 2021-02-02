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
	public class RexAIActionEventSet:MonoBehaviour 
	{
		public RexAIRoutine aiRoutine;
		public RexAIEvent aiEvent;
		public List<RexAIAction> actions = new List<RexAIAction>();

		#if UNITY_EDITOR
		public InspectorHelper.AIInspectorStyles styles;
		#endif

		[HideInInspector]
		public bool isFoldoutActive = true;

		public bool isEnabled;

		public GameObject dataGameObject;

		protected bool hasEventActivated = false;

		void Start()
		{
			aiEvent.actionEventSet = this;
			for(int i = 0; i < actions.Count; i ++)
			{
				actions[i].SetAIRoutine(aiRoutine);
			}
		}

		public void Enable()
		{
			isEnabled = true;
			hasEventActivated = false;

			if(aiEvent != null)
			{
				aiEvent.Enable();
			}
		}

		public void Disable()
		{
			isEnabled = false;
			if(aiEvent != null)
			{
				aiEvent.Disable();
			}
		}

		public void CheckEventStatus()
		{
			if(!hasEventActivated)
			{
				aiEvent.CheckEventStatus();
			}
		}

		public void OnEventActivated()
		{
			if(!hasEventActivated)
			{
				hasEventActivated = true;
				for(int i = 0; i < actions.Count; i ++)
				{
					actions[i].Begin();
				}
			}
		}

		public void DrawInspectorGUI()
		{
			#if UNITY_EDITOR
			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			if(dataGameObject == null)
			{
				dataGameObject = InspectorHelper.DataGameObject(transform);
			}

			for(int i = actions.Count - 1; i >= 0; i --)
			{
				if(actions[i] == null)
				{
					actions.RemoveAt(i);
				}

				actions[i].SetAIRoutine(aiRoutine);
			}

			if(actions.Count <= 0)
			{
				AddAction();
			}

			isEnabled = EditorGUILayout.Toggle("Is Enabled", isEnabled);

			DisplayEvent();

			EditorGUILayout.LabelField("<i>" + "do (all actions execute at once)" + "</i>", styles.labelStyle);

			EditorGUILayout.BeginVertical(styles.eventBoxStyle);

			for(int i = 0; i < actions.Count; i ++)
			{
				DisplayAction(i);
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.LabelField("");

			InspectorHelper.DrawLine();

			EditorGUILayout.LabelField("");

			EditorGUILayout.BeginHorizontal();

			if(GUILayout.Button("Add Action"))
			{
				AddAction();
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("");

			#endif
		}

		protected void DisplayEvent()
		{
			#if UNITY_EDITOR
			string prefix = "when";

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField("<i>" + prefix + "</i> <b>" + aiEvent.GetName().ToUpper() + "</b>", styles.labelStyle);

			DisplayEventOptionsMenu();

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("");
			aiEvent.DrawInspectorGUI();

			EditorGUILayout.EndVertical();

			EditorGUILayout.LabelField("");

			if(EditorGUI.EndChangeCheck())
			{
				if(!Application.isPlaying)
				{
					EditorUtility.SetDirty(aiEvent);
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}
			}
			#endif
		}

		protected void DisplayAction(int index)
		{
			#if UNITY_EDITOR
			string prefix = "";

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField("<i>" + prefix + "</i>", styles.labelStyle);

			DisplayActionOptionsMenu(index);

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginVertical(styles.actionStyle);

			actions[index].DrawInspectorGUI();

			EditorGUILayout.EndVertical();

			EditorGUILayout.LabelField("");
			EditorGUILayout.LabelField("");

			if(EditorGUI.EndChangeCheck())
			{
				if(!Application.isPlaying)
				{
					EditorUtility.SetDirty(actions[index]);
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}
			}
			#endif
		}

		#if UNITY_EDITOR
		protected void DisplayEventOptionsMenu()
		{
			if(GUILayout.Button("Event Options", EditorStyles.miniButton, GUILayout.Width(100))) 
			{
				GenericMenu menu = new GenericMenu();

				DisplayEventsInMenu(menu);

				menu.ShowAsContext();
			}
		}
		#endif

		#if UNITY_EDITOR
		protected void DisplayEventsInMenu(GenericMenu menu)
		{
			int eventType = (int)aiEvent.GetEventType();
			int totalEvents = System.Enum.GetValues(typeof(RexAIEvent.EventType)).Length;
			for(int i = 0; i < totalEvents; i ++)
			{
				if(eventType != i)
				{
					menu.AddItem(new GUIContent("Change Event Type/To: " + InspectorHelper.AddSpacesToString(((RexAIEvent.EventType)i).ToString())), false, ChangeEvent, (object)((int)i));
				}
			}
		}
		#endif

		protected void ChangeEvent(object parameters)
		{
			#if UNITY_EDITOR
			int newEventType = (int)parameters;

			GameObject eventGameObject = aiEvent.gameObject;

			RexAIEvent oldEvent = aiEvent;
			string oldEventName = oldEvent.GetEventType().ToString();

			bool hasGameObjectNameChanged = (oldEventName != gameObject.name);

			DestroyImmediate(oldEvent);

			aiEvent = InspectorHelper.AddEvent(newEventType, eventGameObject);

			if(!hasGameObjectNameChanged)
			{
				string eventName = ((RexAIEvent.EventType)newEventType).ToString();
				gameObject.name = eventName;
			}

			aiEvent.aiRoutine = aiRoutine;
			aiEvent.actionEventSet = this;

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif
		}

		protected void DisplayActionOptionsMenu(int index)
		{
			#if UNITY_EDITOR
			if(GUILayout.Button("Action Options", EditorStyles.miniButton, GUILayout.Width(100))) 
			{
				GenericMenu menu = new GenericMenu();

				if(index != 0)
				{
					menu.AddItem(new GUIContent("Shift (Up)"), false, ShiftItemUp, (object)((int)index));
				}

				if(index < actions.Count - 1 && actions.Count > 1)
				{
					menu.AddItem(new GUIContent("Shift (Down)"), false, ShiftItemDown, (object)((int)index));
				}

				InspectorHelper.DisplayActionsInMenu(menu, index, actions);

				if(actions.Count > 1)
				{
					menu.AddItem(new GUIContent("Remove Item"), false, RemoveItem, (object)((int)index));
				}

				menu.ShowAsContext();
			}
			#endif
		}

		protected void AddAction()
		{
			#if UNITY_EDITOR
			if(dataGameObject == null)
			{
				dataGameObject = InspectorHelper.DataGameObject(transform);
			}

			ToggleSequenceAction newAction = dataGameObject.AddComponent<ToggleSequenceAction>();
			newAction.SetAIRoutine(aiRoutine);

			actions.Add(newAction);

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif
		}

		protected void ShiftItemUp(object indexObject)
		{
			#if UNITY_EDITOR
			int index = (int)indexObject;

			if(index <= 0)
			{
				return;
			}

			RexAIAction action = actions[index];

			actions.RemoveAt(index);
			actions.Insert(index - 1, action);

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif
		}

		protected void ShiftItemDown(object indexObject)
		{
			#if UNITY_EDITOR
			int index = (int)indexObject;

			if(index > actions.Count - 1)
			{
				return;
			}

			RexAIAction action = actions[index];

			actions.RemoveAt(index);
			actions.Insert(index + 1, action);

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif
		}

		protected void RemoveItem(object indexObject)
		{
			#if UNITY_EDITOR
			int index = (int)indexObject;

			RexAIAction action = actions[index];

			DestroyImmediate(action);

			actions.RemoveAt(index);

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(RexAIActionEventSet))]
	public class RexAIActionEventSetEditor:Editor 
	{
		RexAIActionEventSet _target;

		public override void OnInspectorGUI()
		{
			if(_target == null)
			{
				_target = (RexAIActionEventSet)target;
			}

			EditorGUI.BeginChangeCheck();

			_target.DrawInspectorGUI();

			if(EditorGUI.EndChangeCheck())
			{
				if(!Application.isPlaying)
				{
					EditorUtility.SetDirty(_target);
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}
			}
		}
	}
	#endif
}
