using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class RexAIEvent:MonoBehaviour
	{
		public RexAIMovement newMovement;
		public Attack attack;

		public RexAIRoutine aiRoutine;
		public RexAIActionEventSet actionEventSet;

		public bool isEnabled;

		protected bool hasEventActivated;

		public enum EventType
		{
			Damaged,
			Distance,
			Physics,
			TargetFacing,
			Timer
		}

		void Awake() 
		{

		}

		void Start() 
		{

		}

		public virtual void Enable()
		{
			isEnabled = true;
			hasEventActivated = false;
		}

		public virtual void Disable()
		{
			isEnabled = false;
		}

		public virtual void DrawInspectorGUI()
		{

		}

		public virtual string GetName()
		{
			return "Event";
		}

		public virtual void CheckEventStatus()
		{
			
		}

		public virtual EventType GetEventType()
		{
			return EventType.Timer;
		}

		protected virtual void OnEventActivated()
		{
			hasEventActivated = true;
			if(actionEventSet != null)
			{
				actionEventSet.OnEventActivated();
			}
		}
	}
}
