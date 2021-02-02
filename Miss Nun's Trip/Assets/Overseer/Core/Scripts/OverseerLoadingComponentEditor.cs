using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace Lightbug.Overseer
{

[CustomEditor( typeof( OverseerLoadingComponent ) )]
public class OverseerLoadingComponentEditor : Editor
{
	const float ExampleProgress = 58.753f;

	OverseerLoadingComponent monobehaviour = null;

	//Properties
	SerializedProperty type = null;
	SerializedProperty image = null;
	SerializedProperty animation = null;
	SerializedProperty text = null;
	SerializedProperty useFixedText = null;
    	SerializedProperty fixedText = null;
    	SerializedProperty progressFormat = null;

	

	void OnEnable()
	{
		monobehaviour = target as OverseerLoadingComponent;

		type = serializedObject.FindProperty( "type" );
		image = serializedObject.FindProperty( "image" );
		animation = serializedObject.FindProperty( "animation" );
		text = serializedObject.FindProperty( "text" );
		useFixedText = serializedObject.FindProperty( "useFixedText" );
		fixedText = serializedObject.FindProperty( "fixedText" );
		progressFormat = serializedObject.FindProperty( "progressFormat" );
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		GUILayout.Space( 10 );

		GUILayout.BeginVertical( EditorStyles.helpBox );

		GUILayout.Label("Graphics" , EditorStyles.boldLabel );

		EditorGUILayout.PropertyField( type );

		
		if( type.enumValueIndex == 0 )
		{
			EditorGUILayout.PropertyField( image );
		}
		else
		{
			EditorGUILayout.PropertyField( animation );
		}		
		

		GUILayout.EndVertical();

		GUILayout.BeginVertical( EditorStyles.helpBox );

		GUILayout.Label("Text" , EditorStyles.boldLabel );

		EditorGUILayout.PropertyField( text );

		if( text.objectReferenceValue == null )
		{
			EditorGUILayout.HelpBox( "Assign a Unity's Text component!" , MessageType.Warning );
		}
		else
		{	
			EditorGUILayout.PropertyField( useFixedText );

			if( useFixedText.boolValue )
				EditorGUILayout.PropertyField( fixedText );
			
			
			EditorGUILayout.PropertyField( progressFormat );
		}

		GUILayout.Space(10);

		GUILayout.Label( 
			"Example : " + ( useFixedText.boolValue ? 
			fixedText.stringValue + " " + ExampleProgress.ToString(progressFormat.stringValue) + " %" : 
			ExampleProgress.ToString(progressFormat.stringValue) + " %" ) 
		);

		GUILayout.Space(5);

		GUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties();
	}	
	
}

}

#endif
