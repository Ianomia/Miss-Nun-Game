using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class RexAIBranch:MonoBehaviour 
	{
		public enum Condition
		{
			GreaterThan,
			LessThan,
			Equals,
			Else
		}

		public enum BranchType
		{
			Distance,
			FacingDirection,
			FacingTarget,
			HP,
			Physics,
			Random,
			TargetFacing,
			Else
		}

		public virtual string ID()
		{
			return "Branch";
		}

		public virtual bool Determine(RexAIRoutine _routine = null)
		{
			return true;
		}

		public virtual void DrawInspectorGUI()
		{
			
		}

		public virtual BranchType GetBranchType()
		{
			return BranchType.Random;
		}
	}
}

