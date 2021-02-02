
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Lightbug.Overseer
{

[System.Serializable]
public class OverseerElement
{
     public bool isValid = false;

     public string tag = "";     

     public bool showCursor = true;
     public CursorLockMode lockMode = CursorLockMode.None;
     
     public List<string> scenesNames = new List<string>();
     public int activeSceneIndex = 0;
     
     public GameObject loadingCanvasPrefab = null;

     public bool fixedDuration = false;
     public float duration = 2f;

     public bool overrideFadeIn = false;
     public SceneFade fadeIn = new SceneFade( true ); 
     
     public bool overrideFadeOut = false;
     public SceneFade fadeOut = new SceneFade( false );

     public AudioClip audioClip = null;

     [Range( 0f , 1f )]
     public float audioVolume = 1f;

     public bool loopAudio = true;


    
}


#if UNITY_EDITOR

[CustomPropertyDrawer( typeof( OverseerElement ))]
public class OverseerElementPropertyDrawer : PropertyDrawer
{
     const int topBottomMargins = 4;
     const int leftRightMargins = 10;

     List<string> scenes = new List<string>();

     SerializedProperty tag = null;
     SerializedProperty showCursor = null;
     SerializedProperty lockMode = null;
     SerializedProperty scenesNames = null;
     SerializedProperty activeSceneIndex = null;
     SerializedProperty loadingCanvasPrefab = null;   
     SerializedProperty fadeIn = null;
     SerializedProperty fixedDuration = null;
     SerializedProperty duration = null;
     SerializedProperty fadeOut = null;  
     SerializedProperty overrideFadeIn = null;   
     SerializedProperty overrideFadeOut = null;
     SerializedProperty audioClip = null;
     SerializedProperty audioVolume = null;
     SerializedProperty loopAudio = null;

     void FindProperties( SerializedProperty property )
     {          
          tag = property.FindPropertyRelative("tag");
          showCursor = property.FindPropertyRelative("showCursor");
          lockMode = property.FindPropertyRelative("lockMode");

          scenesNames = property.FindPropertyRelative("scenesNames");
          activeSceneIndex = property.FindPropertyRelative("activeSceneIndex");

          loadingCanvasPrefab = property.FindPropertyRelative("loadingCanvasPrefab");
          
          fadeIn = property.FindPropertyRelative("fadeIn");


          fixedDuration = property.FindPropertyRelative("fixedDuration");
          duration = property.FindPropertyRelative("duration");
          fadeOut = property.FindPropertyRelative("fadeOut");  
          overrideFadeIn = property.FindPropertyRelative("overrideFadeIn");   
          overrideFadeOut = property.FindPropertyRelative("overrideFadeOut");
          audioClip = property.FindPropertyRelative("audioClip");
          audioVolume = property.FindPropertyRelative("audioVolume");
          loopAudio = property.FindPropertyRelative("loopAudio");
     }

     public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
     {

          FindProperties( property );

          if( scenes.Count == 0 )
               GatherBuildScenes();
          

          EditorGUI.BeginProperty( position , label , property ); 

          

          Rect fieldRect = position;
          fieldRect.height = EditorGUIUtility.singleLineHeight;
          
          // Tag -----------------------------------------------------------------------------------
          Utilities.DrawTitle( ref fieldRect , "Tag" );
          Utilities.DrawProperty( ref fieldRect , tag );       

          Utilities.DrawEditorHorizontalLine( ref fieldRect , Color.gray );   

          // Cursor ---------------------------------------------------------------------------------
          Utilities.DrawTitle( ref fieldRect , "Cursor" );
          Utilities.DrawProperty( ref fieldRect , showCursor ); 
          Utilities.DrawProperty( ref fieldRect , lockMode );

          Utilities.DrawEditorHorizontalLine( ref fieldRect , Color.gray );
          
          // Scenes -----------------------------------------------------------------------------------
          Utilities.DrawTitle( ref fieldRect , "Scenes" );

          DrawSceneIndexField( ref fieldRect );

         

          Utilities.DrawEditorHorizontalLine( ref fieldRect , Color.gray );
         

          // AudioClip
          Utilities.DrawTitle( ref fieldRect , "Audio" );
          
          Utilities.DrawProperty( ref fieldRect , audioClip , false , "Clip" );

          

          if( audioClip.objectReferenceValue != null )
          {
               Utilities.DrawProperty( ref fieldRect , loopAudio );
               
               Rect labelRect = fieldRect;
               labelRect.width = 80f;

               GUI.Label( labelRect , "Volume" );

               Rect sliderRect = fieldRect;
               sliderRect.width = fieldRect.width - 80f;
               sliderRect.x += 80f;

               audioVolume.floatValue = EditorGUI.Slider( sliderRect , audioVolume.floatValue , 0f , 1f );
               fieldRect.y += fieldRect.height;

          }
          
          Utilities.DrawEditorHorizontalLine( ref fieldRect , Color.gray );
          // Duration -----------------------------------------------------------------------------------
          Utilities.DrawTitle( ref fieldRect , "Duration" );

          Utilities.DrawProperty( ref fieldRect , fixedDuration );

          if( fixedDuration.boolValue )
               Utilities.DrawProperty( ref fieldRect , duration );

          Utilities.DrawEditorHorizontalLine( ref fieldRect , Color.gray );

          // Fade In/Out -----------------------------------------------------------------------------------
          Utilities.DrawTitle( ref fieldRect , "Fade In/Out" );

          Utilities.DrawProperty( ref fieldRect , overrideFadeIn , true );

          if( overrideFadeIn.boolValue )
               Utilities.DrawProperty( ref fieldRect , fadeIn , true );

          Utilities.DrawProperty( ref fieldRect , overrideFadeOut , true );

          if( overrideFadeOut.boolValue )
               Utilities.DrawProperty( ref fieldRect , fadeOut , true );

          

          EditorGUI.EndProperty();

     }

     
 
     public override float GetPropertyHeight( SerializedProperty property , GUIContent label )
     {
          FindProperties( property );

          float space = 0;

          space += EditorGUI.GetPropertyHeight( property );

          space += audioClip.objectReferenceValue != null ? 3 * EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight;

          space += fixedDuration.boolValue ? 2 * EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight;

          space += overrideFadeIn.boolValue ? EditorGUI.GetPropertyHeight( fadeIn , GUIContent.none , true ) : EditorGUIUtility.singleLineHeight;
          space += overrideFadeOut.boolValue ? EditorGUI.GetPropertyHeight( fadeOut , GUIContent.none , true ) : EditorGUIUtility.singleLineHeight;

          space +=  ( property.FindPropertyRelative("scenesNames").arraySize) * EditorGUIUtility.singleLineHeight;
          space += 20 * EditorGUIUtility.singleLineHeight;

          return space;
     }   

         

     void DrawSceneIndexField( ref Rect rect )
     {
          float initialWidth = rect.width;
          float initialX = rect.x;          
          
          Utilities.GUISpace( ref rect );
          
          PrintScenes( ref rect );

          rect.x = initialX;
          rect.width = initialWidth;


          

          Utilities.GUISpace( ref rect , 3 );

          DrawLoadingSreen( ref rect );

          Utilities.GUISpace( ref rect );


          rect.x = initialX;
          rect.width = initialWidth;        
          
     }

     

     
     void PrintScenes( ref Rect rect )
     {

          Rect backgroundRect = rect;

          backgroundRect.height = scenesNames.arraySize != 0 ? EditorGUIUtility.singleLineHeight * ( scenesNames.arraySize + 2 ) + 4f : 
          3 * EditorGUIUtility.singleLineHeight + 4f;
          
          GUI.Box( backgroundRect , "" , EditorStyles.helpBox );

          rect.width -= 8f;
          rect.x += 4f;

          Utilities.GUISpace( ref rect );
          

          if( scenesNames.arraySize == 0 )
          {   
               Utilities.DrawLabel( ref rect , " ---- Empty ---- "   );
          }          
          else
          {
            

               int removedElementIndex = -1;

               for( int i = 0; i < scenesNames.arraySize; i++ )
               {                    
                    string sceneName = scenesNames.GetArrayElementAtIndex( i ).stringValue;                    
                    //int buildIndex = EditorSceneManager.GetSceneByName( sceneName ).buildIndex;

                    if( scenes.Contains( sceneName )  )
                    {

                         if( activeSceneIndex.intValue == i )
                         {
                              GUI.color = Color.green;
                              GUI.Box( rect , "" , EditorStyles.helpBox );
                              GUI.color = Color.white;
                         }

                         GUI.Label( rect , sceneName );

                         Rect buttonRect = rect;

                         buttonRect.width = 20f;
                         buttonRect.x = rect.x + rect.width - buttonRect.width;

                         if( GUI.Button( buttonRect , "x" , EditorStyles.miniButton ) )
                         {
                              removedElementIndex = i;
                         }

                         buttonRect.width = 80f;
                         buttonRect.x -= buttonRect.width;

                         if( GUI.Button( buttonRect , "Set Active" , EditorStyles.miniButton ) )
                              activeSceneIndex.intValue = i;
                         
                         rect.y += rect.height;  
                    }
                    else
                    {
                         GUI.color = Color.red;
                         GUI.Box( rect , "" , EditorStyles.helpBox );
                         GUI.color = Color.white;

                         Rect buttonRect = rect;

                         GUI.Label( rect , "\"" + sceneName + "\" Not included in the build!" );
                         rect.y += rect.height;                         

                         buttonRect.width = 20f;
                         buttonRect.x = rect.x + rect.width - buttonRect.width;

                         if( GUI.Button( buttonRect , "x" , EditorStyles.miniButton ) )
                         {
                              removedElementIndex = i;
                         }

                    }

                     
               }

               if( removedElementIndex != -1 )
               {
                    if( activeSceneIndex.intValue > removedElementIndex  )
                         activeSceneIndex.intValue--;                    

                    scenesNames.DeleteArrayElementAtIndex( removedElementIndex );

                    if( activeSceneIndex.intValue > scenesNames.arraySize - 1 && activeSceneIndex.intValue != 0 )
                         activeSceneIndex.intValue = scenesNames.arraySize - 1;
                    
               }

          }

          Utilities.GUISpace( ref rect );
          
          GUI.enabled = EditorBuildSettings.scenes.Length != scenesNames.arraySize;
          

          if( GUI.Button( rect , "Add Scene" , EditorStyles.miniButton ) )
          {
               GatherBuildScenes();

               GenericMenu menu = new GenericMenu();
               
               for( int i = 0 ; i < scenes.Count ; i++ )
               {
                    string buildScene = scenes[i];
                    bool included = false;

                    for( int j = 0 ; j < scenesNames.arraySize ; j++ )
                    {                         
                         if( buildScene == scenesNames.GetArrayElementAtIndex(j).stringValue )
                              included = true;
                    }


                    if( included )
                         continue;
                    
                    menu.AddItem( new GUIContent( buildScene ) , false , AddSceneToScenesNames , buildScene );
               }
               
               menu.ShowAsContext();
               
          }

          GUI.enabled = true;
         
          
     }


     void AddSceneToScenesNames( object obj )
     {
          string sceneName = (string)obj;

          scenesNames.serializedObject.Update();

          scenesNames.InsertArrayElementAtIndex( scenesNames.arraySize );
          SerializedProperty lastElement = scenesNames.GetArrayElementAtIndex( scenesNames.arraySize - 1 );
          lastElement.stringValue = sceneName;

          scenesNames.serializedObject.ApplyModifiedProperties();
          
     }

     void DrawLoadingSreen( ref Rect rect )
     {
          Utilities.GUISpace( ref rect );

          Utilities.DrawProperty( ref rect , loadingCanvasPrefab );
          
          Utilities.GUISpace( ref rect );
          
     
     }

     void GatherBuildScenes()
     {          
          scenes.Clear();

          for(int i = 0 ; i < EditorBuildSettings.scenes.Length ; i++)
          {
               SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>( EditorBuildSettings.scenes[i].path );
               
               if( sceneAsset != null )
                    scenes.Add( sceneAsset.name ); 
          }

     }

}



#endif

}