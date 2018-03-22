﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateShowHand : State {
	private Transform Layer = null;
	private List<int> Seats	= new List<int> ();
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Awake(){
		Layer = GameObject.Find("Canvas").transform.Find("ShowHandLayer");
	}

	public override void Enter(){
		Debug.Log ("==============================state show hand===================================");
		m_GameController.ShowSeatLayer ();
		AdjustUI ();


		foreach (KeyValuePair<int, SeatResult> pair in m_GameController.SeatResults) {
			Seats.Add (pair.Key);
		}
		Seats.Sort ();

		SHowPokerBack ();
		ShowHand ();
	}

	public override void DisEnter(){
		m_GameController.ShowSeatLayer ();
		AdjustUI ();


		foreach (KeyValuePair<int, SeatResult> pair in m_GameController.SeatResults) {
			Seats.Add (pair.Key);
		}
		Seats.Sort ();

		SHowPokerBack ();
		ShowAll ();
	}

	public override void Exit(){
		Layer.gameObject.SetActive (false);
	}

	public override void AdjustUI(){
		m_StateManage.m_StateSeat.HideAutoBanker ();
		Layer.gameObject.SetActive (true);

		GameObject[] objs = GameObject.FindGameObjectsWithTag ("Hand");
		for(int i = 0; i < objs.Length; i++){
			objs[i].GetComponent<Image>().sprite = Resources.Load("Image/Poker/poker_back", typeof(Sprite)) as Sprite;
		}

		for(int i = 0; i < Layer.Find ("SeatCom").childCount; i++){
			Transform HandObj = Layer.Find ("SeatCom/Seat" + i);
			for(int o = 0; o < 3; o++){
				HandObj.Find ("ParResult" + o).gameObject.SetActive (false);
			}
			HandObj.Find ("Lost").gameObject.SetActive (false);
			HandObj.Find ("Win").gameObject.SetActive (false);
			HandObj.Find ("Getlucky").gameObject.SetActive (false);
		}
	}

	public void ShowHand(){
		Invoke("ShowBankerHands", 0.6f);

		for(int i = 0; i < Seats.Count; i++) {
			Invoke("ShowPlayerHand", i + 1);
		}
	}

	public void ShowAll(){
		for(int i = 0; i < Seats.Count; i++) {
			ShowPokerFace (Seats[i]);
		}
	}

	public void ShowBankerHands(){
		ShowPokerFace (0);
		Seats.RemoveAt (0);
	}

	public void ShowPlayerHand(){
		if(Seats.Count > 0){
			int seat = Seats [0];
			Seats.RemoveAt (0);
			ShowPokerFace (seat);
		}
	}

	public override void UpdateSeatOrder(){
		for(int seatid = 0; seatid < Common.ConfigSeatOrder.Count; seatid++){
			Transform SeatObject = Layer.Find ("SeatCom/Seat" + Common.ConfigSeatOrder[seatid]);
			SeatObject.transform.localPosition = m_GameController.DefaultShowHandPisitions [seatid];
		}
	}

	public void SHowPokerBack(){
		foreach (int seat in Seats){
			Transform HandObj = Layer.Find ("SeatCom/Seat" + seat);
			HandObj.gameObject.SetActive (true);
		}
	}

	public void ShowPokerFace(int SeatID){
		if(m_GameController.SeatResults.ContainsKey(SeatID)){
			SeatResult pinfo = m_GameController.SeatResults [SeatID];
			Transform HandObj = Layer.Find ("SeatCom/Seat" + SeatID);

			for (int i = 0; i <  3; i++) {
				Transform Par 		= HandObj.Find ("Par" + i);

				for (int  o = 0; o <  Par.childCount; o++) {
					GameObject Poker = Par.GetChild (o).gameObject;
					Image image = Poker.GetComponent<Image>();
					image.sprite = Resources.Load("Image/Poker/" + pinfo.Pres[i][o], typeof(Sprite)) as Sprite;
				}

				if(SeatID == 0){continue;}

				Transform ParResult = HandObj.Find ("ParResult" + i);
				Image typeI = ParResult.Find("Type").GetComponent<Image> ();
				Image ResI = ParResult.Find("Res").GetComponent<Image> ();

				if(!pinfo.foul){
					if(pinfo.score.Count > 0){
						if (pinfo.score[i] > 0) {
							typeI.sprite = Resources.Load ("Image/Game/winicon", typeof(Sprite)) as Sprite;
							ResI.sprite = Resources.Load ("Image/Game/win1", typeof(Sprite)) as Sprite;
						} else {
							typeI.sprite = Resources.Load ("Image/Game/losticon", typeof(Sprite)) as Sprite;
							ResI.sprite = Resources.Load ("Image/Game/lost1", typeof(Sprite)) as Sprite;
						}
					}
				}

				ParResult.gameObject.SetActive (true);
			}
				
			if(SeatID == 0){return;}
			Transform number = null;
			string	typestr = "";

			if (pinfo.autowin) {
				HandObj.Find ("Getlucky").gameObject.SetActive (true);
			} else {
				HandObj.Find ("Getlucky").gameObject.SetActive (false);
			}

			if (pinfo.Win < 0) {
				typestr = "lost";
				HandObj.Find ("Lost").gameObject.SetActive (true);
				number = HandObj.Find ("Lost/Number");
			} else {
				typestr = "win";
				HandObj.Find ("Win").gameObject.SetActive (true);
				number = HandObj.Find ("Win/Number");
			}

			string amount = Mathf.Abs (pinfo.Win).ToString ();
			float left = 0;
			for(int c = 0; c < amount.Length; c++){
				GameObject t = new GameObject ();
				t.AddComponent<Image> ();
				t.GetComponent<Image>().sprite = Resources.Load ("Image/Game/"+ typestr + amount[c] , typeof(Sprite)) as Sprite;
				t.transform.SetParent(number);
				t.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (20, 24);
				t.transform.localPosition = new Vector3 (left,0,0);
				left = left + 18;
			}
		}
	}
}
  