using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
	public class SwapPlayer:MonoBehaviour 
	{
		public enum StartType
		{
			OnCollision,
			OnSceneLoad,
			Manual
		}

		public StartType startType;
		public List<RexActor> players = new List<RexActor>();

		protected int currentPlayer = 0;
		protected List<RexActor> playerGameObjects;
		protected BoxCollider2D boxCollider;

		void Awake() 
		{
			boxCollider = GetComponent<BoxCollider2D>();
		}

		void Start() 
		{
			playerGameObjects = new List<RexActor>();
			for(int i = 0; i < players.Count; i ++)
			{
				playerGameObjects.Add(null);
			}

			playerGameObjects[0] = GameManager.Instance.player;

			if(startType == StartType.OnSceneLoad)
			{
				SwapRight();
			}
		}

		public void SwapRight()
		{
			currentPlayer ++;
			if(currentPlayer >= players.Count)
			{
				currentPlayer = 0;
			}

			Swap();
		}

		public void SwapLeft()
		{
			currentPlayer --;
			if(currentPlayer < 0)
			{
				currentPlayer = players.Count - 1;
			}

			Swap();
		}

		public void SwapToIndex(int index)
		{
			if(index > players.Count - 1)
			{
				index = players.Count - 1;
			}
			else if(index < 0)
			{
				index = 0;
			}

			currentPlayer = index;

			Swap();
		}

		protected void Swap()
		{
			RexActor newPlayer = (playerGameObjects[currentPlayer] != null) ? playerGameObjects[currentPlayer] : Instantiate(players[currentPlayer]).GetComponent<RexActor>();
			if(playerGameObjects[currentPlayer] == null)
			{
				playerGameObjects[currentPlayer] = newPlayer;
			}

			DontDestroyOnLoad(newPlayer);

			RexActor oldPlayer = GameManager.Instance.player;

			GameManager.Instance.player = newPlayer;
			GameManager.Instance.players[0] = newPlayer;
			RexSceneManager.Instance.player = newPlayer;

			newPlayer.gameObject.SetActive(true);

			newPlayer.slots.controller.EndAllStates();
			newPlayer.slots.physicsObject.StopAllMovement();
			newPlayer.SetPosition(new Vector2(oldPlayer.transform.position.x, oldPlayer.transform.position.y));
			newPlayer.slots.controller.SetDirection(oldPlayer.slots.controller.direction.horizontal);
			newPlayer.slots.physicsObject.FreezeGravityForSingleFrame();
			newPlayer.ShowEnergyBars();

			oldPlayer.slots.controller.EndAllStates();
			oldPlayer.slots.physicsObject.StopAllMovement();
			oldPlayer.CancelAttack();
			oldPlayer.slots.controller.SetStateToDefault();
			oldPlayer.slots.controller.isEnabled = false;
			oldPlayer.slots.input.ClearInput();
			oldPlayer.slots.input.isEnabled = false;
			oldPlayer.ForceDamagedStateEnd();
			oldPlayer.CancelActivePowerups();
			oldPlayer.HideEnergyBars();
			oldPlayer.gameObject.SetActive(false);

			StartCoroutine("CheckPhysicsCoroutine");

			Camera.main.GetComponent<RexCamera>().SetFocusObject(newPlayer.transform);
		}

		protected IEnumerator CheckPhysicsCoroutine()
		{
			RexActor newPlayer = GameManager.Instance.player;

			int totalFrames = 2;
			for(int i = 0; i < totalFrames; i ++)
			{
				yield return new WaitForFixedUpdate();

				newPlayer.slots.input.ClearInput();
				newPlayer.slots.input.isEnabled = false;
				newPlayer.slots.controller.isEnabled = false;
			}

			newPlayer.slots.input.isEnabled = true;
			newPlayer.slots.controller.isEnabled = true;

			yield return new WaitForFixedUpdate();

			newPlayer.slots.controller.SetStateToDefault(true);
		}

		protected void OnTriggerEnter2D(Collider2D col)
		{
			if(startType == StartType.OnCollision && col.tag == "Player")
			{
				SwapRight();
				boxCollider.enabled = false;
			}
		}
	}
}
