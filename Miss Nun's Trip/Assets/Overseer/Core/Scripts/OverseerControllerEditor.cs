using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace Lightbug.Overseer
{

[CustomEditor( typeof( OverseerController ) )]
public class OverseerControllerEditor : Editor
{
	OverseerController monobehaviour = null;

	SerializedObject mainAssetSerializedObject = null;

	SerializedProperty mainAsset = null;
	SerializedProperty startingIndex = null;

	List<string> elementsNames = new List<string>();
	
	void OnEnable()
	{
		monobehaviour = target as OverseerController;

		mainAsset = serializedObject.FindProperty( "mainAsset" );
		startingIndex = serializedObject.FindProperty( "startingIndex" );

		mainAssetSerializedObject = new SerializedObject( mainAsset.objectReferenceValue );

		GatherElements();
	}


	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		GUILayout.BeginVertical( EditorStyles.helpBox );

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.PropertyField( mainAsset );

		if( EditorGUI.EndChangeCheck() )
		{
			Debug.Log("asdasdasd");

			if( mainAsset.objectReferenceValue != null )
			{				
				mainAssetSerializedObject = new SerializedObject( mainAsset.objectReferenceValue );

				startingIndex.intValue = 0;

				GatherElements();
			}
			
			
			
		}

		if( mainAsset.objectReferenceValue != null )
			startingIndex.intValue = EditorGUILayout.Popup( "Starting Element" , startingIndex.intValue , elementsNames.ToArray() );

		GUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties();
	}

	void GatherElements()
     {          
          elementsNames.Clear();

				
		SerializedProperty elements = mainAssetSerializedObject.FindProperty("elements");

          for( int i = 0 ; i < elements.arraySize ; i++)
          {
               string tag = elements.GetArrayElementAtIndex( i ).FindPropertyRelative("tag").stringValue;
               elementsNames.Add( i.ToString() + " - " + tag ); 
          }
          
     }
}

}

#endif


