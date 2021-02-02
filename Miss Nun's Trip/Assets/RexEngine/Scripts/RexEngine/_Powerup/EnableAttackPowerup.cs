using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class EnableAttackPowerup:Powerup 
	{
		public string attackName;
		public bool willEnableAttack = true;

		protected override void TriggerEffect(RexActor actor)
		{
			if(actor == null)
			{
				return;
			}

			bool foundAttack = false;
			foreach(Attack attack in actor.GetComponentsInChildren<Attack>())
			{
				if(attack.name == attackName)
				{
					foundAttack = true;
					attack.isEnabled = willEnableAttack;
					break;
				}
			}

			if(!foundAttack)
			{
				foreach(ComboChain comboChain in actor.GetComponentsInChildren<ComboChain>())
				{
					if(comboChain.name == attackName)
					{
						foundAttack = true;
						comboChain.SetEnabled(willEnableAttack);
						break;
					}
				}
			}

			if(!foundAttack)
			{
				foreach(AttackSet attackSet in actor.GetComponentsInChildren<AttackSet>())
				{
					if(attackSet.name == attackName)
					{
						foundAttack = true;
						if(willEnableAttack)
						{
							attackSet.Enable();
						}
						else
						{
							attackSet.Disable();
						}

						break;
					}
				}
			}
		}
	}
}
