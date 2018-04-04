using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StateSeat : State{
	private Transform 		Layer = null;
	private bool			Initialized = false;
	private List<UICircle> 	Avatars = new List<UICircle>();

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

		if(!Initialized){
			AdjustUI ();
			Initialized = true;
		}

		m_GameController.UpdateRooimInfo ();
		m_GameController.ShowTableInfo ();
		m_GameController.ShowGameConsole ();
		m_GameController.UpdateOrderList();
	}

	public override void Exit(){
		Layer.Find("LetPlay").gameObject.SetActive(false);
		Layer.Find ("TipsPick").gameObject.SetActive (false);
		Layer.Find("TipsWaitStart").gameObject.SetActive (false);
		Layer.Find("TipsNoPlayers").gameObject.SetActive (false);
	}
		
	public override void AdjustUI(){
		for(int i = 0; i < Layer.Find("SeatCom").childCount; i++){
			UICircle avatar = (UICircle)Instantiate(m_GameController.m_PrefabAvatar);
			avatar.transform.SetParent (Layer.Find ("SeatCom/Seat" + i + "/Avatar"));
			avatar.transform.localPosition = new Vector3 ();
			avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (90, 90);
			Avatars.Add (avatar);
		}
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

	public void UpdatePlayerUI(){
		Layer.Find ("TipsNoPlayers").gameObject.SetActive (false);
		Layer.Find ("LetPlay").gameObject.SetActive (false);

		//LetPlay & AutoBanker
		if (m_GameController.m_SelfSeatID == 0) {
			
			if(m_StateManage.GetCulState() == STATE.STATE_SEAT){
				ShowAutoBanker ();
				UpdateAutoBanker ();

				if (m_GameController.GetTablePlayersCount () <= 1) {
					Layer.Find ("LetPlay").gameObject.SetActive (false);
					Layer.Find ("TipsNoPlayers").gameObject.SetActive (true);
				} else {
					Layer.Find ("LetPlay").gameObject.SetActive (true);
				}
			}

		} else {
			Layer.Find ("LetPlay").gameObject.SetActive (false);
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
			Transform SeatObj = Layer.Find ("SeatCom").GetChild (i);
			SeatObj.Find ("AddSeat").gameObject.SetActive (true);

			SeatObj.Find ("Name").gameObject.SetActive (false);
			SeatObj.Find ("Name").GetComponent<Text> ().text = "";

			Avatars [i].UseDefAvatar ();
			SeatObj.Find ("Avatar").gameObject.SetActive (false);
			SeatObj.Find ("Border").gameObject.SetActive (false);
		}

		foreach(PlayerInfo p in Common.CPlayers){
			if(p.SeatID != -1){
				Transform SeatObject = Layer.Find ("SeatCom/Seat" + p.SeatID);
				SeatObject.Find ("AddSeat").gameObject.SetActive (false);
				SeatObject.Find ("Name").gameObject.SetActive (true);
				SeatObject.Find ("Border").gameObject.SetActive (true);
				SeatObject.Find ("Name").GetComponent<Text> ().text = p.Name;

				if(!string.IsNullOrEmpty(p.FB_avatar)){ StartCoroutine(Common.Load(Avatars [p.SeatID], p.FB_avatar));  }
				else{Avatars [p.SeatID].UseDefAvatar ();}

				SeatObject.Find ("Avatar").gameObject.SetActive (true);
			}
		}
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
