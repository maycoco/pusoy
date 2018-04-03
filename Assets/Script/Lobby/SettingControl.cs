using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using DG.Tweening;

public class SettingControl : MonoBehaviour
{
	// Use this for initialization
	void Start (){
	}

	void Awake(){
	}
	
	// Update is called once per frame
	void Update (){
	}

	public void Enter(){
		transform.Find ("Content").localPosition = new Vector3 (-640, 0,  0);
		this.gameObject.SetActive (true);
	}

	public void Exit(){
		this.gameObject.SetActive (false);
	}

	public void Language(){
	}

	public void Location(){
	}

	public void MusicOn(){
		if (Common.ConfigMusicOn) {
			Common.ConfigMusicOn = false;
		} else {
			Common.ConfigMusicOn = true;
		}

		if (Common.ConfigMusicOn) {
			transform.Find ("Consoles/Music/Button").GetComponent<Image>().sprite = Resources.Load ("Image/Lobby/select_on", typeof(Sprite)) as Sprite;
			transform.Find ("Consoles/Music/Button/Image").DOLocalMoveX (19, 0.2f);
		} else {
			transform.Find ("Consoles/Music/Button").GetComponent<Image>().sprite =  Resources.Load ("Image/Lobby/select_off", typeof(Sprite)) as Sprite;
			transform.Find ("Consoles/Music/Button/Image").DOLocalMoveX (-19, 0.2f);
		}
	}

	public void Disclaimer(){
		
	}

	public void OnContent(){
		transform.Find ("Content").DOLocalMove (new Vector3 (0, 0, 0), 0.2f).SetEase (Ease.Linear);
	}

	public void OffContext(){
		transform.Find ("Content").DOLocalMove (new Vector3 (-640, 0, 0), 0.2f).SetEase (Ease.Linear);
	}
}

  