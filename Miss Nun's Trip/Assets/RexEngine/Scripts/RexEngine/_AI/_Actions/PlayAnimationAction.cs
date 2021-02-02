using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class PlayAnimationAction:RexAIAction 
	{
		public PlayType playType;
		public AnimationClip animation;
		public LoopType loopType;
		public TargetType targetType;
		public RexActor otherActor;

		public enum LoopType
		{
			Single,
			Looping
		}

		public enum TargetType
		{
			Self,
			Player,
			Other
		}

		public enum PlayType
		{
			PlayNew,
			StopCurrent
		}

		public override void Begin()
		{
			RexActor actor = aiRoutine.slots.controller.slots.actor;
			if(targetType == TargetType.Player)
			{
				actor = GameManager.Instance.player;
			}
			else if(targetType == TargetType.Other)
			{
				actor = otherActor;
			}

			RexController controller = actor.slots.controller;

			if(controller == null)
			{
				End();

				return;
			}

			if(playType == PlayType.PlayNew)
			{
				if(loopType == LoopType.Single)
				{
					controller.PlaySingleAnimation(animation);
				}
				else if(loopType == LoopType.Looping)
				{
					controller.PlayLoopingAnimation(animation);
				}
			}
			else if(playType == PlayType.StopCurrent)
			{
				controller.StopLoopingAnimation();
			}

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);

			playType = (PlayType)EditorGUILayout.EnumPopup("Play Type", playType);

			if(playType == PlayType.PlayNew)
			{
				animation = EditorGUILayout.ObjectField("Animation", animation, typeof(AnimationClip), true) as AnimationClip;
				loopType = (LoopType)EditorGUILayout.EnumPopup("Loop Type", loopType);
			}
			else
			{
				EditorGUILayout.LabelField("This will stop AnimationClips that were initiated using a Rex AI Play Animation action. It will not stop animations initiated as part of a RexState or ability.", EditorStyles.helpBox);
			}

			targetType = (TargetType)EditorGUILayout.EnumPopup("Target Type", targetType);

			if(targetType == TargetType.Other)
			{
				otherActor = EditorGUILayout.ObjectField("Other Actor", otherActor, typeof(RexActor), true) as RexActor;
			}
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.PlayAnimation;
		}

		public override string GetName()
		{
			return "Play Animation";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(PlayAnimationAction))]
	public class PlayAnimationActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}
