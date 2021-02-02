using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class StairClimbingState:RexState 
	{
		[System.Serializable]
		public class Animations
		{
			[Tooltip("The animation that plays while the actor walks up stairs.")]
			public AnimationClip movingUp;
			[Tooltip("The animation that plays while the actor walks down stairs.")]
			public AnimationClip movingDown;
			[Tooltip("The animation that plays while the actor stands still while facing downward on stairs.")]
			public AnimationClip standingDown;
		}

		[System.Serializable]
		public class Controls
		{
			public bool requireUpButtonToWalkOntoStairs = true;
			public bool requireUpButtonToJumpOntoStairs = true;
		}

		public enum Substate
		{
			Stopped,
			Moving
		}

		public const string idString = "StairClimbing";

		[Tooltip("AnimationClips that play while the actor is mounted on stairs.")]
		public Animations animations;
		[Tooltip("Whether or not jumping is enabled while the actor is mounted on stairs.")]
		public bool canJump;
		[Tooltip("Whether or not turning is enabled while the actor is mounted on stairs.")]
		public bool canTurn = true;
		[Tooltip("If True, taking damage while the actor is on stairs will knock them off.")]
		public bool willDamageKnockOffStairs = true;
		[Tooltip("If True, the actor can move up and down stairs using the Up and Down buttons in addition to the standard Left and Right buttons.")]
		public bool canMoveWithUpDown;
		[Tooltip("The speed with which the actor climbs stairs.")]
		public float speed = 1.0f;
		[Tooltip("If True, the actor can drop down through stairs they're mounted on when the Down and Jump buttons are pressed simultaneously.")]
		public bool canDropThrough = true;
		public Controls controls;

		protected bool isClimbing;
		protected BoxCollider2D boxCollider;
		protected Substate substate;
		protected Direction.Horizontal stairDirection;
		protected int currentFramesBeforeDropIsAllowed = 0;
		protected int totalFramesBeforeDropIsAllowed = 2;

		protected bool areDownCollisionsEnabled;
		protected Collider2D stairCollider;
		protected bool canEnableStairCollisions = true;

		void Awake() 
		{
			id = idString;
			boxCollider = GetComponent<BoxCollider2D>();
			GetController();

			willAllowDirectionChange = canTurn;
			willPlayAnimationOnBegin = false;
		}

		void FixedUpdate()
		{
			if(isEnabled)
			{
				bool didDrop = false;

				if(controller.StateID() == id || isClimbing)
				{
					float gravityScaleMultiplier = controller.GravityScaleMultiplier();
					bool isOnSurface = controller.slots.physicsObject.IsOnSurface();
					bool isOnStairsLeft = RaycastHelper.IsAtTopOfStairs((Direction.Vertical)(gravityScaleMultiplier * -1), Direction.Horizontal.Left, controller.slots.actor.slots.collider as BoxCollider2D, isOnSurface, gravityScaleMultiplier).didHit;
					bool isOnStairsRight = RaycastHelper.IsAtTopOfStairs((Direction.Vertical)(gravityScaleMultiplier * -1), Direction.Horizontal.Right, controller.slots.actor.slots.collider as BoxCollider2D, isOnSurface, gravityScaleMultiplier).didHit;

					if(controller.slots.physicsObject.GetSurfaceLayer() != "Stairs" && controller.slots.physicsObject.GetSurfaceLayer() != "" && !isOnStairsLeft && !isOnStairsRight && currentFramesBeforeDropIsAllowed <= 0)
					{
						didDrop = true;
						Drop();
					}

					currentFramesBeforeDropIsAllowed --;
					if(currentFramesBeforeDropIsAllowed < 0)
					{
						currentFramesBeforeDropIsAllowed = 0;
					}
				}

				if(!didDrop)
				{
					CheckStairs(controller.axis.y);
				}
			}
		}

		public void Drop()
		{
			isClimbing = false;
			controller.slots.physicsObject.RemoveFromDownCollisions("Stairs");

			canEnableStairCollisions = false;

			End();
		}

		public override void OnBegin()
		{
			JumpState jumpState = controller.GetComponent<JumpState>();
			if(jumpState != null)
			{
				jumpState.End();
			}

			DashState dashState = controller.GetComponent<DashState>();
			if(dashState != null)
			{
				dashState.End();
			}

			controller.slots.physicsObject.SetVelocityY(0.0f);

			if(Mathf.Abs(controller.slots.physicsObject.properties.velocity.x) > 0.0f)
			{
				substate = Substate.Moving;
			}
			else
			{
				substate = Substate.Stopped;
			}

			if(!controller.slots.actor.currentAttack)
			{
				PlayAnimationForSubstate();
			}
		}

		public override void OnEnded()
		{
			controller.slots.physicsObject.RemoveFromDownCollisions("Stairs");
			controller.slots.physicsObject.SetVelocityX(0.0f); //TODO: What about Dashing?
			controller.SetStateToDefault();
		}

		public override void PlayAnimationForSubstate()
		{
			AnimationClip animationToPlay = animation;
			float gravityScale = controller.GravityScaleMultiplier();
			switch(substate)
			{
				case Substate.Moving:
					if((stairDirection == Direction.Horizontal.Right && controller.direction.horizontal == Direction.Horizontal.Right) || (stairDirection == Direction.Horizontal.Left && controller.direction.horizontal == Direction.Horizontal.Left))
					{
						animationToPlay = (gravityScale >= 0.0f) ? animations.movingUp : animations.movingDown;
					}
					else
					{
						animationToPlay = (gravityScale >= 0.0f) ? animations.movingDown : animations.movingUp;
					}
					break;
				case Substate.Stopped:
					if((stairDirection == Direction.Horizontal.Right && controller.direction.horizontal == Direction.Horizontal.Right) || (stairDirection == Direction.Horizontal.Left && controller.direction.horizontal == Direction.Horizontal.Left))
					{
						animationToPlay = (gravityScale >= 0.0f) ? animation : animations.standingDown;
					}
					else
					{
						animationToPlay = (gravityScale >= 0.0f) ? animations.standingDown : animation;
					}
					break;
			}

			PlaySecondaryAnimation(animationToPlay);
		}

		public override void UpdateMovement()
		{
			float _inputDirection = controller.axis.x;
			if(_inputDirection == 0.0f && canMoveWithUpDown)
			{
				_inputDirection = controller.axis.y * (int)stairDirection;
			}

			if(_inputDirection != 0.0f && !controller.isKnockbackActive) 
			{	
				Direction.Horizontal actorDirection = controller.direction.horizontal;
				actorDirection = (_inputDirection > 0.0f) ? Direction.Horizontal.Right : Direction.Horizontal.Left;
				controller.FaceDirection(actorDirection);

				if(controller.slots.actor.currentAttack != null && controller.slots.actor.currentAttack.canceledBy.onStairClimb)
				{
					controller.slots.actor.currentAttack.Cancel();
				}

				if(controller.StateID() == idString)
				{
					controller.slots.physicsObject.properties.velocityCap.x = speed * _inputDirection;
					controller.slots.physicsObject.SetVelocityX(speed * _inputDirection);
					controller.slots.physicsObject.properties.acceleration.x = 0.0f;
					controller.slots.physicsObject.properties.deceleration.x = 0.0f;
				}
			
				if(substate != Substate.Moving && !controller.slots.actor.currentAttack)
				{
					AnimationClip animationToPlay = null;
					float gravityScale = controller.GravityScaleMultiplier();
					if((stairDirection == Direction.Horizontal.Right && controller.direction.horizontal == Direction.Horizontal.Right) || (stairDirection == Direction.Horizontal.Left && controller.direction.horizontal == Direction.Horizontal.Left))
					{
						animationToPlay = (gravityScale >= 0.0f) ? animations.movingUp : animations.movingDown;
						PlaySecondaryAnimation(animationToPlay);
					}
					else
					{
						animationToPlay = (gravityScale >= 0.0f) ? animations.movingDown : animations.movingUp;
						PlaySecondaryAnimation(animationToPlay);
					}
				}

				substate = Substate.Moving;
			} 
			else //Stop moving
			{
				controller.slots.physicsObject.properties.acceleration.x = 0.0f;
				controller.slots.physicsObject.properties.deceleration.x = 0.0f;
				controller.slots.physicsObject.SetVelocityX(0.0f);

				if(substate != Substate.Stopped && !controller.slots.actor.currentAttack)
				{
					AnimationClip animationToPlay = null;
					float gravityScale = controller.GravityScaleMultiplier();
					if((stairDirection == Direction.Horizontal.Right && controller.direction.horizontal == Direction.Horizontal.Right) || (stairDirection == Direction.Horizontal.Left && controller.direction.horizontal == Direction.Horizontal.Left))
					{
						animationToPlay = (gravityScale >= 0.0f) ? animation : animations.standingDown;
						PlaySecondaryAnimation(animationToPlay);
					}
					else
					{
						animationToPlay = (gravityScale >= 0.0f) ? animations.standingDown : animation;
						PlaySecondaryAnimation(animationToPlay);
					}
				}

				substate = Substate.Stopped;
			}
		}

		protected void CheckStairs(float _inputDirection)
		{
			if(!canEnableStairCollisions && (stairCollider == null || controller.slots.physicsObject.IsOnSurface()))
			{
				canEnableStairCollisions = true;
			}

			if(!isClimbing && controller.slots.physicsObject.GetSurfaceLayer() == "Stairs")
			{
				isClimbing = true;
				currentFramesBeforeDropIsAllowed = totalFramesBeforeDropIsAllowed;
				Begin();
				controller.slots.physicsObject.SetVelocityX(0.0f);
				controller.slots.physicsObject.SetVelocityY(0.0f);
				controller.slots.physicsObject.FreezeGravityForSingleFrame();

				Begin();
				return;
			}

			if(!isClimbing)
			{
				if(!canEnableStairCollisions)
				{
					return;
				}

				float gravityScaleMultiplier = controller.GravityScaleMultiplier();
				bool isOnSurface = controller.slots.physicsObject.IsOnSurface();

				RaycastHelper.StairsInfo isAboveStairs = RaycastHelper.IsOnStairs((Direction.Vertical)(gravityScaleMultiplier * -1), controller.slots.actor.slots.collider as BoxCollider2D, isOnSurface, gravityScaleMultiplier);
				RaycastHelper.StairsInfo isAtFootOfStairs = RaycastHelper.IsAtFootOfStairs((Direction.Vertical)gravityScaleMultiplier, controller.direction.horizontal, controller.slots.actor.slots.collider as BoxCollider2D, isOnSurface, gravityScaleMultiplier);
				RaycastHelper.StairsInfo isAtTopOfStairs = RaycastHelper.IsAtTopOfStairs((Direction.Vertical)(gravityScaleMultiplier * -1), controller.direction.horizontal, controller.slots.actor.slots.collider as BoxCollider2D, isOnSurface, gravityScaleMultiplier);

				if(isAboveStairs.didHit)
				{
					stairDirection = isAboveStairs.stairDirection;
					if(stairCollider == null)
					{
						stairCollider = isAboveStairs.stairCollider;
					}
				}
				else if(isAtTopOfStairs.didHit)
				{
					stairDirection = isAtTopOfStairs.stairDirection;
					if(stairCollider == null)
					{
						stairCollider = isAtTopOfStairs.stairCollider;
					}
				}
				else if(isAtFootOfStairs.didHit)
				{
					stairDirection = isAtFootOfStairs.stairDirection;
					if(stairCollider == null)
					{
						stairCollider = isAtFootOfStairs.stairCollider;
					}
				}

				float gravityScale = controller.GravityScaleMultiplier();
				bool didFindStairs = (isAboveStairs.didHit && !isOnSurface) || (isAtFootOfStairs.didHit && ((gravityScale > 0.0f && controller.direction.horizontal == stairDirection) || (gravityScale < 0.0f && controller.direction.horizontal != stairDirection)) && isOnSurface) || (isAtTopOfStairs.didHit && !isAtFootOfStairs.didHit && (controller.direction.horizontal != stairDirection) && isOnSurface && ((gravityScale > 0.0f && boxCollider.bounds.min.y > stairCollider.transform.position.y) || (gravityScale < 0.0f && boxCollider.bounds.max.y < stairCollider.transform.position.y)));
				bool isUpButtonAccountedFor = false;
				if(!isOnSurface )
				{
					if((controls.requireUpButtonToJumpOntoStairs && _inputDirection == 1.0f) || !controls.requireUpButtonToJumpOntoStairs)
					{
						isUpButtonAccountedFor = true;
					}
				}
				else if(isOnSurface)
				{
					if((controls.requireUpButtonToWalkOntoStairs && _inputDirection == 1.0f) || !controls.requireUpButtonToWalkOntoStairs)
					{
						isUpButtonAccountedFor = true;
					}
				}

				bool isNotBlockedFromClimbing = (!IsLockedForAttack(Attack.ActionType.StairClimbing) && !controller.isKnockbackActive && !controller.isStunned);
				bool isMovingInRightDirection = (controller.slots.physicsObject.properties.velocity.y <= 0.0f && controller.GravityScaleMultiplier() >= 0.0f) || (controller.slots.physicsObject.properties.velocity.y >= 0.0f && controller.GravityScaleMultiplier() < 0.0f);
					
				if(didFindStairs && isUpButtonAccountedFor && isNotBlockedFromClimbing && isMovingInRightDirection) 
				{
					controller.slots.physicsObject.AddToDownCollisions("Stairs");
				}
			}
			else if(controller.isKnockbackActive && willDamageKnockOffStairs)
			{
				Drop();
			}
		}

		protected void OnTriggerExit2D(Collider2D col)
		{
			if(col.tag == "Stairs")
			{
				if(stairCollider != null && col == stairCollider)
				{
					stairCollider = null;
					canEnableStairCollisions = true;
				}
			}
		}
	}
}
