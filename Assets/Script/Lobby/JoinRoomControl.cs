using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class JoinRoomControl : MonoBehaviour {
	public LobbyController	LobbyControl;

	private string 			Roomnumber;

	// Use this for initialization
	void Start () {
	}

	void Awake(){
	}

	// Update is called once per frame
	void Update () {
	}

	public void Enter(){
		gameObject.SetActive (true);
		m_Enter = true;
		Roomnumber = "";
		UpdatePssword ();

		transform.localPosition = new Vector3(640, 0, 0);
		Sequence s = DOTween.Sequence ();
		s.Append (transform.DOLocalMoveX (30, 0.2f));
		s.Append (transform.DOLocalMoveX (0, 0.2f));
		s.Play ();
	}

	public void Exit(){
		LobbyControl.PlayerButtonEffect ();
		if(m_Enter){
			transform.DOLocalMoveX (640, 0.15f).onComplete = OnExit;
			m_Enter = false;
		}
	}

	public void OnExit(){
		gameObject.SetActive (false);
	}

	public void InputPassword(int Pword){
		LobbyControl.PlayerButtonEffect ();

		if (Roomnumber.Length < 4) {
			Roomnumber = Roomnumber + Pword.ToString();
		}

		UpdatePssword ();
	}

	public void DeletePassowrd(){
		if (Roomnumber.Length > 0) {
			Roomnumber = Roomnumber.Substring (0, Roomnumber.Length - 1);
		}

		UpdatePssword ();
	}

	public void UpdatePssword(){
		for(int i = 0; i < 4; i++){
			this.transform.Find ("Password/Password" + i).GetComponent<Text> ().text = "";
		}

		for(int i = 0; i < Roomnumber.Length; i++){
			this.transform.Find ("Password/Password" + i).GetComponent<Text> ().text = Roomnumber.Substring(i, 1);
		}

		if(Roomnumber.Length == 4){
			JoinRoom ();
		}
	}

	public void JoinRoom(){
		if(!string.IsNullOrEmpty(Roomnumber)){
			LobbyControl.JoinRoomServer (Roomnumber, false);
		}
	}

	private bool 	m_Enter;
	private Vector2 lastPos;
	private Vector2 currentPos;


	void OnGUI(){
		if(!m_Enter){return;}

		if (Event.current.type == EventType.MouseDown) {
			lastPos = Event.current.mousePosition;
			currentPos = Event.current.mousePosition;
		}

		if (Event.current.type == EventType.MouseDrag) {
			currentPos = Event.current.mousePosition;
			if( (currentPos.x - lastPos.x) > 20 ){
				transform.localPosition = new Vector3(currentPos.x - lastPos.x - 20, 0, 0);	
			}
		}

		if (Event.current.type == EventType.MouseUp) {
			if(lastPos== currentPos){return;}

			if (transform.localPosition.x < 320) {
				transform.DOLocalMoveX (0, 0.2f);
			} else {
				Exit ();
			}
		}  
	}
}

