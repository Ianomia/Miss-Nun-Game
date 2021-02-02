using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RexEngine
{
	public class UIManager:MonoBehaviour 
	{
		public delegate void OnUIHiddenDelegate();
		public event OnUIHiddenDelegate OnUIHidden;

		public delegate void OnUIDisplayedDelegate();
		public event OnUIDisplayedDelegate OnUIDisplayed;

		public DisplayMode currentDisplayMode {get; private set;}

		public enum DisplayMode
		{
			Displayed,
			Hidden
		}

		private static UIManager instance = null;
		public static UIManager Instance 
		{ 
			get 
			{
				if(instance == null)
				{
					GameObject go = new GameObject();
					instance = go.AddComponent<UIManager>();
					go.name = "UIManager";
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

			//DontDestroyOnLoad(gameObject);
			currentDisplayMode = DisplayMode.Displayed;

			/*Invoke("MakeUIInactive", 4.0f);
			Invoke("MakeUIActive", 8.0f);*/
		}

		public void MakeUIActive(bool willResetUI = false)
		{
			currentDisplayMode = DisplayMode.Displayed;

			if(OnUIDisplayed != null)
			{
				OnUIDisplayed();
			}

			RexTouchInput rexTouchInput = GameManager.Instance.players[0].GetComponent<RexTouchInput>();
			if(rexTouchInput != null)
			{
				rexTouchInput.ToggleTouchInterface(true);
			}

			if(GameManager.Instance.settings.isScoreEnabled)
			{
				ScoreManager.Instance.gameObject.SetActive(true);

				if(willResetUI)
				{
					ScoreManager.Instance.SetScoreAtCheckpoint(0);
					ScoreManager.Instance.SetScore(0);
				}
			}

			if(TimerManager.Instance.settings.isEnabled)
			{
				TimerManager.Instance.Show();

				if(willResetUI)
				{
					TimerManager.Instance.StartTimer();
				}
			}

			for(int i = 0; i < GameManager.Instance.players.Count; i ++)
			{
				RexActor player = GameManager.Instance.players[i];
				if(player.hp.bar)
				{
					player.hp.bar.gameObject.SetActive(true);
				}

				if(willResetUI)
				{
					if(player.hp)
					{
						player.RestoreHP(player.hp.max);
					}
				}

				if(player.mp)
				{
					player.mp.bar.gameObject.SetActive(true);
				}

				player.CancelActivePowerups();
			}

			if(LivesManager.Instance.settings.areLivesEnabled)
			{
				LivesManager.Instance.Show();
			}
		}

		public void MakeUIInactive(bool willResetUI = false)
		{
			currentDisplayMode = DisplayMode.Hidden;

			if(OnUIHidden != null)
			{
				OnUIHidden();
			}

			RexTouchInput rexTouchInput = GameManager.Instance.player.GetComponent<RexTouchInput>();
			if(rexTouchInput != null)
			{
				rexTouchInput.ToggleTouchInterface(false);
			}

			ScoreManager.Instance.gameObject.SetActive(false);

			for(int i = 0; i < GameManager.Instance.players.Count; i ++)
			{
				RexActor player = GameManager.Instance.players[i];
				if(player.hp.bar)
				{
					player.hp.bar.gameObject.SetActive(false);
				}

				if(player.mp)
				{
					player.mp.bar.gameObject.SetActive(false);
				}
			}

			ScoreManager.Instance.gameObject.SetActive(false);
			PauseManager.Instance.isPauseEnabled = false;
			LivesManager.Instance.Hide();

			if(TimerManager.Instance.settings.isEnabled)
			{
				TimerManager.Instance.Hide();

				if(willResetUI)
				{
					TimerManager.Instance.StopTimer();
				}
			}
		}

		void Destroy()
		{
			OnUIHidden = null;
			OnUIDisplayed = null;
		}
	}

}
