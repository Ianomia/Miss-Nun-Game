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
public class RexWelcome:EditorWindow
{
	protected Vector2 scrollPosition;
	protected static bool hasInitialized = false;
	protected static bool willDisplayOnStartup;
	protected InspectorHelper.AIInspectorStyles styles;

	static RexWelcome()
	{
		if(!hasInitialized)
		{
			//Debug.Log("RexWelcome :: Awake");
			EditorApplication.update += Startup;
		}
	}

	public static void Startup()
	{
		bool willHide = EditorPrefs.GetBool("HideWelcomeWindow");
		willDisplayOnStartup = !EditorPrefs.GetBool("HideWelcomeWindow");
		//Debug.Log("RexWelcome :: Will display on startup: " + willDisplayOnStartup + "    Editor prefs will hide: " + EditorPrefs.GetBool("HideWelcomeWindow"));

		if(!willHide && !Application.isPlaying)
		{
			//Debug.Log("RexWelcome :: Start");

			UnityEditor.EditorWindow window = GetWindow(typeof(RexWelcome));
			window.Show();

		}
		else
		{
			UnityEditor.EditorWindow window = GetWindow(typeof(RexWelcome));
			window.Close();
		}

		EditorApplication.update -= Startup;
		hasInitialized = true;
	}

	void OnEnable()
	{
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
		if(Application.isPlaying || !hasInitialized)
		{
			return;
		}

		if(styles == null)
		{
			styles = InspectorHelper.SetStyles();
			styles.actionStyle.padding = new RectOffset(20, 20, 20, 20);
			styles.actionStyle.fixedWidth = 400;
		}

		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

		EditorGUILayout.BeginVertical(styles.actionStyle);

		EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Label("WELCOME!", EditorStyles.boldLabel);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Label("We appreciate you getting a copy of Rex Engine, and we're looking forward to seeing the awesome games you create with it. Here are some goodies to help you get started:", styles.boxStyle);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginVertical(styles.helpTextStyle);

		GUILayout.Label("TUTORIALS");

		if(GUILayout.Button("View QuickStart Guide"))
		{
			Application.OpenURL("http://www.skytyrannosaur.com/rexenginequickstart/");
		}

		if(GUILayout.Button("View YouTube Tutorials"))
		{
			Application.OpenURL("https://www.youtube.com/channel/UCLaQOB8mAXzcDrsb-3uPs3g/videos");
		}

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginVertical(styles.helpTextStyle);

		GUILayout.Label("REX TOOLS");

		if(GUILayout.Button("Open Rex Settings Window"))
		{
			ShowRexSettings();
		}

		if(GUILayout.Button("Open Rex Level Editor Window"))
		{
			ShowRexLevelEditor();
		}

		if(GUILayout.Button("Open Rex Palette Window"))
		{
			ShowRexPalette();
		}

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginVertical(styles.helpTextStyle);

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Label("Need help? Feel free to reach out to us at:");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Label("info@skytyrannosaur.com");
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginVertical(styles.helpTextStyle);

		GUILayout.Label("If you like Rex Engine, please take a moment to rate it! We're a small indie studio and your ratings help keep us afloat so we can keep adding new features.", styles.helpTextStyle);
		if(GUILayout.Button("Rate Rex Engine"))
		{
			Application.OpenURL("https://assetstore.unity.com/packages/templates/systems/rex-engine-classic-2d-platformer-engine-92333");
		}

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		{
			willDisplayOnStartup = GUILayout.Toggle(willDisplayOnStartup, " Display this window on startup");

			if(!willDisplayOnStartup && EditorPrefs.GetBool("HideWelcomeWindow") == false)
			{
				EditorPrefs.SetBool("HideWelcomeWindow", true);
				Debug.Log("Rex Engine :: Setting Editor Prefs to Hide Welcome Window");
			}
			else if(willDisplayOnStartup && EditorPrefs.GetBool("HideWelcomeWindow") == true)
			{
				EditorPrefs.SetBool("HideWelcomeWindow", false);
				Debug.Log("Rex Engine :: Setting Editor Prefs to Show Welcome Window");
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(); { GUILayout.Label(""); } EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();

		EditorGUILayout.EndScrollView();
	}

	protected static void ShowRexSettings()
	{
		EditorWindow window = EditorWindow.GetWindow(typeof(RexSettings));
		window.titleContent = new GUIContent("Rex Settings");
	}

	protected static void ShowRexLevelEditor()
	{
		EditorWindow window = EditorWindow.GetWindow(typeof(RexLevelEditor));
		window.titleContent = new GUIContent("Rex Level Editor");
	}

	protected static void ShowRexPalette()
	{
		EditorWindow window = EditorWindow.GetWindow(typeof(RexPalette));
		window.titleContent = new GUIContent("Rex Palette");
	}
}
#endif
