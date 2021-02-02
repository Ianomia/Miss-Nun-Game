using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class PhysicsBranch:RexAIBranch 
	{
		public PhysicsEvent.CollisionSide type;

		public override string ID()
		{
			return "Physics";
		}

		public override bool Determine(RexAIRoutine _routine = null)
		{
			if(_routine == null)
			{
				return false;
			}

			RexActor actor = _routine.slots.controller.slots.actor;
			RexPhysics physicsObject = actor.slots.physicsObject;

			if(actor == null || physicsObject == null)
			{
				return false;
			}

			bool isColliding = false;
			if(type == PhysicsEvent.CollisionSide.Ceiling) 
			{
				if((physicsObject.properties.isAgainstCeiling && physicsObject.gravitySettings.gravityScale >= 0.0f) || (physicsObject.properties.isGrounded && physicsObject.gravitySettings.gravityScale < 0.0f))
				{
					isColliding = true;
				}
			}
			else if(type == PhysicsEvent.CollisionSide.Floor)
			{
				if((physicsObject.properties.isGrounded && physicsObject.gravitySettings.gravityScale >= 0.0f) || (physicsObject.properties.isAgainstCeiling && physicsObject.gravitySettings.gravityScale < 0.0f))
				{
					isColliding = true;
				}
			}
			else if(type == PhysicsEvent.CollisionSide.EitherWall)
			{
				isColliding = physicsObject.IsAgainstEitherWall();
			}
			else if(type == PhysicsEvent.CollisionSide.LeftWall)
			{
				isColliding = physicsObject.properties.isAgainstLeftWall;
			}
			else if(type == PhysicsEvent.CollisionSide.RightWall)
			{
				isColliding = physicsObject.properties.isAgainstRightWall;
			}

			return isColliding;
		}

		public override void DrawInspectorGUI()
		{
			#if UNITY_EDITOR
			type = (PhysicsEvent.CollisionSide)EditorGUILayout.EnumPopup("Type", type);
			#endif
		}

		public override BranchType GetBranchType()
		{
			return BranchType.Physics;
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(PhysicsBranch))]
	public class PhysicsBranchEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

