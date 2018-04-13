using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpringFramework.UI;
using System;

using UnityEngine.UI;
using DG.Tweening;

using Msg;
using Google.Protobuf;
using Google.Protobuf.Collections;

public class Wol{
	public int Win;
	public int Lose;
	public double WinPro;
	public double LosePro;
}

public class CareerControl : MonoBehaviour {
	public LobbyController	LobbyController;
	public PieGraph			Pie7Day;
	public PieGraph			Pie30Day;
	public PieGraph			PieAllDay;
	public PageView 		PageView;

	public GameObject 		m_CareerRecord;
	public GameObject 		m_CareerPlayer;

	private	List<string>	m_months = new List<string>();
	private RepeatedField<CareerRoomRecord> CareerRooms =  new RepeatedField<CareerRoomRecord>();

	// Use this for initialization
	void Start () {
	}

	void Awake(){
		m_months.Add ("Jan");
		m_months.Add ("Feb");
		m_months.Add ("Mar");

		m_months.Add ("Apr");
		m_months.Add ("May");
		m_months.Add ("June");

		m_months.Add ("July");
		m_months.Add ("Aug");
		m_months.Add ("Sept");

		m_months.Add ("Oct");
		m_months.Add ("Nov");
		m_months.Add ("Dec");

//		for(int i = 0; i < 3; i++){
//			Msg.CareerRoomRecord room = new CareerRoomRecord ();
//			room.Name = "Ali's Room";
//			room.BeginTime = 1522393690;
//			room.EndTime = 1522393690;
//			room.PlayedHands = 13;
//			room.Hands = 20;
//			room.IsShare = false;
//			room.MinBet = 100;
//			room.MaxBet = 200;
//
//			for(int o = 0; o < 5; o++){
//				ScoreboardItem s = new ScoreboardItem ();
//				s.Avatar = "https://img5.duitang.com/uploads/item/201609/26/20160926124027_vxRkt.jpeg";
//				s.Name = "Walter";
//				s.Score = 200;
//				s.Uid = 222;
//				room.Items.Add (s);
//			}
//
//			CareerRooms.Add (room);
//		}
//
//		for(int i = 0; i < 3; i++){
//			Msg.CareerRoomRecord room = new CareerRoomRecord ();
//			room.Name = "Ali's Room";
//			room.BeginTime = 1541005261;
//			room.EndTime = 1541005261;
//			room.PlayedHands = 13;
//			room.Hands = 20;
//			room.IsShare = false;
//			room.MinBet = 100;
//			room.MaxBet = 200;
//
//			for(int o = 0; o < 5; o++){
//				ScoreboardItem s = new ScoreboardItem ();
//				s.Avatar = "https://img5.duitang.com/uploads/item/201609/26/20160926124027_vxRkt.jpeg";
//				s.Name = "Walter";
//				s.Score = 200;
//				s.Uid = 222;
//				room.Items.Add (s);
//			}
//
//			CareerRooms.Add (room);
//		}
//
//		UpdateCareerRecord ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void Enter(){
		this.gameObject.SetActive (true);
		CloseRoomInfo ();
		ClearCareerRecord ();

		Pie7Day.gameObject.SetActive (false);
		Pie30Day.gameObject.SetActive (false);
		PieAllDay.gameObject.SetActive (false);

		Pie7Day.transform.DORotate (new Vector3 (0, 0, 0), 0);
		Pie30Day.transform.DORotate (new Vector3 (0, 0, 0), 0);
		PieAllDay.transform.DORotate (new Vector3 (0, 0, 0), 0);

		Transform Content = transform.Find ("Pies/Scroll View/Viewport/Content");

		for (int i = Content.Find("PieScroll7/Win").childCount - 1; i >= 0; i--) {  
			Destroy(Content.Find("PieScroll7/Win").GetChild(i).gameObject);
		}

		for (int i = Content.Find("PieScroll7/Lost").childCount - 1; i >= 0; i--) {  
			Destroy(Content.Find("PieScroll7/Lost").GetChild(i).gameObject);
		} 

		for (int i = Content.Find("PieScroll30/Win").childCount - 1; i >= 0; i--) {  
			Destroy(Content.Find("PieScroll30/Win").GetChild(i).gameObject);
		} 

		for (int i = Content.Find("PieScroll30/Lost").childCount - 1; i >= 0; i--) {  
			Destroy(Content.Find("PieScroll30/Lost").GetChild(i).gameObject);
		} 

		for (int i = Content.Find("PieScrollAll/Win").childCount - 1; i >= 0; i--) {  
			Destroy(Content.Find("PieScrollAll/Win").GetChild(i).gameObject);
		} 

		for (int i = Content.Find("PieScrollAll/Lost").childCount - 1; i >= 0; i--) {  
			Destroy(Content.Find("PieScrollAll/Lost").GetChild(i).gameObject);
		} 

		List<uint> days = new List<uint> ();
		days.Add (7);
		days.Add (30);
		days.Add (365);

		LobbyController.CareerWinLostServer (days);
		LobbyController.CareerRecordsServer (Common.ConfigCareerDays);
	}

	public void Exit(){
		LobbyController.PlayerButtonEffect ();
		this.gameObject.SetActive (false);
	}

	public void UpdateRecordPie(RepeatedField<CareerWinLoseDataItem> winlost){
		Common.CareerWins.Clear ();

		for(int i = 0; i < winlost.Count; i++){
			uint Win = winlost [i].Win;
			uint Lose = winlost [i].Lose;

			if(winlost [i].Win == 0 && winlost [i].Lose == 0){
				Win = 50;Lose = 50;
			}
		
			double winper = Convert.ToDouble (Win) / Convert.ToDouble (Win + Lose);
			double loseper = Convert.ToDouble (Lose) / Convert.ToDouble (Win + Lose);

			int WinPro = Convert.ToInt32( Math.Round ((decimal)winper , 2, MidpointRounding.AwayFromZero) * 100 );
			int LosePro = Convert.ToInt32( Math.Round ((decimal)loseper , 2, MidpointRounding.AwayFromZero) * 100 );

			int Wintemp = (int)Win;
			int Losetemp = -(int)Lose;
			Common.CareerWins.Add (Wintemp + Losetemp);
			if (winlost [i].Win == 0 && winlost [i].Lose == 0) {
				UpdateRecordData (i, WinPro, LosePro, 0, 0);
			} else {
				UpdateRecordData (i, WinPro, LosePro, Win, Lose);
			}
		}

		PageView.pageTo (0);
	}

	public void UpdateRecordData(int pie, int win, int lost, uint wins, uint losts){
		Color LostColor = new Color (8.0f / 255, 42.0f / 255, 50.0f / 255);
		Color WinColor = new Color (27.0f / 255, 116.0f / 255, 126.0f / 255);
		Transform Content = transform.Find ("Pies/Scroll View/Viewport/Content");

		if(pie == 0){
			Pie7Day.gameObject.SetActive (true);
			Pie7Day.SetPie (lost, win, LostColor, WinColor);
		
			float rot = (win / 2) * 3.6f;
			Pie7Day.transform.DORotate (new Vector3 (0, 0, 180.0f + rot), 1.0f);


			if (wins > 0 || losts > 0) {
				UpdatePieTips (Content.Find ("PieScroll7/Win"), Content.Find ("PieScroll7/Lost"), win, lost);
			}
		}

		if(pie == 1){
			Pie30Day.gameObject.SetActive (true);
			Pie30Day.SetPie (lost, win, LostColor, WinColor);

			float rot = (win / 2) * 3.6f;
			Pie30Day.transform.DORotate (new Vector3 (0, 0, 180.0f + rot), 1.0f);

			if (wins > 0 || losts > 0) {
				UpdatePieTips (Content.Find("PieScroll30/Win"), Content.Find("PieScroll30/Lost"), win, lost);
			}
		}

		if(pie == 2){
			PieAllDay.gameObject.SetActive (true);
			PieAllDay.SetPie (lost, win, LostColor, WinColor);

			float rot = (win / 2) * 3.6f;
			PieAllDay.transform.DORotate (new Vector3 (0, 0, 180.0f + rot), 1.0f);

			if (wins > 0 || losts > 0) {
				UpdatePieTips (Content.Find ("PieScrollAll/Win"), Content.Find ("PieScrollAll/Lost"), win, lost);
			}
		}
	}

	public void UpdatePieTips(Transform winobj, Transform lostobj, int win, int lost){
		winobj.GetComponent<Text> ().text = "Win" + win.ToString () + "%";
		lostobj.GetComponent<Text> ().text = "Lost" + lost.ToString () + "%";
//		string winstr = win.ToString();
//		string loststr = lost.ToString();
//
//		float left = 0;
//
//		for (int c = loststr.Length - 1; c >= 0; c--) {  
//			GameObject t = new GameObject ();
//			t.AddComponent<Image> ();
//			t.GetComponent<Image>().sprite = Resources.Load ("Image/Lobby/lost" + loststr[c] , typeof(Sprite)) as Sprite;
//			t.transform.SetParent(lostobj);
//			t.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (22, 29);
//			t.transform.localPosition = new Vector3 (left,0,0);
//			t.transform.localScale = new Vector3 (1,1,1);
//			left -= 20;
//		}
//
//		GameObject licon = new GameObject ();
//		licon.AddComponent<Image> ();
//		licon.GetComponent<Image>().sprite = Resources.Load ("Image/Lobby/losticon", typeof(Sprite)) as Sprite;
//		licon.transform.SetParent(lostobj);
//		licon.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (22, 29);
//		licon.transform.localPosition = new Vector3 (left,0,0);
//		licon.transform.localScale = new Vector3 (1,1,1);
//
//		left = 0;
//		GameObject wicon = new GameObject ();
//		wicon.AddComponent<Image> ();
//		wicon.GetComponent<Image>().sprite = Resources.Load ("Image/Lobby/winicon", typeof(Sprite)) as Sprite;
//		wicon.transform.SetParent(winobj);
//		wicon.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (22, 29);
//		wicon.transform.localPosition = new Vector3 (left,0,0);
//		wicon.transform.localScale = new Vector3 (1,1,1);
//		left += 20;
//
//		for (int c = 0;  c < winstr.Length; c++) {  
//			GameObject t = new GameObject ();
//			t.AddComponent<Image> ();
//			t.GetComponent<Image>().sprite = Resources.Load ("Image/Lobby/win"+ winstr[c] , typeof(Sprite)) as Sprite;
//			t.transform.SetParent(winobj);
//			t.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (22, 29);
//			t.transform.localPosition = new Vector3 (left,0,0);
//			t.transform.localScale = new Vector3 (1,1,1);
//			left += 20;
//		}
	}

	public DateTime ConvertStringToDateTime(int timeStamp)
    {
		DateTime dateTimeStart = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);
		dateTimeStart = dateTimeStart.Add(new TimeSpan(0,0,timeStamp));
		return dateTimeStart;
    }

	public void SetCareerRecordData(RepeatedField<CareerRoomRecord> temp){
		CareerRooms.Clear ();
		CareerRooms = temp;
		UpdateCareerRecord ();
	}

	public void UpdateCareerRecord(){
		Transform Content = transform.Find ("GameList/List/Viewport/Content");

		float height = 140 * CareerRooms.Count + 12 * (CareerRooms.Count - 1);
		if(height < 640){height = 640;}

		Content.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Content.GetComponent<RectTransform> ().sizeDelta.x, height);
		Content.GetComponent<RectTransform> ().localPosition = new Vector3 (0, -height / 2, 0);

		float top = height / 2;
		int month = 0;
		int day = 0;

		foreach(CareerRoomRecord room in CareerRooms){
			GameObject Drecord = Instantiate (m_CareerRecord) as GameObject;

			DateTime time = ConvertStringToDateTime ((int)room.BeginTime);
			if (time.Month != month || time.Day != day) {

				month 	= time.Month;
				day 	= time.Day;

				Drecord.transform.Find ("Month").GetComponent<Text> ().text = m_months [month -1];
				Drecord.transform.Find ("Day").GetComponent<Text> ().text = day.ToString();
			}

			string hour = time.Hour >= 10 ? time.Hour.ToString() : "0" + time.Hour.ToString();
			string min =  time.Minute >= 10 ? time.Minute.ToString() : "0" + time.Minute.ToString();

			Drecord.transform.Find ("RoomName").GetComponent<Text> ().text = hour.ToString () + " : " + min.ToString () + " " + room.Name;
			if(room.Items.Count > 4){Drecord.transform.Find ("More").gameObject.SetActive (true);}

			int score = 0;
			string typestr = "";

			for(int i = 0; i < room.Items.Count; i++){
				if(room.Items[i].Uid == Common.Uid){score = room.Items[i].Score;}

				if(i < 4){
					Drecord.transform.Find ("Players/Player" + i.ToString ()).gameObject.SetActive (true);
					Drecord.transform.Find ("Players/Player" + i.ToString() + "/Name").GetComponent<Text> ().text = room.Items[i].Name;

					UICircle avatar = (UICircle)Instantiate(LobbyController.PrefabAvatar);
					avatar.transform.SetParent (Drecord.transform.Find ("Players/Player" + i.ToString() + "/Avatar"));
					avatar.transform.localPosition = new Vector3 ();
					avatar.transform.localScale = new Vector3 (1,1,1);
					avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (52, 52);
					StartCoroutine(Common.Load(avatar, room.Items[i].Avatar));
				}
			}

			if (score >= 0) {typestr = "win";} else {typestr = "lost";}
			string amount = Mathf.Abs (score).ToString ();
			float left = 0;

			GameObject icon = new GameObject ();
			icon.AddComponent<Image> ();
			icon.GetComponent<Image>().sprite = Resources.Load ("Image/Lobby/"+ typestr + "icon" , typeof(Sprite)) as Sprite;
			icon.transform.SetParent(Drecord.transform.Find ("Score"));
			icon.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (22, 29);
			icon.transform.localPosition = new Vector3 (left,0,0);

			left += 20;
			for (int c = 0;  c < amount.Length; c++) {  
				GameObject t = new GameObject ();
				t.AddComponent<Image> ();
				t.GetComponent<Image>().sprite = Resources.Load ("Image/Lobby/"+ typestr + amount[c] , typeof(Sprite)) as Sprite;
				t.transform.SetParent(Drecord.transform.Find ("Score"));
				t.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (22, 29);
				t.transform.localPosition = new Vector3 (left,0,0);
				left += 20;
			}

			EventTriggerListener.Get(Drecord).onClick = onClickButtonHandler;
			Drecord.name = room.RoomId.ToString();
			Drecord.transform.SetParent (Content);
			Drecord.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
			Drecord.GetComponent<RectTransform> ().localPosition = new Vector3 (0, top, 0);

			top -= 140 + 12;
		}
	}

	public void ClearCareerRecord(){
		Transform Content = transform.Find ("GameList/List/Viewport/Content");
		for (int i = Content.childCount - 1; i >= 0; i--) {  
			Destroy(Content.GetChild(i).gameObject);
		} 
	}

	public void onClickButtonHandler(GameObject obj){
		CareerRoomRecord temp = new CareerRoomRecord ();
		foreach(CareerRoomRecord room in CareerRooms){
			if(obj.name == room.RoomId.ToString()){
				temp = room;
			}
		}

		OpenRoomInfo(temp);
	}

	public void OpenRoomInfo(CareerRoomRecord room){
		Transform RoomInfo = transform.Find ("RoomInfo");

		RoomInfo.gameObject.SetActive (true);
		RoomInfo.Find ("Top/Title").GetComponent<Text> ().text = room.Name;
		RoomInfo.Find ("Time").GetComponent<Text> ().text = ConvertStringToDateTime ((int)room.BeginTime).ToString() + " ~ "+ConvertStringToDateTime ((int)room.EndTime).ToString();
		RoomInfo.Find ("Totalhands/Text").GetComponent<Text> ().text = room.PlayedHands.ToString() + "/" + room.Hands.ToString();
		if(room.Items.Count > 0){RoomInfo.Find ("MVP/Text").GetComponent<Text> ().text = room.Items [0].Name;}
		RoomInfo.Find ("Roomfee/Text").GetComponent<Text> ().text = room.IsShare?"Shared":"Individual";
		RoomInfo.Find ("Betsize/Text").GetComponent<Text> ().text = room.MinBet.ToString() + "-" + room.MaxBet.ToString();

		Transform Content = RoomInfo.Find ("PlayerList/List/Viewport/Content");

		float top = 0;
		float height = 105 * room.Items.Count;
		if(height < 860){height = 860;}

		Content.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Content.GetComponent<RectTransform> ().sizeDelta.x, height);
		Content.GetComponent<RectTransform> ().localPosition = new Vector3 (0, -height / 2, 0);

		top = height / 2;

		for(int i = 0; i < room.Items.Count; i++){
			GameObject CareerPlayer = Instantiate (m_CareerPlayer) as GameObject;

			CareerPlayer.transform.Find ("Name").GetComponent<Text> ().text = room.Items[i].Name;
			if(i < 3){
				CareerPlayer.transform.Find ("ST").gameObject.SetActive (true);
				CareerPlayer.transform.Find ("ST").GetComponent<Image>().sprite = Resources.Load("Image/Lobby/" + i + "st", typeof(Sprite)) as Sprite;
			}

			if(room.Items[i].Uid == Common.Uid){
				CareerPlayer.transform.Find ("Mine").gameObject.SetActive (true);
			}

			UICircle avatar = (UICircle)Instantiate(LobbyController.PrefabAvatar);
			avatar.transform.SetParent (CareerPlayer.transform.Find ("Avatar"));
			avatar.transform.localPosition = new Vector3 ();
			avatar.transform.localScale = new Vector3 (1,1,1);
			avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (68, 68);
			StartCoroutine(Common.Load(avatar, room.Items[i].Avatar));

			string typestr = "";
			if (room.Items[i].Score >= 0) {typestr = "win";} else {typestr = "lost";}
			string amount = Mathf.Abs (room.Items[i].Score).ToString ();
			float left = 0;

			for (int c = amount.Length - 1; c >= 0; c--) {  
				GameObject t = new GameObject ();
				t.AddComponent<Image> ();
				t.GetComponent<Image>().sprite = Resources.Load ("Image/Lobby/"+ typestr + amount[c] , typeof(Sprite)) as Sprite;
				t.transform.SetParent(CareerPlayer.transform.Find ("Amount"));
				t.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (22, 29);
				t.transform.localPosition = new Vector3 (left,0,0);
				left = left - 20;

				if(c == 0){
					GameObject icon = new GameObject ();
					icon.AddComponent<Image> ();
					icon.GetComponent<Image>().sprite = Resources.Load ("Image/Lobby/"+ typestr + "icon" , typeof(Sprite)) as Sprite;
					icon.transform.SetParent(CareerPlayer.transform.Find ("Amount"));
					icon.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (22, 29);
					icon.transform.localPosition = new Vector3 (left,0,0);
				}
			}

			CareerPlayer.transform.SetParent (Content);
			CareerPlayer.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
			CareerPlayer.GetComponent<RectTransform> ().localPosition = new Vector3 (0, top, 0);
			top -= 105;
		}
	}

	public void CloseRoomInfo(){
		ClearRoomInfo ();
		transform.Find("RoomInfo").gameObject.SetActive (false);
	}

	public void ClearRoomInfo(){
		Transform RoomInfo = transform.Find ("RoomInfo");
		RoomInfo.Find ("Time").GetComponent<Text> ().text = "";
		RoomInfo.Find ("Totalhands/Text").GetComponent<Text> ().text = "";
		RoomInfo.Find ("MVP/Text").GetComponent<Text> ().text = "";
		RoomInfo.Find ("Roomfee/Text").GetComponent<Text> ().text = "";
		RoomInfo.Find ("Betsize/Text").GetComponent<Text> ().text = "";

		Transform Content = RoomInfo.Find ("PlayerList/List/Viewport/Content");
		for (int i = Content.childCount - 1; i >= 0; i--) {  
			Destroy(Content.GetChild(i).gameObject);
		} 
	}
}
