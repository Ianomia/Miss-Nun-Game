/* Copyright Sky Tyrannosaur */

using UnityEngine;
using System.Collections;

namespace RexEngine
{
    public class RexTouchInput:RexInput
    {
		[System.Serializable]
		public class TouchControls
		{
			public TouchControlDisplay touchControlDisplay;
			public GameObject touchControlPrefab;
		}

		public enum TouchControlDisplay
		{
			NeverDisplay,
			AlwaysDisplay,
			DisplayForMobile
		}

        public void ToggleTouchInterface(bool willShow)
        {
            if(InputManager.Instance.TouchInputManager != null) 
			{
				InputManager.Instance.TouchInputManager.ToggleTouchInterface(willShow);
			}
        }
    }
}