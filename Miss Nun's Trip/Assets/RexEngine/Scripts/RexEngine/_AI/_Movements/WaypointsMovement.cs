using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace RexEngine
{
	public class WaypointsMovement:RexAIMovement 
	{
		public CoordinateType coordinateType;

		public List<Waypoints> waypoints = new List<Waypoints>();
		public UseDimensions useDimensions;
		public int numberOfLoops;
		public int numberOfLoopsLow;
		public int numberOfLoopsHigh;
		public bool randomizeLoops;
		protected int currentLoop;

		public RexAISequence.Loop loop;

		[System.Serializable]
		public class Waypoints
		{
			public Vector2 position;
			public bool playActionOnWaypointReached;
			public RexAISequence waypointReachedSequence;
		}

		protected bool isMovingToWaypoint = true;
		protected int currentWaypointIndex = 0;
		protected RexAISequence currentSequence; //The sequence this is waiting for before moving on
		protected GUIStyle foldoutStyle;

		protected Direction.Horizontal horizontal;
		protected Direction.Vertical vertical;

		void Awake()
		{
			if(coordinateType == CoordinateType.Relative)
			{
				RexController controller = aiRoutine.slots.controller;
				if(controller)
				{
					for(int i = 0; i < waypoints.Count; i ++)
					{
						Waypoints waypoint = waypoints[i];
						waypoint.position.x += controller.transform.position.x;
						waypoint.position.y += controller.transform.position.y;
					}
				}
			}
		}

		public override void OnBegin()
		{
			if(randomizeLoops)
			{
				numberOfLoops = RexMath.RandomInt(numberOfLoopsLow, numberOfLoopsHigh);
			}

			ResetTimer();

			currentWaypointIndex = 0;
			isMovingToWaypoint = true;

			horizontal = (waypoints[currentWaypointIndex].position.x < aiRoutine.slots.controller.transform.position.x) ? Direction.Horizontal.Left : Direction.Horizontal.Right;
			vertical = (waypoints[currentWaypointIndex].position.y < aiRoutine.slots.controller.transform.position.y) ? Direction.Vertical.Down : Direction.Vertical.Up;

			base.OnBegin();
		}

		public override void OnEnded()
		{
			if(aiRoutine.slots.controller)
			{
				aiRoutine.slots.controller.SetAxis(Vector2.zero);
			}
		}

		public override void NotifyOfSequenceEnd(RexAISequence sequence)
		{
			if(sequence == currentSequence)
			{
				if(currentWaypointIndex < waypoints.Count)
				{
					isMovingToWaypoint = true;
				}
				else
				{
					OnFinalWaypointReached();
				}
			}
		}

		public override void UpdateAI()
		{
			if(mode == Mode.Wait)
			{
				return;
			}

			RexController controller = aiRoutine.slots.controller;
			if(controller == null)
			{
				return;
			}

			bool isNotMovingOnOwn = controller.isKnockbackActive;

			if(isMovingToWaypoint)
			{
				bool useX = (useDimensions == UseDimensions.XAndY || useDimensions == UseDimensions.X);
				bool useY = (useDimensions == UseDimensions.XAndY || useDimensions == UseDimensions.Y);

				Vector2 waypoint = waypoints[currentWaypointIndex].position;

				bool hasHitX = (controller.transform.position.x == waypoint.x);
				bool hasHitY = (controller.transform.position.y == waypoint.y);

				float margin = 0.1f;
				if((Mathf.Abs(controller.transform.position.x - waypoint.x) < margin))
				{
					hasHitX = true;
				}

				if((Mathf.Abs(controller.transform.position.y - waypoint.y) < margin))
				{
					hasHitY = true;
				}

				if(useX && horizontal == Direction.Horizontal.Right && controller.transform.position.x >= waypoint.x)
				{
					hasHitX = true;
				}
				else if(useX && horizontal == Direction.Horizontal.Left && controller.transform.position.x <= waypoint.x) 
				{
					hasHitX = true;
				}

				if(useY && controller.axis.y > 0.0f && controller.transform.position.y >= waypoint.y) 
				{
					hasHitY = true;
				}
				else if(useY && controller.axis.y < 0.0f && controller.transform.position.y <= waypoint.y) 
				{
					hasHitY = true;
				}

				if(((useX && hasHitX) || !useX) && ((useY && hasHitY) || !useY) && !isNotMovingOnOwn)
				{
					isMovingToWaypoint = false;

					if(useX)
					{
						controller.SetAxis(new Vector2(0.0f, controller.axis.y));
					}

					if(useY)
					{
						controller.SetAxis(new Vector2(controller.axis.x, 0.0f));
					}

					bool isPlayingSequence = false;
					if(waypoints[currentWaypointIndex].waypointReachedSequence != null)
					{
						isPlayingSequence = true;

						aiRoutine.ChangeSequence(waypoints[currentWaypointIndex].waypointReachedSequence);
						currentSequence = waypoints[currentWaypointIndex].waypointReachedSequence;
						OnEnded();
					}

					currentWaypointIndex ++;

					waypoint = (currentWaypointIndex < waypoints.Count) ? waypoints[currentWaypointIndex].position : waypoints[0].position;

					if(!isPlayingSequence)
					{
						if(currentWaypointIndex < waypoints.Count)
						{
							isMovingToWaypoint = true;
						}
						else
						{
							OnFinalWaypointReached();
						}
					}
				}

				if(isMovingToWaypoint)
				{
					MoveToWaypoint();
				}

				horizontal = (waypoint.x < controller.transform.position.x) ? Direction.Horizontal.Left : Direction.Horizontal.Right;
				vertical = (waypoint.y < controller.transform.position.y) ? Direction.Vertical.Down : Direction.Vertical.Up;
			}
		}

		public void MoveToWaypoint()
		{
			Vector2 waypoint = waypoints[currentWaypointIndex].position; 
			RexController controller = aiRoutine.slots.controller;
			if(controller)
			{
				bool useX = (useDimensions == UseDimensions.XAndY || useDimensions == UseDimensions.X);
				bool useY = (useDimensions == UseDimensions.XAndY || useDimensions == UseDimensions.Y);

				Direction.Horizontal horizontal = (waypoint.x < controller.transform.position.x) ? Direction.Horizontal.Left : Direction.Horizontal.Right;
				Direction.Vertical vertical = (waypoint.y < controller.transform.position.y) ? Direction.Vertical.Down : Direction.Vertical.Up;

				float margin = 0.1f;
				if((Mathf.Abs(controller.transform.position.x - waypoint.x) < margin))
				{
					useX = false;
				}

				if(!useX)
				{
					horizontal = (Direction.Horizontal)0.0f;
				}

				if((Mathf.Abs(controller.transform.position.y - waypoint.y) < margin))
				{
					useY = false;
				}

				if(!useY)
				{
					vertical = (Direction.Vertical)0.0f;
				}

				controller.SetAxis(new Vector2((int)horizontal, (int)vertical)); 
			}
		}

		protected void OnFinalWaypointReached()
		{
			if(loop == RexAISequence.Loop.Forever || (loop == RexAISequence.Loop.Limited && currentLoop < numberOfLoops - 1))
			{
				if(loop == RexAISequence.Loop.Limited)
				{
					currentLoop ++;
				}

				currentWaypointIndex = 0;
				isMovingToWaypoint = true;
			}
		}

		#if UNITY_EDITOR
		protected GUIStyle GetFoldoutStyle()
		{
			GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
			foldoutStyle.margin.left = 15;
			foldoutStyle.margin.top = 5;
			foldoutStyle.fontStyle = FontStyle.Italic;

			if(EditorGUIUtility.isProSkin)
			{
				foldoutStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
			}

			return foldoutStyle;
		}
		#endif

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			if(foldoutStyle == null)
			{
				foldoutStyle = GetFoldoutStyle();
			}

			mode = (Mode)EditorGUILayout.EnumPopup("Mode", mode);

			EditorGUILayout.LabelField("");

			coordinateType = (CoordinateType)EditorGUILayout.EnumPopup("Coordinate Type", coordinateType);
			useDimensions = (UseDimensions)EditorGUILayout.EnumPopup("Use Dimensions", useDimensions);

			EditorGUILayout.LabelField("");
			InspectorHelper.DrawLine();
			EditorGUILayout.LabelField("");

			EditorGUILayout.LabelField("WAYPOINTS", EditorStyles.boldLabel);

			for(int i = 0; i < waypoints.Count; i ++)
			{
				EditorGUILayout.BeginVertical(styles.actionStyle);
				EditorGUILayout.BeginHorizontal();

				Vector2 waypointPosition = waypoints[i].position;
				if(useDimensions == UseDimensions.XAndY)
				{
					waypointPosition = EditorGUILayout.Vector2Field("", waypointPosition);
				}
				else
				{
					if(useDimensions == UseDimensions.X)
					{
						waypointPosition.x = EditorGUILayout.FloatField("X", waypointPosition.x);
					}
					else if(useDimensions == UseDimensions.Y)
					{
						waypointPosition.y = EditorGUILayout.FloatField("Y", waypointPosition.y);
					}
				}

				waypoints[i].position = waypointPosition;
				DisplayOptionsMenu(i);

				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();

				waypoints[i].playActionOnWaypointReached = EditorGUILayout.Foldout(waypoints[i].playActionOnWaypointReached, "Do Action on Waypoint Reached", foldoutStyle);
				if(waypoints[i].playActionOnWaypointReached)
				{
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();

					waypoints[i].waypointReachedSequence = (RexAISequence)EditorGUILayout.ObjectField("", waypoints[i].waypointReachedSequence, typeof(RexAISequence), true);
				}

				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}

			if(GUILayout.Button("Add Waypoint"))
			{
				AddWaypoint();
			}

			EditorGUILayout.LabelField("");
			InspectorHelper.DrawLine();
			EditorGUILayout.LabelField("");

			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("LOOPS", EditorStyles.boldLabel);

			loop = (RexAISequence.Loop)EditorGUILayout.EnumPopup("Loop Type", loop);
			if(loop == RexAISequence.Loop.Limited)
			{
				randomizeLoops = EditorGUILayout.Toggle("Randomze Number", randomizeLoops);
				if(!randomizeLoops)
				{
					numberOfLoops = EditorGUILayout.IntField("Number", numberOfLoops);
					if(numberOfLoops < 2)
					{
						numberOfLoops = 2;
					}
				}
				else
				{
					numberOfLoopsLow = EditorGUILayout.IntField("Number (Lowest)", numberOfLoopsLow);
					if(numberOfLoopsLow < 2)
					{
						numberOfLoopsLow = 2;
					}

					numberOfLoopsHigh = EditorGUILayout.IntField("Number (Highest)", numberOfLoopsHigh);
					if(numberOfLoopsHigh < 2)
					{
						numberOfLoopsHigh = 2;
					}
				}
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.LabelField("");

			DisplayJumpOptions();

			if(jump.onLedgeContact || turn.onLedgeContact)
			{
				DrawSlopesGUI();
			}
		}

		protected void AddWaypoint()
		{
			waypoints.Add(new Waypoints());
		}

		protected void DisplayOptionsMenu(int index)
		{
			if(GUILayout.Button("Options", EditorStyles.miniButton, GUILayout.Width(70))) 
			{
				GenericMenu menu = new GenericMenu();

				bool isFinalItem = (index == waypoints.Count - 1 && waypoints.Count >= 2);

				if(index != 0 && waypoints.Count > 1)
				{
					menu.AddItem(new GUIContent("Shift (Up)"), false, ShiftItemUp, (object)((int)index));
				}

				if(!isFinalItem && waypoints.Count > 1)
				{
					menu.AddItem(new GUIContent("Shift (Down)"), false, ShiftItemDown, (object)((int)index));
				}

				if(waypoints.Count > 0)
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

			Waypoints waypoint = waypoints[index];

			waypoints.RemoveAt(index);
			waypoints.Insert(index - 1, waypoint);
		}

		protected void ShiftItemDown(object indexObject)
		{
			int index = (int)indexObject;

			if(index > waypoints.Count - 1)
			{
				return;
			}

			Waypoints waypoint = waypoints[index];

			waypoints.RemoveAt(index);
			waypoints.Insert(index + 1, waypoint);
		}

		protected void RemoveItem(object indexObject)
		{
			int index = (int)indexObject;
			waypoints.RemoveAt(index);
		}
		#endif
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(WaypointsMovement))]
	public class WaypointsMovementEditor:Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			(target as WaypointsMovement).DrawInspectorGUI();

			if(!Application.isPlaying && EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}
	}
	#endif
}
