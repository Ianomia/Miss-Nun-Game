/* Copyright Sky Tyrannosaur */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class WeaponEnergyPowerup:EnergyPowerup
	{
		public string energyTag = "";
		public WeaponEnergyRefillType refillType;

		public enum WeaponEnergyRefillType
		{
			RefillAllWeapons,
			RefillMatchingTags,
			RefillAllActive,
			RefillAllActiveWithMatchingTags
		}

		protected override void TriggerEffect(RexActor player)
		{
			if(player == null)
			{
				return;
			}

			string _energyTag = (refillType == WeaponEnergyRefillType.RefillAllWeapons || refillType == WeaponEnergyRefillType.RefillAllActive) ? "ALL" : energyTag;
			bool requireActive = (refillType == WeaponEnergyRefillType.RefillAllActive || refillType == WeaponEnergyRefillType.RefillAllActiveWithMatchingTags);
			if(equation == Equation.Increment)
			{
				player.RestoreMP(amount, _energyTag, requireActive);
			}
			else
			{
				player.DecrementMP(amount, _energyTag, requireActive);
			}
		}
	}
}
