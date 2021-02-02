using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class TimerEvent:RexAIEvent 
	{
		public bool randomizeTime;
		public float time;
		public float lowSeconds;
		public float highSeconds;

		protected float currentTime;

		void Awake() 
		{

		}

		void Start() 
		{

		}

		public override void Enable()
		{
			base.Enable();

			currentTime = 0.0f;
			if(randomizeTime)
			{
				time = RexMath.RandomFloat(lowSeconds, highSeconds);
			}
		}

		public override void Disable()
		{
			base.Disable();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			randomizeTime = EditorGUILayout.Toggle("Randomize Time", randomizeTime);

			if(!randomizeTime)
			{
				time = EditorGUILayout.FloatField("Seconds", time);
			}
			else
			{
				lowSeconds = EditorGUILayout.FloatField("Seconds (Lowest)", lowSeconds);
				highSeconds = EditorGUILayout.FloatField("Seconds (Highest)", highSeconds);
			}
		}
		#endif

		public override string GetName()
		{
			return "Timer";
		}

		public override void CheckEventStatus() 
		{
			currentTime += Time.deltaTime;
			if(currentTime >= time)
			{
				OnEventActivated();
			}
		}

		public override EventType GetEventType()
		{
			return EventType.Timer;
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(TimerEvent))]
	public class TimerEventEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

