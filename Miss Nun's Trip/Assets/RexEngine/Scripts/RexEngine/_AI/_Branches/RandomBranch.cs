using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RexEngine
{
	public class RandomBranch:RexAIBranch 
	{
		public int chance = 50; 

		public override string ID()
		{
			return "Random Chance";
		}

		public override bool Determine(RexAIRoutine _routine = null)
		{
			int randomNumber = RexMath.RandomInt(1, 100);
			return (randomNumber <= chance);
		}

		public override void DrawInspectorGUI()
		{
			#if UNITY_EDITOR
			chance = EditorGUILayout.IntField("Percent Chance", chance);

			if(chance < 1)
			{
				chance = 1;
			}
			else if(chance > 100)
			{
				chance = 100;
			}
			#endif
		}

		public override BranchType GetBranchType()
		{
			return BranchType.Random;
		}
	}

	#if UNITY_EDITOR
	[CustomEditor(typeof(RandomBranch))]
	public class RandomBranchEditor:Editor 
	{
		public override void OnInspectorGUI(){}
	}
	#endif
}

