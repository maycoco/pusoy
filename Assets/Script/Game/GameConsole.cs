using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Msg;

using UnityEngine.UI;
using DG.Tweening;

public class GameConsole : MonoBehaviour
{
	private bool			MenuOpen 	= false;
	public 	GameController 	m_GameController;
	public  GameObject 		m_ConsoleMenu;
	public  GameObject		m_ConsoleMenu_Mask;

	void Start (){
	}

	void Update (){
	}

	void Awake(){
		AdjustUI ();
	}

	void AdjustUI(){
		EventTriggerListener.Get(m_ConsoleMenu_Mask).onClick = onClickButtonHandler;
	}

	public void MenuBTConsole(){
		if (!MenuOpen) {
			m_ConsoleMenu.transform.Find("Menu").gameObject.SetActive(true);
			m_ConsoleMenu.GetComponent<Animation>().Play("GameConsole_Menu_open");
			m_ConsoleMenu_Mask.SetActive (true);
			MenuOpen = true;
		} else {
			m_ConsoleMenu.transform.Find("Menu").gameObject.SetActive(true);
			m_ConsoleMenu.GetComponent<Animation>().Play("GameConsole_Menu_close");
			m_ConsoleMenu_Mask.SetActive (false);
			MenuOpen = false;
		}
	}

	public void onClickButtonHandler(GameObject obj){
		if(MenuOpen){
			m_ConsoleMenu.transform.Find("Menu").gameObject.SetActive(true);
			m_ConsoleMenu.GetComponent<Animation>().Play("GameConsole_Menu_close");
			m_ConsoleMenu_Mask.SetActive (false);
			MenuOpen = false;
		}
	}

	public void StandUp(){
		MenuBTConsole ();
		m_GameController.StandUpServer ();
	}
		
	public void PlayerListReq(){
		m_GameController.ScoreboardServer ();
	}

	public void ShowPlayerList(List<Msg.ScoreboardItem> slist){
		CLearPlayerList ();
		ClearObList ();

		Transform layer  = GameObject.Find("Canvas").transform.Find("PlayersListLayer");
		layer.gameObject.SetActive (true);

		string BeginTime	= "2018-2-27 11:11";
		string EndTime 		= "Now";
		string Hands 		= (Common.CPlayed_hands+1) + " / " + Common.CHands;

		string Mvp 			= "";
		Debug.Log (slist.ToString ());
		Debug.Log (slist.Count);
		if(slist.Count > 0){Mvp = slist [0].Uid.ToString();}

		string Roomfee 		= "";
		if(Common.CIs_share){Roomfee = "shared";}{Roomfee = "no shared";}

		string Betsize 		= Common.CMin_bet + "-" + Common.CMax_bet;
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

			pinfo.transform.Find ("Name").GetComponent<Text> ().text = slist [i].Uid.ToString(); //Common.CPlayers [i].Name;

			UICircle avatar = (UICircle)Instantiate(m_GameController.m_PrefabAvatar);
			avatar.transform.SetParent (pinfo.transform.Find ("Avatar"));
			avatar.transform.localPosition = new Vector3 ();
			avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (70, 70);

//			if (Common.CPlayers [i].FB_avatar != null) {
//				avatar.SetAvatar (Common.CPlayers [i].FB_avatar.texture);
//			}

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

//			if (Common.CPlayers [i].FB_avatar != null) {
//				avatar.SetAvatar (Common.CPlayers [i].FB_avatar.texture);
//			}
//
			obleft += (70 + 24);
		}
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
}

