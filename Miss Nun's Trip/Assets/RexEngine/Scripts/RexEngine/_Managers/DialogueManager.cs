/* Copyright Sky Tyrannosaur */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RexEngine
{
	public class DialogueManager:MonoBehaviour 
	{
		[System.Serializable]
		public class Page
		{
			public string text;
			public int portraitNumber;
		}

		public TextBox textBoxPrefab;

		protected TextBox textBox;
		protected bool willStopTimeOnShow = false;
		protected bool isDialogueActive = false;

		public delegate void OnDialogueComplete();
		public OnDialogueComplete onDialogueComplete;

		public class PageInfo
		{
			public int total = 3;
			public int current;
		}

		private static DialogueManager instance = null;
		public static DialogueManager Instance 
		{ 
			get 
			{
				if(instance == null)
				{
					GameObject go = new GameObject();
					instance = go.AddComponent<DialogueManager>();
					go.name = "DialogueManager";
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

		void Start()
		{
			textBox = Instantiate(textBoxPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<TextBox>();
			textBox.name = "TextBox";
			willStopTimeOnShow = GameManager.Instance.settings.willStopTimeOnDialogueShow;

			if(textBox != null)
			{
				textBox.OnTextComplete += this.OnTextComplete;
			}

			Hide();
		}

		void Update()
		{
			if(textBox.holder.gameObject.activeSelf)
			{
				textBox.CheckInput();
			}
		}

		public void ShowRawText(string _text)
		{
			List<DialogueManager.Page> pages = new List<DialogueManager.Page>();
			DialogueManager.Page page = new DialogueManager.Page();
			page.text = _text;
			pages.Add(page);

			Show(pages);
		}

		public void Show(List<Page> _pages, List<Sprite> _dialoguePortraits = null)
		{
			if(_pages == null || _pages.Count < 1)
			{
				return;
			}

			#if UNITY_ANDROID || UNITY_IPHONE
			RexTouchInput rexTouchInput = GameManager.Instance.player.GetComponent<RexTouchInput>();
			if(rexTouchInput != null)
			{
				rexTouchInput.ToggleTouchInterface(false);
			}
			#endif

			isDialogueActive = true;

			if(willStopTimeOnShow)
			{
				foreach(RexActor actor in GameObject.FindObjectsOfType<RexActor>())
				{
					actor.StopTime();
				}

			}

			textBox.Show(_pages, _dialoguePortraits);
		}

		protected void OnTextComplete()
		{
			Hide();
		}

		public void Hide()
		{
			#if UNITY_ANDROID || UNITY_IPHONE
			RexTouchInput rexTouchInput = GameManager.Instance.player.GetComponent<RexTouchInput>();
			if(rexTouchInput != null)
			{
				rexTouchInput.ToggleTouchInterface(true);
			}
			#endif

			if(willStopTimeOnShow)
			{
				foreach(RexActor actor in GameObject.FindObjectsOfType<RexActor>())
				{
					if(actor.timeStop.isTimeStopped)
					{
						actor.StartTime();
					}
				}
			}

			StartCoroutine("HideCoroutine");
		}

		public bool IsDialogueActive()
		{
			return isDialogueActive;
		}

		protected IEnumerator HideCoroutine()
		{
			yield return new WaitForSeconds(0.1f);

			isDialogueActive = false;

			GameManager.Instance.player.RegainControl(RexActor.DisableControlsType.Dialogue); 

			textBox.isCloseEnabled = true;

			if(onDialogueComplete != null)
			{
				onDialogueComplete.Invoke();
			}
		}
	}
}
