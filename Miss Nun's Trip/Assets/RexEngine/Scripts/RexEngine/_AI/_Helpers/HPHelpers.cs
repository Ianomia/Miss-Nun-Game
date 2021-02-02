using UnityEngine;

namespace RexEngine
{
	public enum HPType
	{
		Percentage,
		Amount
	}

	public enum DamageOperator
	{
		LessThanOrEqualTo,
		LessThan,
		GreaterThanOrEqualTo,
		GreaterThan
	}

	public class HPHelpers:MonoBehaviour
	{
		public static bool CheckHP(DamageOperator damageOperator, HPType hpType, RexActor actor, int hpThreshold, float hpPercentage, int currentActorHP)
		{
			float percentage = RexMath.Percentage(currentActorHP, actor.hp.max);

			bool willTrigger = false;
			if(damageOperator == DamageOperator.LessThanOrEqualTo && ((hpType == HPType.Amount && actor.hp.current <= hpThreshold) || (hpType == HPType.Percentage && percentage <= hpPercentage)))
			{
				willTrigger = true;
			}
			else if(damageOperator == DamageOperator.LessThan && ((hpType == HPType.Amount && actor.hp.current < hpThreshold) || (hpType == HPType.Percentage && percentage < hpPercentage)))
			{
				willTrigger = true;
			}
			else if(damageOperator == DamageOperator.GreaterThanOrEqualTo && ((hpType == HPType.Amount && actor.hp.current >= hpThreshold) || (hpType == HPType.Percentage && percentage >= hpPercentage)))
			{
				willTrigger = true;
			}
			else if(damageOperator == DamageOperator.GreaterThan && ((hpType == HPType.Amount && actor.hp.current > hpThreshold) || (hpType == HPType.Percentage && percentage > hpPercentage)))
			{
				willTrigger = true;
			}

			return willTrigger;
		}
	}
}
