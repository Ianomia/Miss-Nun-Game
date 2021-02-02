using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class AttackAction:RexAIAction 
	{
		public Attack attack;
		public bool isAttackProjectile;
		public float projectileAngle;
		public Attack.ProjectileDestination projectileDestination;

		public FindTargetData findTargetData;

		protected FindTargetMethods findTargetMethods;

		[HideInInspector]
		public string targetName = "";

		void Awake()
		{
			findTargetMethods = gameObject.AddComponent<FindTargetMethods>();//new FindTargetMethods();
		}

		void Start()
		{
			if(findTargetData.targetType != FindTargetData.TargetType.Transform)
			{
				findTargetData.otherTransform = null;
			}
		}

		public override void Begin()
		{
			if(findTargetData.otherTransform == null)
			{
				findTargetMethods.GetTarget(findTargetData);
			}

			if(findTargetData.otherTransform == null)
			{
				End();
				return;
			}

			if(attack != null)
			{
				if(isAttackProjectile)
				{
					if(projectileDestination == Attack.ProjectileDestination.Angle)
					{
						attack.SetProjectileTargeting(Attack.ProjectileDestination.Angle, null, projectileAngle);
					}
					else if(projectileDestination == Attack.ProjectileDestination.Target)
					{
						attack.SetProjectileTargeting(Attack.ProjectileDestination.Target, findTargetData.otherTransform, 0.0f);
					}
				}

				attack.ForceBegin();
			}

			End();
		}

		#if UNITY_EDITOR
		public override void DrawInspectorGUI()
		{
			EditorGUILayout.LabelField(GetName().ToUpper(), EditorStyles.boldLabel);
			attack = EditorGUILayout.ObjectField("Attack", attack, typeof(Attack), true) as Attack;

			isAttackProjectile = EditorGUILayout.Toggle("Is Projectile", isAttackProjectile);

			if(isAttackProjectile)
			{
				projectileDestination = (Attack.ProjectileDestination)EditorGUILayout.EnumPopup("Destination", projectileDestination);

				if(projectileDestination == Attack.ProjectileDestination.Angle)
				{
					projectileAngle = EditorGUILayout.FloatField("Firing Angle", projectileAngle);
					if(projectileAngle > 360.0f)
					{
						projectileAngle = 360.0f;
					}
					else if(projectileAngle < -360.0f)
					{
						projectileAngle = -360.0f;
					}
				}
				else if(projectileDestination == Attack.ProjectileDestination.Target)
				{
					if(findTargetData == null)
					{
						findTargetData = new FindTargetData();
					}

					findTargetData.DrawInspectorGUI();
				}
			}
		}
		#endif

		public override ActionType GetActionType()
		{
			return ActionType.Attack;
		}

		public override string GetName()
		{
			return "Attack";
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(AttackAction))]
	public class AttackActionEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

