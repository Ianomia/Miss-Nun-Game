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
	public class RexAIActionConditionalSet:MonoBehaviour 
	{
		public RexAIRoutine aiRoutine;

		public List<Pair> actions = new List<Pair>();

		[HideInInspector]
		public bool isFoldoutActive = true;

		#if UNITY_EDITOR
		public InspectorHelper.AIInspectorStyles styles;
		#endif

		[System.Serializable]
		public class Pair
		{
			public RexAIBranch branch;
			public RexAIAction action;
		}

		public RexAIAction DetermineAction()
		{
			RexAIAction action = null;
			for(int i = 0; i < actions.Count; i ++)
			{
				if(actions[i].branch.Determine(aiRoutine))
				{
					action = actions[i].action;
					break;
				}
			}

			return action;
		}

		public void Init(RexAIRoutine _routine, int actionIndex)
		{
			aiRoutine = _routine;
			AddElseToEnd(actionIndex);
		}

		public void DrawInspectorGUI()
		{
			#if UNITY_EDITOR
			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			CleanupEmptyPairs();

			for(int i = 0; i < actions.Count; i ++)
			{
				actions[i].action.SetAIRoutine(aiRoutine);
				DisplayPair(i);
			}

			EditorGUILayout.BeginHorizontal();

			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Add Branch", styles.buttonStyle, GUILayout.Width(86)))
			{
				AddPair();
			}
			GUILayout.FlexibleSpace();

			EditorGUILayout.EndHorizontal();
			#endif
		}

		public void DisplayPair(int index)
		{
			#if UNITY_EDITOR
			EditorGUI.BeginChangeCheck();

			int margin = (actions.Count > 1) ? 40 : 0;
			styles.actionStyle.margin = new RectOffset(margin, 0, 0, 0);

			RexAIActionConditionalSet.Pair pair = actions[index] as RexAIActionConditionalSet.Pair;

			string prefix = (index == 0) ? "if " : "else if ";

			string branchName = (index == actions.Count - 1) ? pair.branch.ID().ToLower() : pair.branch.ID().ToUpper();
			if(index == actions.Count - 1 && branchName == "else")
			{
				prefix = (index == 0) ? "" : "else";
				branchName = "";
			}

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField("<i>" + prefix + "</i> <b>" + branchName + "</b>", styles.labelStyle);

			DisplayOptionsMenu(index);

			EditorGUILayout.EndHorizontal();

			if(index < actions.Count - 1)
			{
				EditorGUILayout.BeginVertical(styles.branchStyle);
			}

			pair.branch.DrawInspectorGUI();

			if(index < actions.Count - 1)
			{
				EditorGUILayout.EndVertical();
			}

			EditorGUILayout.BeginVertical(styles.actionStyle);

			pair.action.DrawInspectorGUI();

			EditorGUILayout.EndVertical();

			EditorGUILayout.LabelField("");

			if(!Application.isPlaying && EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(pair.action);
				EditorUtility.SetDirty(pair.branch);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
			#endif
		}

		protected void DisplayOptionsMenu(int index)
		{
			#if UNITY_EDITOR
			bool isFinalItem = (index == actions.Count - 1 && actions.Count >= 2);
			bool useBranches = (actions.Count > 1 && !isFinalItem);

			string buttonText = (useBranches) ? "Edit Branch/Action" : "Edit Action";

			EditorGUI.BeginChangeCheck();

			if(GUILayout.Button(buttonText, EditorStyles.miniButton, GUILayout.Width(100))) 
			{
				GenericMenu menu = new GenericMenu();

				if(index != 0 && !isFinalItem)
				{
					menu.AddItem(new GUIContent("Shift (Up)"), false, ShiftItemUp, (object)((int)index));
				}

				if(index < actions.Count - 2 && !isFinalItem)
				{
					menu.AddItem(new GUIContent("Shift (Down)"), false, ShiftItemDown, (object)((int)index));
				}

				if(actions.Count > 1 && !isFinalItem)
				{
					DisplayBranchesInMenu(menu, index);
				}

				InspectorHelper.DisplayActionsInMenu(menu, index, null, actions);

				if(actions.Count > 1 && !isFinalItem)
				{
					menu.AddItem(new GUIContent("Remove Item"), false, RemoveItem, (object)((int)index));
				}

				menu.ShowAsContext();
			}

			if(!Application.isPlaying && EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());			
			}
			#endif
		}

		#if UNITY_EDITOR
		protected void DisplayBranchesInMenu(GenericMenu menu, int index)
		{
			int branchType = (int)actions[index].branch.GetBranchType();

			if(branchType != 0) menu.AddItem(new GUIContent("Change Branch Type/To: Distance"), false, ChangeBranch, (object)(new Vector2(index, 0)));
			if(branchType != 1) menu.AddItem(new GUIContent("Change Branch Type/To: Facing Direction"), false, ChangeBranch, (object)(new Vector2(index, 1)));
			if(branchType != 2) menu.AddItem(new GUIContent("Change Branch Type/To: Facing Target"), false, ChangeBranch, (object)(new Vector2(index, 2)));
			if(branchType != 3) menu.AddItem(new GUIContent("Change Branch Type/To: HP"), false, ChangeBranch, (object)(new Vector2(index, 3)));
			if(branchType != 4) menu.AddItem(new GUIContent("Change Branch Type/To: Physics"), false, ChangeBranch, (object)(new Vector2(index, 4)));
			if(branchType != 5) menu.AddItem(new GUIContent("Change Branch Type/To: Random"), false, ChangeBranch, (object)(new Vector2(index, 5)));
			if(branchType != 6) menu.AddItem(new GUIContent("Change Branch Type/To: Target Facing"), false, ChangeBranch, (object)(new Vector2(index, 6)));
		}
		#endif

		protected void ChangeBranch(object parameters)
		{
			Vector2 info = (Vector2)parameters;

			int indexOfItemToChange = (int)info.x;
			int newBranchType = (int)info.y;

			GameObject branchGameObject = actions[indexOfItemToChange].branch.gameObject;

			RexAIBranch oldBranch = actions[indexOfItemToChange].branch;
			DestroyImmediate(oldBranch);

			RexAIBranch newBranch;
			switch(newBranchType)
			{
				case 0:
					newBranch = branchGameObject.AddComponent<DistanceBranch>();
					break;
				case 1:
					newBranch = branchGameObject.AddComponent<FacingDirectionBranch>();
					break;
				case 2:
					newBranch = branchGameObject.AddComponent<FacingTargetBranch>();
					break;
				case 3:
					newBranch = branchGameObject.AddComponent<HPBranch>();
					break;
				case 4:
					newBranch = branchGameObject.AddComponent<PhysicsBranch>();
					break;
				case 5:
					newBranch = branchGameObject.AddComponent<RandomBranch>();
					break;
				case 6:
					newBranch = branchGameObject.AddComponent<TargetFacingBranch>();
					break;
				default:
					newBranch = branchGameObject.AddComponent<RandomBranch>();
					break;
			}

			actions[indexOfItemToChange].branch = newBranch;

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif
		}

		protected void ShiftItemUp(object indexObject)
		{
			int index = (int)indexObject;

			if(index <= 0)
			{
				return;
			}

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				Undo.RegisterFullObjectHierarchyUndo(gameObject, "Shift Rex AI Action Up");
			}
			#endif

			RexAIActionConditionalSet.Pair pair = actions[index];

			actions.RemoveAt(index);
			actions.Insert(index - 1, pair);

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif
		}

		protected void ShiftItemDown(object indexObject)
		{
			int index = (int)indexObject;

			if(index > actions.Count - 1)
			{
				return;
			}

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				Undo.RegisterFullObjectHierarchyUndo(gameObject, "Shift Rex AI Action Down");
			}
			#endif

			RexAIActionConditionalSet.Pair pair = actions[index];

			actions.RemoveAt(index);
			actions.Insert(index + 1, pair);

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif		
		}

		protected void RemoveItem(object indexObject)
		{
			int index = (int)indexObject;

			RexAIActionConditionalSet.Pair pair = actions[index];

			if(pair.action)
			{
				DestroyImmediate(pair.action);
			}

			if(pair.branch)
			{
				DestroyImmediate(pair.branch);
			}

			actions.RemoveAt(index);

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
			#endif		
		}

		protected void AddPair()
		{
			RexAIActionConditionalSet.Pair elseSet = actions[actions.Count - 1];
			actions.Remove(elseSet);

			RandomBranch newBranch = gameObject.AddComponent<RandomBranch>();
			WaitAction newAction = gameObject.AddComponent<WaitAction>();

			RexAIActionConditionalSet.Pair newSet = new RexAIActionConditionalSet.Pair();
			actions.Add(newSet);

			newSet.branch = newBranch;
			newSet.action = newAction;

			actions.Add(elseSet);

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				EditorUtility.SetDirty(this);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
			#endif
		}

		protected void CleanupEmptyPairs()
		{
			if(actions.Count < 1)
			{
				AddItemToStart();
			}

			for(int i = actions.Count - 1; i >= 0; i --)
			{
				RexAIActionConditionalSet.Pair pair = actions[i] as RexAIActionConditionalSet.Pair;
				if(pair == null || pair.action == null || pair.branch == null)
				{
					bool isFinalItem = (i == actions.Count - 1 && actions.Count >= 2);
					bool isFirstItem = (i == 0);
					if(isFinalItem)
					{
						//Add a new Else branch here
						actions.RemoveAt(i);
						if(pair.action != null)
						{
							DestroyImmediate(pair.action);
						}

						if(pair.branch != null)
						{
							DestroyImmediate(pair.branch);
						}

						AddElseToEnd();
					}
					else if(isFirstItem && actions.Count == 0)
					{
						AddItemToStart();
					}
					else
					{
						if(pair.action != null)
						{
							DestroyImmediate(pair.action);
						}
						else if(pair.branch != null)
						{
							DestroyImmediate(pair.branch);
						}

						actions.RemoveAt(i);
					}
				}
			}
		}

		protected void AddItemToStart()
		{
			RandomBranch newBranch = gameObject.AddComponent<RandomBranch>();
			WaitAction newAction = gameObject.AddComponent<WaitAction>();

			RexAIActionConditionalSet.Pair newSet = new RexAIActionConditionalSet.Pair();
			actions.Insert(0, newSet);

			newSet.branch = newBranch;
			newSet.action = newAction;
		}

		protected void AddElseToEnd(int actionIndex = 0)
		{
			#if UNITY_EDITOR
			ElseBranch newBranch = gameObject.AddComponent<ElseBranch>();
			RexAIAction newAction = InspectorHelper.AddActionToGameObject(actionIndex, gameObject);

			RexAIActionConditionalSet.Pair newSet = new RexAIActionConditionalSet.Pair();
			actions.Add(newSet);

			newSet.branch = newBranch;
			newSet.action = newAction;
			#endif
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(RexAIActionConditionalSet))]
	public class RexAIActionConditionalSetEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}
