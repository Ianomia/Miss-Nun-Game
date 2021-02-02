using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace RexEngine
{
	#if UNITY_EDITOR
	public class InspectorHelper:MonoBehaviour 
	{
		public class AIInspectorStyles
		{
			public GUIStyle labelStyle;
			public GUIStyle branchStyle;
			public GUIStyle actionStyle;
			public GUIStyle boxStyle;
			public GUIStyle buttonStyle;
			public GUIStyle outerBoxStyle;
			public GUIStyle helpTextStyle;
			public GUIStyle eventBoxStyle;
		}

		public static void DrawLine()
		{
			GUIStyle lineStyle = new GUIStyle("box");
			lineStyle.border.top = 1;
			lineStyle.border.bottom = 1;
			lineStyle.margin.top = 1;
			lineStyle.margin.bottom = 1;
			lineStyle.padding.top = 1;
			lineStyle.padding.bottom = 1;

			GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1.0f));
		}

		public static AIInspectorStyles SetStyles()
		{
			AIInspectorStyles styles = new InspectorHelper.AIInspectorStyles();

			styles.labelStyle = new GUIStyle(GUI.skin.label);
			styles.labelStyle.richText = true;

			styles.branchStyle = new GUIStyle("box");
			styles.branchStyle.margin = new RectOffset(0, 0, 0, 10);

			styles.actionStyle = new GUIStyle("box");

			styles.boxStyle = new GUIStyle(EditorStyles.helpBox);

			styles.buttonStyle = new GUIStyle(EditorStyles.miniButton);
			styles.buttonStyle.alignment = TextAnchor.MiddleCenter;

			styles.outerBoxStyle = new GUIStyle();
			styles.outerBoxStyle.padding = new RectOffset(20, 20, 20, 20);

			styles.helpTextStyle = new GUIStyle(EditorStyles.helpBox);
			styles.helpTextStyle.fontSize = 10;
			styles.helpTextStyle.wordWrap = true;
			styles.helpTextStyle.normal.textColor = new Color(1.0f, 0.5f, 0.5f);

			styles.eventBoxStyle = new GUIStyle("box");
			styles.eventBoxStyle.padding = new RectOffset(20, 20, 20, 20);

			return styles;
		}

		public static GameObject DataGameObject(Transform _transform)
		{
			GameObject dataGameObject;
			Transform dataTransform = _transform.Find("Data");
			if(dataTransform != null)
			{
				dataGameObject = _transform.Find("Data").gameObject;
			}
			else
			{
				dataGameObject = new GameObject();
				dataGameObject.name = "Data";
				dataGameObject.transform.parent = _transform;
				dataGameObject.transform.localPosition = Vector3.zero;
				dataGameObject.transform.localScale = Vector3.one;
			}

			return dataGameObject;
		}

		#if UNITY_EDITOR
		public static void DisplayActionsInMenu(GenericMenu menu, int index, List<RexAIAction> actions = null, List<RexAIActionConditionalSet.Pair> pairs = null)
		{
			bool usesConditionals = (pairs != null);

			int actionType = (usesConditionals) ? (int)pairs[index].action.GetActionType() : (int)actions[index].GetActionType();
			int totalActions = System.Enum.GetValues(typeof(RexAIAction.ActionType)).Length - 1;

			for(int i = 0; i <= totalActions; i ++)
			{
				Hashtable hashtable = new Hashtable();
				hashtable[0] = index;
				hashtable[1] = i;
				hashtable[2] = actions;
				hashtable[3] = pairs;

				bool willHideAction = (!usesConditionals && i == 0); //Hides the Wait action from event-based actions

				if(actionType != i && !willHideAction) menu.AddItem(new GUIContent("Change Action Type/To: " + AddSpacesToString(((RexAIAction.ActionType)i).ToString())), false, ChangeAction, (object)(hashtable));
			}
		}

		public static RexAIEvent AddEvent(int index, GameObject eventGameObject)
		{
			RexAIEvent aiEvent;
			switch(index)
			{
				case 0:
					aiEvent = eventGameObject.AddComponent<DamagedEvent>();
					break;
				case 1:
					aiEvent = eventGameObject.AddComponent<DistanceEvent>();
					break;
				case 2:
					aiEvent = eventGameObject.AddComponent<PhysicsEvent>();
					break;
				case 3:
					aiEvent = eventGameObject.AddComponent<TargetFacingEvent>();
					break;
				case 4:
					aiEvent = eventGameObject.AddComponent<TimerEvent>();
					break;
				default:
					aiEvent = eventGameObject.AddComponent<DamagedEvent>();
					break;
			}

			return aiEvent;
		}

		public static string AddSpacesToString(string text)
		{
			StringBuilder textWithSpaces = new StringBuilder(text.Length * 2);
			textWithSpaces.Append(text[0]);
			for(int i = 1; i < text.Length; i++)
			{
				//RexAIController
				if(char.IsUpper(text[i]) && text[i - 1] != ' ' && !(char.IsUpper(text[i - 1]) && char.IsUpper(text[i + 1])))
				{
					textWithSpaces.Append(' ');
				}

				textWithSpaces.Append(text[i]);
			}

			return textWithSpaces.ToString();
		}
		#endif

		public static RexAIAction AddActionToGameObject(int newActionType, GameObject actionGameObject)
		{
			RexAIAction newAction;
			switch(newActionType)
			{
				case 0:
					newAction = actionGameObject.AddComponent<WaitAction>();
					break;
				case 1:
					newAction = actionGameObject.AddComponent<AbilityAction>();
					break;
				case 2:
					newAction = actionGameObject.AddComponent<AttackAction>();
					break;
				case 3:
					newAction = actionGameObject.AddComponent<ChangeMovementAction>();
					break;
				case 4:
					newAction = actionGameObject.AddComponent<ToggleSequenceAction>();
					break;
				case 5:
					newAction = actionGameObject.AddComponent<ToggleEventAction>();
					break;
				case 6:
					newAction = actionGameObject.AddComponent<FaceTargetAction>();
					break;
				case 7:
					newAction = actionGameObject.AddComponent<FaceDirectionAction>();
					break;
				case 8:
					newAction = actionGameObject.AddComponent<ChangeAIRoutineAction>();
					break;
				case 9:
					newAction = actionGameObject.AddComponent<ChangePhysicsAction>();
					break;
				case 10:
					newAction = actionGameObject.AddComponent<PlayAnimationAction>();
					break;
				case 11:
					newAction = actionGameObject.AddComponent<CallFunctionAction>();
					break;
				case 12:
					newAction = actionGameObject.AddComponent<EnergyAction>();
					break;
				case 13:
					newAction = actionGameObject.AddComponent<ToggleInvincibilityAction>();
					break;
				case 14:
					newAction = actionGameObject.AddComponent<AudioAction>();
					break;
				case 15:
					newAction = actionGameObject.AddComponent<SystemAction>();
					break;
				default:
					newAction = actionGameObject.AddComponent<WaitAction>();
					break;
			}

			return newAction;
		}

		protected static void ChangeAction(object parameters)
		{
			Hashtable hashtable = (Hashtable)parameters;

			List<RexAIAction> actions = (List<RexAIAction>)hashtable[2];
			List<RexAIActionConditionalSet.Pair> pairs = (List<RexAIActionConditionalSet.Pair>)hashtable[3];

			bool usesConditionals = (pairs != null);

			int indexOfItemToChange = (int)hashtable[0];
			int newActionType = (int)hashtable[1];

			GameObject actionGameObject = (usesConditionals) ? pairs[indexOfItemToChange].action.gameObject : actions[indexOfItemToChange].gameObject;

			RexAIAction oldAction = (usesConditionals) ? pairs[indexOfItemToChange].action : actions[indexOfItemToChange];
			DestroyImmediate(oldAction);

			RexAIAction newAction = AddActionToGameObject(newActionType, actionGameObject);

			if(actions != null)
			{
				actions[indexOfItemToChange] = newAction;
			}

			if(pairs != null)
			{
				pairs[indexOfItemToChange].action = newAction;
			}

			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}
	}
	#endif
}