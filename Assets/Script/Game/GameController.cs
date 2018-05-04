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

public enum Effect:int{
	BEGIN,
	BET,
	BOLI,
	BUTTON,
	CLOCK,
	EIDA,
	FAN,
	SEKECT,
	TUOZHUAI
}

public class GameController : MonoBehaviour {
	[HideInInspector] public  Transform									Canvas;
	public  GameConsole									m_GameConsole;
	public 	GameObject 									m_Letplay;

	//State Manage
	public StateManage									m_StateManage;

	//Prefab
	public GameObject									PrefabDialog;
	public GameObject									PrefabTips;

	public Notice 										PrefabNotice;
	public GameObject			   						m_PrefabChip;
	public GameObject			   						m_PrefabRank;
	public GameObject			   						m_PrefabTPlayer;
	public GameObject			   						m_PrefabObInfo;
	public GameObject			   						m_PrefabPreInfo;
	public UICircle			   							m_PrefabAvatar;
	public Poker			   							m_PrefabPoker;
	public GameObject								 	m_PrefabSelectAll;

	public GameObject 									m_LastDialog;
									
	//effect
	public GameObject									m_PrefabEffectSL;
	public GameObject 									m_PrefabEffectDeal;
	public GameObject 									m_PrefabEffectED;

	//Audio
	public AudioSource 									Music;
	public AudioSource 									SoundEffect;
	public List<AudioClip> 								Effects;
	public List<AudioClip> 								BGMs;


	//Self Info
	[HideInInspector] public int 						m_SelfSeatID 	= -1;
	[HideInInspector] public int 						m_TatgetSeatID	= -1;

	//Pos Config
	[HideInInspector] public List<Vector3> 				DefaultSeatPositions 			= new List<Vector3>();
	[HideInInspector] public List<Vector3> 				DefaultBetPositions 			= new List<Vector3>();
	[HideInInspector] public List<Vector3> 				DefaultShowHandPisitions 		= new List<Vector3>();
	[HideInInspector] public List<Vector3> 				DefaultHandPisitions			= new List<Vector3>();

	//PlayerListMode
	[HideInInspector] public int 						m_PlayerListMode = 0;

	[HideInInspector] public bool m_FirstSetBanker 		= false;
	private Notice 					NoticeBar;


	// Use this for initialization
	void Start () {
		NoticeBar = Common.InitNotices (PrefabNotice, Canvas.gameObject);
		Common.Sumbiting = false;
	}

	void Awake(){
		Canvas = GameObject.Find ("Canvas").transform;
		OnMusic ();
		AdjustUI ();

		InitCallbackForNet ();
		InstanceConfig ();
		m_StateManage.initialize(this);
		initializeRoomData ();

		if(Common.CState == Msg.GameState.Ready){
			m_StateManage.SetState (STATE.STATE_SEAT);
		}

		if(Common.CState == Msg.GameState.Bet){
			m_StateManage.SetState (STATE.STATE_SEAT);
			m_StateManage.ChangeState (STATE.STATE_BETTING);
			m_StateManage.m_StateBetting.DisAdjustUIChipControl ();
		}

		if(Common.CState == Msg.GameState.ConfirmBet){
			m_StateManage.SetState (STATE.STATE_SEAT);
			m_StateManage.ChangeState (STATE.STATE_BETTING, true);
			m_StateManage.m_StateBetting.ShowConfimBet();
		}

		if(Common.CState == Msg.GameState.Deal){
			m_StateManage.SetState (STATE.STATE_SEAT);
		}
			
		if(Common.CState == Msg.GameState.Combine){
			m_StateManage.SetState (STATE.STATE_SEAT);
			if (Common.CPokers.Count > 0) {
				m_StateManage.ChangeState (STATE.STATE_SORTING, true);
			} else {
				m_GameConsole.ShowWaitingAnime();
			}
		}

		if(Common.CState == Msg.GameState.ConfirmCombine){
			m_StateManage.SetState (STATE.STATE_SEAT);
			if (Common.CPokers.Count > 0) {
				m_StateManage.ChangeState (STATE.STATE_SORTING, true);
				m_StateManage.m_StateSorting.ConfimCombin ();
			} else {
				m_GameConsole.ShowWaitingAnime();
			}
		}

		if(Common.CState == Msg.GameState.Show){
			m_StateManage.SetState (STATE.STATE_SEAT);
			m_StateManage.ChangeState (STATE.STATE_SHOWHAND, true);
		}

		if(Common.CState == Msg.GameState.Result){
			m_StateManage.SetState (STATE.STATE_SEAT);
			m_StateManage.ChangeState (STATE.STATE_FINISH, true);
		}
	}

	// Update is called once per frame
	void Update () {
	}

	void AdjustUI(){
		m_LastDialog.SetActive (false);
		UpdateRooimInfo ();
	}

	public void initializeRoomData(){
//		Common.FB_id = "123235234";
//		Common.FB_access_token = "sd23edasdasdasdasd12easd";
//		Common.FB_name = "Walter wang";
//		Common.Uid = 111;
//		Common.CMin_bet 	= 20;
//		Common.CMax_bet 	= 1000;
//		Common.CHands 		= 20;
//		Common.CPlayed_hands= 19;
//		Common.CIs_share 	= true;
//		Common.CCredit_points = 1000;
//		Common.CState 		= Msg.GameState.Combine;
//		Common.CAutoBanker	= true;
//		if(Common.CState == Msg.GameState.Show){
//			Common.ConfigBetTime = 5;
//		}
//		else if(Common.CState == Msg.GameState.Ready){
//			Common.ConfigSortTime = 15;
//		}
//		else if(Common.CState == Msg.GameState.Result){
//			Common.ConfigFinishTime = 8;
//		}
//
//		PlayerInfo p0 = new PlayerInfo ();
//		p0.Uid = 111;
//		p0.SeatID = 0;
//		p0.Name = "Bruce";
//		p0.Bet = 0;
//		p0.FB_avatar = "https://imgsa.baidu.com/forum/w%3D580/sign=21c98377362ac65c6705667bcbf2b21d/a8ca36d12f2eb938801e25e5d6628535e5dd6f67.jpg";
//		Common.CPlayers.Add (p0);
//
//		PlayerInfo p1 = new PlayerInfo ();
//		p1.Uid = 222;
//		p1.SeatID =  1;
//		p1.Name = "Ali";
//		p1.Bet = 0;
//		p1.Score = 100;
//		p1.FB_avatar = "https://imgsa.baidu.com/forum/w%3D580/sign=20bd50a0ea50352ab16125006343fb1a/87c154e736d12f2ea376f1774cc2d56285356867.jpg";
//
//		Common.CPlayers.Add (p1);
//
//		PlayerInfo p2 = new PlayerInfo ();
//		p2.Uid = 333;
//		p2.SeatID =  2;
//		p2.Name = "HAHA";
//		p2.Bet = 200;
//		p2.Score = 100;
//		p2.FB_avatar = "https://imgsa.baidu.com/forum/w%3D580/sign=f51e5f0dc55c1038247ececa8211931c/cafc2f2eb9389b50538cbf458635e5dde7116e67.jpg";
//		Common.CPlayers.Add (p2);
//
//		PlayerInfo p3 = new PlayerInfo ();
//		p3.Uid = 444;
//		p3.SeatID =  3;
//		p3.Name = "gegeg";
//		p3.Bet = 100;
//		p3.FB_avatar = "https://imgsa.baidu.com/forum/w%3D580/sign=2864ac7b133853438ccf8729a313b01f/d303b9389b504fc2c82cef12e6dde71190ef6d67.jpg";
//		Common.CPlayers.Add (p3);
//
//		int[] s = new int[]{1,13,3,6,5,7,24,8,18,11,38,12,27};
//		Common.CPokers = new List<int> (s);
//		ResultEvent (null);

		//Bet type
		m_StateManage.m_StateBetting.UpdateDateBetType ();

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
		Common.CRoom_number = "";
		Common.CPlayers.Clear ();
		Common.CPokers.Clear ();
		SceneManager.LoadScene ("Scene/Lobby");
	}

	public void UpdateRooimInfo(){
		Canvas.Find ("TableInfo/RoomNumber").GetComponent<Text> ().text = "Password : " + Common.CRoom_number;
		Canvas.Find ("TableInfo/BetSize").GetComponent<Text> ().text = "Bet Size  " + Common.ToCarryNum((int)Common.CMin_bet) + "-" + Common.ToCarryNum((int)Common.CMax_bet);
		Canvas.Find ("TableInfo/Share").GetComponent<Text> ().text = "Room Fee shared";
		Canvas.Find ("TableInfo/Round").GetComponent<Text> ().text = (Common.CPlayed_hands + 1).ToString () + "/" + Common.CHands.ToString ();
	}

	//===================================Event=================================
	public void DealCardsEvent(RepeatedField<uint> poks){
		Common.CPokers.Clear ();
		for(int i = 0; i < poks.Count; i++){
			Common.CPokers.Add ((int)poks[i]);
		}
	}

	public void DealSeatEvent(RepeatedField<uint> seats){
		Common.CSeats.Clear ();
		for(int i = 0; i < seats.Count; i++){
			Common.CSeats.Add ((int)seats[i]);
		}
	}

	 public void ResultEvent(RepeatedField<global::Msg.SeatResult> ResultList){
		Common.CSeatResults.Clear ();	

		for(int i = 0; i < ResultList.Count; i++){
			
			CSeatResult info 	= new CSeatResult ();
			info.SeatID 		= (int)ResultList [i].SeatId;

			if((int)ResultList [i].SeatId != 0){info.score 			= new List<int> (ResultList [i].Scores);}

			PlayerInfo p = GetPlayerInfoForSeatID (info.SeatID);
			if (p != null) {
				info.Name 	= p.Name;
				info.Avatar = p.FB_avatar;
				info.BWin 	= p.Score;
			}

			
			info.autowin 		= ResultList [i].Autowin;
			info.foul			= ResultList [i].Foul;
			info.Win 			= ResultList [i].Win;
			info.Ranks 			= ResultList [i].Ranks;

			for(int o = 0; o < ResultList [i].CardGroups.Count; o++){
				Msg.CardGroup cg = ResultList [i].CardGroups[o];
				info.Pres.Add (cg.Cards);
			}
			Common.CSeatResults.Add (info.SeatID, info);

			foreach(PlayerInfo player in Common.CPlayers){
				if(player.Uid == ResultList [i].Uid){
					player.Score += ResultList [i].Win;
				}
			}
		}


//		uint[] arr = { 1, 2, 3};
//		RepeatedField<uint> arrr = new RepeatedField<uint>{ arr };
//
//		uint[] arr1 = { 4, 5, 6, 7, 8 };
//		RepeatedField<uint> arrr1 = new RepeatedField<uint>{ arr1 };
//
//		uint[] arr2 = { 9, 10, 11, 12, 13 };
//		RepeatedField<uint> arrr2 = new RepeatedField<uint>{ arr2 };
//
//		CSeatResult hinfo = new CSeatResult ();
//		hinfo.Name = "walter";
//		hinfo.Avatar = "";
//		hinfo.Bet = 2000;
//		hinfo.Win = 120;
//		hinfo.SeatID = 0;
//		hinfo.autowin = true;
//		hinfo.foul = false;
//		hinfo.Pres.Add (arrr);
//		hinfo.Pres.Add (arrr1);
//		hinfo.Pres.Add (arrr2);
//
//		hinfo.score.Add (1);
//		hinfo.score.Add (-1);
//		hinfo.score.Add (-1);
//
//
//		Common.CSeatResults.Add (0, hinfo);
//		Common.CSeatResults.Add (1, hinfo);
//		//SeatResults.Add (2, hinfo);
//		Common.CSeatResults.Add (3, hinfo);
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
		Loom.QueueOnMainThread(()=>{
			Common.IsOnline = false;

			if(Common.needReConnect){
				ExitGame ();
			}
			else{
				Common.needReConnect = true;
				SceneManager.LoadScene("Scene/UpdateVersion");
			}
		}); 
	}

	public void Data(Protocol data){
		Loom.QueueOnMainThread(()=>{  
			Common.Sumbiting = false;
			Common.EndCalling ();
		}); 


		Debug.Log (data.ToString());
		if(data == null){return; }

		switch (data.Msgid) {

		case MessageID.LeaveRoomRsp:
			if (data.LeaveRoomRsp.Ret == 0) {
				Loom.QueueOnMainThread (() => {
					ExitGame ();
				}); 
			} else {
				Loom.QueueOnMainThread (() => {  
					Common.TipsOn (PrefabTips, Canvas.gameObject, Common.TipsCantLeave);
				}); 
			}
			break;

		case MessageID.AutoBankerRsp:
			if(data.AutoBankerRsp.Ret == 0){
			}
			break;

		case MessageID.SitDownRsp:
			if (data.SitDownRsp.Ret == 0) {
				Loom.QueueOnMainThread (() => {

					SetSeatID (Common.Uid, m_TatgetSeatID);
					UpdateOrderList ();

					if (m_TatgetSeatID == 0) {
						if(GetTablePlayersCount() > 1 && Common.CState == Msg.GameState.Ready){m_Letplay.SetActive(true);}
						Common.CAutoBanker = data.SitDownRsp.Autobanker;
					}

					if (m_StateManage.GetCulState () == STATE.STATE_BETTING) {
						m_StateManage.m_StateBetting.SitDown ();
					}
				}); 
			} 
			else if(data.SitDownRsp.Ret == ErrorID.SitDownCreditPointsOut){
				Loom.QueueOnMainThread (() => {
					Common.TipsOn (PrefabTips, Canvas.gameObject, Common.TipsCreditMax);
				});
			}
			else {
				Loom.QueueOnMainThread (() => {
					Common.TipsOn (PrefabTips, Canvas.gameObject, Common.TipsSeatWasToken);
				});
			}
			break;

		case MessageID.StandUpRsp:
			if (data.StandUpRsp.Ret == 0) {
				Loom.QueueOnMainThread(()=>{
					m_StateManage.m_StateBetting.CancelBet();
					SetSeatID (Common.Uid, -1);
					UpdateOrderList ();
					m_Letplay.SetActive(false);
				}); 
			}
			break;

		case MessageID.StartGameRsp:
			if(data.StartGameRsp.Ret == ErrorID.StartGameNotEnoughDiamonds){
				Loom.QueueOnMainThread (() => {  
					Common.ErrorDialog (PrefabDialog, Canvas.gameObject, Common.ErrorCantGame);
				}); 
			}
			break;

		case MessageID.BetRsp:
			if (data.BetRsp.Ret == 0) {
				Loom.QueueOnMainThread(()=>{
					m_StateManage.m_StateBetting.UpdatePlayerBet();
					m_StateManage.m_StateBetting.DCanBetCall();
				}); 
			}
			break;

		case MessageID.CombineRsp:
			if (data.CombineRsp.Ret == 0) {	
				Loom.QueueOnMainThread (() => {
					List<List<uint>> cards = new List<List<uint>> (); 
					foreach(Msg.CardGroup cardg in data.CombineRsp.CardGroups ){
						List<uint> duan = new List<uint> (cardg.Cards);
						cards.Add (duan);
					}
					m_StateManage.m_StateSorting.SynchPoker (cards);
				});
			}
				
			break;

		case MessageID.GetScoreboardRsp:
			if (data.GetScoreboardRsp.Ret == 0) {
				Loom.QueueOnMainThread (() => {
					List<Msg.ScoreboardItem> list = new List<ScoreboardItem>();
					foreach(Msg.ScoreboardItem t in data.GetScoreboardRsp.Items){
						list.Add(t);
					}
					m_GameConsole.ShowPlayerList (list, m_PlayerListMode);
				});
			}
			break;

		case MessageID.CloseResultRsp:
			if (data.CloseResultRsp.Ret == 0) {
				Loom.QueueOnMainThread (() => {
				});
			}
			break;

		case MessageID.GetRoundHistoryRsp:
			if (data.GetRoundHistoryRsp.Ret == 0) {
				Loom.QueueOnMainThread (() => {
					
					if(data.GetRoundHistoryRsp.Results.Count <= 0){return;}

					Dictionary<int,  CSeatResult> LSeatResults 	= new Dictionary<int, CSeatResult>();

					for(int i = 0; i < data.GetRoundHistoryRsp.Results.Count; i++){
						Msg.PlayerRoundHistory ps = data.GetRoundHistoryRsp.Results[i];

						CSeatResult info 	= new CSeatResult ();
						info.SeatID 		= (int)ps.Result.SeatId;
						info.Name 			= ps.Name;
						info.Avatar 		= ps.Avatar;
						info.autowin 		= ps.Result.Autowin;
						info.foul			= ps.Result.Foul;
						info.Win 			= ps.Result.Win;
						info.Ranks 			= ps.Result.Ranks;

						for(int o = 0; o < ps.Result.CardGroups.Count; o++){
							Msg.CardGroup cg = ps.Result.CardGroups[o];
							info.Pres.Add (cg.Cards);
						}
						LSeatResults.Add (info.SeatID, info);
					}

					m_GameConsole.ShowHandReview(LSeatResults);
				});
			}
			break;

		case MessageID.GameStateNotify:
			Common.CState = data.GameStateNotify.State;
			switch(data.GameStateNotify.State){
	
			case Msg.GameState.Ready:
				Loom.QueueOnMainThread(()=>{
					Common.CPlayed_hands = data.GameStateNotify.PlayedHands;
					m_StateManage.ChangeState (STATE.STATE_SEAT);
					if(m_SelfSeatID == 0 && GetTablePlayersCount() > 1 ){m_Letplay.SetActive(true);}
				}); 
				break;

			case Msg.GameState.Bet:
				Loom.QueueOnMainThread(()=>{
					Common.CSeatResults.Clear ();
					Common.CPlayed_hands = data.GameStateNotify.PlayedHands;
					m_Letplay.SetActive(false);
					Common.ConfigBetTime = (int)data.GameStateNotify.Countdown / 1000;
					m_StateManage.ChangeState (STATE.STATE_BETTING);
					m_StateManage.m_StateBetting.AdjustUIChipControl();
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
					DealSeatEvent(data.GameStateNotify.DealSeats);
					m_StateManage.ChangeState (STATE.STATE_DEAL);
				}); 
				break;

			case Msg.GameState.Combine:
				Loom.QueueOnMainThread(()=>{
					if(m_SelfSeatID == 0 || GetPlayerInfoForSeatID(m_SelfSeatID).Bet > 0){
						m_StateManage.m_StateBetting.RealExit();
						Common.ConfigSortTime = (int)data.GameStateNotify.Countdown / 1000;
						m_StateManage.ChangeState (STATE.STATE_SORTING);
					}
					else{
						m_GameConsole.ShowWaitingAnime();
					}
				}); 
				break;

			case Msg.GameState.ConfirmCombine:
				Loom.QueueOnMainThread(()=>{
					m_StateManage.m_StateSorting.AutoSortConfrm();
				}); 
				break;

			case Msg.GameState.Show:
				Loom.QueueOnMainThread(()=>{
					m_StateManage.m_StateBetting.RealExit();
					m_GameConsole.HideWaitAnime();

					ResultEvent(data.GameStateNotify.Result);
					m_StateManage.ChangeState (STATE.STATE_SHOWHAND);
				}); 
				break;

			case Msg.GameState.Result:
				Loom.QueueOnMainThread(()=>{
					//m_StateManage.m_StateSeat.UpdateSeatScore();
					Common.ConfigFinishTime = (int)data.GameStateNotify.Countdown / 1000;
					m_StateManage.ChangeState (STATE.STATE_FINISH);
				}); 
				break;
			}
			break;

		case MessageID.SitDownNotify:
			if(data.SitDownNotify.Type == Msg.SitDownType.Sit){
				Loom.QueueOnMainThread(()=>{

					foreach(PlayerInfo p in Common.CPlayers){
						if(p.Uid == data.SitDownNotify.Uid){
							p.Score = data.SitDownNotify.Score;
						}
					}

					SetSeatID (data.SitDownNotify.Uid, (int)data.SitDownNotify.SeatId);

					foreach(PlayerInfo p1 in Common.CPlayers){
						Debug.Log(p1.Uid + "===============" + p1.SeatID);
					}

					UpdateOrderList ();

					if(m_SelfSeatID == 0 &&  GetTablePlayersCount() > 1 && Common.CState == Msg.GameState.Ready){m_Letplay.SetActive(true);}
				}); 
			}
			break;

		case MessageID.StandUpNotify:
			if(data.StandUpNotify.Reason == StandUpReason.NoActionFor3Hands){
				Loom.QueueOnMainThread (() => {
					if(m_SelfSeatID == 0){m_FirstSetBanker =false;}
					if(data.StandUpNotify.Uid == Common.Uid){
						Common.ErrorDialog (PrefabDialog, Canvas.gameObject, Common.ErrorOutdue);
					}
				});
			}
			else if(data.StandUpNotify.Reason == StandUpReason.CreditPointsOut){
				Loom.QueueOnMainThread (() => {
					Common.ErrorDialog (PrefabDialog, Canvas.gameObject, Common.ErrorGameCreditMax);
				});
			}

			Loom.QueueOnMainThread (() => {
				m_StateManage.m_StateBetting.UpdateChipsUI (GetSeatIDForPlayerID (data.StandUpNotify.Uid), 0);
				SetSeatID (data.StandUpNotify.Uid, -1);
				UpdateOrderList ();
				if(GetTablePlayersCount() <= 1 && Common.CState == Msg.GameState.Ready){m_Letplay.SetActive(false);}
			}); 

			break;

		case MessageID.JoinRoomNotify:
			Loom.QueueOnMainThread(()=>{
				PlayerInfo p = new PlayerInfo ();
				p.Uid 		= data.JoinRoomNotify.Uid;
				p.Name 		= data.JoinRoomNotify.Name;
				p.FB_avatar	= data.JoinRoomNotify.Avatar;
				p.SeatID 	= -1;
				Common.CPlayers.Add (p);

				foreach(PlayerInfo p1 in Common.CPlayers){
					Debug.Log(p1.Uid + "===============" + p1.SeatID);
				}

			}); 
			break;

		case MessageID.KickNotify:
			if(data.KickNotify.Type == Msg.KickType.StopServer){
				Loom.QueueOnMainThread (() => {
					Common.ErrorDialog (PrefabDialog, Canvas.gameObject, Common.ErrorKickGame, Common.CloseServerDialog);
				}); 

			}
			break;

		case MessageID.NoticesNotify:
			Loom.QueueOnMainThread (() => {
				foreach(Msg.Notice s in data.NoticesNotify.Notices){
					if (s.Type == Msg.NoticeType.HorseLamp) {
						NoticeMessage nm = new NoticeMessage ();
						nm.text = s.Content;
						nm.times = 3;
						Common.GameNotices.Add (nm);
					} else {
						Common.ErrorDialog (PrefabDialog, Canvas.gameObject, s.Content);
					}
				}

				NoticeBar.OnPlay ();
			}); 
			break;

		case MessageID.LeaveRoomNotify:
			Loom.QueueOnMainThread (() => {
				LeaveRoomEvent (data.LeaveRoomNotify.Uid);
				if(GetTablePlayersCount() <= 1 && Common.CState == Msg.GameState.Ready){m_Letplay.SetActive(false);}
			}); 
			break;

		case MessageID.BetNotify:
			Loom.QueueOnMainThread (() => {
				m_StateManage.m_StateBetting.UpdateChipsUI ( (int)data.BetNotify.SeatId, (int)data.BetNotify.Chips);
			}); 
			break;
		}
	}

	public void LeaveRoomServer(){
		if(!Common.Sumbit (PrefabTips ,Canvas.gameObject)){return;}

		m_GameConsole.CloseMenu ();

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
		if(!Common.Sumbit (PrefabTips ,Canvas.gameObject)){return;}

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

	public void AutoBankServer(bool auto){
		if(!Common.Sumbit (PrefabTips ,Canvas.gameObject)){return;}

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.AutoBankerReq;
		msg.AutoBankerReq 				= new AutoBankerReq();
		msg.AutoBankerReq.AutoBanker	= auto;

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void StandUpServer(){
		if(m_SelfSeatID == -1){return;}

		if (m_StateManage.GetCulState () == STATE.STATE_SEAT || m_StateManage.GetCulState () == STATE.STATE_BETTING) {
			if(GetPlayerInfoForSeatID(m_SelfSeatID).Bet > 0){return;}
			if(!Common.Sumbit (PrefabTips ,Canvas.gameObject)){return;}
			m_GameConsole.CloseMenu ();

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
		if(!Common.Sumbit (PrefabTips ,Canvas.gameObject)){return;}

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
		if(!Common.Sumbit (PrefabTips ,Canvas.gameObject)){return;}

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
		if(!Common.Sumbit (PrefabTips ,Canvas.gameObject)){return;}

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

	public void ScoreboardServer(){
		Common.Calling (Canvas.gameObject);

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.GetScoreboardReq;
		msg.GetScoreboardReq 			= new GetScoreboardReq();
		msg.GetScoreboardReq.Pos 		= 0;

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void GetRoundHistoryServer(uint round){
		Common.Calling (Canvas.gameObject);

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.GetRoundHistoryReq;
		msg.GetRoundHistoryReq 			= new GetRoundHistoryReq();
		msg.GetRoundHistoryReq.Round 	= round;

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void CloseResultServer(){
		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.CloseResultReq;
		msg.CloseResultReq 				= new CloseResultReq();

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void OnMusic(){
		Music.Stop ();
		if(Common.ConfigMusicOn){
			Music.clip = BGMs [Random.Range (0, 2)];
			Music.Play ();
		}
	}

	public void PlayEffect(Effect ect){
		if(Common.ConfigMusicOn){
			SoundEffect.clip = Effects [(int)ect];
			SoundEffect.Play ();
		}
	}

	public void OnApplicationPause(){
		if(!Common.isPause)
		{
			Common.PauseTime = Common.GetTimeStamp();
		}

		else 
		{
			Common.isFocus=true;
		}

		Common.isPause=true;
	}

	public void OnApplicationFocus(){

		if(Common.isFocus)
		{
			long timet = Common.GetTimeStamp() - Common.PauseTime;

			Debug.Log (timet);
			Debug.Log (Client.Instance.IsConnected());

			if( timet <= Common.PauseTimeOut){
				if( !Client.Instance.IsConnected() ){
					SceneManager.LoadScene ("Scene/Lobby");
				}
			}

//			if( timet > Common.PauseTimeOut && timet < Common.PauseTimeOutLong){
//				if (!Client.Instance.IsConnected ()) {
//					SceneManager.LoadScene ("Scene/Lobby");
//				} else {
//					Common.needReConnect = true;
//					Client.Instance.Disconnect ();
//				}
//
//			}

			if( timet >= Common.PauseTimeOutLong){
				if (!Client.Instance.IsConnected ()) {
					SceneManager.LoadScene("Scene/UpdateVersion");
				} else {
					Common.needReConnect = false;
					Client.Instance.Disconnect ();
				}
			}


			Common.PauseTime = 0;
			Common.isPause=false;
			Common.isFocus=false;
		}

		if(Common.isPause)
		{
			Common.isFocus=true;
		}
	}
}
