using UnityEngine;
using UnityEngine.UI;

namespace Lightbug.Overseer
{

public enum LoadingComponentType
{
    FillAmount ,
    Animation
}


public class OverseerLoadingComponent : MonoBehaviour
{
    [SerializeField] LoadingComponentType type = LoadingComponentType.FillAmount;

    [SerializeField] Image image = null;
    [SerializeField] new Animation animation = null;

    [SerializeField] Text text = null;

    [SerializeField] bool useFixedText = true;
    [SerializeField] string fixedText = "Loading ...";

    [SerializeField] string progressFormat = "F0";

    void Awake()
    {        
        if( type == LoadingComponentType.FillAmount )
        {
            if( image == null )
                throw new System.Exception("Image component is null.");
            
            OverseerController.Instance.SetLoadingComponent( image );
        }
        else
        {
            if( animation == null )
                throw new System.Exception("Animation component is null.");
                        
            animation.playAutomatically = false;

            OverseerController.Instance.SetLoadingComponent( animation );
        }

        if( text != null )
        {
            if( useFixedText )
                OverseerController.Instance.SetLoadingComponentText( text );
            if( useFixedText )
                OverseerController.Instance.SetLoadingComponentText( text , fixedText );
        }
        
        
        
    }
    

    
}





}
