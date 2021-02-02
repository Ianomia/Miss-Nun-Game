using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Lightbug.Overseer
{

public enum OverseerElementState
{
    FadeIn ,
    Normal ,
    FadeOut ,
    Loading
}

public class OverseerController : MonoBehaviour
{    
    static OverseerController instance = null;
    public static OverseerController Instance
    {
        get
        {
            return instance;
        }
    }

    public OverseerMainAsset mainAsset = null;

    public int startingIndex = 0;

    float timeCursor = 0f;
    float mainValue = 0f;

    OverseerElementState currentElementState = OverseerElementState.FadeIn;  

    Image fadeScreen = null;

    Canvas loadingCanvas = null;

    SceneFade currentFade = null;    
    
    
    Image loadingImage = null;
    Animation loadingAnimation = null;

    Text loadingText = null;
    string loadingFixedText = null;
    

    List<OverseerElement> elements;

    OverseerElement previousElement = null;
    OverseerElement currentElement = null;
    OverseerElement nextElement = null;
    
    

    AudioSource audioSource = null;

    bool quitApplication = false;

    Scene loadingScene = new Scene();
    
    List<AsyncOperation> asyncOperations = new List<AsyncOperation>();
    
       
    public void Awake()
    {
        if( instance == null )
        {
            instance = this;
            DontDestroyOnLoad( gameObject );
        }
        else
        {
            Destroy( gameObject );            
            return;
        }
        
        if( mainAsset == null )
            throw new System.Exception("Null main asset!");

        InitializeCanvas();
        InitializeElements();
        InitializeAudio();

        
        if( elements.Count == 0 )
            throw new System.Exception("Empty main asset!");
        
        currentElement = elements[ startingIndex ];

        
        LoadElement();
    }

    
    

    void InitializeElements()
    {
        elements = mainAsset.Elements;
    }
   

    void InitializeCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        DontDestroyOnLoad( canvasObj );

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        CanvasScaler canvasScaler = canvasObj.AddComponent<CanvasScaler>();

        canvas.sortingOrder = 128;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Fade Screen
        GameObject fadeScreenObj = new GameObject("Fade Screen");
        fadeScreenObj.transform.SetParent( canvasObj.transform );

        fadeScreen = fadeScreenObj.AddComponent<Image>();
        fadeScreen.color = Color.black;        

        RectTransform fadeScreenRectTransform = fadeScreenObj.GetComponent<RectTransform>();
        fadeScreenRectTransform.anchorMin = Vector2.zero;
        fadeScreenRectTransform.anchorMax = Vector2.one;

        fadeScreenRectTransform.offsetMin = Vector2.zero;
        fadeScreenRectTransform.offsetMax = Vector2.zero;
        
    }

    void InitializeAudio()
    {
        audioSource = gameObject.AddComponentOverride<AudioSource>();
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
    }

  
    
    void LoadElement()
    {                
        currentElementState = OverseerElementState.Loading;

        StartCoroutine( LoadScene() );      
    }

    bool StartFromHere()
    { 
        int sceneBuildIndex = SceneManager.GetActiveScene().buildIndex;      

        return false;
    }
     
    void Update()
    {
        UpdateElement( Time.deltaTime );
        
    }
    
    
    void UpdateElement( float dt )
    {       
        if( currentElement == null )
            return;
        
        timeCursor += dt;

        Color targetColor = Color.black;

        switch( currentElementState )
        {
            case OverseerElementState.FadeIn:

                if( Fade() )
                {
                    GoToNormalState();
                }

                break;

            case OverseerElementState.Normal:
                

                if( currentElement.fixedDuration )
                {

                    if( timeCursor >=  currentElement.duration )
                    {  
                        GoToNextElement();     
                        DismissCurrentElement();         
                    }
                }        

                audioSource.volume = currentElement.audioVolume;        
                

                break;

            case OverseerElementState.FadeOut:


                if( Fade() )
                {         
                    if( quitApplication )   
                        Application.Quit();
                    else        
                        ShiftElements();
                }                

                break;

            case OverseerElementState.Loading:

                break;
        }

        

    }

    
    void SetFade( OverseerElement element , bool fadeIn )
    {

        if( fadeIn )
            currentFade = element.overrideFadeIn ? element.fadeIn : mainAsset.fadeIn;  
        else
            currentFade = element.overrideFadeOut ? element.fadeOut : mainAsset.fadeOut;  

        if( !currentFade.enabled )
            currentFade = null;

    }

    bool Fade()
    {                   
        Color color = currentFade.gradient.Evaluate( timeCursor / currentFade.duration );
        fadeScreen.color = color;

        float t = timeCursor / currentFade.duration;
        audioSource.volume = currentElement.audioVolume * ( currentElementState == OverseerElementState.FadeIn ? t : 1 - t );

        return timeCursor >= currentFade.duration;
    }
    

    void ShiftElements()
    {
        timeCursor = 0f;

        previousElement = currentElement;
        currentElement = nextElement;
        nextElement = null;  

        if( currentElement == null )
            return;
        

        LoadElement();

        
    }

    // ----------------------------------------------------------------------------------------------------------------------------------


    void DismissCurrentElement()
    {
        timeCursor = 0f;

        SetFade( currentElement , false );    

        if( currentFade != null )
            GoToFadeOutState();
        else
            ShiftElements();

    }

    


    // ----------------------------------------------------------------------------------------------------------------------------------
    
        
    void GoToFadeInState()
    {
        timeCursor = 0f;
        
        currentElementState = OverseerElementState.FadeIn;

        fadeScreen.enabled = true;
        fadeScreen.color = currentElement.fadeIn.gradient.Evaluate( 0f );

        audioSource.volume = 0f;

        
    }

    void GoToFadeOutState()
    {
        timeCursor = 0f;

        currentElementState = OverseerElementState.FadeOut;

        fadeScreen.enabled = true;
        fadeScreen.color = currentElement.fadeOut.gradient.Evaluate( 0f );

        audioSource.volume = currentElement.audioVolume;
        
    }

    void GoToNormalState()
    {
        audioSource.volume = currentElement.audioVolume;

        timeCursor = 0f;
        currentElementState = OverseerElementState.Normal;
        fadeScreen.enabled = false;
    }

    
    // ----------------------------------------------------------------------------------------------------------------------------------
    
    
    
    IEnumerator LoadScene()
    {                
        
        bool useLoadingScene = currentElement.loadingCanvasPrefab != null;

        if( useLoadingScene )
        {
            
            Application.backgroundLoadingPriority = ThreadPriority.High;

            fadeScreen.color = new Color( 0f , 0f , 0f , 0f );

            ShowLoadingCanvas( true );
            

        }

        
        
        if( currentElement.scenesNames.Count == 1 )
        {
            yield return StartCoroutine( LoadSingleScene( useLoadingScene ) );
        }
        else
        {
            yield return StartCoroutine( LoadMultipleScenes( useLoadingScene ) );
        }


        if( useLoadingScene )
        {      
            ShowLoadingCanvas( false );        
            
            if( loadingImage != null )
            {
                loadingImage.fillAmount = 0f;
                loadingImage = null;
            }
            else if( loadingAnimation != null )
            {                
                loadingAnimation = null;
            }

        }
        


        SetFade( currentElement , true );       

        if( currentFade != null )
            GoToFadeInState();        
        else
            GoToNormalState();

        if( currentElement.audioClip != null )
        {
            audioSource.clip = currentElement.audioClip;
            audioSource.loop = currentElement.loopAudio;
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }

        Cursor.visible = currentElement.showCursor;        
        Cursor.lockState = currentElement.lockMode;
        
        
    }

    void ShowLoadingCanvas( bool show )
    {
        if( show )
        {
            GameObject canvasGameObject = Instantiate<GameObject>( currentElement.loadingCanvasPrefab );
            DontDestroyOnLoad( canvasGameObject );
            loadingCanvas = canvasGameObject.GetComponent<Canvas>();

            loadingCanvas.sortingOrder = 100;         //fade canvas = 128
            loadingCanvas.enabled = true;
        }
        else
        {
            Destroy( loadingCanvas.gameObject );
        }

        loadingCanvas.enabled = show;
        
        
    }

    
    IEnumerator LoadSingleScene( bool useLoadingScene = false )
    {       
               
        AsyncOperation async = SceneManager.LoadSceneAsync( currentElement.scenesNames[0] , LoadSceneMode.Single );
        async.allowSceneActivation = true;
        
        if( useLoadingScene )
            InitializeLoadingComponents();

        float totalProgress = 0f;
        while( !async.isDone )
        {
            if( useLoadingScene )
            {
                totalProgress = async.progress;                 

                UpdateLoadingComponents( totalProgress );
                
            }            

            yield return null;
        }

        if( useLoadingScene )
            UpdateLoadingComponents( 1f );

        
        
    }

    

    IEnumerator LoadMultipleScenes( bool useLoadingScene = false )
    {        
        
        int numberOfScenesToLoad = currentElement.scenesNames.Count;


        if( useLoadingScene )
            InitializeLoadingComponents();
        
        float totalProgress = 0f;

  



        for( int i = useLoadingScene ? 0 : 1 ; i < numberOfScenesToLoad ; i++ )
        {
            AsyncOperation async = SceneManager.LoadSceneAsync( currentElement.scenesNames[ i ] , i == 0 ? LoadSceneMode.Single : LoadSceneMode.Additive );            
            async.allowSceneActivation = true;

            // asyncOperations.Add( async );

            while( async.progress < 0.9f )
            {
                if( useLoadingScene )
                {   
                    float currentProgress = async.progress;              
                    
                    totalProgress = ( (float)i / numberOfScenesToLoad ) + currentProgress;                     

                    UpdateLoadingComponents( totalProgress );
                }                              

                yield return null;
            }
            
        }


        if( useLoadingScene )
            UpdateLoadingComponents( 1f );

        

        //Active Scene
        SceneManager.SetActiveScene( SceneManager.GetSceneByName( currentElement.scenesNames[ currentElement.activeSceneIndex ] ) );
        
        
    }

    void InitializeLoadingComponents()
    {
        if( loadingImage != null )
        {
            loadingImage.fillAmount = 0f;
        }
        else if( loadingAnimation != null )
        {                        
            loadingAnimation.Play();
            loadingAnimation[ loadingAnimation.clip.name ].speed = 0f;
        }

        if( loadingText != null )
        {
            loadingText.text = loadingFixedText + " 0 %";
        }
        
    }

    void UpdateLoadingComponents( float totalProgress )
    {
        if( loadingImage != null )
        {
            loadingImage.fillAmount = totalProgress;
        }
        else if( loadingAnimation != null )
        {                        
            loadingAnimation[ loadingAnimation.clip.name ].speed = 1f;
            loadingAnimation[ loadingAnimation.clip.name ].time = totalProgress * loadingAnimation.clip.length;
            loadingAnimation[ loadingAnimation.clip.name ].speed = 0f;
            
        }

        if( loadingText != null )
        {
            loadingText.text = loadingFixedText + ( totalProgress * 100 ).ToString("F0") + " %";
        }
    }
   
    /// <summary>
    /// Starts the transition betweet the current element and next one (based on the elements list).
    /// </summary>
    public void GoToNextElement()
    {        
        if( currentElementState == OverseerElementState.FadeIn || currentElementState == OverseerElementState.FadeOut )
            return;
        
        int currentIndex = elements.IndexOf( currentElement );
        int nextIndex = currentIndex + 1;

        if( nextIndex != elements.Count )
            nextElement = elements[ nextIndex ];
        
        DismissCurrentElement();

    }

    /// <summary>
    /// Starts the transition betweet the current element and previous one (based on the elements list).
    /// </summary>
    public void GoToPreviousElement()
    {        
        if( currentElementState == OverseerElementState.FadeIn || currentElementState == OverseerElementState.FadeOut )
            return;
        
        int currentIndex = elements.IndexOf( currentElement );
        int nextIndex = currentIndex - 1;

        if( nextIndex != -1 )
            nextElement = elements[ nextIndex ];

        DismissCurrentElement();
            
    }

    /// <summary>
    /// Starts the transition betweet the current element and a target element.
    /// </summary>
    /// <param name="tag">The tag of the target element.</param>
    public void GoToElement( string tag )
    {             
        
        if( currentElementState == OverseerElementState.FadeIn || currentElementState == OverseerElementState.FadeOut )
            return;
        
        
        if( tag.IsNullOrEmpty() )
            return;


        for( int i = 0 ; i < elements.Count ; i++ )
        {
            if( string.Equals( elements[ i ].tag , tag ) )
            {
                nextElement = elements[ i ];
                break;
            }
        }

        DismissCurrentElement(); 

    }

    /// <summary>
    /// Starts the transition betweet the current element and a target element.
    /// </summary>
    /// <param name="index">The index (order) of the target element.</param>
    public void GoToElement( int index )
    {                
        if( currentElementState == OverseerElementState.FadeIn || currentElementState == OverseerElementState.FadeOut )
            return;
        
        if( index >= elements.Count )
            return;
        
        nextElement = elements[ index ];    

        DismissCurrentElement(); 

    }

    /// <summary>
    /// Sets the current element.
    /// </summary>
    /// <param name="tag">The index (order) of the target element.</param>
    public void SetCurrentElement( int index )
    {   
        
        if( index >= mainAsset.Elements.Count )
            return;
        
        currentElement = mainAsset.Elements[ index ];
        
    }


    /// <summary>
    /// Reloads the current element instantly, without any transition.
    /// </summary>
    public void ReloadCurrentElement()
    {                
        if( currentElementState == OverseerElementState.FadeIn || currentElementState == OverseerElementState.FadeOut )
            return;
        
        int currentIndex = elements.IndexOf( currentElement );

        nextElement = elements[ currentIndex ];

        DismissCurrentElement();

    }

    /// <summary>
    /// Reloads the current element, using a fade out.
    /// </summary>
    public void ReloadCurrentElementWithFadeOut()
    {                
        if( currentElementState == OverseerElementState.FadeIn || currentElementState == OverseerElementState.FadeOut )
            return;
        
        int currentIndex = elements.IndexOf( currentElement );

        nextElement = elements[ currentIndex ]; 

        DismissCurrentElement();
    }

    
    /// <summary>
    /// Sets the controller loading scene component. This component will indicate the progress of the async operation.
    /// </summary>
    /// <param name="loadingImage">The Unity's Image component.</param>
    public void SetLoadingComponent( Image loadingImage )
    {
        this.loadingImage = loadingImage;
    }

    /// <summary>
    /// Sets the controller loading scene component. This component will indicate the progress of the async operation.
    /// </summary>
    /// <param name="loadingAnimator">Unity's legacy Animation component.</param>
    public void SetLoadingComponent( Animation loadingAnimation )
    {
        this.loadingAnimation = loadingAnimation;
    }

    /// <summary>
    /// Sets the controller loading scene component. This component will indicate the progress of the async operation.
    /// </summary>
    /// <param name="loadingImage">The Unity's Image component.</param>
    public void SetLoadingComponentText( Text loadingText , string loadingFixedText = null )
    {
        if( !loadingFixedText.IsNullOrEmpty() )
            this.loadingFixedText = loadingFixedText;


        this.loadingText = loadingText;
    }

    /// <summary>
    /// Sends the quit signal to the controller.
    /// </summary>
    public void QuitApplication()
    {
        quitApplication = true;       

        Application.Quit();
        
    }

    /// <summary>
    /// Sends the quit signal to the controller, using the current element fade out transition.
    /// </summary>
    public void QuitApplicationWithFadeOut()
    {
        quitApplication = true;
        
        timeCursor = 0f;
        SetFade( currentElement , false );    

        if( currentFade != null )
            GoToFadeOutState();
        else
            Application.Quit();

    }

}

}
