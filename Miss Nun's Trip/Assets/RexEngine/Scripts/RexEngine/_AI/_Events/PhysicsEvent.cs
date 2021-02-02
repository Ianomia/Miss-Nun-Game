using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class PhysicsEvent:RexAIEvent 
	{
		public enum CollisionSide
		{
			EitherWall,
			LeftWall,
			RightWall,
			Floor,
			Ceiling
		}

		public CollisionSide collisionSide;

		protected RexPhysics physicsObject;

		void Awake() 
		{

		}

		void Start() 
		{
		}

		public override void Enable()
		{
			base.Enable();

			physicsObject = aiRoutine.slots.controller.slots.physicsObject;
			if(physicsObject != null)
			{
				physicsObject.OnPhysicsCollision += this.OnPhysicsCollision;
			}
		}

		public override void Disable()
		{
			base.Disable();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			collisionSide = (CollisionSide)EditorGUILayout.EnumPopup("Collision Side", collisionSide);
		}
		#endif

		public override string GetName()
		{
			return "Physics";
		}

		public override EventType GetEventType()
		{
			return EventType.Physics;
		}

		public void OnPhysicsCollision(RexObject.Side side, Collider2D col, RexObject.CollisionType collisionType)
		{
			bool didActivate = false;
			if(collisionType == RexObject.CollisionType.Enter)
			{
				if(collisionSide == CollisionSide.EitherWall && (side == RexObject.Side.Left || side == RexObject.Side.Right))
				{
					didActivate = true;
				}
				else if(collisionSide == CollisionSide.LeftWall && side == RexObject.Side.Left)
				{
					didActivate = true;
				}
				else if(collisionSide == CollisionSide.RightWall && side == RexObject.Side.Right)
				{
					didActivate = true;
				}
				else if(collisionSide == CollisionSide.Ceiling && ((side == RexObject.Side.Top && physicsObject.gravitySettings.gravityScale >= 0.0f) || (side == RexObject.Side.Bottom && physicsObject.gravitySettings.gravityScale < 0.0f)))
				{
					didActivate = true;
				}
				else if(collisionSide == CollisionSide.Floor && ((side == RexObject.Side.Bottom && physicsObject.gravitySettings.gravityScale >= 0.0f) || (side == RexObject.Side.Top && physicsObject.gravitySettings.gravityScale < 0.0f)))
				{
					didActivate = true;
				}
			}

			if(didActivate)
			{
				physicsObject.OnPhysicsCollision -= this.OnPhysicsCollision;
				OnEventActivated();
			}
		}

		void OnDestroy()
		{
			if(physicsObject)
			{
				physicsObject.OnPhysicsCollision -= this.OnPhysicsCollision;
			}
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(PhysicsEvent))]
	public class PhysicsEventEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

