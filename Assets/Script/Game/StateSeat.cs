using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class StateSeat : State{
	private Transform 		Layer = null;

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frames
	void Update () {
	}

	void Awake(){
		Layer = GameObject.Find("Canvas").transform.Find("SeatLayer");
	}

	public override void Enter(){
		Debug.Log ("==============================state seat===================================");
		m_GameController.UpdateRooimInfo ();
		m_GameController.ShowTableInfo ();
		m_GameController.ShowGameConsole ();
		m_GameController.UpdateOrderList();
	}

	public void ShowUI(){
		m_GameController.UpdateRooimInfo ();
		m_GameController.ShowTableInfo ();
		m_GameController.ShowGameConsole ();
	}

	public override void Exit(){
		Layer.Find ("TipsPick").gameObject.SetActive (false);
		Layer.Find("TipsWaitStart").gameObject.SetActive (false);
		Layer.Find("TipsNoPlayers").gameObject.SetActive (false);
	}
		
	public void AddSeat(int SeatID){
		m_GameController.PlayEffect (Effect.BUTTON);

		if(m_GameController.m_SelfSeatID >= 0){return;}
		if(m_StateManage.GetCulState() == STATE.STATE_SEAT || m_StateManage.GetCulState() == STATE.STATE_BETTING){
			if(m_GameController.GetPlayerIDForSeatID(SeatID) != 0){return;}
			m_GameController.m_TatgetSeatID = SeatID;
			m_GameController.SitDownServer (SeatID);
		}
	}

	public override void UpdateSeatOrder(){
		for(int seatid = 0; seatid < Common.ConfigSeatOrder.Count; seatid++){
			Transform SeatObject = Layer.Find ("SeatCom/Seat" + Common.ConfigSeatOrder[seatid]);
			SeatObject.transform.localPosition = m_GameController.DefaultSeatPositions [seatid];
		}
		UpdatePlayerUI ();
	}

	public void HideTipsWaitStart(){
		Layer.Find ("TipsWaitStart").gameObject.SetActive (false);
	}

	public void UpdatePlayerUI(){
		Layer.Find ("TipsNoPlayers").gameObject.SetActive (false);

		//LetPlay & AutoBanker
		if (m_GameController.m_SelfSeatID == 0 && m_StateManage.GetCulState() == STATE.STATE_SEAT) {
			ShowAutoBanker ();
			UpdateAutoBanker ();

			if (m_GameController.GetTablePlayersCount () <= 1) {
				Layer.Find ("TipsNoPlayers").gameObject.SetActive (true);
			}
		} else {
			HideAutoBanker ();
		}
			
		if (m_GameController.m_SelfSeatID > 0 && m_StateManage.GetCulState() == STATE.STATE_SEAT) {
			Layer.Find ("TipsWaitStart").gameObject.SetActive (true);
		} else {
			Layer.Find ("TipsWaitStart").gameObject.SetActive (false);
		}

		//Tips
		if (m_GameController.m_SelfSeatID == -1) {
			Layer.Find ("TipsPick").gameObject.SetActive (true);
		} else {
			Layer.Find ("TipsPick").gameObject.SetActive (false);
		}

		//HandUI
		for(int i = 0; i < Layer.Find ("SeatCom").childCount; i++){
			Debug.Log ("Seat=========="+i);

			Transform SeatObj = Layer.Find ("SeatCom").GetChild (i);
			PlayerInfo player = m_GameController.GetPlayerInfoForSeatID (i);
			if (player != null) {
				SeatObj.Find ("AddSeat").gameObject.SetActive (false);

				SeatObj.Find ("Name").GetComponent<Text> ().text = player.Name;
				SeatObj.Find ("Name").gameObject.SetActive (true);

				SeatObj.Find ("Border").gameObject.SetActive (true);
				SeatObj.Find ("Avatar").gameObject.SetActive (true);

				SeatObj.Find ("Tips").gameObject.SetActive (false);

				SeatObj.Find ("Amount").GetComponent<Text> ().text = Common.ToCarryNum (player.Score);
				SeatObj.Find ("Amount").gameObject.SetActive (true);

				UICircle avatar = (UICircle)Instantiate(m_GameController.m_PrefabAvatar);
				avatar.transform.SetParent (SeatObj.Find("Avatar"));
				avatar.transform.localPosition = new Vector3 ();
				avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (90, 90);

				if(!string.IsNullOrEmpty(player.FB_avatar)){ 
					StartCoroutine(Common.Load(avatar, player.FB_avatar));  }
				else{
					avatar.UseDefAvatar ();
				}

			} else {
				SeatObj.Find ("Amount").GetComponent<Text> ().text = "";
				SeatObj.Find ("Amount").gameObject.SetActive (false);

				SeatObj.Find ("Name").GetComponent<Text> ().text = "";
				SeatObj.Find ("Name").gameObject.SetActive (false);

				SeatObj.Find ("Border").gameObject.SetActive (false);

				for (int c = SeatObj.Find ("Avatar").childCount - 1; c >= 0; c--) {  
					Destroy(SeatObj.Find ("Avatar").GetChild(c).gameObject);
				} 

				if (m_GameController.m_SelfSeatID >= 0) {
					SeatObj.Find ("Tips").gameObject.SetActive (false);
					SeatObj.Find ("AddSeat").gameObject.SetActive (false);
				} else {
					SeatObj.Find ("Tips").gameObject.SetActive (true);
					SeatObj.Find ("AddSeat").gameObject.SetActive (true);
				}
			}
		}
	}

	public void UpdateSeatScore(int seat, int BScore, int Score){
		int temp = BScore;
		Tween t = DOTween.To (() => temp, x => temp = x, Score, 1.4f);
		t.OnUpdate (()=>updateScore(seat, temp));
	}

	public void UpdateAllSeatScore(){
		for (int i = 0; i < Layer.Find ("SeatCom").childCount; i++) {
			Transform SeatObj = Layer.Find ("SeatCom").GetChild (i);

			PlayerInfo player = m_GameController.GetPlayerInfoForSeatID (i);
			if (player != null) {
				SeatObj.Find ("Amount").GetComponent<Text> ().text = Common.ToCarryNum (player.Score);
			}
		}
	}

	public void updateScore(int seat, int num){
		Layer.Find ("SeatCom/Seat" + seat).Find ("Amount").GetComponent<Text> ().text = Common.ToCarryNum (num);
	}

	public void UpdateAutoBanker(){
		bool ison = Common.CAutoBanker ? true : false;
		Layer.Find ("AutoBanker").GetComponent<Toggle> ().isOn = ison;
	}

	public void OnClickAutoBank(){
		if(Layer.Find ("AutoBanker").GetComponent<Toggle> ().isOn != Common.CAutoBanker){
			Common.CAutoBanker = Layer.Find ("AutoBanker").GetComponent<Toggle> ().isOn;
			m_GameController.AutoBankServer (Common.CAutoBanker);
		}
	}

	public void HideAutoBanker(){
		Layer.Find("AutoBanker").gameObject.SetActive (false);
	}

	public void ShowAutoBanker(){
		Layer.Find("AutoBanker").gameObject.SetActive (true);
	}
}
