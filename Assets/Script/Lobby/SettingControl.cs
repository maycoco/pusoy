﻿using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using DG.Tweening;
using cn.sharesdk.unity3d;

public class SettingControl : MonoBehaviour
{
	public 	LobbyController 	m_LobbyController;
	public  GameObject			m_ShareLayer;
	public 	GameObject 			m_CloseArea;
	public 	GameObject 			m_ShareControl;
	private	ShareSDK			ssdk;


	// Use this for initialization
	void Start (){
		EventTriggerListener.Get(m_CloseArea).onClick = HideShareDialog;
	}

	void Awake(){
		transform.Find ("Content/Top/Title").GetComponent<Text> ().text = "";
		transform.Find ("Content/Context").GetComponent<Text> ().text = "";
	}
	
	// Update is called once per frame
	void Update (){
	}

	public void Enter(){
		transform.Find ("Content").localPosition = new Vector3 (-640, 0,  0);
		transform.Find ("LanguageConsole").gameObject.SetActive (false);
		transform.Find ("LanguageConsole").localPosition = new Vector3 (0, -275,  0);

		if (Common.ConfigMusicOn) {
			transform.Find ("Consoles/Music/Button").GetComponent<Image>().sprite = Resources.Load ("Image/Lobby/select_on", typeof(Sprite)) as Sprite;
			transform.Find ("Consoles/Music/Button/Image").localPosition = new Vector3 (19, 0, 0);
		} else {
			transform.Find ("Consoles/Music/Button").GetComponent<Image>().sprite =  Resources.Load ("Image/Lobby/select_off", typeof(Sprite)) as Sprite;
			transform.Find ("Consoles/Music/Button/Image").localPosition = new Vector3 (-19, 0, 0);
		}

		transform.localPosition = new Vector3(-640, 0, 0);
		Sequence s = DOTween.Sequence ();
		s.Append (transform.DOLocalMoveX (30, 0.2f));
		s.Append (transform.DOLocalMoveX (0, 0.2f));
		s.Play ();
	}

	public void Exit(){
		m_LobbyController.PlayerButtonEffect ();
		transform.DOLocalMoveX (-640, 0.15f);
	}

	public void Language(){
	}

	public void Location(){
	}

	public void MusicOn(){
		m_LobbyController.PlayerButtonEffect ();

		if (Common.ConfigMusicOn) {
			Common.ConfigMusicOn = false;
		} else {
			Common.ConfigMusicOn = true;
		}

		if (Common.ConfigMusicOn) {
			transform.Find ("Consoles/Music/Button").GetComponent<Image>().sprite = Resources.Load ("Image/Lobby/select_on", typeof(Sprite)) as Sprite;
			transform.Find ("Consoles/Music/Button/Image").DOLocalMoveX (19, 0.2f);
			m_LobbyController.PlayMusic ();
		} else {
			transform.Find ("Consoles/Music/Button").GetComponent<Image>().sprite =  Resources.Load ("Image/Lobby/select_off", typeof(Sprite)) as Sprite;
			transform.Find ("Consoles/Music/Button/Image").DOLocalMoveX (-19, 0.2f);
			m_LobbyController.StopMusic ();
		}
	}

	public void Disclaimer(){
	}

	public void AboutUs(){
		m_LobbyController.PlayerButtonEffect ();

		OnContent ();
		transform.Find ("Content/Top/Title").GetComponent<Text>().text = "About Us";
		transform.Find ("Content/Context").GetComponent<Text> ().text = "Unlipoker founded and began operating online gameing in 2018 . \n" +
			"The Unlipoker mission is a simple one; \nto create a fun, engaging and trustworthy place to play and one where the happiness of our players is always paramount.";
	}

	public void ContactUs(){
		m_LobbyController.PlayerButtonEffect ();

		OnContent ();
		transform.Find ("Content/Top/Title").GetComponent<Text>().text = "Contact Us";
		transform.Find ("Content/Context").GetComponent<Text> ().text = "Info@unlipoker.com\n" +
			"www.unlipoker.com";
	}

	public void OnContent(){
		transform.Find ("Content").DOLocalMove (new Vector3 (0, 0, 0), 0.2f).SetEase (Ease.OutQuad);
	}

	public void OffContext(){
		transform.Find ("Content").DOLocalMove (new Vector3 (-640, 0, 0), 0.2f).SetEase (Ease.OutQuad);
	}

	public void OnLanguage(){
		m_LobbyController.PlayerButtonEffect ();

		transform.Find ("LanguageConsole").gameObject.SetActive (true);
		transform.Find ("LanguageConsole").DOLocalMove (new Vector3 (0, 0, 0), 0.1f).SetEase (Ease.Linear);
	}

	public void OffLanguage(){
		transform.Find ("LanguageConsole").DOLocalMove (new Vector3 (0, -275, 0), 0.1f).SetEase (Ease.Linear).onComplete = OnHideLanguage;
	}

	public void OnHideLanguage(){
		transform.Find ("LanguageConsole").gameObject.SetActive (false);
	}

	public void ShowShareDialog(){
		m_ShareLayer.SetActive (true);
		m_ShareControl.transform.DOLocalMoveY (-458, 0.12f);
	}

	public void HideShareDialog(GameObject obj){
		m_ShareControl.transform.DOLocalMoveY (-678, 0.12f).onComplete = HideShareLayer;

	}

	public void HideShareLayer(){
		m_ShareLayer.SetActive (false);
	}

	public void Share(int type){
		ssdk = gameObject.GetComponent<ShareSDK>();
		ssdk.shareHandler = ShareResultHandler;

		ShareContent content = new ShareContent();
		content.SetText("kLet's play together.");
		//content.SetImageUrl("https://f1.webshare.mob.com/code/demo/img/1.jpg");
		content.SetUrl ("https://www.baidu.com");
		content.SetTitle("Unlipoker");
		content.SetShareType(ContentType.Auto);
		//ssdk.ShowPlatformList (null, content, 100, 100);

		switch(type){
		case 0:
			ShareFacebook ( content);
			break;

		case 1:
			ShareMessenger (content);
			break;
		}
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

	public void ShareFacebook(ShareContent content){
		ssdk.ShareContent (PlatformType.Facebook, content);
	}

	public void ShareMessenger(ShareContent content){
		ssdk.ShareContent (PlatformType.FacebookMessenger, content);
	}
}

  