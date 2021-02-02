#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

using RexEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
public class RexSettings:EditorWindow
{
	public enum Players
	{
		One = 1,
		Two = 2
	}

	protected RexActor newPlayerPrefab;
	protected RexActor newPlayer2Prefab;

	protected TextBox newTextBoxPrefab;

	protected RexTouchInput.TouchControls touchControlSettings;
	protected GameManager.CameraSettings cameraSettings;

	protected bool isDebugInvincibilityEnabled;
	protected bool isMusicMuted;
	protected bool areSFXMuted;
	protected Vector2 scrollPosition;
	protected bool showFieldPlayer = false;
	protected bool showFieldDialogue = false;
	protected bool showFieldLives = false;
	protected bool showFieldScore = false;
	protected bool showFieldDebug = false;
	protected bool showFieldTimer = false;
	protected bool showFieldReadyMessage = false;
	protected bool showFieldTouchControls = false;
	protected bool showFieldCameras = false;

	protected Players numberOfPlayers;

	protected static bool hasInitialized = false;

	public GameManager.Settings gameManagerSettings;
	public LivesManager.Settings livesManagerSettings;
	public TimerManager.Settings timerManagerSettings;

	static RexSettings()
	{
		#if UNITY_EDITOR
		if(!hasInitialized)
		{
			EditorApplication.update += Startup;
		}
		#endif
	}

	public static void Startup()
	{
		if(!Application.isPlaying)
		{
			LoadSettingsStatic();
		}

		EditorApplication.update -= Startup;
		hasInitialized = true;
	}

	void OnEnable()
	{
		LoadSettings();

		SceneView.onSceneGUIDelegate += this.OnSceneGUI;
	}

	void OnDisable()
	{
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
	}

	void OnSceneGUI(SceneView sceneView)
	{
		if(Application.isPlaying)
		{
			return;
		}
	}

	void OnGUI()
	{
		if(Application.isPlaying)
		{
			return;
		}

		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

		EditorGUILayout.BeginHorizontal();
		{
			//GUILayout.Label(new GUIContent("Project Settings", "Here is a tooltip"), EditorStyles.boldLabel);
			GUILayout.Label("Project Settings", EditorStyles.boldLabel);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

		showFieldPlayer = EditorGUILayout.Foldout(showFieldPlayer, new GUIContent("Player", "Options for the number of players this project uses and what prefabs to use for them."));
		if(showFieldPlayer)
		{
			CollapseAllFields();
			showFieldPlayer = true;

			EditorGUI.indentLevel ++;

			EditorGUILayout.BeginHorizontal();
			{
				numberOfPlayers = (Players)EditorGUILayout.EnumPopup(new GUIContent("Number of Players: ", "Whether the project uses one or two players."), numberOfPlayers);
			}		
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				newPlayerPrefab = EditorGUILayout.ObjectField(new GUIContent("Player 1 Prefab", "The RexActor prefab to load for Player 1; this will be automatically loaded when the game starts."), newPlayerPrefab, typeof(RexActor), true) as RexActor;		
			}		
			EditorGUILayout.EndHorizontal();

			if((int)numberOfPlayers > 1)
			{
				EditorGUILayout.BeginHorizontal();
				{
					newPlayer2Prefab = EditorGUILayout.ObjectField(new GUIContent("Player 2 Prefab", "The RexActor prefab to load for Player 2; this will be automatically loaded when the game starts."),  newPlayer2Prefab, typeof(RexActor), true) as RexActor;		
				}		
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel --;

			ShowSaveButton();
		}

		showFieldLives = EditorGUILayout.Foldout(showFieldLives, new GUIContent("Lives", "Options for if and how a Lives system is used in the project."));
		if(showFieldLives)
		{
			CollapseAllFields();
			showFieldLives = true;

			EditorGUI.indentLevel ++;

			EditorGUILayout.BeginHorizontal();
			{
				livesManagerSettings.areLivesEnabled = EditorGUILayout.Toggle(new GUIContent("Are Lives Enabled", "Whether or not this project uses lives."), livesManagerSettings.areLivesEnabled);
			}
			EditorGUILayout.EndHorizontal();

			if(livesManagerSettings.startingLives < 1 && !livesManagerSettings.does0Count)
			{
				livesManagerSettings.startingLives = 1;
			}
			else if(livesManagerSettings.startingLives < 0)
			{
				livesManagerSettings.startingLives = 0;
			}
			else if(livesManagerSettings.startingLives > livesManagerSettings.maxLives)
			{
				livesManagerSettings.startingLives = livesManagerSettings.maxLives;
			}

			EditorGUILayout.BeginHorizontal();
			{
				livesManagerSettings.does0Count = EditorGUILayout.Toggle(new GUIContent("Does 0 Count", "Whether or not the player is allowed to play for one final life once the Lives counter reaches 0."), livesManagerSettings.does0Count);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				livesManagerSettings.startingLives = EditorGUILayout.IntField(new GUIContent("Starting Lives", "If lives are enabled, this is the number of lives the player begins the game with."), livesManagerSettings.startingLives);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				livesManagerSettings.maxLives = EditorGUILayout.IntField(new GUIContent("Max Lives", "If lives are enabled, this is the maximum number of lives the player can have at once."), livesManagerSettings.maxLives);
			}
			EditorGUILayout.EndHorizontal();

			if(livesManagerSettings.maxLives < 1)
			{
				livesManagerSettings.maxLives = 1;
			}

			EditorGUILayout.BeginHorizontal();
			{
				livesManagerSettings.gameOverScene = EditorGUILayout.TextField(new GUIContent("Game Over Scene", "If lives are enabled, this is the scene that loads when the player's lives are reduced to 0 or below."), livesManagerSettings.gameOverScene);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				livesManagerSettings.postGameOverScene = EditorGUILayout.TextField(new GUIContent("Post Game Over Scene", "The next scene that loads if the player continues from the Game Over scene."), livesManagerSettings.postGameOverScene);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				livesManagerSettings.defaultRespawnScene = EditorGUILayout.TextField(new GUIContent("Default Respawn Scene", "The scene the player spawns in, by default, if they die."), livesManagerSettings.defaultRespawnScene);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				gameManagerSettings.stopMusicOnPlayerDeath = EditorGUILayout.Toggle(new GUIContent("Stop Music on Player Death", "Whether or not music stops when the player dies."), gameManagerSettings.stopMusicOnPlayerDeath);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel --;

			ShowSaveButton();
		}

		showFieldReadyMessage = EditorGUILayout.Foldout(showFieldReadyMessage, new GUIContent("Ready Message", "Options for the Ready Message which appears when the player spawns into the start of a level or a checkpoint."));
		if(showFieldReadyMessage)
		{
			CollapseAllFields();
			showFieldReadyMessage = true;

			EditorGUI.indentLevel ++;

			EditorGUILayout.BeginHorizontal();
			{
				gameManagerSettings.showReadyMessage = EditorGUILayout.Toggle(new GUIContent("Show Ready Message", "Whether or not the Ready Message is enabled."), gameManagerSettings.showReadyMessage);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel --;

			ShowSaveButton();
		}

		showFieldScore = EditorGUILayout.Foldout(showFieldScore, new GUIContent("Score", "Options for using a Score system in the project."));
		if(showFieldScore)
		{
			CollapseAllFields();
			showFieldScore = true;

			EditorGUI.indentLevel ++;

			EditorGUILayout.BeginHorizontal();
			{
				gameManagerSettings.isScoreEnabled = EditorGUILayout.Toggle(new GUIContent("Is Score Displayed", "Whether or not a score is displayed in the project."),  gameManagerSettings.isScoreEnabled);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel --;

			ShowSaveButton();
		}

		showFieldTimer = EditorGUILayout.Foldout(showFieldTimer, new GUIContent("Stage Timer", "Options for using a Timer system in the project."));
		if(showFieldTimer)
		{
			CollapseAllFields();
			showFieldTimer = true;

			EditorGUI.indentLevel ++;

			EditorGUILayout.BeginHorizontal();
			{
				timerManagerSettings.isEnabled = EditorGUILayout.Toggle(new GUIContent("Is Timer Enabled", "Whether or not a Timer system is enabled in the project. If True, a countdown timer will appear during levels which kills the player if it reaches 0."), timerManagerSettings.isEnabled);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				timerManagerSettings.startingTime = EditorGUILayout.FloatField(new GUIContent("Starting Time", "The starting time, in seconds, used by the timer."), timerManagerSettings.startingTime);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel --;

			ShowSaveButton();
		}

		showFieldDialogue = EditorGUILayout.Foldout(showFieldDialogue, new GUIContent("Dialogue", "Options for the dialogue boxes in the project"));
		if(showFieldDialogue)
		{
			CollapseAllFields();
			showFieldDialogue = true;

			EditorGUI.indentLevel ++;

			EditorGUILayout.BeginHorizontal();
			{
				newTextBoxPrefab = EditorGUILayout.ObjectField(new GUIContent("Text Box Prefab", "The TextBox prefab to load as the default Text Box; this will be automatically loaded when the game starts."), newTextBoxPrefab, typeof(TextBox), true) as TextBox;		
			}		
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				gameManagerSettings.willStopTimeOnDialogueShow = EditorGUILayout.Toggle(new GUIContent("Stop Time on Dialogue", "If True, Rex actors and physics will pause when a dialogue box is open."), gameManagerSettings.willStopTimeOnDialogueShow);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel --;

			ShowSaveButton();
		}

		showFieldTouchControls = EditorGUILayout.Foldout(showFieldTouchControls, new GUIContent("Touch Controls", "Options for touch controls."));
		if(showFieldTouchControls)
		{
			CollapseAllFields();
			showFieldTouchControls = true;

			EditorGUI.indentLevel ++;

			EditorGUILayout.BeginHorizontal();
			{
				touchControlSettings.touchControlDisplay = (RexTouchInput.TouchControlDisplay)EditorGUILayout.EnumPopup(new GUIContent("Touch Control Display", "Whether or not touch controls will be displayed."), touchControlSettings.touchControlDisplay);
			}
			EditorGUILayout.EndHorizontal();

			if(touchControlSettings.touchControlDisplay != RexTouchInput.TouchControlDisplay.NeverDisplay)
			{
				EditorGUILayout.BeginHorizontal();
				{
					touchControlSettings.touchControlPrefab = EditorGUILayout.ObjectField(new GUIContent("Touch Controls Prefab", "The GameObject prefab Rex will instantiate for touch controls."), touchControlSettings.touchControlPrefab, typeof(GameObject), true) as GameObject;		
				}		
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel --;

			ShowSaveButton();
		}

		showFieldCameras = EditorGUILayout.Foldout(showFieldCameras, new GUIContent("Cameras", "Options for the cameras used to render scenes."));
		if(showFieldCameras)
		{
			CollapseAllFields();
			showFieldCameras = true;

			EditorGUI.indentLevel ++;

			EditorGUILayout.BeginHorizontal();
			{
				cameraSettings.camerasObject = EditorGUILayout.ObjectField(new GUIContent("Cameras Prefab", "The prefab used for your cameras. Must include a RexCamera component with a standard Unity Camera component attached."), cameraSettings.camerasObject, typeof(GameObject), true) as GameObject;
			}
			EditorGUILayout.EndHorizontal();

			cameraSettings.roomSize = EditorGUILayout.Vector2Field(new GUIContent("Room Size", "The size in units that will be considered one room/screen by Rex's Scene Boundaries."), cameraSettings.roomSize);

			EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel --;

			ShowSaveButton();
		}

		showFieldDebug = EditorGUILayout.Foldout(showFieldDebug, new GUIContent("Debug", "Options for whether music is muted and invincibility is enabled in the Unity Editor. These will not carry over into a published project."));
		if(showFieldDebug)
		{
			CollapseAllFields();
			showFieldDebug = true;

			EditorGUI.indentLevel ++;

			EditorGUILayout.BeginHorizontal();
			{
				isMusicMuted = EditorGUILayout.Toggle(new GUIContent("Is Music Muted", "Whether or not music is muted in the Unity Editor. This setting will not carry over into a published project."), isMusicMuted);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				areSFXMuted = EditorGUILayout.Toggle(new GUIContent("Are SFX Muted", "Whether or not sound effects are muted in the Unity Editor. This setting will not carry over into a published project."), areSFXMuted);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				isDebugInvincibilityEnabled = EditorGUILayout.Toggle(new GUIContent("Is Invincibility Enabled", "If True, the player will be invincible when testing in the Unity Editor. This setting will not carry over into a published project."), isDebugInvincibilityEnabled);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel --;

			ShowSaveButton();
		}

		EditorGUILayout.EndScrollView();
	}

	protected void ShowSaveButton()
	{
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Save"))
		{
			GameObject newObject = Instantiate(Resources.Load("System/Singletons"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
			newObject.name = "Singletons";

			if(newPlayerPrefab != null && newPlayerPrefab.GetComponent<RexActor>() != null)
			{
				GameManager gameManager = newObject.GetComponentInChildren<GameManager>();
				if(gameManager != null)
				{
					gameManagerSettings.numberOfPlayers = (int)numberOfPlayers;
					if(gameManagerSettings.playerPrefabs.Count <= 0)
					{
						gameManagerSettings.playerPrefabs.Add(newPlayerPrefab);
					}
					else
					{
						gameManagerSettings.playerPrefabs[0] = newPlayerPrefab;
					}

					if(newPlayer2Prefab != null)
					{
						if(gameManagerSettings.playerPrefabs.Count == 1)
						{
							gameManagerSettings.playerPrefabs.Add(newPlayerPrefab);
						}
						else
						{
							gameManagerSettings.playerPrefabs[1] = newPlayer2Prefab;
						}
					}

					SaveAssetDatabaseSettings();
				}

				EditorPrefs.SetBool("DebugInvincibility", isDebugInvincibilityEnabled);
				EditorPrefs.SetBool("IsMusicMuted", isMusicMuted);
				EditorPrefs.SetBool("AreSFXMuted", areSFXMuted);

				string localPath = "Assets/RexEngine/Resources/System/Singletons.prefab";

#if UNITY_2018_3_OR_NEWER
PrefabUtility.SaveAsPrefabAssetAndConnect(newObject, localPath, InteractionMode.AutomatedAction);
#else
				Object prefab = PrefabUtility.CreateEmptyPrefab(localPath);
				PrefabUtility.ReplacePrefab(newObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
#endif

				EditorUtility.SetDirty(newObject);

				Debug.Log("Rex-cellent! Your Rex Engine settings were saved!");
			}
			else
			{
				Debug.Log("No dice! You've gotta slot a RexActor prefab into the Player Prefab slot!");
			}

			DestroyImmediate(newObject);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();
	}

	protected static void LoadSettingsStatic()
	{
		GameObject newObject = Instantiate(Resources.Load("System/Singletons"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
		ProjectSettingsAsset projectSettingsAsset = newObject.GetComponentInChildren<ProjectSettingsAsset>();
		if(projectSettingsAsset != null)
		{
			bool wasUsingCustomSettings = projectSettingsAsset.rexSettingsData != null && projectSettingsAsset.rexSettingsData.name == "RexEngineSettings";
			projectSettingsAsset.LoadSettings();
			bool isNowUsingCustomSettings = projectSettingsAsset.rexSettingsData != null && projectSettingsAsset.rexSettingsData.name == "RexEngineSettings";

			if(!wasUsingCustomSettings && isNowUsingCustomSettings)
			{
				string localPath = "Assets/RexEngine/Resources/System/Singletons.prefab";

#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAssetAndConnect(newObject, localPath, InteractionMode.AutomatedAction);
#else
                Object prefab = PrefabUtility.CreateEmptyPrefab(localPath);
                PrefabUtility.ReplacePrefab(newObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
#endif

                EditorUtility.SetDirty(newObject);
            }
        }

		DestroyImmediate(newObject);
	}

	protected void LoadSettings()
	{
		GameObject newObject = Instantiate(Resources.Load("System/Singletons"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
		ProjectSettingsAsset projectSettingsAsset = newObject.GetComponentInChildren<ProjectSettingsAsset>();

		if(projectSettingsAsset != null)
		{
			projectSettingsAsset.LoadSettings();

			gameManagerSettings = projectSettingsAsset.rexSettingsData.gameManagerSettings;

			newPlayerPrefab = gameManagerSettings.playerPrefabs[0];

			if(gameManagerSettings.playerPrefabs.Count > 1)
			{
				newPlayer2Prefab = gameManagerSettings.playerPrefabs[1];
			}

			livesManagerSettings = projectSettingsAsset.rexSettingsData.livesManagerSettings;
			timerManagerSettings = projectSettingsAsset.rexSettingsData.timerManagerSettings;

			newTextBoxPrefab = projectSettingsAsset.rexSettingsData.textBoxPrefab;

			numberOfPlayers = (Players)gameManagerSettings.numberOfPlayers;

			touchControlSettings = projectSettingsAsset.rexSettingsData.touchControlSettings;
			cameraSettings = projectSettingsAsset.rexSettingsData.cameraSettings;
		}

		isMusicMuted = EditorPrefs.GetBool("IsMusicMuted");
		areSFXMuted = EditorPrefs.GetBool("AreSFXMuted");
		isDebugInvincibilityEnabled = EditorPrefs.GetBool("DebugInvincibility");

		DestroyImmediate(newObject);
	}

	protected void SaveAssetDatabaseSettings()
	{
		string[] results;

		results = AssetDatabase.FindAssets("RexEngineSettings");
		foreach(string guid in results)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			RexSettingsData rexSettingsData = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(RexSettingsData)) as RexSettingsData;

			if(rexSettingsData != null)
			{
				rexSettingsData.gameManagerSettings = gameManagerSettings;
				rexSettingsData.livesManagerSettings = livesManagerSettings;
				rexSettingsData.timerManagerSettings = timerManagerSettings;
				rexSettingsData.touchControlSettings = touchControlSettings;
				rexSettingsData.cameraSettings = cameraSettings;
				rexSettingsData.textBoxPrefab = newTextBoxPrefab;
				EditorUtility.SetDirty(rexSettingsData);
				AssetDatabase.SaveAssets();
			}
		}
	}

	protected void CollapseAllFields()
	{
		showFieldPlayer = false;
		showFieldDialogue = false;
		showFieldLives = false;
		showFieldScore = false;
		showFieldDebug = false;
		showFieldTimer = false;
		showFieldTouchControls = false;
		showFieldReadyMessage = false;
		showFieldCameras = false;
	}
}
#endif
