using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;

namespace Lightbug.Overseer
{


public class OverseerEditorWindow : EditorWindow
{
    const float InspectorWidth = 300f;


    SerializedProperty elements = null;    

    SerializedProperty fadeIn = null;
    SerializedProperty fadeOut = null;

    List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
    
    Editor editor = null;

    ReorderableList reorderableList = null;

    int elementToDelete = -1;

    Vector2 inspectorScrollPosition;
    Vector2 mainAssetScrollPosition;

    SerializedObject serializedObject = null;

    static OverseerEditorWindow editorWindow = null;

    bool active = false;
    OverseerMainAsset mainAsset = null;

    GUIStyle centeredBlack = new GUIStyle();

    List<Scene> openScenes = new List<Scene>();

    List<string> scenes = new List<string>();
    
    void Initialize()
    {        
        serializedObject = new SerializedObject( mainAsset );
        
        fadeIn = serializedObject.FindProperty("fadeIn");
        fadeOut = serializedObject.FindProperty("fadeOut");

        elements = serializedObject.FindProperty("elements");

        
        reorderableList = new ReorderableList( serializedObject , elements , true , false , false , false ); 
        reorderableList.elementHeight = 1.2f * EditorGUIUtility.singleLineHeight;
        reorderableList.headerHeight = 0f;
        reorderableList.footerHeight = 0f;

        reorderableList.drawElementCallback += OnDrawElement;
        reorderableList.onAddCallback += OnAdd;

        Undo.undoRedoPerformed += OnUndo;

        // >= 2017.3 sceneListChanged 
        
         
        GatherBuildScenes();
    }

    [MenuItem("Overseer/Show Editor")]
    public static void ShowWindow()
    {
        editorWindow = GetWindow<OverseerEditorWindow>( true , "Overseer Editor");	
        editorWindow.maxSize = new Vector2( 2 * InspectorWidth , 650f );
        editorWindow.minSize = new Vector2( 2 * InspectorWidth , 200f );
        editorWindow.Show();
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

    void OnEnable()
    {        
                
        CheckSelection();

        if( !active )
            return;
        
        Initialize();

        centeredBlack.alignment = TextAnchor.MiddleCenter;

    }

    void OnDisable()
    {
        if( reorderableList != null )
        {
            reorderableList.drawElementCallback -= OnDrawElement;
            reorderableList.onAddCallback -= OnAdd;
        }

        Undo.undoRedoPerformed -= OnUndo;
    }
    

    void CheckSelection()
    {
        Object currentSelectedObject = Selection.activeObject;
        
        if( Selection.activeObject == null )
            return;
                

        if( currentSelectedObject.GetType() == typeof( OverseerMainAsset ) || 
            currentSelectedObject.GetType().IsSubclassOf( typeof( OverseerMainAsset ) ) )
        {
            active = true;
            string path = AssetDatabase.GetAssetPath( currentSelectedObject );
            mainAsset = AssetDatabase.LoadAssetAtPath<OverseerMainAsset>( path );            
            
        }
        else
        {
            active = false;
        }
        

    }

    void OnSelectionChange()
    {
        CheckSelection();

        Repaint();

        if( active )
            Initialize();
    }

    
    void OnDrawHeader( Rect rect )
    {
        GUI.Label( rect , "Elements" );
    }
    
    void OnDrawElement( Rect rect, int index, bool isActive , bool isFocused )
    {        
        
        SerializedProperty element = elements.GetArrayElementAtIndex( index );
        
        SerializedProperty loadingCanvasPrefab = element.FindPropertyRelative("loadingCanvasPrefab");

        Rect auxRect = rect;

        Rect statusRect = auxRect;
        statusRect.width = 10;
        statusRect.height = 10;
        statusRect.x += 3;
        statusRect.y += 3;

        
        if( isValid( element ) )
        {
            EditorGUI.DrawRect( statusRect , Color.green );                  
        }
        else
        {
            EditorGUI.DrawRect( statusRect , Color.red );
        }

        auxRect.x += 15;
        
        GUI.Label( auxRect , string.IsNullOrEmpty( element.FindPropertyRelative("tag").stringValue ) ? "( Untagged )" : element.FindPropertyRelative("tag").stringValue );
        
        
        
        auxRect.height = EditorGUIUtility.singleLineHeight;
        auxRect.width = 20f;
        auxRect.x = rect.x + rect.width - auxRect.width;

        GUI.contentColor = Color.white;

        if( GUI.Button( auxRect , "x" , EditorStyles.miniButton ) )
        {
            elementToDelete = index;                   
        }        
        
        auxRect.x -= 1.2f * auxRect.width;
        if( loadingCanvasPrefab.objectReferenceValue != null )
        {
            GUI.Label( auxRect , "L" );                  
        }

    }

    bool isValid( SerializedProperty element  )
    {

        SerializedProperty scenesNames = element.FindPropertyRelative( "scenesNames" );
        if( scenesNames.arraySize == 0 )
        {            
            return false;
        }  

        for( int i = 0; i < scenesNames.arraySize; i++ )
        {                    
            string sceneName = scenesNames.GetArrayElementAtIndex( i ).stringValue;
            
            if( !scenes.Contains( sceneName )  )  
            {    
                return false;
            }
        
        }

        return true;
    }

    
    void OnAdd( ReorderableList list )
    {        
        mainAsset.Elements.Add( new OverseerElement() );
    }

    void OnRemove( ReorderableList list )
    {
        elements.DeleteArrayElementAtIndex( list.index );
    }
    
    void OnGUI()
    {
        if( !active )
            return;

        if( editorWindow == null )   
            return;
        
        GatherBuildScenes();
        
        serializedObject.Update();

        Rect verticalSeparatorRect = new Rect ( editorWindow.position.width / 2 , 0 , 1 , editorWindow.position.height );
        EditorGUI.DrawRect( verticalSeparatorRect , Color.gray );
       
        Rect mainAssetRect = new Rect ( 0 , 0 , editorWindow.position.width - InspectorWidth , editorWindow.position.height );

        


        GUILayout.BeginArea( mainAssetRect );

        mainAssetRect.x += 20f;
        mainAssetRect.y += 20f;
        mainAssetRect.width -= 50f;        

        mainAssetScrollPosition = GUILayout.BeginScrollView( mainAssetScrollPosition ); 

        DrawElementsEditor();  

        GUILayout.EndScrollView();

        GUILayout.EndArea();


        Rect inspectorRect = new Rect ( editorWindow.position.width - InspectorWidth , 0 , InspectorWidth , editorWindow.position.height );

        

        inspectorRect.x += 10f;
        inspectorRect.y += 10f;
        inspectorRect.width -= 20f;
        inspectorRect.height -= 20f;

        GUILayout.BeginArea( inspectorRect );

        inspectorScrollPosition = GUILayout.BeginScrollView( inspectorScrollPosition ); 
        
        DrawElementInspector();        

        GUILayout.EndScrollView();

        GUILayout.EndArea();

        serializedObject.ApplyModifiedProperties();


    }
    
    void DrawElementsEditor()
    {
        GUILayout.BeginVertical();      

        GUILayout.BeginVertical( EditorStyles.helpBox );  

        GUILayout.Label( "Elements" , EditorStyles.boldLabel );


        GUILayout.Space(10);

        reorderableList.DoLayoutList();

        if( elementToDelete != -1 )
        {
            elements.DeleteArrayElementAtIndex( elementToDelete );
            elementToDelete = -1;

            GUILayout.EndVertical();   
            GUILayout.EndVertical();          
            return;
        }


        if( GUILayout.Button("Add Element" , EditorStyles.miniButton ) )      
            mainAsset.Elements.Add( new OverseerElement() );
        
        GUILayout.Space(5);

        GUILayout.EndVertical();


        GUILayout.BeginVertical( EditorStyles.helpBox );

        GUILayout.Label( "Global Fade In/Out" , EditorStyles.boldLabel );

        EditorGUILayout.PropertyField( fadeIn , true );
        EditorGUILayout.PropertyField( fadeOut , true );

        GUILayout.EndVertical();

        GUILayout.EndVertical();
        
    }

   
    void OnUndo()
    {
        Repaint();
    }

    void DrawElementInspector()
    {        
             
        bool itemSelected = reorderableList.index != -1 && reorderableList.index < elements.arraySize;

        if( !itemSelected )
            return;
                

        SerializedProperty element = elements.GetArrayElementAtIndex( reorderableList.index );

        if( element == null )
            return;
        
        GUILayout.BeginVertical();

        GUILayout.Space( 10f );
        
        GUILayout.Label( "Element Inspector" , EditorStyles.boldLabel );
        

        GUILayout.Space( 10f );

        Utilities.DrawEditorLayoutHorizontalLine( Color.gray , 1 , 1 );
        Utilities.DrawEditorLayoutHorizontalLine( Color.gray , 1 , 1 );

        GUILayout.EndVertical();
        
        DrawElementProperty( element );
        
    }

    
    
    void SaveOpenScenes()
    {
        openScenes.Clear();

        for (int i = 0; i < SceneManager.sceneCount ; i++)
            openScenes.Add( SceneManager.GetSceneAt( i ) );
        

        EditorSceneManager.SaveModifiedScenesIfUserWantsTo( openScenes.ToArray() ); 
    }

    void DrawElementTitle()
    {
        bool itemSelected = reorderableList.index != -1 && reorderableList.index < elements.arraySize;

        if( !itemSelected )
        {
            return;
        }        

        SerializedProperty element = elements.GetArrayElementAtIndex( reorderableList.index );

        if( element == null )
        {
            return;
        }


        SerializedProperty indices = element.FindPropertyRelative("indices");
        SerializedProperty sceneMask = element.FindPropertyRelative("sceneMask");
        SerializedProperty activeSceneIndex = element.FindPropertyRelative("activeSceneIndex");

        int maskLimit = ( 1 << EditorBuildSettings.scenes.Length ) - 1;
        bool allowedIndex = indices.arraySize != 0 && element.FindPropertyRelative("sceneMask").intValue <= maskLimit;
        
        
        GUILayout.BeginVertical();

        GUILayout.Space( 10f );

        GUI.enabled = allowedIndex;
        if( GUILayout.Button( "Play this element" , EditorStyles.miniButton ) )
        {
            if( indices.arraySize != 0 )
            {
                openScenes.Clear();
                for (int i = 0; i < SceneManager.sceneCount ; i++)
                    openScenes.Add( SceneManager.GetSceneAt( i ) );
                

                EditorSceneManager.SaveModifiedScenesIfUserWantsTo( openScenes.ToArray() ); 

                Scene sceneLoaded = new Scene();

                for( int i = 0; i < indices.arraySize ; i++ )
                {
                    int buildIndexToLoad = indices.GetArrayElementAtIndex( i ).intValue;
                    string path = EditorBuildSettings.scenes[ buildIndexToLoad ].path;

                    if( i == 0 )                    
                        sceneLoaded = EditorSceneManager.OpenScene( path , OpenSceneMode.Single); 
                    else
                        sceneLoaded = EditorSceneManager.OpenScene( path , OpenSceneMode.Additive); 
                    
                    if( i == activeSceneIndex.intValue )
                        SceneManager.SetActiveScene( sceneLoaded );

                }            
                
                EditorApplication.isPlaying = true;
            
            }
        }

        GUI.enabled = true;

        GUILayout.Space( 20f );

        Utilities.DrawEditorLayoutHorizontalLine( Color.gray , 2);
        GUILayout.EndVertical();
    }

    void DrawElementProperty( SerializedProperty element )
    {
        EditorGUILayout.PropertyField( element , true );
    }

    void ChangeName( SerializedObject sObject , int targetIndex )
    {
        SerializedProperty sceneProperty = sObject.FindProperty("scene");
        string scenePath = sceneProperty.FindPropertyRelative("path").stringValue;

        Debug.Log( scenePath );
        SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>( scenePath );
        
        if( asset != null )
        {
            if( !string.Equals( sObject.targetObject.name , asset.name ) )
            {
                sObject.targetObject.name =  targetIndex + "_" + asset.name;
                AssetDatabase.SaveAssets();
            }
        }
    }

    void DrawCachedEditor( string title = "" , GUIStyle style = null )
    {
        if( style != null )
            GUILayout.BeginVertical( style );
        
        if( !string.IsNullOrEmpty( title ) )
            GUILayout.Label( title , EditorStyles.boldLabel );

        editor.OnInspectorGUI();

        if( style != null )
            GUILayout.EndVertical();
        
    }
    
    
}

}

#endif



