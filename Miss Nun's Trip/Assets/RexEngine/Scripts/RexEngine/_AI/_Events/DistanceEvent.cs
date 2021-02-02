using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class DistanceEvent:RexAIEvent 
	{
		public UseDimensions useDimensions;

		public Vector2 distance;
		public DistanceOperator distanceOperator;
		public FindTargetData findTargetData;

		protected FindTargetMethods findTargetMethods;

		void Awake() 
		{
			findTargetMethods = gameObject.AddComponent<FindTargetMethods>(); 
		}

		void Start() 
		{
			if(findTargetData.targetType != FindTargetData.TargetType.Transform)
			{
				findTargetData.otherTransform = null;
			}

			findTargetMethods.GetTarget(findTargetData);
		}

		public override void Enable()
		{
			base.Enable();
		}

		public override void Disable()
		{
			base.Disable();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			if(findTargetData == null)
			{
				findTargetData = new FindTargetData();
			}

			findTargetData.DrawInspectorGUI();

			useDimensions = (UseDimensions)EditorGUILayout.EnumPopup("Use Dimensions", useDimensions);
			distanceOperator = (DistanceOperator)EditorGUILayout.EnumPopup("Operator", distanceOperator);

			if(useDimensions == UseDimensions.XAndY)
			{
				distance = EditorGUILayout.Vector2Field("Distance", distance);
			}
			else
			{
				if(useDimensions == UseDimensions.X)
				{
					distance.x = EditorGUILayout.FloatField("X", distance.x);
				}
				else if(useDimensions == UseDimensions.Y)
				{
					distance.y = EditorGUILayout.FloatField("Y", distance.y);
				}
			}

			float minimum = 0.1f;
			if(distance.x < minimum)
			{
				distance.x = minimum;
			}

			if(distance.y < minimum)
			{
				distance.y = minimum;
			}
		}
		#endif

		public override string GetName()
		{
			return "Distance";
		}

		public override void CheckEventStatus() 
		{
			if(findTargetData.otherTransform == null)
			{
				return;
			}

			bool isWithinDistance = false;

			Vector2 currentDistance = new Vector2();
			currentDistance.x = Mathf.Abs(transform.position.x - findTargetData.otherTransform.position.x);
			currentDistance.y = Mathf.Abs(transform.position.y - findTargetData.otherTransform.position.y);

			if(useDimensions == UseDimensions.XAndY)
			{
				isWithinDistance = (currentDistance.x <= distance.x && currentDistance.y <= distance.y);
			}
			else if(useDimensions == UseDimensions.X)
			{
				isWithinDistance = currentDistance.x <= distance.x;
			}
			else if(useDimensions == UseDimensions.Y)
			{
				isWithinDistance = currentDistance.y <= distance.y;
			}

			if((isWithinDistance && distanceOperator == DistanceOperator.Into) || (!isWithinDistance && distanceOperator == DistanceOperator.OutOf))
			{
				OnEventActivated();
			}
		}

		public override EventType GetEventType()
		{
			return EventType.Distance;
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(DistanceEvent))]
	public class DistanceEventEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

