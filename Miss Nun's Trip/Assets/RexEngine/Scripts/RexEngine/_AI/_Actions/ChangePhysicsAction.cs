using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class ChangePhysicsAction:RexAIAction 
	{
		public EnableSettings enableSettings;
		public IgnoreTerrainSettings ignoreTerrainSettings;
		public GravitySettings gravitySettings;
		public FreezeMovementSettings freezeMovementSettings;
		public SetMovementSettings setMovementSettings;
		public StopAllMovementSettings stopAllMovementSettings;

		#if UNITY_EDITOR
		protected InspectorHelper.AIInspectorStyles styles;
		#endif

		protected RexPhysics physicsObject;

		[System.Serializable]
		public class EnableSettings
		{
			public bool changeEnabled;
			public bool isEnabled;
		}

		[System.Serializable]
		public class IgnoreTerrainSettings
		{
			public bool changeIgnoreTerrain;
			public bool ignoreTerrain;
		}

		[System.Serializable]
		public class GravitySettings
		{
			public bool changeGravity;
			public bool usesGravity;
			public float gravity;
		}

		[System.Serializable]
		public class FreezeMovementSettings
		{
			public bool changeFreezeMovement;
			public bool freezeXMovement;
			public bool freezeYMovement;
		}

		[System.Serializable]
		public class SetMovementSettings
		{
			public bool changeSetMovement;
			public bool changeVelocityX;
			public bool changeVelocityY;
			public float velocityX;
			public float velocityY;
			public bool changeAccelerationX;
			public bool changeAccelerationY;
			public float accelerationX;
			public float accelerationY;
			public bool changeDecelerationX;
			public bool changeDecelerationY;
			public float decelerationX;
			public float decelerationY;
			public bool useRelativeX;
		}

		[System.Serializable]
		public class StopAllMovementSettings
		{
			public bool changeStopAllMovement;
			public bool stopAllMovement;
		}

		public override void Begin()
		{
			if(physicsObject == null)
			{
				physicsObject = aiRoutine.slots.controller.slots.physicsObject;
			}

			if(enableSettings == null)
			{
				InitSettings();
			}

			if(physicsObject != null)
			{
				if(enableSettings.changeEnabled)
				{
					physicsObject.isEnabled = enableSettings.isEnabled;
				}

				if(gravitySettings.changeGravity)
				{
					physicsObject.gravitySettings.usesGravity = gravitySettings.usesGravity;
					physicsObject.gravitySettings.gravity = gravitySettings.gravity;
				}

				if(ignoreTerrainSettings.changeIgnoreTerrain)
				{
					physicsObject.willIgnoreTerrain = ignoreTerrainSettings.ignoreTerrain;
				}

				if(freezeMovementSettings.changeFreezeMovement)
				{
					physicsObject.freezeMovementX = freezeMovementSettings.freezeXMovement;
					physicsObject.freezeMovementY = freezeMovementSettings.freezeYMovement;
				}

				if(setMovementSettings.changeSetMovement)
				{
					if(setMovementSettings.changeVelocityX)
					{
						float adjustedX = setMovementSettings.velocityX;
						if(setMovementSettings.useRelativeX && aiRoutine.slots.controller.direction.horizontal == Direction.Horizontal.Left)
						{
							adjustedX *= -1;
						}

						physicsObject.SetVelocityX(adjustedX);
					}

					if(setMovementSettings.changeVelocityY)
					{
						physicsObject.SetVelocityY(setMovementSettings.velocityY);
					}

					if(setMovementSettings.changeAccelerationX)
					{
						float adjustedX = setMovementSettings.accelerationX;
						if(setMovementSettings.useRelativeX && aiRoutine.slots.controller.direction.horizontal == Direction.Horizontal.Left)
						{
							adjustedX *= -1;
						}

						physicsObject.properties.acceleration.x = adjustedX;
						physicsObject.SetAccelerationCapX(adjustedX);
					}

					if(setMovementSettings.changeAccelerationY)
					{
						physicsObject.properties.acceleration.y = setMovementSettings.accelerationY;
						physicsObject.SetAccelerationCapY(setMovementSettings.accelerationY);
					}

					if(setMovementSettings.changeDecelerationX)
					{
						physicsObject.properties.deceleration.x = setMovementSettings.decelerationX;
					}

					if(setMovementSettings.changeDecelerationY)
					{
						physicsObject.properties.deceleration.y = setMovementSettings.decelerationY;
					}

					if(stopAllMovementSettings.changeStopAllMovement)
					{
						if(stopAllMovementSettings.stopAllMovement)
						{
							physicsObject.StopAllMovement();
						}
					}
				}
			}

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			if(styles == null)
			{
				styles = InspectorHelper.SetStyles();
			}

			if(enableSettings == null)
			{
				InitSettings();
			}

			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);

			string alterEnabledString = "Edit Enabled Settings";
			alterEnabledString = (enableSettings.changeEnabled) ? alterEnabledString.ToUpper() : alterEnabledString;
			enableSettings.changeEnabled = EditorGUILayout.Toggle(alterEnabledString, enableSettings.changeEnabled);
			if(enableSettings.changeEnabled)
			{
				InspectorHelper.DrawLine();
				enableSettings.isEnabled = EditorGUILayout.Toggle("Is Enabled", enableSettings.isEnabled);
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			string alterIgnoreTerrainString = "Edit Ignore Terrain Settings";
			alterIgnoreTerrainString = (ignoreTerrainSettings.changeIgnoreTerrain) ? alterIgnoreTerrainString.ToUpper() : alterIgnoreTerrainString;
			ignoreTerrainSettings.changeIgnoreTerrain = EditorGUILayout.Toggle(alterIgnoreTerrainString, ignoreTerrainSettings.changeIgnoreTerrain);
			if(ignoreTerrainSettings.changeIgnoreTerrain)
			{
				InspectorHelper.DrawLine();
				ignoreTerrainSettings.ignoreTerrain = EditorGUILayout.Toggle("Ignore Terrain", ignoreTerrainSettings.ignoreTerrain);
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			string alterGravityString = "Edit Gravity Settings";
			alterGravityString = (gravitySettings.changeGravity) ? alterGravityString.ToUpper() : alterGravityString;
			gravitySettings.changeGravity = EditorGUILayout.Toggle(alterGravityString, gravitySettings.changeGravity);
			if(gravitySettings.changeGravity)
			{
				InspectorHelper.DrawLine();
				gravitySettings.usesGravity = EditorGUILayout.Toggle("Uses Gravity", gravitySettings.usesGravity);
				gravitySettings.gravity = EditorGUILayout.FloatField("Gravity", gravitySettings.gravity);
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			string alterFreezeMovementString = "Edit Freeze Movement Settings";
			alterFreezeMovementString = (freezeMovementSettings.changeFreezeMovement) ? alterFreezeMovementString.ToUpper() : alterFreezeMovementString;
			freezeMovementSettings.changeFreezeMovement = EditorGUILayout.Toggle(alterFreezeMovementString, freezeMovementSettings.changeFreezeMovement);
			if(freezeMovementSettings.changeFreezeMovement)
			{
				InspectorHelper.DrawLine();
				freezeMovementSettings.freezeXMovement = EditorGUILayout.Toggle("Freeze Movement (X)", freezeMovementSettings.freezeXMovement);
				freezeMovementSettings.freezeYMovement = EditorGUILayout.Toggle("Freeze Movement (Y)", freezeMovementSettings.freezeYMovement);
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.LabelField("");
			InspectorHelper.DrawLine();
			EditorGUILayout.LabelField("");

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			string alterSetMovementString = "Set Movement";
			alterSetMovementString = (setMovementSettings.changeSetMovement) ? alterSetMovementString.ToUpper() : alterSetMovementString;
			setMovementSettings.changeSetMovement = EditorGUILayout.Toggle(alterSetMovementString, setMovementSettings.changeSetMovement);
			if(setMovementSettings.changeSetMovement)
			{
				InspectorHelper.DrawLine();
				setMovementSettings.changeVelocityX = EditorGUILayout.Toggle("Set Velocity (X)", setMovementSettings.changeVelocityX);
				if(setMovementSettings.changeVelocityX)
				{
					setMovementSettings.useRelativeX = EditorGUILayout.Toggle("Relative to Direction Actor is Facing: ", setMovementSettings.useRelativeX);
					setMovementSettings.velocityX = EditorGUILayout.FloatField("Velocity (X)", setMovementSettings.velocityX);
					EditorGUILayout.LabelField("");
				}

				setMovementSettings.changeVelocityY = EditorGUILayout.Toggle("Set Velocity (Y)", setMovementSettings.changeVelocityY);
				if(setMovementSettings.changeVelocityY)
				{
					setMovementSettings.velocityY = EditorGUILayout.FloatField("Velocity (Y)", setMovementSettings.velocityY);
					EditorGUILayout.LabelField("");
				}

				setMovementSettings.changeAccelerationX = EditorGUILayout.Toggle("Set Acceleration (X)", setMovementSettings.changeAccelerationX);
				if(setMovementSettings.changeAccelerationX)
				{
					setMovementSettings.useRelativeX = EditorGUILayout.Toggle("Relative to Direction Actor is Facing: ", setMovementSettings.useRelativeX);
					setMovementSettings.accelerationX = EditorGUILayout.FloatField("Acceleration (X)", setMovementSettings.accelerationX);
					EditorGUILayout.LabelField("");
				}

				setMovementSettings.changeAccelerationY = EditorGUILayout.Toggle("Set Acceleration (Y)", setMovementSettings.changeAccelerationY);
				if(setMovementSettings.changeAccelerationY)
				{
					setMovementSettings.accelerationY = EditorGUILayout.FloatField("Acceleration (Y)", setMovementSettings.accelerationY);
					EditorGUILayout.LabelField("");
				}

				setMovementSettings.changeDecelerationX = EditorGUILayout.Toggle("Set Deceleration (X)", setMovementSettings.changeDecelerationX);
				if(setMovementSettings.changeDecelerationX)
				{
					setMovementSettings.decelerationX = EditorGUILayout.FloatField("Deceleration (X)", setMovementSettings.decelerationX);
					EditorGUILayout.LabelField("");
				}

				setMovementSettings.changeDecelerationY = EditorGUILayout.Toggle("Set Deceleration (Y)", setMovementSettings.changeDecelerationY);
				if(setMovementSettings.changeDecelerationY)
				{
					setMovementSettings.decelerationY = EditorGUILayout.FloatField("Deceleration (Y)", setMovementSettings.decelerationY);
					EditorGUILayout.LabelField("");
				}

				EditorGUILayout.LabelField("");

				EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField("Note that these settings can be overridden by any active RexStates or abilities involving movement.", styles.helpTextStyle);
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			string alterStopAllMovementString = "Stop All Movement";
			alterStopAllMovementString = (stopAllMovementSettings.changeStopAllMovement) ? alterStopAllMovementString.ToUpper() : alterStopAllMovementString;
			stopAllMovementSettings.changeStopAllMovement = EditorGUILayout.Toggle(alterStopAllMovementString, stopAllMovementSettings.changeStopAllMovement);
			if(stopAllMovementSettings.changeStopAllMovement)
			{
				InspectorHelper.DrawLine();
				stopAllMovementSettings.stopAllMovement = EditorGUILayout.Toggle("Stop All Movement", stopAllMovementSettings.stopAllMovement);
			}
			EditorGUILayout.EndVertical();

		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.ChangePhysics;
		}

		public override string GetName()
		{
			return "Change Physics";
		}

		protected void InitSettings()
		{
			if(gravitySettings == null)
			{
				gravitySettings = new GravitySettings();
			}

			if(freezeMovementSettings == null)
			{
				freezeMovementSettings = new FreezeMovementSettings();
			}

			if(setMovementSettings == null)
			{
				setMovementSettings = new SetMovementSettings();
			}

			if(enableSettings == null)
			{
				enableSettings = new EnableSettings();
			}

			if(ignoreTerrainSettings == null)
			{
				ignoreTerrainSettings = new IgnoreTerrainSettings();
			}

			if(stopAllMovementSettings == null)
			{
				stopAllMovementSettings = new StopAllMovementSettings();
			}
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(ChangePhysicsAction))]
	public class ChangePhysicsActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

