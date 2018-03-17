using UnityEngine;
using System.Collections;

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

	public void ShowPlayerList(){
		CLearPlayerList ();

		string BeginTime	= "2018-2-27 11:11";
		string EndTime 		= "Now";
		string Hands 		= Common.CPlayed_hands + " / " + Common.CHands;
		string Mvp  		= "Bruce";
		string Roomfee 		= "shared";
		string Betsize 		= Common.CMin_bet + "-" + Common.CMax_bet;

		Transform layer  = GameObject.Find("Canvas").transform.Find("PlayersListLayer");

		layer.Find ("Date/Time").GetComponent<Text> ().text = BeginTime + " ~ " + EndTime;
		layer.Find ("Totalhands/Text").GetComponent<Text> ().text = Hands;
		layer.Find ("MVP/Text").GetComponent<Text> ().text = Mvp;
		layer.Find ("Roomfee/Text").GetComponent<Text> ().text = Roomfee;
		layer.Find ("Betsize/Text").GetComponent<Text> ().text = Betsize;

		int count = 0;
		for(int i = 0; i < Common.CPlayers.Count; i++){
			if(Common.CPlayers[i].Win != 0){
				count++;
			}
		}

		if(count > 4){
			layer.Find ("TablePlayers/Viewport/Content").GetComponent<RectTransform> ().sizeDelta = new Vector2 (0, 100 * count);
		}

		float y = (layer.Find ("TablePlayers/Viewport/Content").GetComponent<RectTransform> ().sizeDelta.y / 2) - 3;
		for(int i = 0; i < Common.CPlayers.Count; i++){
			if(Common.CPlayers[i].Win != 0){
				GameObject pinfo = (GameObject)Instantiate(m_GameController.m_PrefabTPlayer);  
				pinfo.transform.SetParent (layer.Find("TablePlayers/Viewport/Content"));
				pinfo.transform.localPosition = new Vector3 (0, y ,0);
				pinfo.transform.localScale = new Vector3 (1,1,1);

				pinfo.transform.Find ("Name").GetComponent<Text>().text = Common.CPlayers [i].Name;

				UICircle avatar = (UICircle)Instantiate(m_GameController.m_PrefabAvatar);
				avatar.transform.SetParent (pinfo.transform.Find ("Avatar"));
				avatar.transform.localPosition = new Vector3 ();
				avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (70, 70);

				if (Common.CPlayers [i].FB_avatar != null) {
					avatar.SetAvatar (Common.CPlayers [i].FB_avatar.texture);
				}

				string typestr = "";
				if (Common.CPlayers [i].Win < 0) {typestr = "lost";}
				else {typestr = "win";}

				float left = 0;
				GameObject icon = new GameObject ();
				icon.AddComponent<Image> ();
				icon.GetComponent<Image>().sprite = Resources.Load ("Image/Game/"+ typestr + "icon" , typeof(Sprite)) as Sprite;
				icon.transform.SetParent(pinfo.transform.Find ("Amount"));
				icon.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (20, 24);
				icon.transform.localPosition = new Vector3 (left,0,0);
				left = left + 18;

				string amount = Mathf.Abs (Common.CPlayers [i].Win).ToString ();
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
		}
	}

	public void Text(){
		Transform layer  = GameObject.Find("Canvas").transform.Find("PlayersListLayer");
		for(int i = 0; i < layer.Find("TablePlayers/Viewport/Content").childCount; i++){
			Debug.Log (layer.Find("TablePlayers/Viewport/Content").GetChild(i).transform.localPosition);
		}
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

