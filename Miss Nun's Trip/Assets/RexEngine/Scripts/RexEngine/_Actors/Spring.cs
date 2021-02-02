using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class Spring:RexActor 
	{
		public SpringAnimations springAnimations;
		public string actorAnimationName;
		public AudioClip bounceSound;
		public float bounceSpeed = 40.0f;
		public int bounceFrames = 18;

		[System.Serializable]
		public class SpringAnimations
		{
			public AnimationClip defaultAnimation;
			public AnimationClip bounceAnimation;
		}

		void Awake() 
		{

		}

		void Start() 
		{

		}

		protected IEnumerator AnimateBounceCoroutine()
		{
			if(bounceSound != null)
			{
				PlaySoundIfOnCamera(bounceSound);
			}

			if(slots.anim != null)
			{
				if(springAnimations.bounceAnimation)
				{
					slots.anim.Play(springAnimations.bounceAnimation.name, 0, 0);
					yield return new WaitForSeconds(springAnimations.bounceAnimation.length);

					if(springAnimations.defaultAnimation)
					{
						slots.anim.Play(springAnimations.defaultAnimation.name, 0, 0);
					}
				}
			}
		}

		protected void AnimateActor(RexActor actor)
		{
			if(actorAnimationName == "")
			{
				return;
			}

			Animator anim = actor.slots.anim;
			RexController controller = actor.slots.controller;

			if(anim != null && controller != null)
			{
				AnimationClip actorAnimation = null;
				AnimationClip[] animationClips = anim.runtimeAnimatorController.animationClips;
				for(int i = 0; i < animationClips.Length; i ++)
				{
					if(animationClips[i].name == actorAnimationName)
					{
						actorAnimation = animationClips[i];
					}
				}

				if(actorAnimation != null)
				{
					controller.PlaySingleAnimation(actorAnimation, true);
				}
			}
		}

		//This can be overidden by individual objects to have them do unique things when another RexPhysics component collides with it
		public override void NotifyOfCollisionWithPhysicsObject(Collider2D col, Side side, CollisionType type)
		{
			float gravityScaleMultiplier = PhysicsManager.Instance.gravityScale > 0.0f ? 1.0f : -1.0f;
			if((side == Side.Top && gravityScaleMultiplier == 1.0f) || (side == Side.Bottom && gravityScaleMultiplier == -1.0f))
			{
				RexActor actor = col.GetComponent<RexActor>();
				if(actor != null)
				{
					RexPhysics physicsObject = actor.slots.physicsObject;
					if(physicsObject != null)
					{
						AnimateActor(actor);

						actor.SetPosition(new Vector2(actor.transform.position.x, actor.transform.position.y + (0.01f * gravityScaleMultiplier)));

						if(gravityScaleMultiplier == 1.0f)
						{
							physicsObject.properties.isGrounded = false;
						}
						else if(gravityScaleMultiplier == -1.0f)
						{
							physicsObject.properties.isAgainstCeiling = false;
						}

						physicsObject.ApplyForce(new Vector2(0.0f, bounceSpeed * gravityScaleMultiplier), bounceFrames, false, true);

						StopCoroutine("AnimateBounceCoroutine");
						StartCoroutine("AnimateBounceCoroutine");
					}
				}
			}
		}
	}
}

