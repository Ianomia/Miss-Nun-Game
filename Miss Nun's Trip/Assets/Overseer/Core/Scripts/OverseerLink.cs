using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lightbug.Overseer
{

public class OverseerLink : MonoBehaviour
{
    OverseerController controller = null;

    void Awake()
    {
        controller = OverseerController.Instance;

        if( controller == null )
            throw new System.Exception( "Overseer controller not instantiated." );     
              
        
    }

    /// <summary>
    /// Starts the transition betweet the current element and next one (based on the elements list).
    /// </summary>
    public void GoToNextElement()
    {
        controller.GoToNextElement();
    }

    /// <summary>
    /// Starts the transition betweet the current element and previous one (based on the elements list).
    /// </summary>
    public void GoToPreviousElement()
    {
        controller.GoToPreviousElement();
    }

    /// <summary>
    /// Starts the transition betweet the current element and a target element.
    /// </summary>
    /// <param name="tag">The tag of the target element.</param>
    public void GoToElement( string tag )
    {
        controller.GoToElement( tag );
    }
    
    /// <summary>
    /// Reloads the current element, without any transition.
    /// </summary>
    public void ReloadCurrentElement()
    {                
        controller.ReloadCurrentElement();
    }

    /// <summary>
    /// Reloads the current element, using a fade out.
    /// </summary>
    public void ReloadCurrentElementWithFadeOut()
    {                
        controller.ReloadCurrentElementWithFadeOut();
    }

    /// <summary>
    /// Sends the quit signal to the controller.
    /// </summary>
    public void QuitApplication()
    {
        controller.QuitApplication();
        
    }

    /// <summary>
    /// Sends the quit signal to the controller, using the current element fade out transition.
    /// </summary>
    public void QuitApplicationWithFadeOut()
    {
        controller.QuitApplicationWithFadeOut();

    }

    
}

}
