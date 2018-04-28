using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Facebook.Unity;

using networkengine;
using Google.Protobuf;
using Msg;
using System.IO;

using Google.Protobuf.Collections;
using Google.Protobuf;

public class RoomInfo{
	public uint 		Owner;
	public uint 		RoomId ;
	public string 		RoomNumber ;
	public string		RoomName;
	public uint 		Hands;
	public uint 		PlayerHands;
	public List<ListRoomPlayerInfo> Players;
}

public class LobbyController : MonoBehaviour {
	//Prefab
	public GameObject 				PrefabRoomInfo;
	public GameObject 				PrefabDialog;
	public UICircle 				PrefabAvatar;
	public GameObject 				PrefabTips;

	//Controller
	public CreateRoomControl 		CreateRoomControl;
	public JoinRoomControl 			JoinRoomControl;

	//Layer
	public CareerControl 			CareerControl;
	public ConterControl 			ConterControl;
	public PrefileControl		 	PrefileControl;
	public SettingControl		 	SettingControl;

	//Audio
	public AudioSource 				Music;
	public AudioSource 				ButtonEffects;
	public List<AudioClip> 			BGMs;

	public GameObject 				Conneting;
	public GameObject 				Canvas;
	private List<RoomInfo>			RoomLists= new List<RoomInfo>();
	private List<uint>				RoomIDs = new List<uint>();

	public GameObject				NoData;
	
	// Use this for initialization
	void Start () {
		Common.Sumbiting = false;
		Common.Trying = 0;
		NoData.SetActive (false);
		Conneting.SetActive (false);
		CheckConnection ();
	}

	void Awake(){
		InitCallbackForNet ();
		OnMusic ();
	}
	
	// Update is called once per frame
	void Update () {
	}
		
	public void ComingSoon(){
		Common.TipsOn (PrefabTips, Canvas, Common.TipsComingSoon);
	}

	public void CreateRoom(){
		PlayerButtonEffect ();
		CreateRoomControl.Enter();
	}

	public void JoinRoom(){
		PlayerButtonEffect ();
		JoinRoomControl.Enter ();
	}

	public void GoToCareer(){
		PlayerButtonEffect ();
		CareerControl.Enter ();
	}

	public void GoToConter(){
		PlayerButtonEffect ();
	}

	public void GoToPrefile(){
		PlayerButtonEffect ();
		PrefileControl.Enter ();
	}

	public void GoToSetting(){
		PlayerButtonEffect ();
		SettingControl.Enter ();
	}
		
	public void PlayGame(){
		SceneManager.LoadScene ("Scene/Game");
	}

	public void GoLogin(){
		SceneManager.LoadScene ("Scene/Login");
	}

	//connecting
	public void CheckConnection(){
		if (!Common.IsOnline) {
			protonet.ConnectServer ();
		} else {
			GetPlayingRoomServer ();
		}
	}


	//===================================Room list=================================
	public void onCloseRoomHandler(GameObject obj){
		foreach( RoomInfo room in RoomLists){
			if(room.RoomId.ToString() == obj.transform.parent.name){
				CloseRoomServer (room.RoomId);
			}
		}
	}

	public void onJoinRoomHandler(GameObject obj){
		foreach( RoomInfo room in RoomLists){
			if(room.RoomId.ToString() == obj.transform.parent.name){
				JoinRoomServer (room.RoomNumber);
			}
		}
	}

	public void UpdateRoomHand(uint roomid, uint round){
		if(RoomIDs.BinarySearch(roomid) == 0){
			Transform RoomInfo = Canvas.transform.Find("RoomList/Viewport/Content/" + roomid.ToString());
			RoomInfo.transform.Find ("PHand").GetComponent<Text> ().text = round.ToString();
		}
	}

	public void UpdateRoomClose(uint roomid){
		
		foreach(int id in RoomIDs){
			if(id == roomid){
				foreach(RoomInfo room in RoomLists){
					if(roomid == room.RoomId){
						RoomLists.Remove (room);
						UpdateRoomsInfo ();
						break;
					}
				}
			}
		}
	}

	public void UpdateRoomSitDown(uint roomid, Msg.ListRoomPlayerInfo player){
		if(RoomIDs.BinarySearch(roomid) == 0){
			Transform RoomInfo = Canvas.transform.Find("RoomList/Viewport/Content/" + roomid.ToString());

			UICircle avatar = (UICircle)Instantiate(PrefabAvatar);
			avatar.transform.SetParent (RoomInfo.Find ("Player/" + player.SeatId + "/Avatar" + player.SeatId));
			avatar.transform.localPosition = new Vector3 ();
			avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (56, 56);
			StartCoroutine(Common.Load(avatar, player.Avatar));
		}
	}

	public void UpdateRoomStandUp(uint roomid, Msg.ListRoomPlayerInfo player){
		if(RoomIDs.BinarySearch(roomid) == 0){
			Transform RoomInfo = Canvas.transform.Find("RoomList/Viewport/Content/" + roomid.ToString());
			Destroy (RoomInfo.Find("Player/" + player.SeatId + "/Avatar").gameObject);
		}
	}

	public void UpdateRoomsInfo(){
		Transform Content = Canvas.transform.Find("RoomList/Viewport/Content");

		for (int i = Content.childCount - 1; i >= 0; i--) {  
			Destroy(Content.GetChild(i).gameObject);
		}  

		if(RoomLists.Count == 0){
			NoData.SetActive (true);
			return;
		}
		else{
			NoData.SetActive (false);
		}

		float width =Canvas.transform.Find ("RoomList").GetComponent<RectTransform> ().sizeDelta.x;;
		float left 	= 4;

		if (RoomLists.Count < 3) {width = 0;
		} else {width = 200 * RoomLists.Count + 33 * (RoomLists.Count - 1) - width + 8;}

		Content.GetComponent<RectTransform> ().sizeDelta = new Vector2 (width, Content.GetComponent<RectTransform> ().sizeDelta.y);

		for(int i = 0; i < RoomLists.Count; i++){
			GameObject RoomInfo = (GameObject)Instantiate(PrefabRoomInfo);

			RoomInfo.name = RoomLists [i].RoomId.ToString();
			RoomInfo.transform.Find ("Name").GetComponent<Text> ().text = RoomLists [i].RoomName;
			RoomInfo.transform.Find ("PHand").GetComponent<Text> ().text = RoomLists [i].PlayerHands.ToString();
			RoomInfo.transform.Find ("Hands").GetComponent<Text> ().text = "/" + RoomLists [i].Hands.ToString();

			for(int o = 0; o < RoomLists[i].Players.Count; o++){
				UICircle avatar = (UICircle)Instantiate(PrefabAvatar);
				avatar.transform.SetParent (RoomInfo.transform.Find ("Player" + o + "/Avatar"));
				avatar.transform.localPosition = new Vector3 ();
				avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (56, 56);
				StartCoroutine(Common.Load(avatar, RoomLists[i].Players[o].Avatar));
			}
				
			if (RoomLists [i].Owner == Common.Uid) {
				RoomInfo.transform.Find ("Close").gameObject.SetActive (true);
				EventTriggerListener.Get(RoomInfo.transform.Find ("Close").gameObject).onClick = onCloseRoomHandler;
			} else {
				RoomInfo.transform.Find ("Close").gameObject.SetActive (false);
			}

			EventTriggerListener.Get(RoomInfo.transform.Find ("Join").gameObject).onClick = onJoinRoomHandler;
			RoomInfo.transform.SetParent (Content);
			RoomInfo.transform.localScale = new Vector3 (1, 1, 1);
			RoomInfo.transform.localPosition = new Vector3 (left, 0, 0);
			left += 200 + 33;
		}
	}

	//===================================connect=================================
	public void InitCallbackForNet(){
		protonet.SetConnectedCall(Connected);
		protonet.SetFailConnectedCall(FailedConnect);
		protonet.SetDisConnectedCall(DisConnect);
		protonet.SetDataCall(Data);
	}

	public void LoginServer(){
		Protocol msg 			= new Protocol();
		msg.Msgid 				= MessageID.LoginReq;
		msg.LoginReq = new LoginReq();
		msg.LoginReq.Type		= LoginType.Facebook;
		msg.LoginReq.Fb 		= new LoginFBReq();
		msg.LoginReq.Fb.FbId 	= Common.FB_id;
		msg.LoginReq.Fb.Token 	= Common.FB_access_token;

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void Connected(){
		Loom.QueueOnMainThread(()=>{  
			Common.Trying = 0;
			LoginServer ();
		}); 
	}

	public void FailedConnect(){
		Common.Trying++;
		if (Common.Trying > Common.MaxTrying) {
			Loom.QueueOnMainThread(()=>{  
				Common.ErrorDialog (PrefabDialog, Canvas, Common.ErrorNoConnect, GoLoginCall);
			}); 

		} else {
			protonet.ConnectServer ();
		}
	}

	public void GoLoginCall(GameObject Obj){
		GoLogin ();
	}

	public void DisConnect(){
		Loom.QueueOnMainThread(()=>{  
			Common.IsOnline = false;
			if(Common.needReConnect){
				CheckConnection ();
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

		if(data == null){return;}

		Debug.Log (data.ToString ());
		switch (data.Msgid) {

		case MessageID.LoginRsp:
			
			if (data.LoginRsp.Ret == 0) {
				Common.Uid = data.LoginRsp.Uid;
				Common.FB_name = data.LoginRsp.Name;
				Common.FB_avatar = data.LoginRsp.Avatar;
				Common.IsOnline = true;

				Loom.QueueOnMainThread (() => {
					GetPlayingRoomServer ();
				}); 
			} else {
				Loom.QueueOnMainThread (() => { 
					Common.ErrorDialog (PrefabDialog, Canvas, Common.ErrorLogin);
				}); 
			}
			break;

		case MessageID.ListRoomsRsp:
			if(data.ListRoomsRsp.Ret == 0){
				Loom.QueueOnMainThread(()=>{
					RoomIDs.Clear();
					RoomLists.Clear();

					foreach(ListRoomItem room in data.ListRoomsRsp.Rooms){
						RoomInfo r = new RoomInfo();
						r.RoomId = room.RoomId;
						r.Owner = room.OwnerUid;
						r.RoomName = room.RoomName;
						r.RoomNumber = room.RoomNumber;
						r.Hands = room.Hands;
						r.PlayerHands = room.PlayedHands;
						r.Players =  new List<ListRoomPlayerInfo>();

						foreach(ListRoomPlayerInfo player in room.Players){
							r.Players.Add(player);
						}
							
						RoomLists.Add(r);
						RoomIDs.Add (room.RoomId);
					}
					UpdateRoomsInfo();
				}); 
			}
			break;

		case MessageID.CloseRoomRsp:
			if (data.CloseRoomRsp.Ret == 0) {
				Loom.QueueOnMainThread (() => {
					UpdateRoomClose (data.CloseRoomRsp.RoomId);
				}); 
			} else {
				Loom.QueueOnMainThread (() => {  
					Common.TipsOn (PrefabTips, Canvas, Common.TipsCantCloseRoom);
				}); 
			}
			break;

		case MessageID.GetPlayingRoomRsp:
			if (data.GetPlayingRoomRsp.Ret == 0) {
				Loom.QueueOnMainThread (() => {
					Common.CRoom_number = data.GetPlayingRoomRsp.RoomNumber;

					if(string.IsNullOrEmpty(Common.CRoom_number)){
						RoomListServer ();
					}
					else{
						JoinRoomServer (Common.CRoom_number);
					}
				}); 
			} 
			break;

		case MessageID.CreateRoomRsp:
			if (data.CreateRoomRsp.Ret == 0) {
				Loom.QueueOnMainThread (() => {
					Common.CRoom_id = data.CreateRoomRsp.RoomId;
					Common.CRoom_number	= data.CreateRoomRsp.RoomNumber;
					JoinRoomServer (Common.CRoom_number, false);
				}); 
			} else if (data.CreateRoomRsp.Ret == ErrorID.CreateRoomNotEnoughDiamonds) {
				Loom.QueueOnMainThread (() => {  
					Common.ErrorDialog (PrefabDialog, Canvas, Common.ErrorInsufficient);
				}); 
			} 
			else if(data.CreateRoomRsp.Ret == ErrorID.CreateRoomExceedLimitationRooms){
				Loom.QueueOnMainThread (() => {  
					Common.TipsOn (PrefabTips, Canvas, Common.TipsCreateRoomMax);
				}); 
			}else {
				Loom.QueueOnMainThread (() => {  
					Common.TipsOn (PrefabTips, Canvas, Common.TipsCreareRoom);
				}); 
			}
			break;

		case MessageID.JoinRoomRsp:
			Loom.QueueOnMainThread (() => {
				Conneting.SetActive (false);
			}); 

			if (data.JoinRoomRsp.Ret == 0) {
				Common.CRoom_id = data.JoinRoomRsp.Room.RoomId;
				Common.CRoom_number = data.JoinRoomRsp.Room.Number;
				Common.CRoom_name	= data.JoinRoomRsp.Room.Name;
				Common.CMin_bet = data.JoinRoomRsp.Room.MinBet;
				Common.CMax_bet = data.JoinRoomRsp.Room.MaxBet;
				Common.CHands = data.JoinRoomRsp.Room.Hands;
				Common.CPlayed_hands = data.JoinRoomRsp.Room.PlayedHands;
				Common.CIs_share = data.JoinRoomRsp.Room.IsShare;
				Common.CCredit_points = data.JoinRoomRsp.Room.CreditPoints;
				Common.CState = data.JoinRoomRsp.Room.State;

				Common.CPokers.Clear ();
				Common.CPlayers.Clear ();
				Common.CSeatResults.Clear ();	

				for (int i = 0; i < data.JoinRoomRsp.Room.Players.Count; i++) {
					PlayerInfo p = new PlayerInfo ();
					p.Uid = data.JoinRoomRsp.Room.Players [i].Uid;
					p.SeatID = data.JoinRoomRsp.Room.Players [i].SeatId;
					p.Name = data.JoinRoomRsp.Room.Players [i].Name;
					p.Bet = data.JoinRoomRsp.Room.Players [i].Bet;
					p.FB_avatar = data.JoinRoomRsp.Room.Players [i].Avatar;
					p.Score = data.JoinRoomRsp.Room.Players [i].Score;

					Debug.Log (p.Uid + "===" + p.SeatID + "===" + p.Name);
					Common.CPlayers.Add (p);
				}

				switch (Common.CState) {

				case Msg.GameState.Bet:
					Common.ConfigBetTime = (int)data.JoinRoomRsp.Room.Countdown / 1000;
					break;

				case Msg.GameState.Deal:
					for (int i = 0; i < data.JoinRoomRsp.Room.Cards.Count; i++) {
						Common.CPokers.Add ((int)data.JoinRoomRsp.Room.Cards [i]);
					}
					break;

				case Msg.GameState.Combine:
					Common.ConfigSortTime = (int)data.JoinRoomRsp.Room.Countdown / 1000;
					for (int i = 0; i < data.JoinRoomRsp.Room.Cards.Count; i++) {
						Common.CPokers.Add ((int)data.JoinRoomRsp.Room.Cards [i]);
					}


					foreach( Msg.SeatResult res in data.JoinRoomRsp.Room.Result){
						if(res.Uid == Common.Uid){
							for(int o = 0; o < res.CardGroups.Count; o++){
								List<uint> t = new List<uint> (res.CardGroups[o].Cards);
								Common.CCPokers.Add (t);
							}
						}
					}

					break;

				case Msg.GameState.ConfirmCombine:
					Common.ConfigSortTime = (int)data.JoinRoomRsp.Room.Countdown / 1000;
					for (int i = 0; i < data.JoinRoomRsp.Room.Cards.Count; i++) {
						Common.CPokers.Add ((int)data.JoinRoomRsp.Room.Cards [i]);
					}

					foreach( Msg.SeatResult res in data.JoinRoomRsp.Room.Result){
						if(res.Uid == Common.Uid){
							for(int o = 0; o < res.CardGroups.Count; o++){
								List<uint> t = new List<uint> (res.CardGroups[o].Cards);
								Common.CCPokers.Add (t);
							}
						}
					}
					break;

				case Msg.GameState.Show:
					InitSeatResult (data.JoinRoomRsp.Room.Result);
					break;

				case  Msg.GameState.Result:
					Common.ConfigFinishTime = (int)data.JoinRoomRsp.Room.Countdown / 1000;
					InitSeatResult (data.JoinRoomRsp.Room.Result);
					break;
				}

				Loom.QueueOnMainThread (() => {
					PlayGame ();
				}); 
			} else {
				Loom.QueueOnMainThread (() => {  
					Common.TipsOn (PrefabTips, Canvas, Common.TipsJoinRoom);
				}); 
			}
			break;

		case MessageID.GetProfileRsp:
			if (data.GetProfileRsp.Ret == 0) {
				Common.Uid 				= data.GetProfileRsp.Uid;
				Common.FB_name 			= data.GetProfileRsp.Name;
				Common.FB_avatar 		= data.GetProfileRsp.Avatar;
				Common.DiamondAmount 	= data.GetProfileRsp.Diamonds;
				Loom.QueueOnMainThread(()=>{
					PrefileControl.UpdateSelfInfo();
				}); 
			}
			break;

		case MessageID.SendDiamondsRsp:
			if (data.SendDiamondsRsp.Ret == 0) {
				Loom.QueueOnMainThread(()=>{
					PrefileControl.HideSendDiamond ();
				}); 
			}
			else if(data.SendDiamondsRsp.Ret == ErrorID.SendDiamondsNoUser){
				Loom.QueueOnMainThread (() => {  
					Common.TipsOn (PrefabTips, Canvas, Common.TipsInvalidUserID);
				}); 
			}
			else if(data.SendDiamondsRsp.Ret == ErrorID.SendDiamondsCannotSelf){
				Loom.QueueOnMainThread (() => {  
					Common.TipsOn (PrefabTips, Canvas, Common.TipsCantSendSelf);
				}); 
			}
			else if(data.SendDiamondsRsp.Ret == ErrorID.SendDiamondsNotEnoughDiamonds){
				Loom.QueueOnMainThread (() => {  
					Common.TipsOn (PrefabTips, Canvas, Common.TipsInsufficient);
				}); 
			}
			break;

		case MessageID.DiamondsRecordsRsp:
			if (data.DiamondsRecordsRsp.Ret == 0) {
				Loom.QueueOnMainThread(()=>{
					PrefileControl.FormentDiamondRecord (data.DiamondsRecordsRsp.Records, data.DiamondsRecordsRsp.Users);
				}); 
			}
			break;

		case MessageID.CareerWinLoseDataRsp:
			if (data.CareerWinLoseDataRsp.Ret == 0) {
				Loom.QueueOnMainThread(()=>{
					CareerControl.UpdateRecordPie( data.CareerWinLoseDataRsp.Data);
				}); 
			}
			break;

		case MessageID.CareerRoomRecordsRsp:
			if (data.CareerRoomRecordsRsp.Ret == 0) {
				Loom.QueueOnMainThread(()=>{
					CareerControl.SetCareerRecordData(data.CareerRoomRecordsRsp.Records);
				}); 
			}
			break;

		case MessageID.ConsumeDiamondsNotify:
			break;
		}
	}

	public void InitSeatResult(RepeatedField<Msg.SeatResult> ResultList){
		for(int i = 0; i <ResultList.Count; i++){
			Msg.SeatResult result = ResultList [i];

			CSeatResult info 	= new CSeatResult ();

			info.SeatID 		= (int)result.SeatId;

			for(int o = 0; o < result.CardGroups.Count; o++){
				Msg.CardGroup cg = result.CardGroups[o];
				info.Pres.Add (cg.Cards);
			}
				
			if((int)result.SeatId != 0){info.score 			= new List<int> (result.Scores);}

			foreach(PlayerInfo p in Common.CPlayers){
				if(p.SeatID == info.SeatID){
					info.Name 	= p.Name;
					info.Avatar = p.FB_avatar;
					info.BWin 	= p.Score;
				}
			}

			info.autowin 		= result.Autowin;
			info.foul			= result.Foul;
			info.Win 			= result.Win;
			info.Ranks 			= result.Ranks;

			Common.CSeatResults.Add (info.SeatID, info);
		}
	}
		
	public void CreatRoomServer(string roomname, uint min_bet, uint max_bet, uint hands, uint credit_points, bool is_share){
		if(!Common.Sumbit (PrefabTips ,Canvas)){return;}

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.CreateRoomReq;
		msg.CreateRoomReq 				= new CreateRoomReq();
		msg.CreateRoomReq.Name			= roomname;
		msg.CreateRoomReq.MinBet		= min_bet;
		msg.CreateRoomReq.MaxBet		= max_bet;
		msg.CreateRoomReq.Hands			= hands;
		msg.CreateRoomReq.CreditPoints	= credit_points;
		msg.CreateRoomReq.IsShare		= is_share;

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void JoinRoomServer(string room_number, bool reconect = true){
		if(reconect){
			Conneting.SetActive (true);
		}

		if(!Common.Sumbit (PrefabTips ,Canvas)){return;}

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.JoinRoomReq;
		msg.JoinRoomReq 				= new JoinRoomReq();
		msg.JoinRoomReq.RoomNumber		= room_number;

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void RoomListServer(){
		Common.Calling (Canvas);

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.ListRoomsReq;
		msg.ListRoomsReq 				= new ListRoomsReq();

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void CloseRoomServer(uint roomid){
		if(!Common.Sumbit (PrefabTips ,Canvas)){return;}

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.CloseRoomReq;
		msg.CloseRoomReq 				= new CloseRoomReq();
		msg.CloseRoomReq.RoomId 		= roomid;

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void GetProfileServer(){
		Common.Calling (Canvas);

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.GetProfileReq;
		msg.GetProfileReq 				= new GetProfileReq();

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void SendDiamondsServer(uint uid, uint amount){
		if(!Common.Sumbit (PrefabTips ,Canvas)){return;}

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.SendDiamondsReq;
		msg.SendDiamondsReq 			= new SendDiamondsReq();
		msg.SendDiamondsReq.Uid 		= uid;
		msg.SendDiamondsReq.Diamonds 	= amount;

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void  DiamondsRecordsServer(string begin_time, string end_time){
		Common.Calling (Canvas);

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.DiamondsRecordsReq;
		msg.DiamondsRecordsReq 			= new DiamondsRecordsReq();
		msg.DiamondsRecordsReq.BeginTime = begin_time;
		msg.DiamondsRecordsReq.EndTime 	= end_time;

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void  CareerWinLostServer(List<uint> days){
		Common.Calling (Canvas);

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.CareerWinLoseDataReq;
		msg.CareerWinLoseDataReq 		= new CareerWinLoseDataReq();
		foreach(uint d in days){
			msg.CareerWinLoseDataReq.Days.Add (d);
		}

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void  CareerRecordsServer(uint days, uint index, uint count){
		Common.Calling (Canvas);

		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.CareerRoomRecordsReq;
		msg.CareerRoomRecordsReq 		= new CareerRoomRecordsReq();
		msg.CareerRoomRecordsReq.Days	= days;
		msg.CareerRoomRecordsReq.Pos	= index;
		msg.CareerRoomRecordsReq.Count	= count;

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void  GetPlayingRoomServer(){
		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.GetPlayingRoomReq;
		msg.GetPlayingRoomReq 			= new GetPlayingRoomReq();

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

	public void StopMusic(){
		Music.Pause ();
	}

	public void PlayMusic(){
		Music.Play ();
	}

	public void PlaySoundEffect(string effect){
	}

	public void PlayerButtonEffect(){
		if(Common.ConfigMusicOn){
			ButtonEffects.Play ();
		}
	}

	public void Logout(){
		FB.LogOut ();
		GoLogin ();
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

			if( timet <= Common.PauseTimeOut){
				if( !Client.Instance.IsConnected() ){
					CheckConnection ();
				}
			}

			if( timet > Common.PauseTimeOut && timet < Common.PauseTimeOutLong){
				if (!Client.Instance.IsConnected ()) {
					CheckConnection ();
				} else {
					Common.needReConnect = true;
					Client.Instance.Disconnect ();
				}

			}

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