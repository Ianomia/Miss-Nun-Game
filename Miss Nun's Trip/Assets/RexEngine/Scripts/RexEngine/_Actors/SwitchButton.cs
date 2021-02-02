using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace RexEngine
{
	public class SwitchButton:RexActor 
	{
		public WorldActionSequenceSet worldActionSequenceSet;
		public int currentSwitchActivationEffect = 0;
		public LoopType loopType;
		public ActivationType activationType;
		public float timeBeforeReEnable = 1.5f;

		public AnimationClip pressDownAnimation;
		public AnimationClip riseUpAnimation;
		public AnimationClip defaultUpAnimation;
		public AnimationClip defaultDownAnimation;

		public AudioClip pressSound;
		public AudioClip riseSound;

		public enum ActivationType
		{
			Touch,
			BounceOn,
			StandOn,
			JumpUnder,
			Attack
		}

		public enum LoopType
		{
			OnceThrough,
			Looping
		}

		protected bool hasActivated;

		void Start()
		{
			if(activationType == ActivationType.StandOn)
			{
				gameObject.layer = LayerMask.NameToLayer("PassThroughBottom");
			}
			else if(activationType == ActivationType.BounceOn)
			{
				canBounceOn = true;
			}
			else if(activationType == ActivationType.JumpUnder)
			{
				gameObject.layer = LayerMask.NameToLayer("Terrain");
			}

			if(slots.anim && defaultUpAnimation)
			{
				slots.anim.Play(defaultUpAnimation.name, 0, 0);
			}
		}

		public virtual void Activate()
		{
			if(pressSound)
			{
				PlaySoundIfOnCamera(pressSound);
			}

			hasActivated = true;
			ScreenShake.Instance.Shake();

			StartCoroutine("AnimatePressCoroutine");

			StartCoroutine("ReEnableCoroutine");

			TriggerEffect();
		}

		protected IEnumerator AnimatePressCoroutine()
		{
			if(slots.anim && pressDownAnimation)
			{
				slots.anim.Play(pressDownAnimation.name, 0, 0);

				yield return new WaitForSeconds(pressDownAnimation.length);
			}

			if(slots.anim && defaultDownAnimation)
			{
				slots.anim.Play(defaultDownAnimation.name, 0, 0);
			}
		}

		protected IEnumerator AnimateRiseCoroutine()
		{
			if(slots.anim && riseUpAnimation)
			{
				slots.anim.Play(riseUpAnimation.name, 0, 0);

				yield return new WaitForSeconds(riseUpAnimation.length);
			}

			if(slots.anim && defaultUpAnimation)
			{
				slots.anim.Play(defaultUpAnimation.name, 0, 0);
			}
		}

		public virtual void TriggerEffect()
		{
			if(worldActionSequenceSet != null && worldActionSequenceSet.worldActionSequences != null && worldActionSequenceSet.worldActionSequences.Count > 0)
			{
				WorldActionSequence switchActivationEffect = worldActionSequenceSet.worldActionSequences[currentSwitchActivationEffect];
				for(int i = 0; i < switchActivationEffect.worldActions.Count; i  ++)
				{
					switchActivationEffect.worldActions[i].TriggerEffect();
				}

				currentSwitchActivationEffect ++;
				if(loopType == LoopType.Looping && currentSwitchActivationEffect >= worldActionSequenceSet.worldActionSequences.Count)
				{
					currentSwitchActivationEffect = 0;
				}
			}
		}

		public override void OnBouncedOn(Collider2D col = null)
		{
			if(!hasActivated && activationType == ActivationType.BounceOn)
			{
				Activate();
			}
		}

		public override void NotifyOfCollisionWithPhysicsObject(Collider2D col, Side side, CollisionType type)
		{
			if(!hasActivated)
			{
				if(activationType == ActivationType.StandOn && col.tag == "Player")
				{
					float gravityScale = PhysicsManager.Instance.gravityScale;
					if(side == Side.Top && gravityScale >= 0.0f || side == Side.Bottom && gravityScale < 0.0f)
					{
						Activate();
					}
				}
				else if(activationType == ActivationType.JumpUnder && col.tag == "Player")
				{
					float gravityScale = PhysicsManager.Instance.gravityScale;
					if(side == Side.Bottom && gravityScale >= 0.0f || side == Side.Top && gravityScale < 0.0f)
					{
						Activate();
					}
				}
			}
		}

		protected IEnumerator ReEnableCoroutine()
		{
			yield return new WaitForSeconds(timeBeforeReEnable);

			if(!(loopType == LoopType.OnceThrough && currentSwitchActivationEffect >= worldActionSequenceSet.worldActionSequences.Count))
			{
				EnableSwitch();
				StartCoroutine("AnimateRiseCoroutine");
			}
		}

		protected void EnableSwitch()
		{
			hasActivated = false;
		}

		protected new void OnTriggerEnter2D(Collider2D col)
		{
			if(!hasActivated)
			{
				if(activationType == ActivationType.Touch && col.tag == "Player")
				{
					Activate();
				}
				else if(activationType == ActivationType.Attack && col.GetComponent<Attack>() != null)
				{
					Activate();
				}
			}
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(SwitchButton))]
	public class SwitchButtonEditor:Editor 
	{
		protected bool showSlots;
		protected bool showAesthetics;

		protected InspectorHelper.AIInspectorStyles styles;

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			//base.DrawDefaultInspector();

			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			SwitchButton _target = target as SwitchButton;

			showSlots = EditorGUILayout.Foldout(showSlots, "Slots");
			if(showSlots)
			{
				EditorGUI.indentLevel ++;

				_target.slots.anim = (Animator)EditorGUILayout.ObjectField("Animator", _target.slots.anim, typeof(Animator), true);
				_target.slots.spriteRenderer = (SpriteRenderer)EditorGUILayout.ObjectField("Sprite Renderer", _target.slots.spriteRenderer, typeof(SpriteRenderer), true);
				_target.slots.physicsObject = (RexPhysics)EditorGUILayout.ObjectField("Physics Object", _target.slots.physicsObject, typeof(RexPhysics), true);
				_target.worldActionSequenceSet = (WorldActionSequenceSet)EditorGUILayout.ObjectField("World Action Sequence Set", _target.worldActionSequenceSet, typeof(WorldActionSequenceSet), true);

				EditorGUI.indentLevel --;
			}

			showAesthetics = EditorGUILayout.Foldout(showAesthetics, "Aesthetics");
			if(showAesthetics)
			{
				EditorGUI.indentLevel ++;

				_target.pressSound = (AudioClip)EditorGUILayout.ObjectField("Press Sound", _target.pressSound, typeof(AudioClip), false);
				_target.riseSound = (AudioClip)EditorGUILayout.ObjectField("Rise Sound", _target.riseSound, typeof(AudioClip), false);

				_target.defaultUpAnimation = (AnimationClip)EditorGUILayout.ObjectField("Default Up Animation", _target.defaultUpAnimation, typeof(AnimationClip), false);
				_target.defaultDownAnimation = (AnimationClip)EditorGUILayout.ObjectField("Default Down Animation", _target.defaultDownAnimation, typeof(AnimationClip), false);
				_target.pressDownAnimation = (AnimationClip)EditorGUILayout.ObjectField("Press Down Animation", _target.pressDownAnimation, typeof(AnimationClip), false);
				_target.riseUpAnimation = (AnimationClip)EditorGUILayout.ObjectField("Rise Up Animation", _target.riseUpAnimation, typeof(AnimationClip), false);

				EditorGUI.indentLevel --;
			}

			_target.activationType = (SwitchButton.ActivationType)EditorGUILayout.EnumPopup("Activation Type", _target.activationType);
			_target.loopType = (SwitchButton.LoopType)EditorGUILayout.EnumPopup("Loop Type", _target.loopType);

			if(_target.worldActionSequenceSet != null && _target.worldActionSequenceSet.worldActionSequences != null && _target.worldActionSequenceSet.worldActionSequences.Count > 1)
			{
				_target.timeBeforeReEnable = EditorGUILayout.FloatField("Time Before Re-Enable", _target.timeBeforeReEnable);
			}

			if(!Application.isPlaying && EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}
	}
	#endif
}
