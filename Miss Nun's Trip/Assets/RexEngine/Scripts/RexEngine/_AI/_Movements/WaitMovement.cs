using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class WaitMovement:RexAIMovement 
	{
		public override void OnBegin()
		{
			base.OnBegin();
			if(aiRoutine.slots.controller)
			{
				aiRoutine.slots.controller.SetAxis(Vector2.zero);
				aiRoutine.slots.controller.slots.physicsObject.StopAllMovement();
			}
		}
	}
}