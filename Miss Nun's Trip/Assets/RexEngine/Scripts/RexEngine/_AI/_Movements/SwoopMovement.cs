using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace RexEngine
{
	public class SwoopMovement:RexAIMovement 
	{
		public Curve curve = new Curve();
		public TargetType targetType;
		public Repeating repeating;

		public Vector2 targetWaypoint = new Vector2(0.0f, 0.0f);

		public RexAISequence swoopCompletedSequence;

		public int totalFrames = 100;
		public bool randomizeFrames = false;
		public int minFrames = 80;
		public int maxFrames = 120;

		public bool limitMaxSwoopDistance;
		public Vector2 maxSwoopDistance = new Vector2(5.0f, 5.0f);

		public Vector2 minSwoopDistance = new Vector2(1.0f, 0.0f);

		public Vector2 minBounds;
		public Vector2 maxBounds;
		public bool useBounds;

		public float waitTime = 1.0f;
		public bool randomizeWaitTime = false;
		public float minWaitTime;
		public float maxWaitTime;

		public bool useOnlyTargetX;

		protected int currentFrame = 0;
		protected Vector2 startPosition;
		protected bool isWaiting;
		protected float currentWaitTime;

		protected bool hasStartedMoving = false;

		public enum TargetType
		{
			Explicit,
			Random
		}

		public enum Repeating
		{
			Once,
			Repeating
		}

		[System.Serializable]
		public class Curve
		{
			public float curveAmount = 0.25f;
			public bool randomizeCurve = false;
			public float minAmount;
			public float maxAmount;
		}

		void Awake()
		{
			findTargetMethods = gameObject.AddComponent<FindTargetMethods>();
		}

		public override void OnBegin()
		{
			if(RexSceneManager.Instance.isLoadingNewScene)
			{
				StartCoroutine("WaitForSceneLoadCoroutine");
				return;
			}

			if(findTargetData.otherTransform == null)
			{
				findTargetMethods.GetTarget(findTargetData);
			}

			ResetTimer();

			if(aiRoutine.slots.controller == null)
			{
				return;
			}

			hasStartedMoving = true;
			isWaiting = false;
			currentWaitTime = 0.0f;

			startPosition = new Vector2(transform.position.x, transform.position.y);
			currentFrame = 0;

			if(randomizeFrames)
			{
				totalFrames = RexMath.RandomInt(minFrames, maxFrames);
			}

			if(curve.randomizeCurve)
			{
				curve.curveAmount = RexMath.RandomFloat(curve.minAmount, curve.maxAmount);
			}

			if(targetType == TargetType.Explicit)
			{
				if(findTargetData != null && findTargetData.otherTransform != null)
				{
					targetWaypoint = findTargetData.otherTransform.position;
				}

				if(useOnlyTargetX)
				{
					targetWaypoint.y = RexMath.RandomFloat(minBounds.y, maxBounds.y);
				}
			}
			else if(targetType == TargetType.Random)
			{
				targetWaypoint.x = RexMath.RandomFloat(minBounds.x, maxBounds.x);
				targetWaypoint.y = RexMath.RandomFloat(minBounds.y, maxBounds.y);
			}

			if(useBounds)
			{
				if(targetWaypoint.x < minBounds.x)
				{
					targetWaypoint.x = minBounds.x;
				}
				else if(targetWaypoint.x > maxBounds.x)
				{
					targetWaypoint.x = maxBounds.x;
				}

				if(targetWaypoint.y < minBounds.y)
				{
					targetWaypoint.y = minBounds.y;
				}
				else if(targetWaypoint.y > maxBounds.y)
				{
					targetWaypoint.y = maxBounds.y;
				}
			}

			Vector2 currentDistance;
			if(limitMaxSwoopDistance)
			{
				currentDistance.x = targetWaypoint.x - startPosition.x;
				currentDistance.y = targetWaypoint.y - startPosition.y;

				if(Mathf.Abs(currentDistance.x) > Mathf.Abs(maxSwoopDistance.x))
				{
					int sideMultiplier = (targetWaypoint.x < startPosition.x) ? -1 : 1;
					targetWaypoint.x = startPosition.x + (maxSwoopDistance.x * sideMultiplier);
				}

				if(Mathf.Abs(currentDistance.y) > Mathf.Abs(maxSwoopDistance.y))
				{
					int sideMultiplier = (targetWaypoint.y < startPosition.y) ? -1 : 1;
					targetWaypoint.y = startPosition.y + (maxSwoopDistance.y * sideMultiplier);
				}
			}

			currentDistance.x = targetWaypoint.x - startPosition.x;
			currentDistance.y = targetWaypoint.y - startPosition.y;

			if(Mathf.Abs(currentDistance.x) < Mathf.Abs(minSwoopDistance.x))
			{
				int sideMultiplier = (targetWaypoint.x < startPosition.x) ? -1 : 1;
				targetWaypoint.x = startPosition.x + (minSwoopDistance.x * sideMultiplier);
			}

			if(Mathf.Abs(currentDistance.y) < Mathf.Abs(minSwoopDistance.y))
			{
				int sideMultiplier = (targetWaypoint.y < startPosition.y) ? -1 : 1;
				targetWaypoint.y = startPosition.y + (minSwoopDistance.y * sideMultiplier);
			}

			base.OnBegin();
		}

		public override void OnEnded()
		{
			currentFrame = 0;
			currentWaitTime = 0.0f;
			hasStartedMoving = false;
		}

		public override void UpdateAI()
		{
			if(aiRoutine.slots.controller == null || aiRoutine.slots.controller.slots.physicsObject == null || mode == Mode.Wait)
			{
				return;
			}

			if(hasStartedMoving && !aiRoutine.slots.controller.isStunned)
			{
				if(!isWaiting)
				{
					MoveToWaypoint();
				}
				else
				{
					currentWaitTime += Time.deltaTime;
					if(currentWaitTime >= waitTime)
					{
						isWaiting = false;
						currentWaitTime = 0.0f;

						if(repeating == Repeating.Repeating)
						{
							OnBegin();
						}
					}
				}
			}
		}

		protected override void OnModeSetToActive()
		{
			OnBegin();
		}

		protected IEnumerator WaitForSceneLoadCoroutine()
		{
			while(RexSceneManager.Instance.isLoadingNewScene)
			{
				yield return new WaitForEndOfFrame();
			}

			OnBegin();
		}

		protected void MoveToWaypoint() 
		{
			Vector2 distance = targetWaypoint - startPosition;
			Vector2 newPosition;

			newPosition.x = distance.x / totalFrames * currentFrame;
			float adjustmentForCurve = (distance.y - (curve.curveAmount * distance.x * distance.x)) / distance.x;
			newPosition.y = (curve.curveAmount * newPosition.x * newPosition.x) + (adjustmentForCurve * newPosition.x);

			SetToPosition(newPosition + startPosition);

			currentFrame ++;
			if(currentFrame > totalFrames || transform.position == new Vector3(targetWaypoint.x, targetWaypoint.y, transform.position.z))
			{
				currentFrame = totalFrames;

				if(repeating == Repeating.Repeating)
				{
					isWaiting = true;
					currentWaitTime = 0.0f;

					if(randomizeWaitTime)
					{
						waitTime = RexMath.RandomFloat(minWaitTime, maxWaitTime);
					}
				}
				else if(repeating == Repeating.Once)
				{
					if(swoopCompletedSequence != null)
					{
						aiRoutine.ChangeSequence(swoopCompletedSequence);
					}

					OnEnded();
				}
			}
		}

		protected void SetToPosition(Vector2 _position)
		{
			Vector2 difference = _position - aiRoutine.slots.controller.slots.physicsObject.properties.position;
			aiRoutine.slots.controller.slots.physicsObject.ApplyDirectTranslation(difference);
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			mode = (Mode)EditorGUILayout.EnumPopup("Mode", mode);

			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("REPEAT", EditorStyles.boldLabel);

			repeating = (Repeating)EditorGUILayout.EnumPopup("Repeat", repeating);

			if(repeating == Repeating.Repeating)
			{
				randomizeWaitTime = EditorGUILayout.Toggle("Randomize Wait Time", randomizeWaitTime);
				if(!randomizeWaitTime)
				{
					waitTime = EditorGUILayout.FloatField("Time Between Swoops", waitTime);
				}
				else
				{
					minWaitTime = EditorGUILayout.FloatField("Minimum Wait Time", minWaitTime);
					maxWaitTime = EditorGUILayout.FloatField("Maximum Wait Time", maxWaitTime);
				}
			}
			else
			{
				swoopCompletedSequence = EditorGUILayout.ObjectField("Play Sequence On Complete", swoopCompletedSequence, typeof(RexAISequence), true) as RexAISequence;
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("DURATION", EditorStyles.boldLabel);

			randomizeFrames = EditorGUILayout.Toggle("Randomize Frames", randomizeFrames);
			if(randomizeFrames)
			{
				minFrames = EditorGUILayout.IntField("Minimum Frames", minFrames);
				maxFrames = EditorGUILayout.IntField("Maximum Frames", maxFrames);

				if(minFrames < 0)
				{
					minFrames = 0;
				}

				if(maxFrames < 0)
				{
					maxFrames = 0;
				}

				if(maxFrames < minFrames)
				{
					maxFrames = minFrames;
				}
			}
			else
			{
				totalFrames = EditorGUILayout.IntField("Duration in Frames", totalFrames);

				if(totalFrames < 0)
				{
					totalFrames = 0;
				}
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("CURVE", EditorStyles.boldLabel);

			curve.randomizeCurve = EditorGUILayout.Toggle("Randomize Curve", curve.randomizeCurve);

			if(!curve.randomizeCurve)
			{
				curve.curveAmount = EditorGUILayout.FloatField("Curve Amount", curve.curveAmount);
				if(Mathf.Abs(curve.curveAmount) > 0.5f)
				{
					EditorGUILayout.LabelField("Curve amounts with an absolute value greater than 0.5 may be excessive.", styles.helpTextStyle);
				}
			}
			else
			{
				curve.minAmount = EditorGUILayout.FloatField("Minimum", curve.minAmount);
				curve.maxAmount = EditorGUILayout.FloatField("Maximum", curve.maxAmount);

				if(curve.minAmount > curve.maxAmount)
				{
					curve.minAmount = curve.maxAmount;
				}

				if(Mathf.Abs(curve.minAmount) > 0.5f || Mathf.Abs(curve.maxAmount) > 0.5f)
				{
					EditorGUILayout.LabelField("Curve amounts with an absolute value greater than 0.5 may be excessive.", styles.helpTextStyle);
				}
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("TARGET", EditorStyles.boldLabel);

			targetType = (TargetType)EditorGUILayout.EnumPopup("Target", targetType);

			if(targetType == TargetType.Explicit)
			{
				if(findTargetData == null)
				{
					findTargetData = new FindTargetData();
				}

				DrawFaceTargetGUI();

				useOnlyTargetX = EditorGUILayout.Toggle("Use Only Target X", useOnlyTargetX);
			}


			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("LIMIT SWOOP DISTANCE", EditorStyles.boldLabel);

			limitMaxSwoopDistance = EditorGUILayout.Toggle("Limit Max Swoop Distance", limitMaxSwoopDistance);
			if(limitMaxSwoopDistance)
			{
				maxSwoopDistance = EditorGUILayout.Vector2Field("Maximum Distance", maxSwoopDistance);
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("MINIMUM SWOOP DISTANCE", EditorStyles.boldLabel);

			minSwoopDistance = EditorGUILayout.Vector2Field("Minimum Distance", minSwoopDistance);
			if(minSwoopDistance.x < 0.1f)
			{
				minSwoopDistance.x = 0.1f;
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(styles.actionStyle);

			EditorGUILayout.LabelField("BOUNDS", EditorStyles.boldLabel);

			if(targetType != TargetType.Random)
			{
				useBounds = EditorGUILayout.Toggle("Constrain to Bounds", useBounds);
			}

			if(useBounds || targetType == TargetType.Random || useOnlyTargetX)
			{
				minBounds = EditorGUILayout.Vector2Field("Minimum Bounds", minBounds);
				maxBounds = EditorGUILayout.Vector2Field("Maximum Bounds", maxBounds);
			}

			EditorGUILayout.EndVertical();

		}
		#endif
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(SwoopMovement))]
	public class SwoopMovementEditor:Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			(target as SwoopMovement).DrawInspectorGUI();

			if(!Application.isPlaying && EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(target);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}
	}
	#endif
}
