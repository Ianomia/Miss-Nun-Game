using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace Lightbug.Overseer
{

[CustomEditor( typeof( OverseerMainAsset ) )]
public class OverseerMainAssetEditor : Editor
{
    void OnEnable()
    {

    }
    
            
    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        // Debug 
        //DrawDefaultInspector();

        if( GUILayout.Button( "Show Editor" ) )
        {
            OverseerEditorWindow.ShowWindow();
        }

        serializedObject.ApplyModifiedProperties();
    }

}

}

#endif
