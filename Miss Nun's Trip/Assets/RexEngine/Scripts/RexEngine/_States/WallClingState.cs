/* Copyright Sky Tyrannosaur */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class WallClingState:RexState
	{
		public enum Substate
		{
			None,
			Stopped,
			Sliding, 
			Climbing,
			LedgeHanging
		}

		public class BufferZoneInfo
		{
			public bool isLedgeDetected = false;
			public bool isInBufferZone = false;
			public float bufferCutoff = 0.0f;
			public float actorSnapPosition;
		}

		[System.Serializable]
		public class Animations
		{
			[Tooltip("The AnimationClip that plays while the actor climbs up or down a wall.")]
			public AnimationClip climbWallMoving;
			[Tooltip("The AnimationClip that plays while the actor clings to a wall without moving up or down.")]
			public AnimationClip climbWallStopped;
			[Tooltip("The AnimationClip that plays while the actor hangs from a ledge.")]
			public AnimationClip ledgeHang;
		}

		[System.Serializable]
		public class RequireWallTag
		{
			public bool requiresSpecificTag = false;
			public string tag = "";
		}

		[System.Serializable]
		public class WallJump
		{
			[Tooltip("If True, the actor can jump from walls by pressing the Jump button when touching a wall.")]
			public bool enableWallJump = true;
			[Tooltip("The number of frames an actor can still perform a wall jump for after disengaging from a wall.")]
			public int wallJumpGraceFrames = 3;
			[Tooltip("The number of frames after performing a wall jump where the actor is forcibly pushed away from the wall.")]
			public int wallJumpKickbackFrames = 7;
			[Tooltip("If True, the player must press the Left button on the d-pad along with the jump button to execute a jump from a wall on their right, and vice versa.")]
			public bool requireHorizontalPressForJump;
			[Tooltip("The vertical speed of the jump.")]
			public float speed = 12.0f;
		}

		[System.Serializable]
		public class WallClimb
		{
			[Tooltip("If True, the actor can climb up and down while clinging to a wall.")]
			public bool enableClimbing;
			[Tooltip("The speed with which the actor climbs up and down on a wall.")]
			public float climbSpeed = 5.0f;
		}

		[System.Serializable]
		public class LedgeGrab
		{
			[Tooltip("If True, the actor can hang from ledges.")]
			public bool enableLedgeGrab;
			[Tooltip("If True, the actor can jump while hanging from a ledge.")]
			public bool canLedgeJump = true;
		}

		public const string idString = "WallCling";

		[Tooltip("If this is set to True, the actor will only be able to utilize walls that are tagged with the specified tag.")]
		public RequireWallTag requireSpecificTagOnWalls;
		[Tooltip("Settings for jumping from walls.")]
		public WallJump wallJump;
		[Tooltip("Settings for climbing up and down while clinging to a wall.")]
		public WallClimb wallClimb;
		[Tooltip("Settings for grabbing ledges.")]
		public LedgeGrab ledgeGrab;

		[Tooltip("If True, the actor can cling to a wall even while moving upwards from a jump; if False, the actor must be moving downwards in order for a wall cling to register.")]
		public bool enableClingWhileJumping;
		[Tooltip("If True, clinging to a wall forces the actor to face that wall.")]
		public bool willFaceWallOnCling = true;
		[Tooltip("If True, sticking to a wall requires the actor to be pressing Left or Right into the wall consistently to maintain the cling. If False, they will drop as soon as the Left/Right button is lifted.")]
		public bool clingRequiresDirectionalHold = true;
		[Tooltip("If True, the actor can drop from a wall cling by pressing the d-pad away from the direction of the wall.")]
		public bool canDisengageWithDirectionalPress = true;
		[Tooltip("The speed with which the actor slides down the wall while clinging to it; if 0, the actor will stay rooted in one place while standing still on a wall.")]
		public float wallSlideSpeed = 1.0f;
		[Tooltip("If True, any attacks performed while stuck to a wall will face away from the wall, as opposed to facing into it.")]
		public bool attacksReverseOnWall = true;
		[Tooltip("AnimationClips that play while the actor is stuck to a wall, climbing a wall, or hanging from a ledge.")]
		public Animations animations;
		[Tooltip("If True, the actor will face away from the wall automatically when a wall jump is performed.")]
		public bool faceAwayFromWallOnJump = false;

		protected Substate substate;
		protected bool isClingingToWall = false;
		protected int cooldownFrames = 20;
		protected int currentClingCooldownFrame = 0;
        protected int currentJumpCooldownFrame = 0;
        protected int currentWallJumpGraceFrame = 0;
		protected bool isDropping;
		protected Direction.Horizontal mostRecentWallJumpDirection;
		protected JumpState jumpState;
		protected bool isAgainstRightWall;
		protected bool isAgainstLeftWall;
		protected BoxCollider2D boxCollider;

		void Awake() 
		{
			id = idString;
			isConcurrent = false;
			willPlayAnimationOnBegin = false;
			blockAutoChange.toDefaultOnLanding = false;
			GetController();

			boxCollider = GetComponent<BoxCollider2D>();
		}

		void Update()
		{
			if(!(isEnabled && controller.isEnabled))
			{
				isClingingToWall = false;
				return;
			}

			CheckForWallCling();
		}

		void FixedUpdate()
		{
			if(!isEnabled || (controller.slots.actor.currentAttack != null && controller.slots.actor.currentAttack.cancels.wallClinging))
			{
				currentWallJumpGraceFrame = 0;
				currentClingCooldownFrame = 0;
                currentJumpCooldownFrame = 0;

                isClingingToWall = false;

				return;
			}

			if(controller.StateID() != FallingState.idString && !controller.isKnockbackActive && !controller.isStunned)
			{
				isDropping = false;
			}

			//All of the calculations here are just making sure this doesn't repeat raycasts that the RexPhysics have already done this frame; this won't use CheckForWallContact if it determines RexPhysics already has accurate wall contact data
			float controllerVelocityX = controller.slots.physicsObject.properties.velocity.x + controller.slots.physicsObject.properties.externalVelocity.x;
			isAgainstLeftWall = controller.slots.physicsObject.properties.isAgainstLeftWall;
			if(!isAgainstLeftWall && controllerVelocityX == 0.0f)
			{
				isAgainstLeftWall = controller.slots.physicsObject.CheckForWallContact(Direction.Horizontal.Left);
			}

			isAgainstRightWall = controller.slots.physicsObject.properties.isAgainstRightWall; 
			if(!isAgainstRightWall && controllerVelocityX == 0.0f)
			{
				isAgainstRightWall = controller.slots.physicsObject.CheckForWallContact(Direction.Horizontal.Right);
			}

			CheckForWallCling();

			bool isPressingIntoWall = false;
			if(IsClingCooldownComplete())
			{
				isPressingIntoWall = IsPressingIntoWall();
			}

			float gravityScale = controller.GravityScaleMultiplier();

			BufferZoneInfo bufferZoneInfo_Top = GetBufferZoneInfo(gravityScale, Direction.Vertical.Down);
			BufferZoneInfo bufferZoneInfo_Bottom = GetBufferZoneInfo(gravityScale, Direction.Vertical.Up);
			if(isPressingIntoWall && IsClingAllowed() && !isDropping)
			{
				if(!(IsJumpActive() && !enableClingWhileJumping))
				{
					if(bufferZoneInfo_Top.isLedgeDetected && !bufferZoneInfo_Top.isInBufferZone)
					{
						if((controller.slots.physicsObject.properties.velocity.y * gravityScale) < 0.0f && controller.StateID() != this.id)
						{
							controller.slots.actor.SetPosition(new Vector2(controller.slots.actor.transform.position.x, bufferZoneInfo_Top.bufferCutoff));

							if(ledgeGrab.enableLedgeGrab)
							{
                                Begin();

                                if(substate != Substate.LedgeHanging)
								{
									PlaySecondaryAnimation(animations.ledgeHang);
								}

                                controller.slots.physicsObject.properties.isFalling = false;
                                substate = Substate.LedgeHanging;
							}
						}
					}
					else if(bufferZoneInfo_Bottom.isLedgeDetected && !bufferZoneInfo_Bottom.isInBufferZone && wallClimb.enableClimbing)
					{
						if(!(bufferZoneInfo_Top.isLedgeDetected && bufferZoneInfo_Bottom.isLedgeDetected))
						{
							if((controller.slots.physicsObject.properties.velocity.y * gravityScale) > 0.0f && controller.StateID() != this.id)
							{
								controller.slots.actor.SetPosition(new Vector2(controller.slots.actor.transform.position.x, bufferZoneInfo_Bottom.bufferCutoff));
							}
						}
					}

				}
			}

			float climbableArea = boxCollider.size.y;
			if(bufferZoneInfo_Top.isLedgeDetected && bufferZoneInfo_Bottom.isLedgeDetected)
			{
				climbableArea = (bufferZoneInfo_Top.bufferCutoff - bufferZoneInfo_Bottom.bufferCutoff) * gravityScale;
			}

			//Test to see if we're on the very bottom of a wall and drop if so. (Commented out due to buginess.)
			/*if((bufferZoneInfo_Top.isLedgeDetected && bufferZoneInfo_Top.isInBufferZone && controller.slots.physicsObject.properties.velocity.y * gravityScale < 0.0f) || (bufferZoneInfo_Bottom.isLedgeDetected && bufferZoneInfo_Bottom.isInBufferZone && controller.slots.physicsObject.properties.velocity.y * gravityScale >= 0.0f))
			{
				bool isInBothBufferZones = false;
				if(bufferZoneInfo_Top.isLedgeDetected && bufferZoneInfo_Bottom.isLedgeDetected)
				{
					climbableArea = (bufferZoneInfo_Top.bufferCutoff - bufferZoneInfo_Bottom.bufferCutoff) * gravityScale;
					if(climbableArea < 0.0f && ((transform.position.y <= bufferZoneInfo_Top.bufferCutoff && gravityScale > 0.0f) || (transform.position.y >= bufferZoneInfo_Top.bufferCutoff && gravityScale < 0.0f)))
					{
						isInBothBufferZones = true;
					}
				}

				if(!isInBothBufferZones || !IsClingCooldownComplete() || isDropping) //Ordinarily, disengage from the wall; however, if we're in a buffer zone only because the entire wall is a buffer zone, don't disengage; allow us to stay at the top
				{
					isClingingToWall = false;
				}
			}*/

			if(wallSlideSpeed == 0.0f && !wallClimb.enableClimbing && substate != Substate.LedgeHanging)
			{
				isClingingToWall = false;
			}

            if(controller.currentState == this && substate == Substate.LedgeHanging)
            {
                isClingingToWall = true;
            }

            if(isClingingToWall)
			{
				currentWallJumpGraceFrame = wallJump.wallJumpGraceFrames;
				if(controller.StateID() != idString && !(IsJumpActive() && !enableClingWhileJumping))
				{
					if(controller.StateID() != LadderState.idString && IsClingCooldownComplete() && !bufferZoneInfo_Top.isInBufferZone)
					{
						if(!(bufferZoneInfo_Bottom.isLedgeDetected && bufferZoneInfo_Bottom.isInBufferZone && wallClimb.enableClimbing) || climbableArea < 0.0f)
						{
							if(substate == Substate.Stopped)
							{
								substate = Substate.None;
							}

							Begin();
							controller.slots.physicsObject.SetVelocityX(0);
						}
					}
				}

				if(controller.currentState == this)
				{
					controller.aerialPeak = transform.position.y;
					if(Mathf.Abs(wallSlideSpeed) > 0.0f)
					{
						controller.slots.physicsObject.FreezeGravityForSingleFrame();
						controller.slots.physicsObject.SetVelocityY(wallSlideSpeed * gravityScale * -1.0f);

						if(!wallClimb.enableClimbing && substate != Substate.LedgeHanging && !controller.slots.actor.IsAttacking())
						{
							if(substate != Substate.Sliding && substate != Substate.LedgeHanging)
							{
								PlayAnimation();
							}

							substate = Substate.Sliding;
						}
					}

					if(wallClimb.enableClimbing || substate == Substate.LedgeHanging)
					{
						controller.slots.physicsObject.FreezeGravityForSingleFrame();
						controller.slots.physicsObject.SetVelocityY(0.0f);
					}
				}

				if(wallClimb.enableClimbing && Mathf.Abs(controller.slots.input.verticalAxis) > 0.0f && controller.StateID() == idString)
				{
					float climbVelocity = controller.slots.input.verticalAxis * wallClimb.climbSpeed * gravityScale;
					if(climbableArea < boxCollider.size.y)
					{
						climbVelocity = 0.0f;
					}

					if(bufferZoneInfo_Top.isLedgeDetected)
					{
						if((gravityScale < 0.0f && !controller.slots.physicsObject.CheckForCeilingFloorContact(Direction.Vertical.Down, true) && transform.position.y + (climbVelocity * PhysicsManager.Instance.fixedDeltaTime) < bufferZoneInfo_Top.bufferCutoff) || (gravityScale > 0.0f && !controller.slots.physicsObject.CheckForCeilingFloorContact(Direction.Vertical.Up, true) && transform.position.y + (climbVelocity * PhysicsManager.Instance.fixedDeltaTime) > bufferZoneInfo_Top.bufferCutoff))
						{
							controller.slots.actor.SetPosition(new Vector2(controller.slots.actor.transform.position.x, bufferZoneInfo_Top.bufferCutoff));
							climbVelocity = 0.0f;

							if(ledgeGrab.enableLedgeGrab)
							{
                                Begin();

                                if (substate != Substate.LedgeHanging && !controller.slots.actor.IsAttacking())
								{
									PlaySecondaryAnimation(animations.ledgeHang);
								}

                                controller.slots.physicsObject.properties.isFalling = false;
                                substate = Substate.LedgeHanging;
							}
						}
					}
					else if(bufferZoneInfo_Bottom.isLedgeDetected)
					{
						if((gravityScale < 0.0f && transform.position.y + (climbVelocity * PhysicsManager.Instance.fixedDeltaTime) > bufferZoneInfo_Bottom.bufferCutoff) || (gravityScale > 0.0f && transform.position.y + (climbVelocity * PhysicsManager.Instance.fixedDeltaTime) < bufferZoneInfo_Bottom.bufferCutoff))
						{
							controller.slots.actor.SetPosition(new Vector2(controller.slots.actor.transform.position.x, bufferZoneInfo_Bottom.bufferCutoff));
							climbVelocity = 0.0f;
						}
					}

					if(controller.currentState == this)
					{
						controller.slots.physicsObject.SetVelocityY(climbVelocity);
					}

					if(!(substate == Substate.LedgeHanging && controller.slots.input.verticalAxis > 0.0f))
					{
						if(substate != Substate.Climbing && !controller.slots.actor.IsAttacking())
						{
							PlaySecondaryAnimation(animations.climbWallMoving);
						}

						substate = Substate.Climbing;
					}
				}
				else if(wallClimb.enableClimbing)
				{
					if(!bufferZoneInfo_Top.isInBufferZone && substate != Substate.LedgeHanging && substate != Substate.Sliding)
					{
						if(substate != Substate.Stopped && !controller.slots.actor.IsAttacking() && controller.currentState == this)
						{
							PlaySecondaryAnimation(animations.climbWallStopped);
						}

						substate = Substate.Stopped;
					}
				}
			}
			else
			{
				if(controller.StateID() == this.id)
				{
					controller.SetStateToDefault();
				}
			}

			UpdateCooldownFrames();
		}

		#region override public methods

		public override void OnBegin()
		{
            if(controller.slots.actor.currentAttack != null && controller.slots.actor.currentAttack.canceledBy.onWallCling)
            {
                controller.slots.actor.currentAttack.Cancel();
            }
        }

		public override void OnNewStateAdded(RexState _state)
		{
			if(_state.id == JumpState.idString && jumpState == null)
			{
				jumpState = _state as JumpState;
			}	
		}

		public override void PlayAnimationForSubstate()
		{
			AnimationClip animationToPlay = animation;
			switch(substate)
			{
				case Substate.Climbing:
					animationToPlay = animations.climbWallMoving;
					break;
				case Substate.Stopped:
					animationToPlay = animations.climbWallStopped;
					break;
				case Substate.Sliding:
					animationToPlay = animation;
					break;
				case Substate.LedgeHanging:
					animationToPlay = animations.ledgeHang;
					break;
			}

			PlaySecondaryAnimation(animationToPlay);
		}

		public override void OnEnded()
		{
			isClingingToWall = false;
			substate = Substate.None;
		}

		public override void OnStateChanged()
		{
			isClingingToWall = false;
			substate = Substate.None;

			if(controller.StateID() == DashState.idString)
			{
				ResetCooldownFrames();
            }
        }

		#endregion

		public void ResetCooldownFrames()
		{
			currentClingCooldownFrame = cooldownFrames;
			currentJumpCooldownFrame = cooldownFrames;
		}

		public bool IsWallJumpPossible()
		{
			bool isWallJumpPossible = false;
			if(wallJump.enableWallJump && currentWallJumpGraceFrame > 0)
			{
				isWallJumpPossible = true;
			}

			if(substate == Substate.LedgeHanging)
			{
				isWallJumpPossible = true;
			}

			return isWallJumpPossible;
		}

		protected BufferZoneInfo GetBufferZoneInfo(float gravityScale, Direction.Vertical verticalDirection)
		{
			BufferZoneInfo bufferZoneInfo = new BufferZoneInfo();
			bool willSnapToLedgeHeight = wallClimb.enableClimbing || ledgeGrab.enableLedgeGrab;
			if(willSnapToLedgeHeight)
			{
				RaycastHelper.LedgeInfo ledgeInfo = RaycastHelper.DetectLedgeOnWall(controller.direction.horizontal, (Direction.Vertical)((int)verticalDirection * gravityScale), boxCollider, controller.slots.physicsObject.properties.velocity.y * PhysicsManager.Instance.fixedDeltaTime * -(int)verticalDirection);
				if(!ledgeInfo.didHit)
				{
					ledgeInfo = RaycastHelper.DetectLedgeOnWall(controller.direction.horizontal, (Direction.Vertical)((int)verticalDirection * gravityScale), boxCollider, controller.slots.physicsObject.properties.velocity.y * PhysicsManager.Instance.fixedDeltaTime * -(int)verticalDirection, 0.0f);
				}

				if(ledgeInfo.didHit)
				{
					bufferZoneInfo.isLedgeDetected = true;
					bufferZoneInfo.bufferCutoff = ledgeInfo.hitY - (boxCollider.size.y * 0.5f * gravityScale * -(int)verticalDirection);
					float adjustedPositionY = transform.position.y + controller.slots.physicsObject.properties.velocity.y * PhysicsManager.Instance.fixedDeltaTime * -(int)verticalDirection;
					if(verticalDirection == Direction.Vertical.Down && ((gravityScale < 0.0f && adjustedPositionY < bufferZoneInfo.bufferCutoff) || (gravityScale > 0.0f && adjustedPositionY > bufferZoneInfo.bufferCutoff))) //At top of wall
					{
						bufferZoneInfo.isInBufferZone = true;
					}
					else if(verticalDirection == Direction.Vertical.Up && ((gravityScale < 0.0f && adjustedPositionY > bufferZoneInfo.bufferCutoff) || (gravityScale > 0.0f && adjustedPositionY < bufferZoneInfo.bufferCutoff)))
					{
						bufferZoneInfo.isInBufferZone = true;
					}
				}
			}

			return bufferZoneInfo;
		}

		protected void UpdateCooldownFrames()
		{
			currentClingCooldownFrame --;
			if(currentClingCooldownFrame < 0)
			{
				currentClingCooldownFrame = 0;
			}

            currentJumpCooldownFrame--;
            if (currentJumpCooldownFrame < 0)
            {
                currentJumpCooldownFrame = 0;
            }

            currentWallJumpGraceFrame --;
			if(currentWallJumpGraceFrame < 0)
			{
				currentWallJumpGraceFrame = 0;
			}
		}

		protected bool IsClingAllowed()
		{
			bool isClingAllowed = true;
			if(!IsClingCooldownComplete())
			{
				isClingAllowed = false;
			}

			if(isDropping)
			{
				if(!(isAgainstRightWall && controller.axis.x == 1.0f) || (isAgainstLeftWall && controller.axis.x == -1.0f))
				{
					isClingAllowed = false;
				}
			}

			return isClingAllowed;
		}

		protected bool IsPressingIntoWall()
		{
			bool isPressingIntoWall = false;
			if(isAgainstRightWall && controller.direction.horizontal == Direction.Horizontal.Right && (controller.axis.x == 1.0f || !clingRequiresDirectionalHold))
			{
				isPressingIntoWall = true;
			}
			else if(isAgainstLeftWall && controller.direction.horizontal == Direction.Horizontal.Left && (controller.axis.x == -1.0f || !clingRequiresDirectionalHold))
			{
				isPressingIntoWall = true;
			}

			if(controller.slots.physicsObject.IsAgainstEitherWall() && willFaceWallOnCling)
			{
				isPressingIntoWall = true;
				if((isAgainstRightWall && controller.direction.horizontal == Direction.Horizontal.Left) || (isAgainstLeftWall && controller.direction.horizontal == Direction.Horizontal.Right) && controller.StateID() != DashState.idString)
				{
					controller.Turn();
				}
			}

			if(clingRequiresDirectionalHold && controller.slots.input.horizontalAxis == 0.0f)
			{
				isPressingIntoWall = false;
			}

			return isPressingIntoWall;
		}

		protected void CheckForWallCling()
		{
			if(controller.StateID() == KnockbackState.idString || controller.isStunned)
			{
				isClingingToWall = false;
				return;
			}

			if(requireSpecificTagOnWalls.requiresSpecificTag && controller.slots.physicsObject.properties.wallTag != requireSpecificTagOnWalls.tag)
			{
				isClingingToWall = false;
				return;
			}

			if(isAgainstRightWall)
			{
				mostRecentWallJumpDirection = Direction.Horizontal.Left;
			}
			else if(isAgainstLeftWall)
			{
				mostRecentWallJumpDirection = Direction.Horizontal.Right;
			}

			isClingingToWall = false;
			if(!controller.slots.physicsObject.IsOnSurface() && IsClingCooldownComplete())
			{
				if(IsPressingIntoWall() && IsClingAllowed() && !(controller.StateID() == JumpState.idString && !enableClingWhileJumping) && !(clingRequiresDirectionalHold && controller.slots.input.horizontalAxis == 0.0f))
				{
					isClingingToWall = true;
					currentWallJumpGraceFrame = wallJump.wallJumpGraceFrames;
				}
			}

			if(controller.slots.input.isJumpButtonDownThisFrame)
			{
				bool canJump = false;
				if((((isAgainstRightWall || isAgainstLeftWall)) || currentWallJumpGraceFrame > 0) && wallJump.enableWallJump && (!IsJumpActive() || enableClingWhileJumping) && !controller.slots.physicsObject.IsOnSurface() && substate != Substate.LedgeHanging && IsJumpCooldownComplete())
				{
					canJump = true;
				}
				else if(substate == Substate.LedgeHanging && ledgeGrab.canLedgeJump && !controller.slots.physicsObject.IsOnSurface())
				{
					canJump = true;
                }

                if(wallJump.requireHorizontalPressForJump && canJump)
				{
                    bool isPressingInOppositeDirection = false;
					if((controller.slots.physicsObject.CheckForWallContact(Direction.Horizontal.Right) && controller.slots.input.horizontalAxis == -1.0f) || (controller.slots.physicsObject.CheckForWallContact(Direction.Horizontal.Left) && controller.slots.input.horizontalAxis == 1.0f))
					{
                        isPressingInOppositeDirection = true;
					}

                    if((mostRecentWallJumpDirection == Direction.Horizontal.Right && currentWallJumpGraceFrame > 0 && controller.slots.input.horizontalAxis == 1.0f) || (mostRecentWallJumpDirection == Direction.Horizontal.Left && currentWallJumpGraceFrame > 0 && controller.slots.input.horizontalAxis == -1.0f))
                    {
                        isPressingInOppositeDirection = true;
                    }

                    canJump = isPressingInOppositeDirection;
                }

				if(canJump)
				{
					if(jumpState == null)
					{
						jumpState = controller.GetComponent<JumpState>();
					}

					if(jumpState)
					{
						if(faceAwayFromWallOnJump)
						{
							controller.Turn();
						}

						currentJumpCooldownFrame = cooldownFrames;
						jumpState.NotifyOfWallJump(wallJump.wallJumpKickbackFrames, mostRecentWallJumpDirection, wallJump.speed);

						isClingingToWall = false;
					}
				}
				else if(!(wallJump.enableWallJump && substate != Substate.LedgeHanging) || (substate == Substate.LedgeHanging && !ledgeGrab.canLedgeJump) && (wallClimb.enableClimbing || substate == Substate.LedgeHanging))
				{
					if(isClingingToWall)
					{
						currentClingCooldownFrame = cooldownFrames;

						isClingingToWall = false;

						if(substate != Substate.None)
						{
							isDropping = true;

							if(jumpState)
							{
								jumpState.BlockForFrame();
							}
						}

						if(controller.StateID() == id)
						{
							controller.SetStateToDefault();
						}
					}
				}
			}

			bool isPressingAwayFromWall = canDisengageWithDirectionalPress && ((isAgainstRightWall && controller.axis.x == -1.0f) || (isAgainstLeftWall && controller.axis.x == 1.0f));
			if(!isPressingAwayFromWall)
			{
				isPressingAwayFromWall = clingRequiresDirectionalHold && ((isAgainstRightWall && controller.axis.x <= 0.0f) || (isAgainstLeftWall && controller.axis.x >= 0.0f));
			}

			if(isPressingAwayFromWall && controller.StateID() == id)
			{
				currentClingCooldownFrame = cooldownFrames;

				isClingingToWall = false;
				controller.SetStateToDefault();
			}

			if(isClingingToWall)
			{
				isDropping = false;
			}
		}

		protected bool IsJumpActive()
		{
			bool isJumpActive = false;
			if(jumpState == null)
			{
				jumpState = GetComponent<JumpState>();
			}

			if(jumpState)
			{
				isJumpActive = jumpState.IsJumpActive();
			}

			return isJumpActive;
		}

		protected bool IsJumpCooldownComplete()
		{
			return (currentJumpCooldownFrame <= 0);
		}

        protected bool IsClingCooldownComplete()
        {
            return (currentClingCooldownFrame <= 0);
        }
    }
}
