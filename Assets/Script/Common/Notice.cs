using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using  UnityEngine.EventSystems;
using System.Collections.Generic;

public class Notice : MonoBehaviour,IPointerClickHandler ,IPointerDownHandler,IPointerUpHandler
{
	public GameObject 	bk;
	public Text 		message;
	private bool		isPlaying;

	// Use this for initialization
	void Start ()
	{
		isPlaying = false;
		bk.SetActive (false);
		message.text = "";
		message.transform.localPosition = new Vector3 (320, 0 , 0);

		if(Common.GameNotices.Count > 0){
			bk.SetActive (true);
			Play ();
		}
	}

	// Update is called once per frame
	void Update ()
	{
		
	}

	public void  OnPlay(){
		if (isPlaying) {
			return;
		} else {
			Play ();
		}
	}

	void Play(){
		if (Common.GameNotices.Count > 0) {
			bk.SetActive (true);
			isPlaying = true;
			NoticeMessage not = Common.GameNotices [0];
			message.text = not.text;

			message.transform.localPosition = new Vector3 (320, 0, 0);

			Sequence mySequence = DOTween.Sequence ();
			mySequence.Append (message.transform.DOLocalMoveX (-320 - message.preferredWidth, 8).SetEase (Ease.Linear));
			mySequence.Append (message.transform.DOLocalMoveX (320, 0));
			mySequence.Play ().SetLoops (not.times).onComplete = PlayComplete;
		} else {
			isPlaying = false;
			bk.SetActive (false);
			message.text = "";
		}
	}

	void PlayComplete(){
		if (Common.GameNotices.Count > 0) {
			Common.GameNotices.RemoveAt (0);
			Play ();
		} else {
			isPlaying = false;
			bk.SetActive (false);
			message.text = "";
		}
	}
		
	public void OnPointerDown(PointerEventData eventData)
	{
		PassEvent(eventData,ExecuteEvents.pointerDownHandler);
	}
		
	public void OnPointerUp(PointerEventData eventData)
	{
		PassEvent(eventData,ExecuteEvents.pointerUpHandler);
	}
		
	public void OnPointerClick(PointerEventData eventData)
	{
		PassEvent(eventData,ExecuteEvents.submitHandler);
		PassEvent(eventData,ExecuteEvents.pointerClickHandler);
	}


	public void  PassEvent<T>(PointerEventData data,ExecuteEvents.EventFunction<T> function)
		where T : IEventSystemHandler
	{
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(data, results); 
		GameObject current = data.pointerCurrentRaycast.gameObject ;
		for(int i =0; i< results.Count;i++)
		{
			if(current!= results[i].gameObject)
			{
				ExecuteEvents.Execute(results[i].gameObject, data,function);
				break;
			}
		}
	}
}

