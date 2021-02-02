using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class ElseBranch:RexAIBranch 
	{
		public override string ID()
		{
			return "Else";
		}

		public override bool Determine(RexAIRoutine _routine = null)
		{
			return true;
		}

		public override void DrawInspectorGUI()
		{
		}

		public override BranchType GetBranchType()
		{
			return BranchType.Else;
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(ElseBranch))]
	public class ElseBranchEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

