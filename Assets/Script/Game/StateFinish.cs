using Google.Protobuf.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateFinish : State {
	private Transform 		Layer 		= 	null;
	private int 			CountDowm	= 0;

	void Start () {
	}

	void Update () {
	}

	void Awake(){
		Layer = GameObject.Find ("Canvas").transform.Find ("FinishLayer");
	}

	public override void Enter(){
		Layer.gameObject.SetActive (true);
		AdjustUI ();
		ShowResultInfo ();

		CountDowm = Common.ConfigFinishTime;
		BeginCountDown ();
	}

	public override void Exit(){
		CancelInvoke ();
		Layer.gameObject.SetActive (false);
	}

	public override void AdjustUI(){
		Transform PreInfoCom = Layer.Find ("PreInfoCom");
		for(int i = 0; i <PreInfoCom.childCount; i++){
			
			Transform PreInfoObj = PreInfoCom.Find ("PreInfo" + i).transform;

			PreInfoObj.Find ("Name").GetComponent<Text> ().text = "";

			for (int o = PreInfoObj.Find ("Avatar").childCount - 1; o >= 0; o--) {  
				Destroy (PreInfoObj.Find ("Avatar").GetChild(o).gameObject);
			}  

			PreInfoObj.Find ("GetLucky").gameObject.SetActive (false);
			PreInfoObj.Find ("Foul").gameObject.SetActive (false);

			for(int o = PreInfoObj.Find ("Amount").childCount - 1; o >= 0; o--){
				Destroy (PreInfoObj.Find ("Amount").GetChild(o).gameObject);
			}

			PreInfoObj.Find ("Mask0").gameObject.SetActive (false);
			PreInfoObj.Find ("Mask1").gameObject.SetActive (false);
			PreInfoObj.Find ("Mask2").gameObject.SetActive (false);

			PreInfoObj.Find ("Type0").GetComponent<Text> ().text = "";
			PreInfoObj.Find ("Type1").GetComponent<Text> ().text = "";
			PreInfoObj.Find ("Type2").GetComponent<Text> ().text = "";
			PreInfoObj.gameObject.SetActive (false);
		}
	}

	public void BeginCountDown(){
		InvokeRepeating("UpdateSortingTime", 0f, 1.0f);
	}

	public void UpdateSortingTime(){
		if (CountDowm >= 0) {
			Layer.Find ("OK/CountDown").GetComponent<Text> ().text = "（" + CountDowm + "）";
		} 
		else {
			CancelInvoke ();

			//for demo
			//Next ();
		}
		CountDowm--;
	}


	//for Demo
	public void Next(){
		m_StateManage.ChangeState (STATE.STATE_SEAT);
	}

	public void ShowResultInfo(){
		Common.CPlayed_hands++;
		Layer.Find ("HandCount").GetComponent<Text>().text = "# " + Common.CPlayed_hands + " / " + Common.CHands + " Hand";

		List<int> Seats = new List<int> ();
		foreach (KeyValuePair<int, SeatResult> pair in m_GameController.SeatResults) {
			Seats.Add (pair.Key);
		}
		Seats.Sort ();

		int index = 0;
		foreach (int seatid in Seats){
			SeatResult hInfo = m_GameController.SeatResults[seatid];

			Transform PreInfoObj = Layer.Find ("PreInfoCom/PreInfo" + index).transform;
			PreInfoObj.gameObject.SetActive (true);

		

			PlayerInfo p = m_GameController.GetPlayerInfoForSeatID (seatid);
			if(p == null){return;}

			UICircle avatar = (UICircle)Instantiate(m_GameController.m_PrefabAvatar);
			avatar.transform.SetParent (PreInfoObj.Find ("Avatar"));
			avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 ();
			avatar.transform.localPosition = new Vector3 ();
			avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (60, 60);

			if (p.FB_avatar == null) {avatar.UseDefAvatar ();}
			else {avatar.SetAvatar (p.FB_avatar.texture);}

			PreInfoObj.Find ("Name").GetComponent<Text> ().text = p.Name;

			for(int i = 0; i < hInfo.Pres.Count; i++){
                RepeatedField<uint> pInfo = hInfo.Pres [i];

				for(int o = 0; o < pInfo.Count; o++){
					Transform Poker = PreInfoObj.Find ("Hand" + i + "/Poker" + o);
					Image image = Poker.GetComponent<Image>();
					image.sprite = Resources.Load("Image/Poker/" + pInfo[o], typeof(Sprite)) as Sprite;
				}
			}

			Transform number = PreInfoObj.Find ("Amount");
			string	typestr = "";

			if (hInfo.Win < 0) {typestr = "lost";}
			else {typestr = "win";}

			string amount = Mathf.Abs (hInfo.Win).ToString ();
			float left = 0;
			for (int c = amount.Length - 1; c >= 0; c--) {  
				GameObject t = new GameObject ();
				t.AddComponent<Image> ();
				t.GetComponent<Image>().sprite = Resources.Load ("Image/Game/"+ typestr + amount[c] , typeof(Sprite)) as Sprite;
				t.transform.SetParent(number);
				t.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (20, 24);
				t.transform.localPosition = new Vector3 (left,0,0);
				left = left - 18;
			}

			float right = number.localPosition.x - 16 * amount.Length;
			if (hInfo.Win >= 0) {
				PreInfoObj.Find ("GetLucky").gameObject.SetActive (true);
				PreInfoObj.Find ("GetLucky").localPosition = new Vector3 ( right, PreInfoObj.Find ("GetLucky").localPosition.y, 0);
			} else {
				PreInfoObj.Find ("Foul").gameObject.SetActive (true);
				PreInfoObj.Find ("Foul").localPosition = new Vector3( right, PreInfoObj.Find ("GetLucky").localPosition.y, 0);
			}
			index++;
		}
	}
}
