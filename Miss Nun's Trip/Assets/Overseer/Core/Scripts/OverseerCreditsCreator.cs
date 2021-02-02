using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.UI;


#if UNITY_EDITOR

using UnityEditor;

namespace Lightbug.Overseer
{

public class OverseerCreditsCreator : EditorWindow
{
    [SerializeField]
    TextAsset textFile = null;

    public string path = "";

    public Text targetLeftText = null;
    public Text targetRightText = null;

    StringBuilder leftBuilder = new StringBuilder();
    StringBuilder rightBuilder = new StringBuilder();

    string columnsSeparatorString = "\\&";
    char[] columnsDivisorCharArray;

    bool validFile = false;

    static OverseerCreditsCreator editorWindow = null;

    [MenuItem("Overseer/Credits Creator")]
    static void ShowWindow()
    {
        editorWindow = GetWindow<OverseerCreditsCreator>( true , "Overseer Credits Creator" );	
        editorWindow.minSize = new Vector2( 300f , 200f );
        editorWindow.Show();
    }



    void CreateCredits()
    {        
        StreamReader reader = new StreamReader(path);

        if( reader == null )
            throw new System.Exception("Null text file");

        leftBuilder.Length = 0;
        rightBuilder.Length = 0;

        columnsSeparatorString = columnsSeparatorString.Trim();

        string line = null;
        while( ( line = reader.ReadLine() ) != null )
        {
            if( line.Contains( columnsSeparatorString ) )
            {
                string[] splits = line.Split( columnsSeparatorString.ToCharArray() );

                //Left Column
                leftBuilder.Append( splits[ 0 ].Trim() );
                leftBuilder.Append( '\n' );

                //Right Column
                if( splits[ 2 ] != null )
                {
                    rightBuilder.Append( splits[ 2 ].Trim() );
                    rightBuilder.Append( '\n' );
                }
                
            }
            else
            {                
                if( line.IsNullOrWhiteSpace() )
                {
                    leftBuilder.Append( '\n' );                    
                    rightBuilder.Append( '\n' );
                }
                else
                {
                    leftBuilder.Append( line.Trim() );
                    leftBuilder.Append( '\n' );
                    
                    rightBuilder.Append( '\n' );
                }
            }     
            
        }

        

        targetLeftText.text = leftBuilder.ToString();
        targetRightText.text = rightBuilder.ToString();

        reader.Close();

        Debug.Log("Text Created");
        
    }

    void OnGUI()
    {        
        GUILayout.Space( 20 );

        EditorGUILayout.HelpBox(
            "This tool will take a txt file, split it into two columns of text, left and right (based on the chosen separator) " +
            "and assign each one of the columns to the left and right Text components (text fields). " +
            "The strings obtained from each line of text will be trimmed" , MessageType.Info );
        
        Utilities.DrawEditorLayoutHorizontalLine( Color.gray );

        EditorGUI.BeginChangeCheck();


        textFile = (TextAsset)EditorGUILayout.ObjectField( "Text File (.txt)" , textFile , typeof(TextAsset) , false );


        if( EditorGUI.EndChangeCheck() )
        {
            if( textFile != null )
            {
                path = AssetDatabase.GetAssetPath( textFile );

                validFile = path.EndsWith( ".txt" );
            }
        }

        if( !validFile )
        {
            if( textFile != null )
            {
                EditorGUILayout.HelpBox(
                    "This is not a txt file!" , MessageType.Info );
            }
            
            return;
        }

        

        Utilities.DrawEditorLayoutHorizontalLine( Color.gray );

        EditorGUILayout.HelpBox(
            "This is the delimiter string, what separates the left and right columns." , MessageType.Info );
        
        columnsSeparatorString = EditorGUILayout.TextField( "Delimiter String", columnsSeparatorString );

        Utilities.DrawEditorLayoutHorizontalLine( Color.gray );

        EditorGUILayout.HelpBox(
            "Choose the left and right Text components" , MessageType.Info );
        targetLeftText = (Text) EditorGUILayout.ObjectField( "Left Column" , targetLeftText , typeof(Text) , true );
        targetRightText = (Text) EditorGUILayout.ObjectField( "Right Column" , targetRightText , typeof(Text) , true );

        

        if( targetLeftText != null && targetRightText != null )
        {
            Utilities.DrawEditorLayoutHorizontalLine( Color.gray );

            if( GUILayout.Button( "Create Credits Text" ) )
            {
                CreateCredits();
            }
        }

        


    }

    
}

}

#endif


