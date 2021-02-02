using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lightbug.Overseer
{

[System.Serializable]
public class SceneFade
{
    public bool enabled = true;
    public float duration = 1f;
    public Gradient gradient;

    public SceneFade( bool fadeIn )
    {
        gradient = new Gradient();

        gradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey( Color.black , 0f )
        };

        if(fadeIn)
        {
            
            gradient.alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey( 1f , 0f ) ,
                new GradientAlphaKey( 0f , 1f )
            };
            
        }
        else
        {
            gradient.alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey( 0f , 0f ) ,
                new GradientAlphaKey( 1f , 1f )
            };
        }
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer( typeof(SceneFade ))]
public class SceneFadePropertyDrawer : PropertyDrawer
{
    const int topBottomMargins = 4;
    const int leftRightMargins = 10;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    { 
        EditorGUI.BeginProperty( position , label , property );
        
        SerializedProperty enabled = property.FindPropertyRelative("enabled");
        SerializedProperty duration = property.FindPropertyRelative("duration");
        SerializedProperty gradient = property.FindPropertyRelative("gradient");

        Rect boxRect = position;
        boxRect.height = position.height - 2 * topBottomMargins ;
        boxRect.y += topBottomMargins;

        //GUI.Box( boxRect , "" , EditorStyles.helpBox);
        
        Rect fieldRect = boxRect;
        fieldRect.height = EditorGUIUtility.singleLineHeight;
        fieldRect.width -= leftRightMargins;
        fieldRect.x += leftRightMargins / 2f;
        fieldRect.y += topBottomMargins ;

        Utilities.DrawTitleWithToggle( ref fieldRect , label , enabled );

        GUI.enabled = enabled.boolValue;        

        if( duration.floatValue < 0f )
            duration.floatValue = 0f;

        Utilities.DrawProperty( ref fieldRect , duration );
        Utilities.DrawProperty( ref fieldRect , gradient );
        
               

        GUI.enabled = true;


        

        EditorGUI.EndProperty();

    }

    public override float GetPropertyHeight( SerializedProperty property , GUIContent label )
    {
        return 3 * EditorGUIUtility.singleLineHeight + 4 * topBottomMargins;
    }   
    


}

#endif

}


