using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class RexAIAction:MonoBehaviour 
	{
		protected RexAIRoutine aiRoutine;

		public enum ActionType
		{
			Wait,
			Ability,
			Attack,
			ChangeMovement,
			ToggleSequence,
			ToggleEvent,
			FaceTarget,
			FaceDirection,
			ChangeAIRoutine,
			ChangePhysics,
			PlayAnimation,
			CallFunction,
			SetEnergy,
			ToggleInvincibility,
			Audio,
			System
		}

		void Awake() 
		{

		}

		void Start() 
		{

		}

		public virtual void Begin()
		{
			
		}

		public virtual void End()
		{
			OnEnded();
		}

		public virtual void OnBegin()
		{
			
		}

		public virtual void OnEnded()
		{
			if(aiRoutine)
			{
				aiRoutine.NotifyOfActionEnded(this);
			}
		}

		public void SetAIRoutine(RexAIRoutine _routine)
		{
			aiRoutine = _routine;
		}

		protected IEnumerator WaitOneFrameAndEndCoroutine()
		{
			yield return new WaitForSeconds(0.01f);

			End();
		}

		public virtual void DrawInspectorGUI()
		{

		}

		public virtual ActionType GetActionType()
		{
			return ActionType.Wait;
		}

		public virtual string GetName()
		{
			return "Wait";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(RexAIAction))]
	public class RexAIActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}
