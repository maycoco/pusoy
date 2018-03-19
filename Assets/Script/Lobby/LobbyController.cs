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

public class RoomInfo{
	public int 			RoomId;
	public int 			TotalRound;
	public int 			CurRound;
	public string		Name;
	public List<int> 	Players;
}

public class LobbyController : MonoBehaviour {
	//Prefab
	public GameObject 				PrefabRoomInfo;
	public GameObject 				PrefabDialog;

	//Controller
	public CreateRoomControl 		CreateRoomControl;
	public JoinRoomControl 			JoinRoomControl;

	//Layer
	public CareerControl 			CareerControl;
	public ConterControl 			ConterControl;
	public PrefileControl		 	PrefileControl;
	public SettingControl		 	SettingControl;

	//Data
	private List<RoomInfo> 			RoomInfos = new List<RoomInfo>();

	// Use this for initialization
	void Start () {
	}

	void Awake(){
		InitCallbackForNet ();
		GetUserInfo ();
		//GetRoomInfo ();
		UpdateRoomsInfo ();
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
		SceneManager.LoadScene (2);
	}


	//===================================Room list=================================
	public void GetRoomInfo(){
		for(int i = 0; i < 27; i++){
			RoomInfo a 		= new RoomInfo ();
			a.CurRound 		= 10;
			a.TotalRound 	= 20;
			a.Name 			= "Room"+i;
			a.RoomId 		= 123;
			a.Players 		= new List<int> ();
			RoomInfos.Add (a);
		}
	}

	public void UpdateRoomsInfo(){
		Transform Content = GameObject.Find ("Canvas").transform.Find("RoomList/Viewport/Content");

		float width = GameObject.Find ("Canvas").transform.Find ("RoomList").GetComponent<RectTransform> ().sizeDelta.x;;
		float left 	= 4;

		if (RoomInfos.Count < 3) {
			width = 0;
		} else {
			width = 200 * RoomInfos.Count + 33 * (RoomInfos.Count - 1) - width + 8;
		}

		Content.GetComponent<RectTransform> ().sizeDelta = new Vector2 (width, Content.GetComponent<RectTransform> ().sizeDelta.y);

		for(int i = 0; i < RoomInfos.Count; i++){
			GameObject RoomInfo = (GameObject)Instantiate(PrefabRoomInfo);

			RoomInfo.transform.Find ("Name").GetComponent<Text> ().text = RoomInfos [i].Name;
			RoomInfo.transform.Find ("Hand").GetComponent<Text> ().text = RoomInfos [i].CurRound + "/" + RoomInfos [i].TotalRound;

			RoomInfo.transform.SetParent (Content);
			RoomInfo.transform.localScale = new Vector3 (1, 1, 1);
			RoomInfo.transform.localPosition = new Vector3 (left, 0, 0);
			left += 200 + 33;
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
					
				Common.CPokers.Clear ();
				for(int i = 0; i < data.JoinRoomRsp.Room.Players.Count; i++){
					PlayerInfo p = new PlayerInfo ();
					p.Uid = data.JoinRoomRsp.Room.Players[i].Uid;
					p.SeatID = data.JoinRoomRsp.Room.Players[i].SeatId;
					p.Name = data.JoinRoomRsp.Room.Players[i].Name;
					p.Bet = data.JoinRoomRsp.Room.Players[i].Bet;

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


	//===================================facebook=================================
	public void GetUserInfo(){
		if(FB.IsLoggedIn){
			FB.API("/me/picture?Type=square&height=128&width=128", HttpMethod.GET, this.ProfilePhotoCallback);
		}
	}

	private void ProfilePhotoCallback(IGraphResult result){
		if (string.IsNullOrEmpty(result.Error) && result.Texture != null){
			Common.FB_avatar = Sprite.Create (result.Texture, new Rect (0, 0, 128, 128), new Vector2 (0.5f, 0.5f));
			GameObject.Find ("Canvas").transform.Find ("Avatar").gameObject.GetComponent<Image> ().sprite = Common.FB_avatar;
		}

	}
}