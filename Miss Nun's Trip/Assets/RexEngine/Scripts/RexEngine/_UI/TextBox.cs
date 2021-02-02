/* Copyright Sky Tyrannosaur */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class TextBox:MonoBehaviour 
	{
		public TextMesh text;
		public AudioSource audioSource;
		public AudioClip showSound;
		public AudioClip hideSound;
		public GameObject advanceIcon;
		public SpriteRenderer portrait;
		public GameObject holder;
		public TextPositioning textPositioningNoPortrait;
		public TextPositioning textPositioningPortrait;
		public float delayBetweenLetters = 0.025f;

		public delegate void OnTextCompleteDelegate();
		public event OnTextCompleteDelegate OnTextComplete;

		[HideInInspector]
		public bool isCloseEnabled = false;

		protected DialogueManager.PageInfo pageInfo = new DialogueManager.PageInfo();
		protected List<DialogueManager.Page> pages;
		protected List<Sprite> dialoguePortraits;
		protected float textFieldWidth = 130.0f;
		protected bool willSkipToEnd = false;

		private string currentlyDisplayedText;
		private string[] wordsInText;

		[System.Serializable]
		public class TextPositioning
		{
			public float xPosition;
			public float textFieldWidth;
		}

		void Awake() 
		{

		}

		void Start() 
		{
			holder.SetActive(false);
		}

		public void Show(List<DialogueManager.Page> _pages, List<Sprite> _dialoguePortraits = null)
		{
			if(_pages == null || _pages.Count < 1)
			{
				return;
			}

			pages = _pages;
			dialoguePortraits = _dialoguePortraits;

			if(dialoguePortraits == null || dialoguePortraits.Count < 1)
			{
				text.transform.localPosition = new Vector3(textPositioningNoPortrait.xPosition, text.transform.localPosition.y, text.transform.localPosition.z);
				textFieldWidth = textPositioningNoPortrait.textFieldWidth;
			}
			else
			{
				text.transform.localPosition = new Vector3(textPositioningPortrait.xPosition, text.transform.localPosition.y, text.transform.localPosition.z);
				textFieldWidth = textPositioningPortrait.textFieldWidth;
			}

			pageInfo.current = 0;
			pageInfo.total = pages.Count;

			DisplayPage(pageInfo.current);
		}

		public void Hide()
		{
			if(audioSource && hideSound && holder.gameObject.activeSelf)
			{
				audioSource.PlayOneShot(hideSound);
			}

			text.text = "";
			holder.gameObject.SetActive(false);
		}

		public void AdvancePage()
		{
			pageInfo.current ++;
			DisplayPage(pageInfo.current);
		}

		public void CheckInput()
		{
			if((GameManager.Instance.input.isJumpButtonDownThisFrame || Input.GetMouseButtonDown(0)))
			{
				if(isCloseEnabled)
				{
					if(pageInfo.current < pageInfo.total - 1)
					{
						StopCoroutine("ShowCoroutine");
						AdvancePage();
					}
					else
					{
						if(OnTextComplete != null)
						{      
							OnTextComplete();
						}

						Hide();
					}
				}
				else
				{
					willSkipToEnd = true;
				}
			}
		}

		public float GetLineWidth(TextMesh mesh, string text)
		{
			float width = 0;
			string[] lines = text.Split('\n');
			foreach(char symbol in lines[lines.Length - 1])
			{
				CharacterInfo info;
				if(mesh.font.GetCharacterInfo(symbol, out info, mesh.fontSize, mesh.fontStyle))
				{
					width += info.advance;
				}
			}

			return width * mesh.characterSize * 0.1f;
		}

		protected void DisplayPage(int _page)
		{
			if(audioSource && showSound)
			{
				audioSource.PlayOneShot(showSound);
			}

			willSkipToEnd = false;
			text.text = pages[pageInfo.current].text;

			int portraitNumber = pages[pageInfo.current].portraitNumber;
			portrait.sprite = null;

			if(dialoguePortraits != null && dialoguePortraits.Count > 0)
			{
				portrait.sprite = ((dialoguePortraits.Count) > portraitNumber) ? dialoguePortraits[portraitNumber] : dialoguePortraits[0];
			}

			holder.gameObject.SetActive(true);

			GameManager.Instance.player.RemoveControl(RexActor.DisableControlsType.Dialogue);

			isCloseEnabled = false;
			advanceIcon.SetActive(false);
			StartCoroutine("ShowCoroutine");
		}

		protected IEnumerator ShowCoroutine()
		{
			wordsInText = text.text.Split(' ');
			text.text = "";
			currentlyDisplayedText = "";

			int currentWord = 0;
			int currentLetter = 0;
			while(currentWord < wordsInText.Length)
			{
				string textIncludingNextWord = (currentWord <= wordsInText.Length - 1) ? text.text + wordsInText[currentWord + 0] : text.text;
				float textWidth = GetLineWidth(text, textIncludingNextWord);

				if(textWidth >= textFieldWidth)
				{
					currentlyDisplayedText += "\n";
				}

				currentLetter = 0;
				char[] letters = wordsInText[currentWord].ToCharArray();
				while(currentLetter < letters.Length)
				{
					text.text = currentlyDisplayedText + letters[currentLetter];

					if(!willSkipToEnd)
					{
						yield return new WaitForSeconds(delayBetweenLetters);
					}

					currentlyDisplayedText += letters[currentLetter];
					currentLetter ++;
				}

				currentlyDisplayedText += ' ';

				currentWord ++;
			}

			yield return new WaitForSeconds(0.5f);

			SkipTextAhead();
		}

		protected void SkipTextAhead()
		{
			advanceIcon.SetActive(true);
			isCloseEnabled = true;
			StopCoroutine("ShowCoroutine");
		}
	}	
}
