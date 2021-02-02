/* Copyright Sky Tyrannosaur */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class HPPowerup:EnergyPowerup
	{
		protected override void TriggerEffect(RexActor player)
		{
			if(player == null)
			{
				return;
			}

			if(equation == Equation.Increment)
			{
				player.RestoreHP(amount);
			}
			else
			{
				player.Damage(amount);
			}
		}
	}
}
