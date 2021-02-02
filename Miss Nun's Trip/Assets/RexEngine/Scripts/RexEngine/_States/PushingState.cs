using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class PushingState:RexState 
	{
		public const string idString = "Pushing";

		public AnimationClip endingAnimation;
		public AnimationClip pullingAnimation;

		public PullButton pullButton;
		public ActionsAllowed actionsAllowed;

		protected bool isPushing;
		protected RexPhysics otherPhysics;
		protected RexObject.Side pushSide;
		protected bool isAttachedToPushableObject; //This is for pulling; it signifies you grabbed the pushable object so you could pull it backwards
		protected PushMode pushMode;
		protected BoxCollider2D controllerCollider;
		protected BoxCollider2D otherCollider;
		protected int frameBuffer = 2;
		protected int currentFramesSincePush = 0;

		public enum PushMode
		{
			None,
			Pushing,
			Pulling
		}

		public enum PullButton
		{
			Up,
			Misc_1,
			Misc_2
		}

		public enum ActionsAllowed
		{
			PushingAndPulling,
			PushingOnly,
			PullingOnly
		}

		void Awake()
		{
			id = idString;
			GetController();
			if(controller.slots.physicsObject)
			{
				controller.slots.physicsObject.pushing.canPushObjects = true;
				controller.slots.physicsObject.OnPhysicsCollision += this.OnPush;
			}	

			controllerCollider = controller.GetComponent<BoxCollider2D>();
		}

		void Update()
		{
			if(!isPushing)
			{
				if(actionsAllowed != ActionsAllowed.PushingOnly && GetIsButtonDownThisFrame() && controller.slots.physicsObject.IsOnSurface())
				{
					Collider2D pullableCollider = RaycastHelper.IsNextToPullableObject(controller.direction.horizontal, controllerCollider);
					if(pullableCollider != null)
					{
						RexObject.Side side = (controller.direction.horizontal == Direction.Horizontal.Right) ? RexObject.Side.Right : RexObject.Side.Left;
						pushMode = PushMode.Pulling;
						AssignPushableCollider(side, pullableCollider);
						PlaySecondaryAnimation(pullingAnimation);

						float xPosition = (controller.direction.horizontal == Direction.Horizontal.Right) ? otherCollider.bounds.min.x - (controllerCollider.size.x * 0.5f) : otherCollider.bounds.max.x + (controllerCollider.size.x * 0.5f);
						controller.slots.actor.SetPosition(new Vector2(xPosition, controller.slots.actor.transform.position.y));
					}
				}
			}
			else
			{
				if(GetIsButtonDownThisFrame() && pushMode == PushMode.Pulling)
				{
					pushMode = PushMode.None;
					otherPhysics = null;
					currentFramesSincePush = frameBuffer;
				}
			}

			if(!isPushing)
			{
				currentFramesSincePush --;
				if(currentFramesSincePush < 0)
				{
					currentFramesSincePush = 0;
				}
			}
		}

		public override void UpdateMovement()
		{
			if(!isPushing || otherPhysics == null || !controller.slots.physicsObject.IsOnSurface())
			{
				isPushing = false;
				otherPhysics = null;
				currentFramesSincePush = frameBuffer;

				if(!hasEnded)
				{
					End();
				}
			}

			if(isPushing && pushMode != PushMode.Pulling)
			{
				Direction.Horizontal direction = (pushSide == RexObject.Side.Right) ? Direction.Horizontal.Right : Direction.Horizontal.Left;
				if((direction == Direction.Horizontal.Right && controller.axis.x < 0.0f) || (direction == Direction.Horizontal.Left && controller.axis.x > 0.0f))
				{
					isPushing = false;
					otherPhysics = null;
					currentFramesSincePush = frameBuffer;

					if(!hasEnded)
					{
						End();
					}
				}
			}

			if(isPushing && otherPhysics.IsOnSurface())
			{
				Direction.Horizontal direction = (pushSide == RexObject.Side.Right) ? Direction.Horizontal.Right : Direction.Horizontal.Left;

				blockMovingStateMovement = false;

				if(actionsAllowed != ActionsAllowed.PushingOnly && isAttachedToPushableObject && ((direction == Direction.Horizontal.Right && controller.axis.x < 0.0f) || (direction == Direction.Horizontal.Left && controller.axis.x > 0.0f)))
				{
					if(pushMode != PushMode.Pulling)
					{
						PlaySecondaryAnimation(pullingAnimation);
					}

					blockMovingStateMovement = true;
					pushMode = PushMode.Pulling;
					direction = (Direction.Horizontal)((int)direction * -1);

					controller.slots.physicsObject.MoveX((int)direction * otherPhysics.pushing.movementFactorWhenPushed);
					controller.slots.physicsObject.properties.externalVelocity.x = 0.0f;
					controller.slots.physicsObject.properties.acceleration.x = 0.0f;
					controller.slots.physicsObject.properties.deceleration.x = 0.0f;

					bool isObjectOnTopOfActor = false;

					if((direction == Direction.Horizontal.Left && otherCollider.bounds.min.x < controllerCollider.bounds.max.x) || (direction == Direction.Horizontal.Right && otherCollider.bounds.max.x > controllerCollider.bounds.min.x))
					{
						isObjectOnTopOfActor = true;
					}

					if(!isObjectOnTopOfActor)
					{
						otherPhysics.ApplySingleFrameVelocityAddition(new Vector2(otherPhysics.pushing.movementFactorWhenPushed * (int)direction, 0.0f));
					}
				}
				else if(actionsAllowed != ActionsAllowed.PullingOnly && (direction == Direction.Horizontal.Right && controller.axis.x > 0.0f) || (direction == Direction.Horizontal.Left && controller.axis.x < 0.0f))
				{
					if(pushMode != PushMode.Pushing)
					{
						PlayAnimation();
					}

					pushMode = PushMode.Pushing;

					otherPhysics.ApplySingleFrameVelocityAddition(new Vector2(otherPhysics.pushing.movementFactorWhenPushed * (int)direction, 0.0f)); //TODO: Different speeds for pushing and pulling
				}
			}


			if(!isAttachedToPushableObject || pushMode == PushMode.None || (pushMode == PushMode.Pushing && controller.axis.x == 0.0f))
			{
				isPushing = false;
				currentFramesSincePush = frameBuffer;
			}
		}

		public override void OnStateChanged()
		{
			isPushing = false;
			otherPhysics = null;
		}

		public override void OnEnded()
		{
			isPushing = false;
			otherPhysics = null;

			if(currentFramesSincePush > 0)
			{
				controller.SetStateToDefault();

				if(gameObject.activeSelf)
				{
					StartCoroutine("EndingCoroutine");
				}
			}
		}

		public void OnPush(RexObject.Side side, Collider2D col, RexObject.CollisionType collisionType)
		{
			if(actionsAllowed == ActionsAllowed.PullingOnly)
			{
				return;
			}

			bool isFacingObject = (controller.direction.horizontal == Direction.Horizontal.Right && side == RexObject.Side.Right) || (controller.direction.horizontal == Direction.Horizontal.Left && side == RexObject.Side.Left);
			if(col.gameObject.layer == LayerMask.NameToLayer("Pushable") && isFacingObject)
			{
				if(controller.slots.physicsObject.IsOnSurface() && !controller.isKnockbackActive && !controller.isDashing && !controller.slots.actor.IsAttacking() && controller.StateID() != CrouchState.idString)
				{
					if(controller.StateID() != idString || (otherPhysics != null && col.gameObject != otherPhysics.gameObject))
					{
						AssignPushableCollider(side, col);
					}
					else if(controller.StateID() == idString)
					{
						isAttachedToPushableObject = true;
						isPushing = true;
					}
				}
			}
		}

		protected IEnumerator EndingCoroutine()
		{
			if(endingAnimation != null && controller.slots.physicsObject.IsOnSurface())
			{
				PlaySecondaryAnimation(endingAnimation);

				yield return new WaitForSeconds(endingAnimation.length);
			}

			if(controller.StateID() == DefaultState.idString)
			{
				controller.currentState.PlayAnimation();
			}
		}

		protected void AssignPushableCollider(RexObject.Side side, Collider2D col)
		{
			isAttachedToPushableObject = true;
			isPushing = true;
			otherPhysics = col.GetComponent<RexPhysics>();
			otherCollider = col as BoxCollider2D;

			if(otherPhysics != null)
			{
				pushSide = side;
				Begin();
			}
		}

		protected bool GetIsButtonDownThisFrame()
		{
			bool isButtonDownThisFrame = false;
			switch(pullButton)
			{
				case PullButton.Up:
					isButtonDownThisFrame = controller.slots.input.IsUpButtonDownThisFrame();
					break;
				case PullButton.Misc_1:
					isButtonDownThisFrame = controller.slots.input.isMisc_1ButtonDownThisFrame;
					break;
				case PullButton.Misc_2:
					isButtonDownThisFrame = controller.slots.input.isMisc_2ButtonDownThisFrame;
					break;
			}

			return isButtonDownThisFrame;
		}

		void OnDestroy()
		{
			otherPhysics = null;
			otherCollider = null;
			if(controller.slots.physicsObject)
			{
				controller.slots.physicsObject.OnPhysicsCollision -= this.OnPush;
			}
		}
	}
}