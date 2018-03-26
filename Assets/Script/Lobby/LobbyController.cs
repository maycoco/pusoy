﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Facebook.Unity;

using networkengine;
using Google.Protobuf;
using Msg;
using System.IO;

public class RoomInfo{
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

	//Controller
	public CreateRoomControl 		CreateRoomControl;
	public JoinRoomControl 			JoinRoomControl;

	//Layer
	public CareerControl 			CareerControl;
	public ConterControl 			ConterControl;
	public PrefileControl		 	PrefileControl;
	public SettingControl		 	SettingControl;

	public GameObject 				Canvas;
	private List<RoomInfo>			RoomLists= new List<RoomInfo>();
	private List<uint>				RoomIDs = new List<uint>();
	
	// Use this for initialization
	void Start () {
		CheckConnection ();
	}

	void Awake(){
		InitCallbackForNet ();
		RoomListServer ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void AdjustUI(){
	}

	public void CreateRoom(){
		CreateRoomControl.Enter();
	}

	public void JoinRoom(){
		JoinRoomControl.Enter ();
	}

	public void GoToCareer(){
		CareerControl.Enter ();
	}

	public void GoToConter(){
	}

	public void GoToPrefile(){
		PrefileControl.Enter ();
	}

	public void GoToSetting(){
		SettingControl.Enter ();
	}
		
	public void PlayGame(){
		RoomListListenServer (false, RoomIDs);
		SceneManager.LoadScene (2);
	}

	//connecting
	public void CheckConnection(){
		Canvas.transform.Find ("Connecting").gameObject.SetActive (false);
		if(!string.IsNullOrEmpty(Common.CRoom_number)){
			Canvas.transform.Find ("Connecting").gameObject.SetActive (true);
			JoinRoomServer (Common.CRoom_number);
		}
	}


	//===================================Room list=================================
	public void onCloseRoomHandler(GameObject obj){
		CloseRoomServer ((uint)obj.name);
	}

	public void UpdateRoomHand(uint roomid, uint round){
		if(RoomIDs.BinarySearch(roomid) == 0){
			Transform RoomInfo = Canvas.transform.Find("RoomList/Viewport/Content/" + roomid.ToString());
			RoomInfo.transform.Find ("PHand").GetComponent<Text> ().text = round.ToString();
		}
	}

	public void UpdateRoomClose(uint roomid){
		if(RoomIDs.BinarySearch(roomid) == 0){
			foreach(RoomInfo room in RoomLists){
				if(roomid == room.RoomId){
					RoomLists.Remove (room);
					UpdateRoomsInfo ();
					break;
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

		float width =Canvas.transform.Find ("RoomList").GetComponent<RectTransform> ().sizeDelta.x;;
		float left 	= 4;

		if (RoomLists.Count < 3) {width = 0;
		} else {width = 200 * RoomLists.Count + 33 * (RoomLists.Count - 1) - width + 8;}

		Content.GetComponent<RectTransform> ().sizeDelta = new Vector2 (width, Content.GetComponent<RectTransform> ().sizeDelta.y);

		for(int i = 0; i < RoomLists.Count; i++){
			GameObject RoomInfo = (GameObject)Instantiate(PrefabRoomInfo);

			RoomInfo.name = RoomLists [i].RoomId.ToString();
			RoomInfo.transform.Find ("Name").GetComponent<Text> ().text = RoomLists [i].RoomName;
			RoomInfo.transform.Find ("PHand").GetComponent<Text> ().text = (RoomLists [i].PlayerHands + 1).ToString();
			RoomInfo.transform.Find ("Hands").GetComponent<Text> ().text = RoomLists [i].Hands.ToString();

			for(int o = 0; o < RoomLists[i].Players.Count; o++){
				UICircle avatar = (UICircle)Instantiate(PrefabAvatar);
				avatar.transform.SetParent (RoomInfo.transform.Find ("Player" + o + "/Avatar"));
				avatar.transform.localPosition = new Vector3 ();
				avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (56, 56);
				StartCoroutine(Common.Load(avatar, RoomLists[i].Players[o].Avatar));
			}

			EventTriggerListener.Get(RoomInfo).onClick = onCloseRoomHandler;
			RoomInfo.transform.SetParent (Content);
			RoomInfo.transform.localScale = new Vector3 (1, 1, 1);
			RoomInfo.transform.localPosition = new Vector3 (left, 0, 0);
			left += 200 + 33;
		}

		if(RoomLists.Count > 0){
			RoomListListenServer (true, RoomIDs);
		}
	}

	public void ComingSoon(){
		GameObject Dialog = (GameObject)Instantiate(PrefabDialog);
		Dialog.transform.Find("Title").GetComponent<Text>().text = "Unlipoker";
		Dialog.transform.Find("Conten").GetComponent<Text>().text = "Coming soon...";
		Dialog.transform.Find ("Yes").gameObject.SetActive (false);
		Dialog.transform.Find ("No").gameObject.SetActive (false);
		Dialog.transform.SetParent (GameObject.Find ("Canvas").transform);
		Dialog.transform.localPosition = new Vector3 (0,0,0);
		Dialog.gameObject.SetActive (true);
		EventTriggerListener.Get(Dialog.transform.Find ("Close").gameObject).onClick = CloseDialog;
	}

	public void CloseDialog(GameObject obj){
		Destroy (obj.transform.parent.gameObject);
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
		if(data == null){
			return;
		}

		switch (data.Msgid) {

		case MessageID.ListRoomsRsp:
			if(data.ListRoomsRsp.Ret == 0){
				Loom.QueueOnMainThread(()=>{
					RoomIDs.Clear();
					RoomLists.Clear();
					foreach(ListRoomItem room in data.ListRoomsRsp.Rooms){
						RoomInfo r = new RoomInfo();
						r.RoomId = room.RoomId;
						r.RoomName = room.RoomName;
						r.RoomNumber = room.RoomNumber;
						r.Hands = room.Hands;
						r.PlayerHands = room.PlayedHands;

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
			if(data.CloseRoomRsp.Ret == 0){
				UpdateRoomClose (data.CloseRoomRsp.RoomId);
			}
			break;

		case MessageID.ListRoomsListenRsp:
			if(data.ListRoomsListenRsp.Ret == 0){
			}
			break;

		case MessageID.CreateRoomRsp:
			if(data.CreateRoomRsp.Ret == 0){
				Common.CRoom_id 	= data.CreateRoomRsp.RoomId;
				Common.CRoom_number	= data.CreateRoomRsp.RoomNumber;
				Loom.QueueOnMainThread(()=>{
					JoinRoomServer(Common.CRoom_number);
				}); 
			}
			break;


		case MessageID.JoinRoomRsp:
			if (data.JoinRoomRsp.Ret == 0) {
				Common.CRoom_id 	= data.JoinRoomRsp.Room.RoomId;
				Common.CRoom_number = data.JoinRoomRsp.Room.Number;
				Common.CRoom_name	= data.JoinRoomRsp.Room.Name;
				Common.CMin_bet 	= data.JoinRoomRsp.Room.MinBet;
				Common.CMax_bet 	= data.JoinRoomRsp.Room.MaxBet;
				Common.CHands 		= data.JoinRoomRsp.Room.Hands;
				Common.CPlayed_hands= data.JoinRoomRsp.Room.PlayedHands;
				Common.CIs_share 	= data.JoinRoomRsp.Room.IsShare;
				Common.CCredit_points = data.JoinRoomRsp.Room.CreditPoints;
				Common.CState 		= data.JoinRoomRsp.Room.State;

				if(Common.CState == Msg.GameState.Bet){
					Common.ConfigBetTime = (int)data.JoinRoomRsp.Room.Countdown / 1000;
				}
				else if(Common.CState == Msg.GameState.Combine){
					Common.ConfigSortTime = (int)data.JoinRoomRsp.Room.Countdown / 1000;
				}
				else if(Common.CState == Msg.GameState.Result){
					Common.ConfigFinishTime = (int)data.JoinRoomRsp.Room.Countdown / 1000;
				}
				else if(Common.CState == Msg.GameState.Deal){
					Common.CPokers.Clear ();
					for(int i = 0; i < data.JoinRoomRsp.Room.Cards.Count; i++){
						Common.CPokers.Add ( (int)data.JoinRoomRsp.Room.Cards[i] );
					}
				}
					
				Common.CPlayers.Clear ();
				Common.CPokers.Clear ();
				for(int i = 0; i < data.JoinRoomRsp.Room.Players.Count; i++){
					PlayerInfo p = new PlayerInfo ();
					p.Uid 		= data.JoinRoomRsp.Room.Players[i].Uid;
					p.SeatID 	= data.JoinRoomRsp.Room.Players[i].SeatId;
					p.Name 		= data.JoinRoomRsp.Room.Players[i].Name;
					p.Bet 		= data.JoinRoomRsp.Room.Players[i].Bet;
					p.FB_avatar = data.JoinRoomRsp.Room.Players[i].Avatar;
					p.Score		= data.JoinRoomRsp.Room.Players[i].Score;

					Debug.Log (p.Uid + "===" + p.SeatID + "===" +p.Name);
					Common.CPlayers.Add (p);
				}

				Loom.QueueOnMainThread(()=>{
					PlayGame();
				}); 
			}
			break;

		case MessageID.ConsumeDiamondsNotify:
			break;

		case MessageID.ListRoomsNotify:
			switch(data.ListRoomsNotify.Type){
			case Msg.ListRoomNotifyType.Round:
				Loom.QueueOnMainThread(()=>{
					UpdateRoomHand(data.ListRoomsNotify.RoomId, data.ListRoomsNotify.Round);
				}); 
				break;

			case Msg.ListRoomNotifyType.SitDown:
				Loom.QueueOnMainThread(()=>{
					UpdateRoomSitDown(data.ListRoomsNotify.RoomId, data.ListRoomsNotify.Player);
				}); 
				break;

			case Msg.ListRoomNotifyType.StandUp:
				Loom.QueueOnMainThread(()=>{
					UpdateRoomStandUp(data.ListRoomsNotify.RoomId, data.ListRoomsNotify.Player);
				}); 
				break;

			case Msg.ListRoomNotifyType.Close:
				Loom.QueueOnMainThread(()=>{
					UpdateRoomClose(data.ListRoomsNotify.RoomId);
				}); 
				break;
			}
			break;
		}
	}
		
	public void CreatRoomServer(string roomname, uint min_bet, uint max_bet, uint hands, uint credit_points, bool is_share){
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

	 public void JoinRoomServer(string room_number){
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
		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.CloseRoomReq;
		msg.CloseRoomReq 				= new CloseRoomReq();

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	public void RoomListListenServer(bool listen, List<uint> ids){
		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.ListRoomsListenReq;
		msg.ListRoomsListenReq 			= new ListRoomsListenReq();
		foreach(uint id in ids){
			msg.ListRoomsListenReq.RoomIds.Add (id);
		}

		using (var stream = new MemoryStream())
		{
			msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}


	//===================================facebook=================================
//	public void GetUserInfo(){
//		if(FB.IsLoggedIn){
//			FB.API("/me/picture?Type=square&height=128&width=128", HttpMethod.GET, this.ProfilePhotoCallback);
//		}
//	}
//
//	private void ProfilePhotoCallback(IGraphResult result){
//		if (string.IsNullOrEmpty(result.Error) && result.Texture != null){
//			Common.FB_avatar = Sprite.Create (result.Texture, new Rect (0, 0, 128, 128), new Vector2 (0.5f, 0.5f));
//			GameObject.Find ("Canvas").transform.Find ("Avatar").gameObject.GetComponent<Image> ().sprite = Common.FB_avatar;
//		}
//
//	}
}