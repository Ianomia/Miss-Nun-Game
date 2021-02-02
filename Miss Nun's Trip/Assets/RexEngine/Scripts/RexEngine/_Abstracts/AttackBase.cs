using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class AttackBase:MonoBehaviour 
	{
		public bool isEnabled = true;
		public AttackInput input;


		[System.Serializable]
		public class AttackInput
		{
			public AttackImportance button; //if an Input is attached, this governs whether the attach is called on the Attack or the SubAttack button
			public bool requireVerticalPress = false;
			public Direction.Vertical verticalPressRequired = Direction.Vertical.Neutral;
			public bool requireFreshButtonPress = true; //If True, the attack will only initiate  when the attack button is first pressed; if false, you can hold the button to continuously repeat the attack
		}

		//Whether the attack is triggered with the primary or secondary attack button
		public enum AttackImportance
		{
			Primary,
			Sub,
			Sub_2,
			Sub_3,
			All
		}

		public void Enable()
		{
			isEnabled = true;
			OnEnabled();
		}

		public void Disable()
		{
			isEnabled = false;
			OnDisabled();
		}

		public virtual void OnEnabled(){}

		public virtual void OnDisabled(){}
	}
}
