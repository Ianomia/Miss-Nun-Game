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
	public class RexAISequence:MonoBehaviour 
	{
		public List<RexAIActionConditionalSet> actions;
		public RexAIRoutine aiRoutine;
		public Loop loop;
		public bool isPlaying;
		public int numberOfLoops;
		public int numberOfLoopsLow;
		public int numberOfLoopsHigh;
		public bool randomizeLoops;
		public bool playSequenceOnAllLoopsComplete;
		public RexAISequence loopCompleteSequence;
		public GameObject dataGameObject;

		public enum Loop
		{
			None,
			Forever,
			Limited
		}

		protected RexAIAction currentAction;
		protected bool hasCurrentActionEnded = false;
		protected int actionIndex = 0;
		protected int currentLoop;

		void Awake() 
		{
			for(int i = 0; i < actions.Count; i ++)
			{
				for(int j = 0; j < actions[i].actions.Count; j ++)
				{
					actions[i].actions[j].action.SetAIRoutine(aiRoutine);
				}
			}
		}

		public void PlayCurrentAction()
		{
			currentAction = actions[actionIndex].DetermineAction();
			currentAction.SetAIRoutine(aiRoutine);
			currentAction.Begin();
		}

		public void Begin()
		{
			if(aiRoutine != null)
			{
				aiRoutine.aiSequence = this;
			}

			isPlaying = true;

			actionIndex = 0;
			currentLoop = 0;

			if(randomizeLoops)
			{
				numberOfLoops = RexMath.RandomInt(numberOfLoopsLow, numberOfLoopsHigh);
			}

			PlayCurrentAction();
		}

		public void Stop()
		{
			isPlaying = false;
			actionIndex = 0;

			if(currentAction != null)
			{
				currentAction.End();
			}
		}

		public void NotifyOfActionEnded(RexAIAction action)
		{
			if(isPlaying)
			{
				if(action == currentAction)
				{
					actionIndex ++;
					if(actionIndex < actions.Count)
					{
						PlayCurrentAction();
					}
					else
					{
						if(loop == Loop.None || (loop == Loop.Limited && currentLoop >= numberOfLoops - 1))
						{
							aiRoutine.NotifyOfSequenceEnded(this);
						}

						if(loop == Loop.Forever || (loop == Loop.Limited && currentLoop < numberOfLoops - 1))
						{
							if(loop == Loop.Limited)
							{
								currentLoop ++;
							}

							actionIndex = 0;
							PlayCurrentAction();
						}
						else if(loop == Loop.Limited && currentLoop >= numberOfLoops - 1)
						{
							if(playSequenceOnAllLoopsComplete && loopCompleteSequence && aiRoutine)
							{
								aiRoutine.ChangeSequence(loopCompleteSequence);
							}
						}
					}
				}
			}
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(RexAISequence))]
	public class RexAISequenceEditor:Editor 
	{
		protected RexAISequence _target;
		public InspectorHelper.AIInspectorStyles styles;

		public override void OnInspectorGUI()
		{
			_target = (RexAISequence)target;

			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			EditorGUI.BeginChangeCheck();

			if(_target.actions == null)
			{
				_target.actions = new List<RexAIActionConditionalSet>();
			}

			if(_target.actions.Count <= 0)
			{
				AddAction(null);
			}

			for(int i = _target.actions.Count - 1; i >= 0; i --)
			{
				if(_target.actions[i] == null)
				{
					_target.actions.RemoveAt(i);
				}
			}
				
			_target.loop = (RexAISequence.Loop)EditorGUILayout.EnumPopup("Loop Type", _target.loop);
			if(_target.loop == RexAISequence.Loop.Limited)
			{
				_target.randomizeLoops = EditorGUILayout.Toggle("Randomze Number of Loops", _target.randomizeLoops);
				if(!_target.randomizeLoops)
				{
					_target.numberOfLoops = EditorGUILayout.IntField("Number of Loops", _target.numberOfLoops);
					if(_target.numberOfLoops < 2)
					{
						_target.numberOfLoops = 2;
					}
				}
				else
				{
					_target.numberOfLoopsLow = EditorGUILayout.IntField("Number of Loops (Lowest)", _target.numberOfLoopsLow);
					if(_target.numberOfLoopsLow < 2)
					{
						_target.numberOfLoopsLow = 2;
					}

					_target.numberOfLoopsHigh = EditorGUILayout.IntField("Number of Loops (Highest)", _target.numberOfLoopsHigh);
					if(_target.numberOfLoopsHigh < 2)
					{
						_target.numberOfLoopsHigh = 2;
					}
				}
			}

			for(int i = 0; i < _target.actions.Count; i ++)
			{
				RexAIActionConditionalSet action = _target.actions[i];

				string suffix = (action.actions.Count > 1) ? "Conditional" : "Action";
				int numberOfBranches = action.actions.Count;
				string conditionalSuffix = (numberOfBranches > 1) ? " branches" : " branch";
				string actionName = (action.actions.Count == 1) ? ": " + action.actions[0].action.GetName() : " (" + numberOfBranches + conditionalSuffix +")";
				string foldoutName = (i + 1) + ": " + suffix + actionName;
				action.isFoldoutActive = EditorGUILayout.Foldout(action.isFoldoutActive, foldoutName);
				if(action.isFoldoutActive)
				{
					EditorGUILayout.BeginVertical(styles.eventBoxStyle);

					action.DrawInspectorGUI();

					EditorGUILayout.EndVertical();

					if(_target.actions.Count > 0)
					{
						DisplayOptionsMenu(i);
					}

					if(i < _target.actions.Count - 1 && _target.actions.Count > 1)
					{
						for(int j = 0; j < action.actions.Count; j ++)
						{
							RexAIAction.ActionType actionType = action.actions[j].action.GetActionType();
							if(actionType == RexAIAction.ActionType.ChangeAIRoutine || actionType == RexAIAction.ActionType.ToggleSequence)
							{
								EditorGUILayout.BeginVertical();
								EditorGUILayout.LabelField("WARNING: No Rex AI Actions below this point will execute.", styles.helpTextStyle);
								EditorGUILayout.EndVertical();
							}
						}
					}

					EditorGUILayout.LabelField("");
				}
			}

			InspectorHelper.DrawLine();

			EditorGUILayout.LabelField("");

			DisplayAddButton();

			EditorGUILayout.LabelField("");

			if(_target.loop == RexAISequence.Loop.Limited)
			{
				InspectorHelper.DrawLine();

				EditorGUILayout.LabelField("");

				_target.playSequenceOnAllLoopsComplete = EditorGUILayout.Toggle("Play New Sequence When Final Loop Completes", _target.playSequenceOnAllLoopsComplete);

				if(_target.playSequenceOnAllLoopsComplete)
				{
					_target.loopCompleteSequence = (RexAISequence)EditorGUILayout.ObjectField("Sequence", _target.loopCompleteSequence, typeof(RexAISequence), true);
				}

				EditorGUILayout.LabelField("");
			}

			if(EditorGUI.EndChangeCheck())
			{
				if(!Application.isPlaying)
				{
					EditorUtility.SetDirty(_target);
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}
			}
		}

		protected void DisplayAddButton()
		{
			if(GUILayout.Button("Add Action"))
			{
				GenericMenu menu = new GenericMenu();

				int totalActions = System.Enum.GetValues(typeof(RexAIAction.ActionType)).Length - 1;

				for(int i = 0; i <= totalActions; i ++)
				{
					Hashtable hashtable = new Hashtable();
					hashtable[0] = i;
					hashtable[1] = 0;
					hashtable[2] = 0;

					menu.AddItem(new GUIContent(InspectorHelper.AddSpacesToString(((RexAIAction.ActionType)i).ToString())), false, AddAction, (object)(hashtable));
				}

				menu.ShowAsContext();
			}
		}

		protected void AddAction(object parameters)
		{
			Hashtable hashtable = (Hashtable)parameters;

			GameObject actionGameObject = GetDataGameObject();

			RexAIActionConditionalSet set = actionGameObject.AddComponent<RexAIActionConditionalSet>();

			int index = (hashtable != null) ? (int)hashtable[0] : 0;
			set.Init(_target.aiRoutine, index);

			int insertionIndex = (hashtable != null) ? (int)hashtable[1] : 0;
			int currentActionIndex = (hashtable != null) ? (int)hashtable[2] : 0;

			if(insertionIndex == 0) //Insert at the end of the Actions list
			{
				_target.actions.Add(set);
			}
			else //Insert above or below
			{
				int newIndex = (insertionIndex == 1) ? currentActionIndex + insertionIndex : currentActionIndex;
				if(newIndex < 0)
				{
					newIndex = 0;
				}

				_target.actions.Insert(newIndex, set);
			}

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(_target);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif
		}

		protected void DisplayOptionsMenu(int index)
		{
			if(GUILayout.Button("Item Options", EditorStyles.miniButton)) 
			{
				GenericMenu menu = new GenericMenu();

				bool isFinalItem = (index == _target.actions.Count - 1 && _target.actions.Count >= 2);

				if(index != 0 && _target.actions.Count > 1)
				{
					menu.AddItem(new GUIContent("Shift (Up)"), false, ShiftItemUp, (object)((int)index));
				}

				if(!isFinalItem && _target.actions.Count > 1)
				{
					menu.AddItem(new GUIContent("Shift (Down)"), false, ShiftItemDown, (object)((int)index));
				}

				int totalActions = System.Enum.GetValues(typeof(RexAIAction.ActionType)).Length - 1;
				for(int i = 0; i <= totalActions; i ++)
				{
					Hashtable hashtable = new Hashtable();
					hashtable[0] = i;
					hashtable[1] = -1;
					hashtable[2] = index;

					menu.AddItem(new GUIContent("Add Action (Above)/" + InspectorHelper.AddSpacesToString(((RexAIAction.ActionType)i).ToString())), false, AddAction, (object)(hashtable));
				}

				for(int i = 0; i <= totalActions; i ++)
				{
					Hashtable hashtable = new Hashtable();
					hashtable[0] = i;
					hashtable[1] = 1;
					hashtable[2] = index;

					menu.AddItem(new GUIContent("Add Action (Below)/" + InspectorHelper.AddSpacesToString(((RexAIAction.ActionType)i).ToString())), false, AddAction, (object)(hashtable));
				}


				if(_target.actions.Count > 1)
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

			RexAIActionConditionalSet actionConditionalSet = _target.actions[index];

			#if UNITY_EDITOR
			Undo.RegisterFullObjectHierarchyUndo(_target.gameObject, "Shift Rex AI Action Up");
			#endif

			_target.actions.RemoveAt(index);
			_target.actions.Insert(index - 1, actionConditionalSet);

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(_target);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif
		}

		protected void ShiftItemDown(object indexObject)
		{
			int index = (int)indexObject;

			if(index > _target.actions.Count - 1)
			{
				return;
			}

			RexAIActionConditionalSet actionConditionalSet = _target.actions[index];

			#if UNITY_EDITOR
			Undo.RegisterFullObjectHierarchyUndo(_target.gameObject, "Shift Rex AI Action Down");
			#endif

			_target.actions.RemoveAt(index);
			_target.actions.Insert(index + 1, actionConditionalSet);

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(_target);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif
		}

		protected void AddActionAbove(object indexObject)
		{
			int index = (int)indexObject;
			index -= 1;

			if(index < 0)
			{
				index = 0;
			}

			RexAIActionConditionalSet actionConditionalSet = _target.actions[index];

			#if UNITY_EDITOR
			Undo.RegisterFullObjectHierarchyUndo(_target.gameObject, "Add Action (Above)");
			#endif

			GameObject actionGameObject = GetDataGameObject();

			RexAIActionConditionalSet set = actionGameObject.AddComponent<RexAIActionConditionalSet>();
			set.Init(_target.aiRoutine, 0);

			_target.actions.Insert(index, set);

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(_target);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif
		}

		protected void AddActionBelow(object indexObject)
		{
			int index = (int)indexObject;
			index += 1;

			RexAIActionConditionalSet actionConditionalSet = _target.actions[index];

			#if UNITY_EDITOR
			Undo.RegisterFullObjectHierarchyUndo(_target.gameObject, "Add Action (Below)");
			#endif

			GameObject actionGameObject = GetDataGameObject();

			RexAIActionConditionalSet set = actionGameObject.AddComponent<RexAIActionConditionalSet>();
			set.Init(_target.aiRoutine, 0);

			_target.actions.Insert(index, set);

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(_target);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif
		}

		protected void RemoveItem(object indexObject)
		{
			int index = (int)indexObject;

			RexAIActionConditionalSet actionConditionalSet = _target.actions[index];

			_target.actions.RemoveAt(index);

			for(int i = actionConditionalSet.actions.Count - 1; i >= 0; i --)
			{
				DestroyImmediate(actionConditionalSet.actions[i].action);
				DestroyImmediate(actionConditionalSet.actions[i].branch);
			}

			DestroyImmediate(actionConditionalSet);
			//DestroyImmediate(actionConditionalSet.gameObject);

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(_target);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif
		}

		protected GameObject GetDataGameObject()
		{
			if(_target.dataGameObject == null)
			{
				_target.dataGameObject = new GameObject();
				_target.dataGameObject.name = "Data";
				_target.dataGameObject.transform.parent = _target.transform;
				_target.dataGameObject.transform.localPosition = Vector3.zero;
				_target.dataGameObject.transform.localScale = Vector3.one;
			}

			return _target.dataGameObject;
		}
	}
	#endif
}
