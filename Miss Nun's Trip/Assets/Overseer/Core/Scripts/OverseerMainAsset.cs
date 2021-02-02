using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lightbug.Overseer
{

[CreateAssetMenu( menuName = "Overseer/Main Asset" )]
public class OverseerMainAsset : ScriptableObject
{

    [HideInInspector]
    [SerializeField]
    List<OverseerElement> elements = new List<OverseerElement>();
    public List<OverseerElement> Elements
    {
        get
        {
            return elements;
        }
    }

    public SceneFade fadeIn = new SceneFade( true );    
    
    public SceneFade fadeOut = new SceneFade( false );
    

}

}
