/* Copyright Sky Tyrannosaur */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HitSparkManager:MonoBehaviour 
{
	public RexPool damagedSparkPool;
	public RexPool notDamagedSparkPool;

	public enum Type
	{
		DamagedSpark,
		NotDamagedSpark
	}

	private static HitSparkManager instance = null;
	public static HitSparkManager Instance 
	{ 
		get 
		{
			if(instance == null)
			{
				GameObject go = new GameObject();
				instance = go.AddComponent<HitSparkManager>();
				go.name = "HitSparkManager";
			}
			
			return instance; 
		} 
	}

	void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
	}

	public void CreateHitSpark(BoxCollider2D col1, BoxCollider2D col2, bool wasTargetDamaged = false, int targetHealth = 0)
	{
		Vector2 position = RexMath.GetColliderOverlapCenter(col1, col2);
		Type type = (wasTargetDamaged) ? Type.DamagedSpark : Type.NotDamagedSpark;
		CreateHitSparkAtPosition(position, type);
	}

	public void CreateHitSparkAtPosition(Vector2 position, Type type)
	{
		if(damagedSparkPool == null || notDamagedSparkPool == null)
		{
			return;
		}

		GameObject sparkGameObject = (type == Type.DamagedSpark) ? damagedSparkPool.Spawn() : notDamagedSparkPool.Spawn();
		RexParticle spark = (sparkGameObject != null) ? sparkGameObject.GetComponent<RexParticle>() : null;

		if(spark != null)
		{
			spark.transform.position = position;
			spark.transform.localEulerAngles = new Vector3(0, 0, RexMath.RandomFloat(0, 360));
			spark.Play();
		}
	}
}
