using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class DistanceBranch:RexAIBranch 
	{
		public UseDimensions useDimensions;
		public DistanceOperator distanceOperator;

		public Vector2 distance;
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

		public override string ID()
		{
			return "Distance";
		}

		public override bool Determine(RexAIRoutine _routine = null)
		{
			if(findTargetData.otherTransform == null)
			{
				findTargetData.otherTransform = GameManager.Instance.players[0].transform;
			}

			if(findTargetData.otherTransform == null)
			{
				return false;
			}

			bool isWithinDistance = false;

			Vector2 currentDistance = new Vector2();
			currentDistance.x = Mathf.Abs(transform.position.x - findTargetData.otherTransform.position.x);
			currentDistance.y = Mathf.Abs(transform.position.y - findTargetData.otherTransform.position.y);

			if(useDimensions == UseDimensions.XAndY)
			{
				if(distanceOperator == DistanceOperator.Into)
				{
					isWithinDistance = (currentDistance.x <= distance.x && currentDistance.y <= distance.y);
				}
				if(distanceOperator == DistanceOperator.OutOf)
				{
					isWithinDistance = currentDistance.x > distance.x || currentDistance.y > distance.y;
				}
			}
			else if(useDimensions == UseDimensions.X)
			{
				isWithinDistance = (currentDistance.x <= distance.x && distanceOperator == DistanceOperator.Into) || (currentDistance.x > distance.x && distanceOperator == DistanceOperator.OutOf);
			}
			else if(useDimensions == UseDimensions.Y)
			{
				isWithinDistance = (currentDistance.y <= distance.y && distanceOperator == DistanceOperator.Into) || (currentDistance.y > distance.y && distanceOperator == DistanceOperator.OutOf);
			}

			return isWithinDistance;
		}

		public override void DrawInspectorGUI()
		{
			#if UNITY_EDITOR
			EditorGUILayout.LabelField("WITHIN DISTANCE", EditorStyles.boldLabel);

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

			if(findTargetData == null)
			{
				findTargetData = new FindTargetData();
			}

			findTargetData.DrawInspectorGUI();
			#endif
		}

		public override BranchType GetBranchType()
		{
			return BranchType.Distance;
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(DistanceBranch))]
	public class DistanceBranchEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}
