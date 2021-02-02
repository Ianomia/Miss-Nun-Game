using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Lightbug.Overseer
{

public class OverseerCredits : MonoBehaviour
{
	[Tooltip("speed in pixels per second")]
	public float speed = 20f;

	[SerializeField]
	RectTransform creditsRectTransform = null;

	[SerializeField]
	RectTransform maskRectTransform = null;

	[SerializeField]
	UnityEvent isDoneEvent;

	

	float targetDelta = 0f;


	void Awake()
	{
		if( isDoneEvent.GetPersistentEventCount() == 0 )
			throw new System.Exception("Empty persistent event list.");
		
		if( creditsRectTransform == null )
			throw new System.Exception("Select the UI credits Object.");

		if( maskRectTransform == null )
			throw new System.Exception("Select the UI mask.");


		targetDelta = 0.5f * ( creditsRectTransform.sizeDelta.y + maskRectTransform.sizeDelta.y );

	}

	void Update()
	{
		
		creditsRectTransform.Translate( Vector3.up * speed * Time.deltaTime );

		float delta = creditsRectTransform.anchoredPosition.y - maskRectTransform.anchoredPosition.y;
		if( delta > targetDelta )
		{
		 	isDoneEvent.Invoke();
			this.enabled = false;
		}

		
	}


	// DEBUG --------------------------------------------------------------------------------------
	// void OnDrawGizmos()
	// {
	// 	if( creditsRectTransform == null )
	// 		return;

	// 	if( maskRectTransform == null )
	// 		return;			
		
	// 	float delta = creditsRectTransform.anchoredPosition.y - maskRectTransform.anchoredPosition.y;	

	// 	Gizmos.color = delta > 0.5f * ( creditsRectTransform.sizeDelta.y + maskRectTransform.sizeDelta.y ) ? Color.green : Color.red;
	// 	Gizmos.DrawLine( creditsRectTransform.anchoredPosition.y * Vector3.up , maskRectTransform.anchoredPosition.y * Vector3.up );
	// }

	
}

}
