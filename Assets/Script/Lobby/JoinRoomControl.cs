using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
		Roomnumber = "";
		UpdatePssword ();
		this.gameObject.SetActive (true);
	}

	public void Exit(){
		this.gameObject.SetActive (false);
	}

	public void InputPassword(int Pword){
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
			LobbyControl.JoinRoomServer (Roomnumber);
		}
	}
}

