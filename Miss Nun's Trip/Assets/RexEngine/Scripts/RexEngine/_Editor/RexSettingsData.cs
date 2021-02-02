﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class RexSettingsData:ScriptableObject 
	{
		public GameManager.Settings gameManagerSettings;
		public LivesManager.Settings livesManagerSettings;
		public TimerManager.Settings timerManagerSettings;
		public RexTouchInput.TouchControls touchControlSettings;
		public GameManager.CameraSettings cameraSettings;
		public TextBox textBoxPrefab;
	}
}
