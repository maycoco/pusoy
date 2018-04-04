using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using DG.Tweening;

public class SettingControl : MonoBehaviour
{
	public LobbyController		m_LobbyController;

	// Use this for initialization
	void Start (){
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
		this.gameObject.SetActive (true);
	}

	public void Exit(){
		m_LobbyController.PlayerButtonEffect ();
		this.gameObject.SetActive (false);
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
}

  