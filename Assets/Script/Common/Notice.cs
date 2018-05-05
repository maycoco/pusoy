using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class Notice : MonoBehaviour
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
}

