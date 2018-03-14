using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Facebook.Unity;

using Google.Protobuf;
using networkengine;
using Msg;
using System.IO;
using Google.Protobuf.Collections;

public class PreInfo{
	public List<int> Hand = new List<int>();
	public int Result;
	public int Type;
}

public class SeatResult{
	public int SeatID;
	public RepeatedField<RepeatedField<uint>> Pres 	= new RepeatedField<RepeatedField<uint>>();
	public List<int> score		 	= new List<int> ();
	public RepeatedField<global::Msg.CardRank> Ranks = new RepeatedField<global::Msg.CardRank>();
	public bool autowin;
	public int Bet;
	public int Win;
}
	
public class GameController : MonoBehaviour {
	private Transform									Canvas;

	//State Manage
	public StateManage									m_StateManage;

	//Prefab
	public GameObject			   						m_PrefabChip;
	public GameObject			   						m_PrefabRank;
	public GameObject			   						m_PrefabTPlayer;
	public UICircle			   							m_PrefabAvatar;
	public Poker			   							m_PrefabPoker;

	//effect
	public GameObject									m_PrefabEffectSL;

	//Self Info
	[HideInInspector] public int 						m_SelfSeatID 	= -1;
	[HideInInspector] public int 						m_TatgetSeatID	= -1;

	//Players Info
	[HideInInspector]   public Dictionary<int,  SeatResult> SeatResults 	= new Dictionary<int, SeatResult>();

	//Chips Info
	[HideInInspector] public  int 				    	m_MaxChips    	= 0;
	[HideInInspector] public  List<int> 				m_ChipsType		= new List<int>();

	//Pos Config
	[HideInInspector] public List<Vector3> 				DefaultSeatPositions 			= new List<Vector3>();
	[HideInInspector] public List<Vector3> 				DefaultBetPositions 			= new List<Vector3>();
	[HideInInspector] public List<Vector3> 				DefaultShowHandPisitions 		= new List<Vector3>();
	[HideInInspector] public List<Vector3> 				DefaultHandPisitions			= new List<Vector3>();		

	// Use this for initialization
	void Start () {
	}

	void Awake(){
		Canvas = GameObject.Find ("Canvas").transform;
		AdjustUI ();

		InitCallbackForNet ();
		InstanceConfig ();
		initializeRoomData ();
		m_StateManage.initialize(this);


		//
		if(Common.CState == Msg.GameState.Ready){
			m_StateManage.SetState (STATE.STATE_SEAT);
		}

		if(Common.CState == Msg.GameState.Bet){
			m_StateManage.SetState (STATE.STATE_SEAT);
			m_StateManage.ChangeState (STATE.STATE_BETTING);
		}

		if(Common.CState == Msg.GameState.ConfirmBet){
			m_StateManage.SetState (STATE.STATE_SEAT);
			m_StateManage.ChangeState (STATE.STATE_BETTING);
			m_StateManage.m_StateBetting.ShowConfimBet();
		}

		if(Common.CState == Msg.GameState.Deal){
			m_StateManage.SetState (STATE.STATE_SEAT);
			//ResultEvent(data.gameStateNotify.Results);
		}

//		if(Common.CState == Msg.GameState.Deal){
//			m_StateManage.SetState (STATE.STATE_SEAT);
//		}

		if(Common.CState == Msg.GameState.Combine){
			m_StateManage.SetState (STATE.STATE_SEAT);
			m_StateManage.ChangeState (STATE.STATE_SORTING);
		}

		if(Common.CState == Msg.GameState.Show){
			m_StateManage.SetState (STATE.STATE_SEAT);
			m_StateManage.SetState (STATE.STATE_SHOWHAND);
		}

		if(Common.CState == Msg.GameState.Result){
			m_StateManage.SetState (STATE.STATE_SEAT);
			m_StateManage.ChangeState (STATE.STATE_FINISH);
		}
	}

	// Update is called once per frame
	void Update () {
	}

	void AdjustUI(){
		UpdateRooimInfo ();
	}

	public void initializeRoomData(){
//		Common.FB_id = "123235234";
//		Common.FB_access_token = "sd23edasdasdasdasd12easd";
//		Common.FB_name = "Walter wang";
//		Common.Uid = 222;
//		Common.CMin_bet 	= 20;
//		Common.CMax_bet 	= 1000;
//		Common.CHands 		= 300;
//		Common.CPlayed_hands= 10;
//		Common.CIs_share 	= true;
//		Common.CCredit_points = 1000;
//		Common.CState 		= Msg.GameState.Combine;
//		if(Common.CState == Msg.GameState.Show){
//			Common.ConfigBetTime = 5;
//		}
//		else if(Common.CState == Msg.GameState.Combine){
//			Common.ConfigSortTime = 1000;
//		}
//		else if(Common.CState == Msg.GameState.Result){
//			Common.ConfigFinishTime = 10;
//		}
//
//		PlayerInfo p2 = new PlayerInfo ();
//		p2.Uid = 111;
//		p2.SeatID = 2;
//		p2.Name = "Bruce";
//		p2.Win = 123456789;
//		p2.Bet = 100;
//		Common.CPlayers.Add (p2);
//
//		PlayerInfo p1 = new PlayerInfo ();
//		p1.Uid = 222;
//		p1.SeatID = 1;
//		p1.Name = "Ali";
//		p1.Win = -123456789;
//		Common.CPlayers.Add (p1);
//
//		PlayerInfo p3 = new PlayerInfo ();
//		p3.Uid = 777;
//		p3.SeatID = 0;
//		p3.Name = "HAHA";
//		p3.Win = -76456789;
//		Common.CPlayers.Add (p3);
//
//		uint[] ps =  new uint[]{1,13,3,36,5,6,24,8,18,10,38,12,27};
//		DealCardsEvent (ps);
//		ResultEvent (null);

		// Already Sit Down
		if (GetSeatIDForPlayerID (Common.Uid) == 999) {
			PlayerInfo p = new PlayerInfo ();
			p.Uid = Common.Uid;
			p.Name = Common.FB_name;
			p.SeatID = -1;
			m_SelfSeatID = -1;
			Common.CPlayers.Add (p);
		} else {
			m_SelfSeatID = GetSeatIDForPlayerID (Common.Uid);
		}

		// Chips Type
		if(Common.CMin_bet > 0){
			for(int i = 0; i < Common.ConfigChips.Length; i++){
				m_ChipsType.Add ( ((int)Common.CMin_bet * Common.ConfigChips[i]) );
			}
			m_MaxChips = (int)Common.CMin_bet * Common.ConfigMaxChips;
		}
	}

	public void SetSeatID(uint Uid, int SeatID){
		for(int i = 0; i < Common.CPlayers.Count; i++){
			if(Common.CPlayers[i].Uid == Uid){
				Common.CPlayers[i].SeatID = SeatID;

				if(Uid == Common.Uid){
					m_SelfSeatID = SeatID;
				}
			}
		}
	}

	public uint GetPlayerIDForSeatID(int SeatID){
		foreach(PlayerInfo p in Common.CPlayers){
			if(p.SeatID == SeatID){
				return p.Uid;
			}
		}
		return 0;
	}
		
	public int GetSeatIDForPlayerID(uint PlayerID){
		foreach(PlayerInfo p in Common.CPlayers){
			if(p.Uid == PlayerID){
				return p.SeatID;
			}
		}
		return 999;
	}

	public PlayerInfo GetPlayerInfoForSeatID(int SeatID){
		foreach(PlayerInfo p in Common.CPlayers){
			if(p.SeatID == SeatID){
				return p;
			}
		}

		return null;
	}

	public int GetTablePlayersCount(){
		int c = 0;
		foreach(PlayerInfo p in Common.CPlayers){
			if(p.SeatID != -1){
				c++;
			}
		}
		return c;
	}

	public void HideSeatLayer(){
		Canvas.Find ("SeatLayer").gameObject.SetActive (false);
	}

	 public void ShowSeatLayer(){
		Canvas.Find ("SeatLayer").gameObject.SetActive (true);
	}

	public void HideTableInfo(){
		Canvas.Find ("TableInfo").gameObject.SetActive (false);
	}

	public void ShowTableInfo(){
		Canvas.Find ("TableInfo").gameObject.SetActive (true);
	}

	public void HideGameConsole(){
		Canvas.Find ("GameConsole").gameObject.SetActive (false);
	}

	public void ShowGameConsole(){
		Canvas.Find ("GameConsole").gameObject.SetActive (true);
	}

	public void InstanceConfig(){
		for(int i = 0; i < 4; i++){
			Vector3 pos = Canvas.Find ("SeatLayer/SeatCom/Seat" + i).localPosition;
			DefaultSeatPositions.Add(pos);
		}

		for(int i = 0; i < 4; i++){
			Vector3 pos = Canvas.Find ("BettingLayer/SeatCom/Seat" + i).localPosition;
			DefaultBetPositions.Add(pos);
		}

		for(int i = 0; i < 4; i++){
			Vector3 pos = Canvas.Find ("ShowHandLayer/SeatCom/Seat" + i).localPosition;
			DefaultShowHandPisitions.Add(pos);
		}
			
		for(int i = 0; i < 13; i++){
			Vector3 pos = Canvas.Find ("SortingLayer/SortingLayer01/Pokers/Poker" + i).localPosition;
			DefaultHandPisitions.Add(pos);
			Destroy (Canvas.Find ("SortingLayer/SortingLayer01/Pokers/Poker" + i).gameObject);
		}
	}

	public void UpdateOrderList(){
		Common.ConfigSeatOrder.Clear ();

		if (m_SelfSeatID != -1) {
			for (int seatid = m_SelfSeatID; seatid < 4; seatid++) {
				Common.ConfigSeatOrder.Add (seatid);
			}

			for (int seatid = 0; seatid < m_SelfSeatID; seatid++) {
				Common.ConfigSeatOrder.Add (seatid);
			}
		} 
		else {
			for(int seatid = 0; seatid < 4; seatid++){
				Common.ConfigSeatOrder.Add (seatid);
			}
		}

		m_StateManage.GetStateInstance (STATE.STATE_SEAT).UpdateSeatOrder ();
		m_StateManage.GetStateInstance (STATE.STATE_BETTING).UpdateSeatOrder ();
		m_StateManage.GetStateInstance (STATE.STATE_SHOWHAND).UpdateSeatOrder ();
	}

	public void StartGame(){
		StartGameServer();
	}

	public void ExitGame(){
		SceneManager.LoadScene (1);
	}

	public void UpdateRooimInfo(){
		Canvas.Find ("TableInfo/RoomNumber").GetComponent<Text> ().text = "Password : " + Common.CRoom_number;
		Canvas.Find ("TableInfo/BetSize").GetComponent<Text> ().text = "Bet Size  " + Common.CMin_bet.ToString () + "-" + Common.CMax_bet.ToString ();
		Canvas.Find ("TableInfo/Share").GetComponent<Text> ().text = "Room fee shared";
		Canvas.Find ("TableInfo/Round").GetComponent<Text> ().text = (Common.CPlayed_hands + 1).ToString () + "/" + Common.CHands.ToString ();
	}

	//===================================Event=================================
	public void DealCardsEvent(RepeatedField<uint> poks){
		Common.CPokers.Clear ();
		for(int i = 0; i < poks.Count; i++){
			Common.CPokers.Add ((int)poks[i]);
		}
	}

	public void ResultEvent(RepeatedField<global::Msg.SeatResult> ResultList){
		SeatResults.Clear ();	

		for(int i = 0; i < ResultList.Count; i++){
			
			SeatResult info 	= new SeatResult ();
			info.SeatID 		= (int)ResultList [i].SeatId;

			if((int)ResultList [i].SeatId != 0){
				info.score 			= new List<int> (ResultList [i].Scores);
			}

			info.autowin 		= ResultList [i].Autowin;
			//info.Bet 			= (int)ResultList [i].Bet;
			info.Win 			= ResultList [i].Win;
			info.Ranks 			= ResultList [i].Ranks;

			for(int o = 0; o < ResultList [i].CardGroups.Count; o++){
				Msg.CardGroup cg = ResultList [i].CardGroups[o];
				info.Pres.Add (cg.Cards);
			}
			SeatResults.Add (info.SeatID, info);
		}



//		uint[] arr = { 1, 2, 3 };
//		uint[] arr1 = { 4, 5, 6, 7, 8 };
//		uint[] arr2 = { 9, 10, 11, 12, 13 };
//
//		SeatResult hinfo = new SeatResult ();
//		hinfo.Bet = 2000;
//		hinfo.Win = -150;
//		hinfo.SeatID = 0;
//		hinfo.Pres.Add (arr);
//		hinfo.Pres.Add (arr1);
//		hinfo.Pres.Add (arr2);
//
//		hinfo.score.Add (1);
//		hinfo.score.Add (-1);
//		hinfo.score.Add (-1);
//
//
//		SeatResults.Add (0, hinfo);
//		SeatResults.Add (1, hinfo);
//		SeatResults.Add (2, hinfo);
	}

	public void LeaveRoomEvent(uint uid){
		if (uid == Common.Uid) {
			ExitGame ();
		} else {
			for(int i = 0; i < Common.CPlayers.Count; i++){
				if(Common.CPlayers[i].Uid == uid){
					Common.CPlayers.RemoveAt (i);
					UpdateOrderList ();
				}
			}
		}
	}

	//===================================connect=================================
	public void InitCallbackForNet(){
		protonet.SetConnectedCall(Connected);
		protonet.SetFailConnectedCall(FailedConnect);
		protonet.SetDisConnectedCall(DisConnect);
		protonet.SetDataCall(Data);
	}
		
	public void Connected(){
	}

	public void FailedConnect(){
	}

	public void DisConnect(){
		Debug.Log ("DisConnect");
	}

	public void Data(Protocol data){
		Debug.Log (data.ToString ());
		Debug.Log (data.Msgid.ToString());

		if(data == null){
			return; 
		}

		switch (data.Msgid) {

		case MessageID.LeaveRoomRsp:
			if(data.LeaveRoomRsp.Ret == 0){
				Loom.QueueOnMainThread(()=>{
					ExitGame();
				}); 
			}
			break;

		case MessageID.SitDownRsp:
			if (data.SitDownRsp.Ret == 0) {
				Loom.QueueOnMainThread(()=>{
					SetSeatID (Common.Uid, m_TatgetSeatID);
					UpdateOrderList ();
				}); 
			}
			break;

		case MessageID.StandUpRsp:
			if (data.StandUpRsp.Ret == 0) {
				Loom.QueueOnMainThread(()=>{
					SetSeatID (Common.Uid, -1);
					UpdateOrderList ();
				}); 
			}
			break;

		case MessageID.StartGameRsp:
			if (data.StartGameRsp.Ret == 0) {
			}
			break;

		case MessageID.BetRsp:
			if (data.BetRsp.Ret == 0) {
				Loom.QueueOnMainThread(()=>{
					m_StateManage.m_StateBetting.DCanBetCall();
				}); 
			}
			break;

		case MessageID.CombineRsp:
			if (data.CombineRsp.Ret == 0) {
			}
			break;

		case MessageID.GameStateNotify:
			Debug.Log (data.GameStateNotify.State.ToString());
			switch(data.GameStateNotify.State){
	
			case Msg.GameState.Ready:
				Loom.QueueOnMainThread(()=>{
					m_StateManage.ChangeState (STATE.STATE_SEAT);
				}); 
				break;

			case Msg.GameState.Bet:
				Debug.Log (data.GameStateNotify.Countdown);
				Loom.QueueOnMainThread(()=>{
					Common.ConfigBetTime = (int)data.GameStateNotify.Countdown / 1000;
					m_StateManage.ChangeState (STATE.STATE_BETTING);
				}); 
				break;

			case Msg.GameState.ConfirmBet:
				Loom.QueueOnMainThread(()=>{
					m_StateManage.m_StateBetting.ShowConfimBet();
				}); 
				break;

			case Msg.GameState.Deal:
				Loom.QueueOnMainThread(()=>{
					DealCardsEvent(data.GameStateNotify.DealCards);
				}); 
				break;

			case Msg.GameState.Combine:
				Loom.QueueOnMainThread(()=>{
					Common.ConfigSortTime = (int)data.GameStateNotify.Countdown / 1000;
					m_StateManage.ChangeState (STATE.STATE_SORTING);
				}); 
				break;

			case Msg.GameState.Show:
				Loom.QueueOnMainThread(()=>{
					Debug.Log(data.GameStateNotify.Result.Count);
                    
					ResultEvent(data.GameStateNotify.Result);
					m_StateManage.ChangeState (STATE.STATE_SHOWHAND);
				}); 
				break;

			case Msg.GameState.Result:
				Loom.QueueOnMainThread(()=>{
					Common.ConfigFinishTime = (int)data.GameStateNotify.Countdown / 1000;
					m_StateManage.ChangeState (STATE.STATE_FINISH);
				}); 
				break;
			}
			break;

		case MessageID.SitDownNotify:
			if(data.SitDownNotify.Type == Msg.SitDownType.Sit){
				Debug.Log (data.SitDownNotify.Uid);
				Debug.Log (data.SitDownNotify.SeatId);
				Loom.QueueOnMainThread(()=>{
					SetSeatID (data.SitDownNotify.Uid, (int)data.SitDownNotify.SeatId);
					UpdateOrderList ();
				}); 
			}
			break;

		case MessageID.StandUpNotify:
			Loom.QueueOnMainThread(()=>{
				SetSeatID (data.StandUpNotify.Uid, -1);
				UpdateOrderList ();
			}); 
			break;

		case MessageID.JoinRoomNotify:
			Loom.QueueOnMainThread(()=>{
				if (GetSeatIDForPlayerID (data.JoinRoomNotify.Uid) == 999) {
					PlayerInfo p = new PlayerInfo ();
					p.Uid 		= data.JoinRoomNotify.Uid;
					p.Name 		= data.JoinRoomNotify.Name;
					p.SeatID 	= -1;
					Common.CPlayers.Add (p);
				} 
			}); 
			break;

		case MessageID.LeaveRoomNotify:
			Loom.QueueOnMainThread (() => {
				LeaveRoomEvent (data.LeaveRoomNotify.Uid);
			}); 
			break;

		case MessageID.BetNotify:
			Loom.QueueOnMainThread (() => {
				m_StateManage.m_StateBetting.UpdateChipsUI ((int)data.BetNotify.Chips, (int)data.BetNotify.SeatId);
			}); 
			break;
		}
	}

	public void LeaveRoomServer(){
		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.LeaveRoomReq;
		msg.LeaveRoomReq 				= new LeaveRoomReq();

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void SitDownServer(int seatID){
		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.SitDownReq;
		msg.SitDownReq 					= new SitDownReq();
		msg.SitDownReq.SeatId 			= (uint)seatID;

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void StandUpServer(){
		if(m_SelfSeatID == -1){return;}
		if(m_StateManage.GetCulState() == STATE.STATE_SEAT || m_StateManage.GetCulState() == STATE.STATE_BETTING){
			
			//for demo
//			SetSeatID (Common.Uid, -1);
//			UpdateOrderList ();
//			return;



			Protocol msg 					= new Protocol();
			msg.Msgid 						= MessageID.StandUpReq;
			msg.StandUpReq 					= new StandUpReq();

			using (var stream = new MemoryStream())
			{
				msg.WriteTo(stream);
				Client.Instance.Send(stream.ToArray());
			}
		}
	}

	public void StartGameServer(){
		//for demo
		//m_StateManage.ChangeState(STATE.STATE_BETTING);
		//return;

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.StartGameReq;
		msg.StartGameReq 				= new StartGameReq();

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void BetServer(uint chips){
		m_StateManage.m_StateBetting.ClearChipsButton ();

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.BetReq;
		msg.BetReq 						= new BetReq();
		msg.BetReq.Chips 				= chips;

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void CombineServer(List<Msg.CardGroup> cards, bool autowin){
		m_StateManage.m_StateSorting.HideConfim ();

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.CombineReq;
		msg.CombineReq 					= new CombineReq();
		msg.CombineReq.Autowin 			= autowin;
		msg.CombineReq.CardGroups.AddRange (cards);
	
		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}
}
