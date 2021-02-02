/* Copyright Sky Tyrannosaur */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy:MonoBehaviour 
{
	public int current = 1;
	public int max = 1;
	public EnergyBar barPrefab;
	public RefillProperties refillOnActorRevive;
	public string energyTag = "";

	[HideInInspector]
	public int previous = 1;

	[HideInInspector]
	public EnergyBar bar;

	[System.Serializable]
	public class RefillProperties
	{
		[Tooltip("Options for what happens to the actor's MP value when they die and are revived.")]
		public RefillType refillType;
		[Tooltip("The value the actor's MP will be set to on death if the above On Death option is Set To Value.")]
		public int setToValue;
	}

	//TODO: What about actors dying or otherwise unequipping the attack that uses this Energy component entirely? Destroy the bar?

	public enum RefillType //TODO: This feels like kind of a bad name, given what it does
	{
		Refill,
		LoseAll,
		SetToValue
	}

	void Awake()
	{
		//CreateBar();
	}

	public void Restore(int amount)
	{
		previous = current;
		current += amount;
		if(current > max)
		{
			current = max;
		}

		if(bar != null)
		{
			bar.SetValue(current);
		}
	}

	public void Decrement(int amount)
	{
		previous = current;
		current -= amount;
		if(current <= 0)
		{
			current = 0;
		}

		if(bar != null)
		{
			bar.SetValue(current);
		}
	}

	public void SetToMax()
	{
		current = max;

		if(bar != null)
		{
			bar.SetValue(current, false);
		}
	}

	public void SetValue(int value)
	{
		if(value > max)
		{
			value = max;
		}

		current = value;

		if(bar != null)
		{
			bar.SetValue(current, false);
		}
	}

	public void CreateBar(bool willDestroyOnLoad = true)
	{
		if(barPrefab != null)
		{
			GameObject newObject = Instantiate(barPrefab).gameObject;
			newObject.name = newObject.name.Split('(')[0];

			ParentHelper.Parent(newObject, ParentHelper.ParentObject.UI);
			bar = newObject.GetComponent<EnergyBar>();

			bar.willDestroyWhenSceneChanges = willDestroyOnLoad;
			bar.SetMaxValue(max, false);
			bar.SetValue(current, false);
		}
	}
}
