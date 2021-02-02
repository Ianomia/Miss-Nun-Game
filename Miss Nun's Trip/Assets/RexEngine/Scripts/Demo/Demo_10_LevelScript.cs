/* Copyright Sky Tyrannosaur */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RexEngine;

public class Demo_10_LevelScript:MonoBehaviour 
{
	public TextMesh victoryText;
	public TextMesh scoreText;
	public TextMesh deathsText;
	public string sceneToLoad = "Demo_Ending";
	public bool willDisableUI = true;
	public GameObject bossGate;

	public void RunEnding()
	{
		StartCoroutine("EndingCoroutine");
	}

	protected IEnumerator EndingCoroutine()
	{
		RexSoundManager.Instance.Fade();

		for(int i = 0; i < GameManager.Instance.players.Count; i ++)
		{
			RexActor player = GameManager.Instance.players[i];
			player.slots.input.isEnabled = false;
			player.CancelActivePowerups();
			player.GetComponent<Booster>().OnBlueprintFound();

			if(willDisableUI)
			{
				player.hp.bar.gameObject.SetActive(false);
				if(player.mp)
				{
					player.mp.bar.gameObject.SetActive(false);
				}
			}
		}

		if(willDisableUI)
		{
			ScoreManager.Instance.text.gameObject.SetActive(false);
			PauseManager.Instance.isPauseEnabled = false;
			LivesManager.Instance.Hide();

			if(TimerManager.Instance.settings.isEnabled)
			{
				TimerManager.Instance.StopTimer();
			}
		}

		ScreenShake.Instance.Shake(); 

		victoryText.gameObject.SetActive(true);
		yield return new WaitForSeconds(2.0f);

		if(sceneToLoad == "")
		{
			for(int i = 0; i < GameManager.Instance.players.Count; i ++)
			{
				RexActor player = GameManager.Instance.players[i];
				player.slots.input.isEnabled = true;
			}

			victoryText.gameObject.SetActive(false);
		}

		if(bossGate != null)
		{
			bossGate.SetActive(false);
			ScreenShake.Instance.Shake();
		}

		if(scoreText != null)
		{
			string scoreString = "Total bolts collected: " + ScoreManager.Instance.score.ToString();
			scoreText.text = scoreString;
			scoreText.gameObject.SetActive(true);
			yield return new WaitForSeconds(1.0f);
		}

		if(deathsText != null)
		{
			string deathString = "Total deaths: " + LivesManager.Instance.deaths.ToString();
			deathsText.text = deathString;
			deathsText.gameObject.SetActive(true);
		}

		if(sceneToLoad != "")
		{
			yield return new WaitForSeconds(5.0f);

			RexSceneManager.Instance.LoadSceneWithFadeOut(sceneToLoad);
		}
	}
}
