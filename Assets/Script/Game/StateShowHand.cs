using System.Collections;
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
		AdjustUI ();
		m_GameController.ShowSeatLayer ();

		foreach (KeyValuePair<int, CSeatResult> pair in Common.CSeatResults) {
			Seats.Add (pair.Key);
		}
		Seats.Sort ();

		SHowPokerBack ();
		ShowHand ();
	}

	public override void DisEnter(){
		AdjustUI ();
		m_GameController.ShowSeatLayer ();

		foreach (KeyValuePair<int, CSeatResult> pair in Common.CSeatResults) {
			Seats.Add (pair.Key);
		}
		Seats.Sort ();

		SHowPokerBack ();
		ShowAll ();
	}

	public override void Exit(){
		ClearShowInfo ();
		Layer.gameObject.SetActive (false);
	}

	public override void AdjustUI(){
		m_GameController.HideTableInfo ();
		m_StateManage.m_StateSeat.HideAutoBanker ();
		Layer.gameObject.SetActive (true);

		GameObject[] objs = GameObject.FindGameObjectsWithTag ("Hand");
		for(int i = 0; i < objs.Length; i++){
			objs[i].GetComponent<Image>().sprite = Resources.Load("Image/Poker/poker_back", typeof(Sprite)) as Sprite;
		}

		ClearShowInfo ();
	}

	public void ClearShowInfo(){
		for(int i = 0; i < 4; i++){
			Transform HandObj = Layer.Find ("SeatCom/Seat" + i);

			for(int n = 0; n < 3; n++){
				HandObj.Find ("ParResult" + n).gameObject.SetActive (false);
			}

			HandObj.Find ("Lost").gameObject.SetActive (false);
			HandObj.Find ("Win").gameObject.SetActive (false);
			HandObj.Find ("Getlucky").gameObject.SetActive (false);

			for (int o = HandObj.Find ("Number").childCount - 1; o >= 0; o--) {  
				Destroy(HandObj.Find ("Number").GetChild(o).gameObject);  
			}

			HandObj.gameObject.SetActive (false);
		}
	}

	public void ShowHand(){
		Invoke("ShowBankerHands", 0.3f);

		for(int i = 0; i < Seats.Count; i++) {
			Invoke("ShowPlayerHand", i + 1.4f);
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
		if(Common.CSeatResults.ContainsKey(SeatID)){
			CSeatResult pinfo = Common.CSeatResults [SeatID];
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
					Debug.Log (pinfo.score.Count);
					if(pinfo.score.Count > 0){
						Debug.Log (pinfo.score [i]);
						if (pinfo.score[i] > 0) {
							typeI.sprite = Resources.Load ("Image/Game/winicon", typeof(Sprite)) as Sprite;
							ResI.sprite = Resources.Load ("Image/Game/win1", typeof(Sprite)) as Sprite;

						} else {
							typeI.sprite = Resources.Load ("Image/Game/losticon", typeof(Sprite)) as Sprite;
							ResI.sprite = Resources.Load ("Image/Game/lost1", typeof(Sprite)) as Sprite;
						}

						ParResult.gameObject.SetActive (true);
					}
				}
			}
				

			m_StateManage.m_StateSeat.UpdateSeatScore (SeatID, pinfo.BWin, pinfo.BWin + pinfo.Win);
			if(SeatID == 0){return;}

			string	typestr = "";

			if (pinfo.autowin) {
				HandObj.Find ("Getlucky").gameObject.SetActive (true);
			} else {
				HandObj.Find ("Getlucky").gameObject.SetActive (false);
			}

			if (pinfo.Win < 0) {
				typestr = "lost";
				HandObj.Find ("Lost").gameObject.SetActive (true);
			} else {
				typestr = "win";
				HandObj.Find ("Win").gameObject.SetActive (true);
			}
				
			string amount = Mathf.Abs (pinfo.Win).ToString ();
			float left = 0;
			for(int c = 0; c < amount.Length; c++){
				GameObject t = new GameObject ();
				t.AddComponent<Image> ();
				t.GetComponent<Image>().sprite = Resources.Load ("Image/Game/"+ typestr + amount[c] , typeof(Sprite)) as Sprite;
				t.transform.SetParent(HandObj.Find ("Number"));
				t.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (20, 24);
				t.transform.localPosition = new Vector3 (left,0,0);
				left = left + 18;
			}
		}
	}
}
  