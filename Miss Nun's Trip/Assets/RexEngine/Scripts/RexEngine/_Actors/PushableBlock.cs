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
	public class PushableBlock:PhysicsMover 
	{
		public RexPhysics physicsObject;

		protected Vector2 distanceMovedThisFrame;

		void Awake()
		{
			if(physicsObject == null)
			{
				physicsObject = GetComponent<RexPhysics>();
			}

			framesBeforeRemoval = 0;
		}

		public override void MovePhysics(RexPhysics _physicsObject)
		{
			if(_physicsObject.IsOnSurface())
			{
				distanceMovedThisFrame = physicsObject.GetPositionChangeFromLastFrame();
				_physicsObject.ApplyDirectTranslation(distanceMovedThisFrame);
			}
		}

		public Vector2 GetVelocity()
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

		public void SetPosition(Vector2 position)
		{
			transform.position = position;

			if(physicsObject)
			{
				physicsObject.properties.position = position;
				physicsObject.previousFrameProperties.position = position;
			}
		}
	}
}
