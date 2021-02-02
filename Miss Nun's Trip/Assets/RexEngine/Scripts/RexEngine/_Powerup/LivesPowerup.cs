/* Copyright Sky Tyrannosaur */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class LivesPowerup:EnergyPowerup
	{
		protected override void TriggerEffect(RexActor player)
		{
			if(player == null)
			{
				return;
			}

			if(equation == Equation.Increment)
			{
				LivesManager.Instance.IncrementLives(amount);
			}
			else
			{
				LivesManager.Instance.DecrementLives(amount);
			}
		}
	}
}
