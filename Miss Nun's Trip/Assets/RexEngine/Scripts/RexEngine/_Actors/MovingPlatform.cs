/* Copyright Sky Tyrannosaur */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace RexEngine
{
	public class MovingPlatform:PhysicsMover 
	{
		public enum MovementMode
		{
			Waypoints,
			TurnOnWallContact
		}

		public CoordinateType coordinateType;

		public List<Vector2> waypoints = new List<Vector2>();
		public MovementMode movementMode;
		public UseDimensions useDimensions;
		public bool willStartAtWaypoint;
		public int startingWaypoint;

		public Vector2 moveSpeed = new Vector2(1.0f, 1.0f);
		public bool isMovementEnabled = true;
		public bool willStartWhenPlayerIsOnTop;
		public Direction.Horizontal startingDirectionX = Direction.Horizontal.Right;
		public Direction.Vertical startingDirectionY = Direction.Vertical.Up;
		public RexPhysics physicsObject;

		[HideInInspector]
		public Vector2 moveDistance;

		protected Vector2 minMovePositionVector;
		protected Vector2 maxMovePositionVector;

		protected bool isMovingToWaypoint = true;
		protected Vector2 currentMovementSpeed;

		protected int currentWaypointIndex = 0;

		[HideInInspector]
		public bool hasPlayerOnTop;

		protected bool isMoving;
		protected Direction.Horizontal directionX;
		protected Direction.Vertical directionY;
		protected Vector2 distanceMovedThisFrame;

		void Awake()
		{
			if(physicsObject == null)
			{
				physicsObject = GetComponent<RexPhysics>();
			}

			if(coordinateType == CoordinateType.Relative)
			{
				for(int i = 0; i < waypoints.Count; i ++)
				{
					waypoints[i] = new Vector2(waypoints[i].x + transform.position.x, waypoints[i].y);
					waypoints[i] = new Vector2(waypoints[i].x, waypoints[i].y + transform.position.y);
				}
			}

			framesBeforeRemoval = 0;

			if(movementMode == MovementMode.Waypoints && willStartAtWaypoint)
			{
				if(useDimensions == UseDimensions.XAndY)
				{
					SetPosition(waypoints[startingWaypoint]);
				}
				else if(useDimensions == UseDimensions.X)
				{
					SetPosition(new Vector2(waypoints[startingWaypoint].x, transform.position.y));
				}
				else if(useDimensions == UseDimensions.Y)
				{
					SetPosition(new Vector2(transform.position.x, waypoints[startingWaypoint].y));
				}

				currentWaypointIndex = startingWaypoint;
			}
		}

		void Start()
		{
			directionX = startingDirectionX;
			directionY = startingDirectionY;
			if(!willStartWhenPlayerIsOnTop && isMovementEnabled)
			{
				StartMoving();
			}
		}

		void FixedUpdate()
		{
			if(movementMode == MovementMode.Waypoints && isMoving && isMovementEnabled)
			{
				MoveForWaypoints();
			}
			else if(movementMode == MovementMode.TurnOnWallContact && isMoving && isMovementEnabled)
			{
				MoveHorizontal();
				MoveVertical();
			}

			if(willStartWhenPlayerIsOnTop && hasPlayerOnTop && !isMoving && isMovementEnabled)
			{
				StartMoving();
			}
		}

		public override void NotifyOfObjectOnTop(RexPhysics _physicsObject)
		{
			hasPlayerOnTop = true;
		}

		public override void MovePhysics(RexPhysics _physicsObject)
		{
			if(_physicsObject.IsOnSurface())
			{
				distanceMovedThisFrame = physicsObject.GetPositionChangeFromLastFrame();//new Vector2(physicsObject.properties.position.x - physicsObject.previousFrameProperties.position.x, physicsObject.properties.position.y - physicsObject.previousFrameProperties.position.y);
				_physicsObject.ApplyDirectTranslation(distanceMovedThisFrame);
			}
		}

		public virtual Vector2 GetVelocity()
		{
			if(physicsObject)
			{
				return physicsObject.properties.velocity;
			}
			else
			{
				return new Vector2(0.0f, 0.0f);
			}
		}

		protected virtual void StartMoving()
		{
			isMoving = true;
			if(movementMode == MovementMode.Waypoints)
			{
				SetWaypointMovementSpeed();
			}
		}

		protected void MoveHorizontal()
		{
			if(moveSpeed.x == 0.0f)
			{
				return;
			}

			bool willTurn = false;
			if((movementMode == MovementMode.TurnOnWallContact && physicsObject.DidHitEitherWallThisFrame()))
			{
				willTurn = true;
			}
			else if(movementMode == MovementMode.Waypoints)
			{

			}

			if(willTurn)
			{
				directionX = (directionX == Direction.Horizontal.Left) ? Direction.Horizontal.Right : Direction.Horizontal.Left;
			}

			physicsObject.SetVelocityX(moveSpeed.x * (int)directionX);
		}

		protected void MoveVertical()
		{
			if(moveSpeed.y == 0.0f)
			{
				return;
			}

			bool willTurn = false;
			if((movementMode == MovementMode.TurnOnWallContact && physicsObject.IsOnSurface()))
			{
				willTurn = true;
			}
			else if(movementMode == MovementMode.Waypoints)
			{
				
			}

			if(willTurn)
			{
				directionY = (directionY == Direction.Vertical.Up) ? Direction.Vertical.Down : Direction.Vertical.Up;
			}

			physicsObject.SetVelocityY(moveSpeed.y * (int)directionY);
		}

		public void SetPosition(Vector2 position)
		{
			transform.position = position;

			if(physicsObject)
			{
				physicsObject.properties.position = position;
				physicsObject.previousFrameProperties.position = position;
			}
		}

		protected void MoveForWaypoints()
		{
			Vector2 waypoint = waypoints[currentWaypointIndex];

			bool hasHitX = (transform.position.x == waypoint.x);
			bool hasHitY = (transform.position.y == waypoint.y);

			if(useDimensions == UseDimensions.X)
			{
				directionY = Direction.Vertical.Neutral;
			}
			else if(useDimensions == UseDimensions.Y)
			{
				directionX = Direction.Horizontal.Neutral;
			}

			if(directionX == Direction.Horizontal.Right)
			{
				if(transform.position.x >= waypoint.x)
				{
					SetPosition(new Vector2(waypoint.x, transform.position.y));
					physicsObject.SetVelocityX(0.0f);
					directionX = Direction.Horizontal.Neutral;
					hasHitX = true;
				}
				else
				{
					physicsObject.SetVelocityX(currentMovementSpeed.x);
				}
			}
			else if(directionX == Direction.Horizontal.Left) 
			{
				if(transform.position.x <= waypoint.x)
				{
					SetPosition(new Vector2(waypoint.x, transform.position.y));
					physicsObject.SetVelocityX(0.0f);
					directionX = Direction.Horizontal.Neutral;
					hasHitX = true;
				}
				else
				{
					physicsObject.SetVelocityX(currentMovementSpeed.x);
				}
			}

			if(directionY == Direction.Vertical.Up) 
			{
				if(transform.position.y >= waypoint.y)
				{
					SetPosition(new Vector2(transform.position.x, waypoint.y));
					physicsObject.SetVelocityY(0.0f);
					directionY = Direction.Vertical.Neutral;
					hasHitY = true;
				}
				else
				{
					physicsObject.SetVelocityY(currentMovementSpeed.y);
				}
			}
			else if(directionY == Direction.Vertical.Down) 
			{
				if(transform.position.y <= waypoint.y)
				{
					SetPosition(new Vector2(transform.position.x, waypoint.y));
					physicsObject.SetVelocityY(0.0f);
					directionY = Direction.Vertical.Neutral;
					hasHitY = true;
				}
				else
				{
					physicsObject.SetVelocityY(currentMovementSpeed.y);
				}
			}

			if(directionX == Direction.Horizontal.Neutral)
			{
				hasHitX = true;
			}

			if(directionY == Direction.Vertical.Neutral)
			{
				hasHitY = true;
			}

			if(hasHitX && hasHitY)
			{
				isMovingToWaypoint = false;
				physicsObject.StopAllMovement();

				if(useDimensions == UseDimensions.XAndY)
				{
					SetPosition(new Vector2(waypoint.x, waypoint.y));
				}
				else if(useDimensions == UseDimensions.X)
				{
					SetPosition(new Vector2(waypoint.x, transform.position.y));
				}
				else if(useDimensions == UseDimensions.Y)
				{
					SetPosition(new Vector2(transform.position.x, waypoint.y));
				}

				currentWaypointIndex ++;
				if(currentWaypointIndex >= waypoints.Count)
				{
					currentWaypointIndex = 0;
				}

				waypoint = waypoints[currentWaypointIndex];
				SetWaypointMovementSpeed();

				directionX = (waypoint.x < transform.position.x) ? Direction.Horizontal.Left : Direction.Horizontal.Right;
				directionY = (waypoint.y < transform.position.y) ? Direction.Vertical.Down : Direction.Vertical.Up;

				Vector2 roundedPosition;
				roundedPosition.x = Mathf.Round(transform.position.x * 100.0f) / 100.0f;
				roundedPosition.y = Mathf.Round(transform.position.y * 100.0f) / 100.0f;

				Vector2 roundedWaypoint;
				roundedWaypoint.x = Mathf.Round(waypoint.x * 100.0f) / 100.0f;
				roundedWaypoint.y = Mathf.Round(waypoint.y * 100.0f) / 100.0f;

				if(roundedPosition.x == roundedWaypoint.x)
				{
					directionX = Direction.Horizontal.Neutral;
				}

				if(roundedPosition.y == roundedWaypoint.y)
				{
					directionY = Direction.Vertical.Neutral;
				}
			}
		}

		public void MoveToWaypoint()
		{
			Vector2 waypoint = waypoints[currentWaypointIndex]; 
			Direction.Horizontal horizontal = (waypoint.x < transform.position.x) ? Direction.Horizontal.Left : Direction.Horizontal.Right;
			Direction.Vertical vertical = (waypoint.y < transform.position.y) ? Direction.Vertical.Down : Direction.Vertical.Up;

			directionX = horizontal;
			directionY = vertical;
		}

		protected void SetWaypointMovementSpeed()
		{
			currentMovementSpeed = Vector2.zero;

			Vector2 adjustedWaypoint = waypoints[currentWaypointIndex];
			if(useDimensions == UseDimensions.X)
			{
				adjustedWaypoint.y = transform.position.y;
			}
			else if(useDimensions == UseDimensions.Y)
			{
				adjustedWaypoint.x = transform.position.x;
			}

			float angle = RexMath.AngleFromPoint(transform.position, adjustedWaypoint);
			currentMovementSpeed = RexMath.VelocityForAngle(moveSpeed.x, angle);
		}
	}


	#if UNITY_EDITOR
	[CustomEditor(typeof(MovingPlatform))]
	public class MovingPlatformEditor:Editor 
	{
		protected InspectorHelper.AIInspectorStyles styles;
		
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			MovingPlatform _target = target as MovingPlatform;
			_target.isMovementEnabled = EditorGUILayout.Toggle("Is Movement Enabled", _target.isMovementEnabled);
			_target.willStartWhenPlayerIsOnTop = EditorGUILayout.Toggle("Only Start When Player is On Top", _target.willStartWhenPlayerIsOnTop);

			_target.movementMode = (MovingPlatform.MovementMode)EditorGUILayout.EnumPopup("Movement Mode", _target.movementMode);

			if(_target.movementMode == MovingPlatform.MovementMode.Waypoints)
			{
				if(_target.waypoints.Count < 1)
				{
					_target.waypoints.Add(Vector2.zero);
				}

				float moveSpeed = _target.moveSpeed.x;
				moveSpeed = EditorGUILayout.FloatField("Movement Speed", moveSpeed);
				_target.moveSpeed = new Vector2(moveSpeed, _target.moveSpeed.y);

				_target.coordinateType = (CoordinateType)EditorGUILayout.EnumPopup("Coordinate Type", _target.coordinateType);
				_target.useDimensions = (UseDimensions)EditorGUILayout.EnumPopup("Use Dimensions", _target.useDimensions);

				EditorGUILayout.BeginHorizontal(); EditorGUILayout.LabelField(""); EditorGUILayout.EndHorizontal();

				EditorGUILayout.LabelField("WAYPOINTS", EditorStyles.boldLabel);

				for(int i = 0; i < _target.waypoints.Count; i ++)
				{
					EditorGUILayout.BeginVertical(styles.actionStyle);
					EditorGUILayout.BeginHorizontal();

					EditorGUILayout.LabelField(i + ": ");
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					Vector2 waypointPosition = _target.waypoints[i];
					if(_target.useDimensions == UseDimensions.XAndY)
					{
						
						waypointPosition = EditorGUILayout.Vector2Field("", waypointPosition);
					}
					else
					{
						if(_target.useDimensions == UseDimensions.X)
						{
							waypointPosition.x = EditorGUILayout.FloatField("X", waypointPosition.x);
						}
						else if(_target.useDimensions == UseDimensions.Y)
						{
							waypointPosition.y = EditorGUILayout.FloatField("Y", waypointPosition.y);
						}
					}

					_target.waypoints[i] = waypointPosition;

					if(_target.waypoints.Count > 1)
					{
						DisplayOptionsMenu(i);
					}

					EditorGUILayout.EndHorizontal();
					EditorGUILayout.EndVertical();
				}

				if(GUILayout.Button("Add Waypoint"))
				{
					_target.waypoints.Add(new Vector2());
				}

				EditorGUILayout.BeginHorizontal(); EditorGUILayout.LabelField(""); EditorGUILayout.EndHorizontal();

				_target.willStartAtWaypoint = EditorGUILayout.Toggle("Start At Waypoint", _target.willStartAtWaypoint);
				if(_target.willStartAtWaypoint)
				{
					_target.startingWaypoint = EditorGUILayout.IntField("Starting Waypoint", _target.startingWaypoint);
					if(_target.startingWaypoint < 0)
					{
						_target.startingWaypoint = 0;
					}
					else if(_target.startingWaypoint > (_target.waypoints.Count - 1))
					{
						_target.startingWaypoint = _target.waypoints.Count - 1;
					}
				}
			}
			else if(_target.movementMode == MovingPlatform.MovementMode.TurnOnWallContact)
			{
				_target.moveSpeed = EditorGUILayout.Vector2Field("Movement Speed", _target.moveSpeed);
				_target.startingDirectionX = (Direction.Horizontal)EditorGUILayout.EnumPopup("Starting Horizontal Direction", _target.startingDirectionX);
				_target.startingDirectionY = (Direction.Vertical)EditorGUILayout.EnumPopup("Starting Vertical Direction", _target.startingDirectionY);
			}

			if(_target.physicsObject == null)
			{
				_target.physicsObject = _target.GetComponent<RexPhysics>();
			}

			if(!Application.isPlaying && EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}

		protected void DisplayOptionsMenu(int index)
		{
			if(GUILayout.Button("Options", EditorStyles.miniButton, GUILayout.Width(70))) 
			{
				GenericMenu menu = new GenericMenu();

				MovingPlatform _target = target as MovingPlatform;

				bool isFinalItem = (index == _target.waypoints.Count - 1 && _target.waypoints.Count >= 2);

				if(index != 0 && _target.waypoints.Count > 1)
				{
					menu.AddItem(new GUIContent("Shift (Up)"), false, ShiftItemUp, (object)((int)index));
				}

				if(!isFinalItem && _target.waypoints.Count > 1)
				{
					menu.AddItem(new GUIContent("Shift (Down)"), false, ShiftItemDown, (object)((int)index));
				}

				if(_target.waypoints.Count > 1)
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

			MovingPlatform _target = target as MovingPlatform;
			Vector2 waypoint = _target.waypoints[index];

			_target.waypoints.RemoveAt(index);
			_target.waypoints.Insert(index - 1, waypoint);
		}

		protected void ShiftItemDown(object indexObject)
		{
			int index = (int)indexObject;

			MovingPlatform _target = target as MovingPlatform;
			if(index > _target.waypoints.Count - 1)
			{
				return;
			}

			Vector2 waypoint = _target.waypoints[index];

			_target.waypoints.RemoveAt(index);
			_target.waypoints.Insert(index + 1, waypoint);
		}

		protected void RemoveItem(object indexObject)
		{
			MovingPlatform _target = target as MovingPlatform;

			int index = (int)indexObject;
			_target.waypoints.RemoveAt(index);
		}
	}
	#endif
}
