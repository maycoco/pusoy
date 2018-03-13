using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Facebook.Unity;

using networkengine;
using ProtoBuf;
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
		if(RoomInfos.Count <= 0){
			GameObject.Find ("Canvas").transform.Find ("RoomList/Notable").gameObject.SetActive (true);
			return;
		}

		GameObject.Find ("Canvas").transform.Find ("RoomList/Notable").gameObject.SetActive (false);
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
		Debug.Log(data.Msgid.ToString());
		switch (data.Msgid) {
		case MessageID.CreateRoomRsp:
			Debug.Log(data.createRoomRsp.Ret.ToString());
			if(data.createRoomRsp.Ret == 0){
				Common.CRoom_id 	= data.createRoomRsp.RoomId;
				Common.CRoom_number	= data.createRoomRsp.RoomNumber;
				Loom.QueueOnMainThread(()=>{
					JoinRoomServer(Common.CRoom_number);
				}); 
			}
			break;


		case MessageID.JoinRoomRsp:
			if (data.joinRoomRsp.Ret == 0) {
				Common.CRoom_id 	= data.joinRoomRsp.Room.RoomId;
				Common.CRoom_number = data.joinRoomRsp.Room.Number;
				Common.CRoom_name	= data.joinRoomRsp.Room.Name;
				Common.CMin_bet 	= data.joinRoomRsp.Room.MinBet;
				Common.CMax_bet 	= data.joinRoomRsp.Room.MaxBet;
				Common.CHands 		= data.joinRoomRsp.Room.Hands;
				Common.CPlayed_hands= data.joinRoomRsp.Room.PlayedHands;
				Common.CIs_share 	= data.joinRoomRsp.Room.IsShare;
				Common.CCredit_points = data.joinRoomRsp.Room.CreditPoints;
				Common.CState 		= data.joinRoomRsp.Room.State;

				if(Common.CState == Msg.GameState.Bet){
					Common.ConfigBetTime = data.joinRoomRsp.Room.Countdown;
				}
				else if(Common.CState == Msg.GameState.Combine){
					Common.ConfigSortTime = data.joinRoomRsp.Room.Countdown;
				}
				else if(Common.CState == Msg.GameState.Result){
					Common.ConfigFinishTime = data.joinRoomRsp.Room.Countdown;
				}
				else if(Common.CState == Msg.GameState.Deal){
					
				}

				for(int i = 0; i < data.joinRoomRsp.Room.Players.Count; i++){
					PlayerInfo p = new PlayerInfo ();
					p.Uid = data.joinRoomRsp.Room.Players[i].Uid;
					p.SeatID = data.joinRoomRsp.Room.Players[i].SeatId;
					p.Name = data.joinRoomRsp.Room.Players[i].Name;
					p.Bet = data.joinRoomRsp.Room.Players[i].Bet;
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
		msg.createRoomReq 				= new CreateRoomReq();
		msg.createRoomReq.Name			= roomname;
		msg.createRoomReq.MinBet		= min_bet;
		msg.createRoomReq.MaxBet		= max_bet;
		msg.createRoomReq.Hands			= hands;
		msg.createRoomReq.CreditPoints	= credit_points;
		msg.createRoomReq.IsShare		= is_share;

		using (var stream = new MemoryStream())
		{
			Serializer.Serialize<Protocol>(stream, msg);
			Client.Instance.Send(stream.ToArray());
		}
	}

	 public void JoinRoomServer(string room_number){
		Protocol msg 					= new Protocol();
		msg.Msgid 						= MessageID.JoinRoomReq;
		msg.joinRoomReq 				= new JoinRoomReq();
		msg.joinRoomReq.RoomNumber		= room_number;

		using (var stream = new MemoryStream())
		{
			Serializer.Serialize<Protocol>(stream, msg);
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