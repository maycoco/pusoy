using System.Collections;
using System.Collections.Generic;

using Msg;
using Google.Protobuf.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Internal;

using DG.Tweening;
using cn.sharesdk.unity3d;

public class GameConsole : MonoBehaviour
{
	private bool				MenuOpen 	= false;
	private uint 				RoundCount	= 0;
	private Sequence		 	m_Sequence;

	public 	GameController 		m_GameController;
	public  GameObject 			m_ConsoleMenu;
	public  GameObject			m_ConsoleMenu_Mask;
	public  GameObject 			m_WaitingSort;
	public  List<GameObject> 	m_Pokers;

	public Image 				SoundIcon;
	private	ShareSDK			ssdk;

	void Start (){
		HideWaitAnime();
	}

	void Update (){
	}

	void Awake(){
		AdjustUI ();
	}

	void AdjustUI(){
		EventTriggerListener.Get(m_ConsoleMenu_Mask).onClick = onClickButtonHandler;
		if (Common.ConfigMusicOn) {
			SoundIcon.sprite = Resources.Load ("Image/Game/menu_sound_open", typeof(Sprite)) as Sprite;
		} else {
			SoundIcon.sprite = Resources.Load ("Image/Game/menu_sound_close", typeof(Sprite)) as Sprite;
		}
	}

	public void MenuBTConsole(){
		m_GameController.PlayEffect (Effect.BUTTON);

		if (!MenuOpen) {
			OpenMenu ();
		} else {
			CloseMenu ();
		}
	}

	public void onClickButtonHandler(GameObject obj){
		if(MenuOpen){
			CloseMenu ();
		}
	}

	public void OpenMenu(){
		if(!MenuOpen){
			m_ConsoleMenu.transform.Find("Menu").gameObject.SetActive(true);
			m_ConsoleMenu.GetComponent<Animation>().Play("GameConsole_Menu_open");
			m_ConsoleMenu_Mask.SetActive (true);
			MenuOpen = true;
		}
	}
		
	public void CloseMenu(){
		if(MenuOpen){
			m_ConsoleMenu.transform.Find("Menu").gameObject.SetActive(true);
			m_ConsoleMenu.GetComponent<Animation>().Play("GameConsole_Menu_close");
			m_ConsoleMenu_Mask.SetActive (false);
			MenuOpen = false;
		}
	}

	public void StandUp(){
		m_GameController.PlayEffect (Effect.BUTTON);
		MenuBTConsole ();
		m_GameController.StandUpServer ();
	}

	public void OnSound(){
		if (Common.ConfigMusicOn) {
			Common.SetMusicConfig (false);
			SoundIcon.sprite = Resources.Load ("Image/Game/menu_sound_close", typeof(Sprite)) as Sprite;
			m_GameController.Music.Pause ();

		} else {
			Common.SetMusicConfig (true);
			SoundIcon.sprite = Resources.Load ("Image/Game/menu_sound_open", typeof(Sprite)) as Sprite;
			m_GameController.Music.Play ();
		}
	}
		
	///==============================
	///============================== Player list
	///==============================
	public void PlayerListReq(){
		m_GameController.m_PlayerListMode = 0;
		m_GameController.ScoreboardServer ();
	}

	public void ShowPlayerList(List<Msg.ScoreboardItem> slist, int mode){
		CLearPlayerList ();
		ClearObList ();

		Transform layer  = GameObject.Find("Canvas").transform.Find("PlayersListLayer");

		if (mode == 0) {
			layer.Find ("Close").gameObject.SetActive (true);
			layer.Find ("Observer").gameObject.SetActive (true);
			layer.Find ("OK").gameObject.SetActive (false);
		} else {
			layer.Find ("Close").gameObject.SetActive (false);
			layer.Find ("Observer").gameObject.SetActive (false);
			layer.Find ("OK").gameObject.SetActive (true);
		}

		layer.gameObject.SetActive (true);

		//string BeginTime	= "2018-2-27 11:11";
		//string EndTime 		= "Now";
		string Hands 		= (Common.CPlayed_hands+1) + " / " + Common.CHands;

		string Mvp 			= "";
		if(slist.Count > 0){Mvp = slist [0].Name;}

		string Roomfee 		= "";
		if(Common.CIs_share){Roomfee = "Shared";}{Roomfee = "Individual";}

		string Betsize 		= Common.ToCarryNum((int)Common.CMin_bet) + "-" + Common.ToCarryNum((int)Common.CMax_bet);
		//layer.Find ("Date/Time").GetComponent<Text> ().text = BeginTime + " ~ " + EndTime;
		layer.Find ("Totalhands/Text").GetComponent<Text> ().text = Hands;
		layer.Find ("MVP/Text").GetComponent<Text> ().text = Mvp;
		layer.Find ("Roomfee/Text").GetComponent<Text> ().text = Roomfee;
		layer.Find ("Betsize/Text").GetComponent<Text> ().text = Betsize;

		if(slist.Count > 4){
			layer.Find ("TablePlayers/Viewport/Content").GetComponent<RectTransform> ().sizeDelta = new Vector2 (0, 100 * slist.Count);
			float height = layer.Find ("TablePlayers/Viewport/Content").GetComponent<RectTransform> ().sizeDelta.y;
			float left = layer.Find ("TablePlayers/Viewport/Content").GetComponent<RectTransform> ().localPosition.x;
			layer.Find ("TablePlayers/Viewport/Content").GetComponent<RectTransform> ().localPosition = new Vector3 (left, -height / 2, 0);
		}

		float y = (layer.Find ("TablePlayers/Viewport/Content").GetComponent<RectTransform> ().sizeDelta.y / 2) - 3;


		//player list
		for(int i = 0; i < slist.Count; i++){
			
			GameObject pinfo = (GameObject)Instantiate(m_GameController.m_PrefabTPlayer);  
			pinfo.transform.SetParent (layer.Find("TablePlayers/Viewport/Content"));
			pinfo.transform.localPosition = new Vector3 (0, y ,0);
			pinfo.transform.localScale = new Vector3 (1,1,1);

			pinfo.transform.Find ("Name").GetComponent<Text> ().text = slist [i].Name; 

			UICircle avatar = (UICircle)Instantiate(m_GameController.m_PrefabAvatar);
			avatar.transform.SetParent (pinfo.transform.Find ("Avatar"));
			avatar.transform.localPosition = new Vector3 ();
			avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (65, 65);

			if (string.IsNullOrEmpty (slist [i].Avatar)) {avatar.UseDefAvatar ();
			} else {StartCoroutine(Common.Load(avatar, slist [i].Avatar));}

			string typestr = "";
			if (slist [i].Score < 0) {typestr = "lost";}
			else {typestr = "win";}

			float left = 0;
			GameObject icon = new GameObject ();
			icon.AddComponent<Image> ();
			icon.GetComponent<Image>().sprite = Resources.Load ("Image/Game/"+ typestr + "icon" , typeof(Sprite)) as Sprite;
			icon.transform.SetParent(pinfo.transform.Find ("Amount"));
			icon.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (20, 24);
			icon.transform.localPosition = new Vector3 (left,0,0);
			left = left + 18;

			string amount = Mathf.Abs (slist [i].Score).ToString ();
			for(int c = 0; c < amount.Length; c++){
				GameObject t = new GameObject ();
				t.AddComponent<Image> ();
				t.GetComponent<Image>().sprite = Resources.Load ("Image/Game/"+ typestr + amount[c] , typeof(Sprite)) as Sprite;
				t.transform.SetParent(pinfo.transform.Find ("Amount"));
				t.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (20, 24);
				t.transform.localPosition = new Vector3 (left,0,0);
				left = left + 18;
			}

			y -= 100;
		}

		//Ob list
		if(mode == 0){
			List<PlayerInfo> oblist = new List<PlayerInfo>();
			foreach( PlayerInfo p in Common.CPlayers) {
				if(p.SeatID == -1){
					oblist.Add (p);
				}
			}

			Transform ObContent = layer.Find ("Observer/ObList/Viewport/Content");
			Vector3 oblistsize = layer.Find ("Observer/ObList").GetComponent<RectTransform> ().sizeDelta;
			if(oblist.Count > 5){
				ObContent.GetComponent<RectTransform> ().sizeDelta = new Vector2 (95 * oblist.Count - oblistsize.x, oblistsize.y);
				ObContent.GetComponent<RectTransform> ().localPosition = new Vector3 (0, 0, 0);
			}

			float obleft = 37;
			foreach(PlayerInfo p in oblist){
				GameObject obinfo = (GameObject)Instantiate(m_GameController.m_PrefabObInfo);  
				obinfo.transform.SetParent (ObContent);
				obinfo.transform.localPosition = new Vector3 (obleft, 0 ,0);
				obinfo.transform.localScale = new Vector3 (1,1,1);

				UICircle avatar = (UICircle)Instantiate(m_GameController.m_PrefabAvatar);
				avatar.transform.SetParent (obinfo.transform.Find ("Avatar"));
				avatar.transform.localPosition = new Vector3 ();
				avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (68, 68);

				if (string.IsNullOrEmpty (p.FB_avatar)) {avatar.UseDefAvatar ();
				} else {StartCoroutine(Common.Load(avatar, p.FB_avatar));}

				obleft += (70 + 24);
			}
		}
	}
		
	public void LeaveRoom(){
		m_GameController.PlayEffect (Effect.BUTTON);
		m_GameController.LeaveRoomServer ();
	}

	public void HidePlayerList(){
		Transform layer  = GameObject.Find("Canvas").transform.Find("PlayersListLayer");
		layer.gameObject.SetActive (false);
	}

	public void ClearObList(){
		Transform layer  = GameObject.Find("Canvas").transform.Find("PlayersListLayer");
		for (int i = layer.Find("Observer/ObList/Viewport/Content").childCount - 1; i >= 0; i--) {  
			Destroy (layer.Find("Observer/ObList/Viewport/Content").GetChild(i).gameObject);
		}  

		layer.Find ("Observer/ObList/Viewport/Content").GetComponent<RectTransform> ().sizeDelta = new Vector2 (0, layer.Find ("Observer/ObList").GetComponent<RectTransform> ().sizeDelta.y);
	}

	public void CLearPlayerList(){
		Transform layer  = GameObject.Find("Canvas").transform.Find("PlayersListLayer");

		layer.Find ("Date/Time").GetComponent<Text> ().text = "";
		layer.Find ("Totalhands/Text").GetComponent<Text> ().text = "";
		layer.Find ("MVP/Text").GetComponent<Text> ().text = "";
		layer.Find ("Roomfee/Text").GetComponent<Text> ().text = "";
		layer.Find ("Betsize/Text").GetComponent<Text> ().text = "";

		for (int i = layer.Find("TablePlayers/Viewport/Content").childCount - 1; i >= 0; i--) {  
			Destroy (layer.Find("TablePlayers/Viewport/Content").GetChild(i).gameObject);
		}  

		layer.Find ("TablePlayers/Viewport/Content").GetComponent<RectTransform> ().sizeDelta = new Vector2 (0, layer.Find ("TablePlayers").GetComponent<RectTransform> ().sizeDelta.y);
	}


	///==============================
	///============================== HandReview
	///==============================

	public void OpenHandReview(){
		m_GameController.PlayEffect (Effect.BUTTON);

		if(Common.CPlayed_hands > 0){
			RoundCount = Common.CPlayed_hands - 1;
			m_GameController.GetRoundHistoryServer (RoundCount);
		}
	}

	public void CloseHandReview(){
		ClearHandReview ();
		Transform Layer  = GameObject.Find("Canvas").transform.Find("HandReview");
		Layer.gameObject.SetActive (false);
	}

	public void LastRoundReview(){
		m_GameController.PlayEffect (Effect.BUTTON);
		if(RoundCount > 0){
			RoundCount--;
			m_GameController.GetRoundHistoryServer (RoundCount);
		}
	}

	public void NextRoundReview(){
		m_GameController.PlayEffect (Effect.BUTTON);
		if(RoundCount < Common.CPlayed_hands){
			RoundCount++;
			m_GameController.GetRoundHistoryServer (RoundCount);
		}
	}

	public void ShowHandReview(Dictionary<int,  CSeatResult> SeatResults){
		UpdateRoundCount ();
		ClearHandReview ();

		Transform Layer  = GameObject.Find("Canvas").transform.Find("HandReview");
		Layer.gameObject.SetActive (true);

		List<Vector3> poslist = new List<Vector3> ();
		poslist.Add (new Vector3(0,286,0));
		poslist.Add (new Vector3(0,54,0));
		poslist.Add (new Vector3(0,-165,0));
		poslist.Add (new Vector3(0,-386,0));

		List<int> Seats = new List<int> ();
		foreach (KeyValuePair<int, CSeatResult> pair in SeatResults) {
			Seats.Add (pair.Key);
		}
		Seats.Sort ();

		int index = 0;
		foreach (int seatid in Seats){
			CSeatResult hInfo = SeatResults[seatid];
		
			GameObject PreInfoObj = (GameObject)Instantiate(m_GameController.m_PrefabPreInfo);
			PreInfoObj.transform.SetParent (Layer.Find ("PreInfoCom"));
			PreInfoObj.transform.localPosition = poslist [index];
			PreInfoObj.transform.localScale = new Vector3 (1, 1, 1);

			UICircle avatar = (UICircle)Instantiate(m_GameController.m_PrefabAvatar);
			avatar.transform.SetParent (PreInfoObj.transform.Find ("Avatar"));
			avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 ();
			avatar.transform.localPosition = new Vector3 ();
			avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (60, 60);

			if (string.IsNullOrEmpty(hInfo.Avatar)) {avatar.UseDefAvatar ();}
			else {StartCoroutine(Common.Load(avatar, hInfo.Avatar));}

			if (seatid == 0) {
				PreInfoObj.transform.Find ("BBorder").gameObject.SetActive (true);
				PreInfoObj.transform.Find ("BName").GetComponent<Text> ().text = hInfo.Name;
			} else {
				PreInfoObj.transform.Find ("PBorder").gameObject.SetActive (true);
				PreInfoObj.transform.Find ("PName").GetComponent<Text> ().text = hInfo.Name;
			}


			for(int i = 0; i < hInfo.Pres.Count; i++){
				RepeatedField<uint> pInfo = hInfo.Pres [i];

				for(int o = 0; o < pInfo.Count; o++){
					Transform Poker = PreInfoObj.transform.Find ("Hand" + i + "/Poker" + o);
					Image image = Poker.GetComponent<Image>();
					image.sprite = Resources.Load("Image/Poker/" + pInfo[o], typeof(Sprite)) as Sprite;
				}
			}

			Transform number = PreInfoObj.transform.Find ("Amount");
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

				if(c == 0){
					GameObject icon = new GameObject ();
					icon.AddComponent<Image> ();
					icon.GetComponent<Image>().sprite = Resources.Load ("Image/Game/"+ typestr + "icon" , typeof(Sprite)) as Sprite;
					icon.transform.SetParent(number);
					icon.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (20, 24);
					icon.transform.localPosition = new Vector3 (left,0,0);
				}
			}

			float right = number.localPosition.x - 16 * amount.Length;
			if (hInfo.autowin) {
				PreInfoObj.transform.Find ("GetLucky").gameObject.SetActive (true);
				PreInfoObj.transform.Find ("GetLucky").localPosition = new Vector3 (right, PreInfoObj.transform.Find ("GetLucky").localPosition.y, 0);
			}

			if (hInfo.foul) {
				PreInfoObj.transform.Find ("Foul").gameObject.SetActive (true);
				PreInfoObj.transform.Find ("Foul").localPosition = new Vector3( right, PreInfoObj.transform.Find ("GetLucky").localPosition.y, 0);
			}
			index++;
		}
	}

	public void ClearHandReview(){
		Transform Layer  = GameObject.Find("Canvas").transform.Find("HandReview");
		for (int i = Layer.Find ("PreInfoCom").childCount - 1; i >= 0; i--) {  
			Destroy(Layer.Find ("PreInfoCom").GetChild(i).gameObject);
		}  
	}

	public void UpdateRoundCount(){
		Transform Layer  = GameObject.Find("Canvas").transform.Find("HandReview");
		Layer.Find ("RoundCount").GetComponent<Text> ().text = "";
		Layer.Find ("Left").gameObject.SetActive (false);
		Layer.Find ("Right").gameObject.SetActive (false);

		if (Common.CPlayed_hands > 0) {
			Layer.Find ("RoundCount").GetComponent<Text> ().text = (RoundCount + 1).ToString () + "/" + (Common.CPlayed_hands).ToString ();

			Layer.Find ("Left").gameObject.SetActive (true);
			Layer.Find ("Right").gameObject.SetActive (true);

			if(RoundCount <= 0){
				Layer.Find ("Left").gameObject.SetActive (false);
			}

			if((RoundCount+1) == Common.CPlayed_hands){
				Layer.Find ("Right").gameObject.SetActive (false);
			}
		} 
	}

	public void ShowWaitingAnime(){
		m_WaitingSort.SetActive (true);
		m_Sequence = DOTween.Sequence ();
		m_Sequence.Append (m_Pokers[0].transform.DOLocalMoveY (20, 0.2f));
		m_Sequence.Append (m_Pokers[1].transform.DOLocalMoveY (20, 0.2f));
		m_Sequence.Append (m_Pokers[2].transform.DOLocalMoveY (20, 0.2f));
		m_Sequence.Append (m_Pokers[3].transform.DOLocalMoveY (20, 0.2f));
		m_Sequence.Append (m_Pokers[4].transform.DOLocalMoveY (20, 0.2f));
		m_Sequence.Append (m_Pokers[0].transform.DOLocalMoveY (0, 0.2f));
		m_Sequence.Append (m_Pokers[1].transform.DOLocalMoveY (0, 0.2f));
		m_Sequence.Append (m_Pokers[2].transform.DOLocalMoveY (0, 0.2f));
		m_Sequence.Append (m_Pokers[3].transform.DOLocalMoveY (0, 0.2f));
		m_Sequence.Append (m_Pokers[4].transform.DOLocalMoveY (0, 0.2f));
		m_Sequence.Play ().SetLoops (-1);
	}

	public void HideWaitAnime(){
		m_Sequence.Kill ();
		foreach(GameObject pok in m_Pokers){
			pok.transform.localPosition = new Vector3 (pok.transform.localPosition.x, 0, 0);
		}
		m_WaitingSort.SetActive (false);
	}

	public void ShareInTable(){
		#if UNITY_IPHONE
		ssdk = gameObject.GetComponent<ShareSDK>();
		ssdk.shareHandler = ShareResultHandler;

		ShareContent content = new ShareContent();
		content.SetText("kLet's play together.");
		content.SetUrl ("https://www.baidu.com");
		content.SetTitle("Unlipoker");
		content.SetShareType(ContentType.Auto);
		ssdk.ShareContent (PlatformType.FacebookMessenger, content);
		#endif

		#if UNITY_ANDROID
		// Get the required Intent and UnityPlayer classes.
		AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
		AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

		// Construct the intent.
		AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
		intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
		intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), "kLet's play together.http://www.baidu.com");
		intent.Call<AndroidJavaObject>("setPackage","com.facebook.orca");
		intent.Call<AndroidJavaObject>("setType", "*/*");


		// Display the chooser.
		AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, "Share");
		currentActivity.Call("startActivity", chooser);
		#endif
	}
		
	public void ShareResultHandler (int reqID, ResponseState state, PlatformType type, Hashtable result)
	{
		if (state == ResponseState.Success)
		{
			Debug.Log ("share result :");
			Debug.Log  (MiniJSON.jsonEncode(result));
		}
		else if (state == ResponseState.Fail)
		{
			Debug.Log  ("fail! error code = " + result["error_code"] + "; error msg = " + result["error_msg"]);
		}
		else if (state == ResponseState.Cancel) 
		{
			Debug.Log  ("cancel !");
		}
	}

}

