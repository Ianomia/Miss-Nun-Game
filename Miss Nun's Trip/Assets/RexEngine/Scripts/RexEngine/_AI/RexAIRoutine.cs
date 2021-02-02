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
	public class RexAIRoutine:MonoBehaviour 
	{
		public bool isEnabled = true;
		public Slots slots;

		public RexAIMovement aiMovement;
		public RexAISequence aiSequence;
		public List<RexAIActionEventSet> aiEvents;

		protected RexAIMovement aiMovementActiveAtStart;
		protected RexAISequence aiSequenceActiveAtStart;
		protected List<RexAIActionEventSet> aiEventsActiveAtStart;

		[System.Serializable]
		public class Slots
		{
			public RexController controller;
		}

		void Awake() 
		{
			if(aiMovement)
			{
				aiMovement.aiRoutine = this;
			}
		}

		void Start() 
		{
			//The starting movement, sequence, and events are set here. These are saved and enabled again if this AIRoutine is re-enabled down the line.
			aiMovementActiveAtStart = aiMovement;
			aiSequenceActiveAtStart = aiSequence;
			aiEventsActiveAtStart = new List<RexAIActionEventSet>();
			for(int i = 0; i < aiEvents.Count; i ++)
			{
				if(aiEvents[i] != null && aiEvents[i].isEnabled)
				{
					aiEventsActiveAtStart.Add(aiEvents[i]);
				}
			}

			if(slots.controller && slots.controller.slots.actor != null && slots.controller.slots.actor.slots.aiRoutine == this)
			{
				Enable();
			}
			else
			{
				isEnabled = false;
			}
		}

		void FixedUpdate() 
		{
			if(!isEnabled || slots.controller.slots.actor.timeStop.isTimeStopped || !slots.controller.isEnabled)
			{
				return;
			}

			UpdateEvents();

			if(aiMovement == null)
			{
				return;
			}

			aiMovement.UpdateAI();
		}

		public void ChangeMovement(RexAIMovement newMovement)
		{
			if(aiMovement != null)
			{
				aiMovement.OnEnded();
			}

			aiMovement = newMovement;
			newMovement.aiRoutine = this;

			newMovement.OnBegin();
		}

		public void ChangeSequence(RexAISequence newSequence)
		{
			if(aiSequence != null)
			{
				aiSequence.Stop();
			}

			aiSequence = newSequence;

			newSequence.aiRoutine = this;
			newSequence.Begin();
		}

		public void StopMovement()
		{
			if(aiMovement != null)
			{
				aiMovement.OnEnded();
			}
		}

		public void NotifyOfSequenceEnded(RexAISequence sequence)
		{
			if(aiMovement != null)
			{
				aiMovement.NotifyOfSequenceEnd(sequence);
			}
		}

		public void NotifyOfActionEnded(RexAIAction action)
		{
			if(aiSequence)
			{
				aiSequence.NotifyOfActionEnded(action);
			}
		}

		public void Enable()
		{
			isEnabled = true;

			if(aiEventsActiveAtStart == null)
			{
				aiEventsActiveAtStart = new List<RexAIActionEventSet>();
			}

			if(aiMovementActiveAtStart != null)
			{
				ChangeMovement(aiMovementActiveAtStart); 
			}

			if(aiSequenceActiveAtStart != null)
			{
				ChangeSequence(aiSequenceActiveAtStart);
			}

			for(int i = 0; i < aiEvents.Count; i ++)
			{
				if(aiEvents[i] != null)
				{
					aiEvents[i].Disable();
				}
			}

			for(int i = 0; i < aiEventsActiveAtStart.Count; i ++)
			{
				if(aiEvents[i] != null)
				{
					aiEventsActiveAtStart[i].Enable();
				}
			}
		}

		public void Disable()
		{
			isEnabled = false;

			StopMovement();

			if(aiSequence != null)
			{
				aiSequence.Stop();
			}

			for(int i = 0; i < aiEvents.Count; i ++)
			{
				aiEvents[i].Disable();
			}
		}

		protected void UpdateEvents()
		{
			for(int i = 0; i < aiEvents.Count; i ++)
			{
				if(aiEvents[i] != null && aiEvents[i].isEnabled)
				{
					aiEvents[i].CheckEventStatus();
				}
			}
		}

		public void GetMovements()
		{
			RexAIMovement[] movements = transform.GetComponentsInChildren<RexAIMovement>();
			for(int i = 0; i < movements.Length; i ++)
			{
				RexAIMovement movement = movements[i];
				movement.aiRoutine = this;
			}
		}

		public void GetSequences()
		{
			RexAISequence[] sequences = transform.GetComponentsInChildren<RexAISequence>();
			for(int i = 0; i < sequences.Length; i ++)
			{
				RexAISequence sequence = sequences[i];
				sequence.aiRoutine = this;
			}
		}

		public void GetEvents()
		{
			RexAIActionEventSet[] events = transform.GetComponentsInChildren<RexAIActionEventSet>();
			for(int i = 0; i < events.Length; i ++)
			{
				RexAIActionEventSet aiEvent = events[i];
				aiEvent.aiRoutine = this;

				if(!aiEvents.Contains(aiEvent))
				{
					aiEvents.Add(aiEvent);
				}
			}
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(RexAIRoutine))]
	public class RexAIRoutineEditor:Editor 
	{
		protected InspectorHelper.AIInspectorStyles styles;
		protected bool showFieldAdvanced;

		public override void OnInspectorGUI()
		{
			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			EditorGUI.BeginChangeCheck();

			RexAIRoutine _target = (target as RexAIRoutine); 

			if(_target.aiEvents == null)
			{
				_target.aiEvents = new List<RexAIActionEventSet>();
			}

			for(int i = _target.aiEvents.Count - 1; i >= 0; i --)
			{
				if(_target.aiEvents[i] == null)
				{
					_target.aiEvents.RemoveAt(i);
				}
			}

			EditorGUILayout.TextField("MOVEMENTS", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical(styles.actionStyle);
			EditorGUILayout.BeginHorizontal();
			_target.aiMovement = (RexAIMovement)EditorGUILayout.ObjectField("Starting Movement", _target.aiMovement, typeof(RexAIMovement), true);
			EditorGUILayout.EndHorizontal();

			DisplayAddMovementMenu();

			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginHorizontal(); EditorGUILayout.EndHorizontal();

			EditorGUILayout.TextField("SEQUENCES", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical(styles.actionStyle);
			EditorGUILayout.BeginHorizontal();
			_target.aiSequence = (RexAISequence)EditorGUILayout.ObjectField("Starting Sequence", _target.aiSequence, typeof(RexAISequence), true);
			EditorGUILayout.EndHorizontal();

			if(GUILayout.Button("Add Sequence"))
			{
				AddSequence();
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginHorizontal(); EditorGUILayout.EndHorizontal();

			EditorGUILayout.TextField("EVENTS", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical(styles.actionStyle);
			EditorGUILayout.LabelField("Events", styles.labelStyle);

			for(int i = 0; i < _target.aiEvents.Count; i ++)
			{
				RexAIActionEventSet aiActionEventSet = _target.aiEvents[i];

				EditorGUILayout.BeginVertical(styles.actionStyle);

				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.BeginVertical(GUILayout.MaxWidth(20));
				aiActionEventSet.isEnabled = EditorGUILayout.Toggle(aiActionEventSet.isEnabled);
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField("<b>" + aiActionEventSet.aiEvent.GetName().ToUpper() + "</b>: " + aiActionEventSet.name, styles.labelStyle);
				EditorGUILayout.EndVertical();

				DisplayOptionsMenu(i);

				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}

			DisplayAddEventMenu();

			if(!Application.isPlaying && EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginHorizontal(); EditorGUILayout.EndHorizontal();
			EditorGUILayout.LabelField("NOTE: You should only add new Sequences, Movements, and Events via this menu. Sequences, Movements, and Events added via copying or via AddComponent may not work properly.", styles.boxStyle);

			if(_target.slots.controller == null)
			{
				FindController();
			}

			if(_target.slots.controller == null)
			{
				EditorGUILayout.LabelField("WARNING: This RexAIRoutine is missing a reference to a RexController. A RexController must be slotted before this can function properly.", styles.helpTextStyle);
			}

			showFieldAdvanced = EditorGUILayout.Foldout(showFieldAdvanced, "Advanced");
			if(showFieldAdvanced)
			{
				_target.slots.controller = EditorGUILayout.ObjectField("RexController", _target.slots.controller, typeof(RexController), true) as RexController;
			}

			_target.GetMovements();
			_target.GetSequences();
			_target.GetEvents();
		}

		protected void FindController()
		{
			RexAIRoutine _target = (target as RexAIRoutine); 

			RexActor actor = null;
			Transform parentTransform = _target.transform.parent;
			if(parentTransform != null)
			{
				actor = parentTransform.GetComponent<RexActor>();
				if(actor == null)
				{
					actor = parentTransform.transform.parent.GetComponent<RexActor>();
				}
			}

			if(actor != null)
			{
				_target.slots.controller = actor.slots.controller;
				if(_target.slots.controller)
				{
					Debug.Log("A RexAIRoutine was missing its RexController. It was found and automatically slotted.");
				}
			}
		}

		protected void DisplayOptionsMenu(int index)
		{
			RexAIRoutine _target = (target as RexAIRoutine); 

			if(GUILayout.Button("Options", EditorStyles.miniButton, GUILayout.Width(70))) 
			{
				GenericMenu menu = new GenericMenu();

				menu.AddItem(new GUIContent("Highlight Event"), false, HighlightEvent, (object)((int)index));

				if(_target.aiEvents.Count > 0)
				{
					menu.AddItem(new GUIContent("Remove Event"), false, RemoveEvent, (object)((int)index));
				}

				menu.ShowAsContext();
			}
		}

		protected void HighlightEvent(object indexObject)
		{
			RexAIRoutine _target = (target as RexAIRoutine); 

			int index = (int)indexObject;

			RexAIActionEventSet aiActionEventSet = _target.aiEvents[index];
			Selection.activeGameObject = aiActionEventSet.gameObject;
			EditorGUIUtility.PingObject(aiActionEventSet.gameObject);
		}

		protected void RemoveEvent(object indexObject)
		{
			RexAIRoutine _target = (target as RexAIRoutine); 

			int index = (int)indexObject;

			RexAIActionEventSet aiActionEventSet = _target.aiEvents[index];

			DestroyImmediate(aiActionEventSet.gameObject);

			_target.aiEvents.RemoveAt(index);

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

		protected void DisplayAddEventMenu()
		{
			if(GUILayout.Button("Add Event")) 
			{
				GenericMenu menu = new GenericMenu();

				int totalEvents = System.Enum.GetValues(typeof(RexAIEvent.EventType)).Length;
				for(int i = 0; i < totalEvents; i ++)
				{
					menu.AddItem(new GUIContent(InspectorHelper.AddSpacesToString(((RexAIEvent.EventType)i).ToString())), false, AddEvent, (object)((int)i));
				}

				menu.ShowAsContext();
			}
		}

		protected void DisplayAddMovementMenu()
		{
			if(GUILayout.Button("Add Movement")) 
			{
				GenericMenu menu = new GenericMenu();

				menu.AddItem(new GUIContent("Follow Target"), false, AddMovement, (object)((int)0));
				menu.AddItem(new GUIContent("Patrol"), false, AddMovement, (object)((int)1));
				menu.AddItem(new GUIContent("Swoop"), false, AddMovement, (object)((int)2));
				menu.AddItem(new GUIContent("Wait"), false, AddMovement, (object)((int)3));
				menu.AddItem(new GUIContent("Waypoints"), false, AddMovement, (object)((int)4));

				menu.ShowAsContext();
			}
		}

		protected void AddMovement(object indexObject)
		{
			RexAIRoutine _target = (target as RexAIRoutine); 

			Transform movementsTransform = _target.transform.Find("Movements");
			GameObject movementsParentObject = null;
			if(movementsTransform != null)
			{
				movementsParentObject = movementsTransform.gameObject;
			}

			if(movementsParentObject == null)
			{
				movementsParentObject = new GameObject();
				movementsParentObject.name = "Movements";
				movementsParentObject.transform.parent = _target.gameObject.transform;
				movementsParentObject.transform.localScale = Vector3.one;
				movementsParentObject.transform.localPosition = Vector3.zero;
			}

			GameObject aiMovementGameObject = new GameObject();
			aiMovementGameObject.name = "Movement";
			aiMovementGameObject.transform.parent = movementsParentObject.transform;
			aiMovementGameObject.transform.localScale = Vector3.one;
			aiMovementGameObject.transform.localPosition = Vector3.zero;

			int index = (int)indexObject;
			RexAIMovement aiMovement = null;

			switch(index)
			{
				case 0:
					aiMovement = aiMovementGameObject.AddComponent<FollowTargetMovement>();
					aiMovementGameObject.name = "FollowTarget";
					break;
				case 1:
					aiMovement = aiMovementGameObject.AddComponent<PatrolMovement>();
					aiMovementGameObject.name = "Patrol";
					break;
				case 2:
					aiMovement = aiMovementGameObject.AddComponent<SwoopMovement>();
					aiMovementGameObject.name = "Swoop";
					break;
				case 3:
					aiMovement = aiMovementGameObject.AddComponent<WaitMovement>();
					aiMovementGameObject.name = "Wait";
					break;
				case 4:
					aiMovement = aiMovementGameObject.AddComponent<WaypointsMovement>();
					aiMovementGameObject.name = "Waypoints";
					break;
				default:
					aiMovement = aiMovementGameObject.AddComponent<FollowTargetMovement>();
					aiMovementGameObject.name = "FollowTarget";
					break;
			}

			aiMovement.aiRoutine = _target;

			if(_target.aiMovement == null)
			{
				_target.aiMovement = aiMovement;
			}

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				EditorGUIUtility.PingObject(aiMovementGameObject);
			}			
		}

		protected void AddSequence()
		{
			RexAIRoutine _target = (target as RexAIRoutine); 

			Transform sequencesTransform = _target.transform.Find("Sequences");
			GameObject sequencesParentObject = null;
			if(sequencesTransform != null)
			{
				sequencesParentObject = sequencesTransform.gameObject;
			}

			if(sequencesParentObject == null)
			{
				sequencesParentObject = new GameObject();
				sequencesParentObject.name = "Sequences";
				sequencesParentObject.transform.parent = _target.gameObject.transform;
				sequencesParentObject.transform.localScale = Vector3.one;
				sequencesParentObject.transform.localPosition = Vector3.zero;
			}

			GameObject aiSequenceGameObject = new GameObject();
			aiSequenceGameObject.name = "Sequence";
			aiSequenceGameObject.transform.parent = sequencesParentObject.transform;
			aiSequenceGameObject.transform.localScale = Vector3.one;
			aiSequenceGameObject.transform.localPosition = Vector3.zero;

			RexAISequence rexAIConditionalSequence = aiSequenceGameObject.AddComponent<RexAISequence>();
			if(_target.aiSequence == null)
			{
				_target.aiSequence = rexAIConditionalSequence;
			}

			rexAIConditionalSequence.aiRoutine = _target;

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				EditorGUIUtility.PingObject(aiSequenceGameObject);
			}
		}

		protected void AddEvent(object indexObject)
		{
			RexAIRoutine _target = (target as RexAIRoutine); 

			Transform eventsTransform = _target.transform.Find("Events");
			GameObject eventsParentObject = null;
			if(eventsTransform != null)
			{
				eventsParentObject = eventsTransform.gameObject;
			}

			if(eventsParentObject == null)
			{
				eventsParentObject = new GameObject();
				eventsParentObject.name = "Events";
				eventsParentObject.transform.parent = _target.gameObject.transform;
				eventsParentObject.transform.localScale = Vector3.one;
				eventsParentObject.transform.localPosition = Vector3.zero;
			}

			GameObject aiEventGameObject = new GameObject();
			aiEventGameObject.transform.parent = eventsParentObject.transform;
			aiEventGameObject.transform.localScale = Vector3.one;
			aiEventGameObject.transform.localPosition = Vector3.zero;

			RexAIActionEventSet newActionEventSet = aiEventGameObject.AddComponent<RexAIActionEventSet>();
			newActionEventSet.aiRoutine = _target;

			GameObject dataGameObject = InspectorHelper.DataGameObject(aiEventGameObject.transform);

			int index = (int)indexObject;

			newActionEventSet.aiEvent = InspectorHelper.AddEvent(index, dataGameObject);

			newActionEventSet.aiEvent.isEnabled = true;
			newActionEventSet.aiEvent.aiRoutine = _target;
			newActionEventSet.aiEvent.actionEventSet = newActionEventSet;

			newActionEventSet.isEnabled = true;
			_target.aiEvents.Add(newActionEventSet);

			aiEventGameObject.name = ((RexAIEvent.EventType)index).ToString();

			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				EditorGUIUtility.PingObject(aiEventGameObject);
			}
		}
	}
	#endif
}
