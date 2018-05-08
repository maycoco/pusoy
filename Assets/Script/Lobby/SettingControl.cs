using UnityEngine;
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
	private bool 				m_OnConrent;


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
		m_OnConrent = false;
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
		transform.Find ("Content/Context").GetComponent<Text> ().text = "Email：Info@unlipoker.com\n" +
			"Webesite：www.unlipoker.com\n" + "Facebook：UnliPoker";
	}

	public void OnContent(){
		m_OnConrent = true;
		transform.Find ("Content").DOLocalMove (new Vector3 (0, 0, 0), 0.2f).SetEase (Ease.OutQuad);
	}

	public void OffContext(){
		m_OnConrent = false;
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
		content.SetUrl ("https://www.baidu.com");
		content.SetTitle("Unlipoker");
		content.SetShareType(ContentType.Auto);
		ssdk.GetUserInfo (PlatformType.FacebookMessenger);

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
		#if UNITY_IPHONE
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
		
	enum slideVector { nullVector, left, right };
	private Vector2 lastPos;
	private Vector2 currentPos;
	private slideVector currentVector = slideVector.nullVector;
	private float timer;
	public float offsetTime = 0.01f;

	void OnGUI(){
		if (Event.current.type == EventType.MouseDown) {//滑动开始
			lastPos = Event.current.mousePosition;
			currentPos = Event.current.mousePosition;
			timer = 0;
		}

		if (Event.current.type == EventType.MouseDrag) {//滑动过程
			currentPos = Event.current.mousePosition;
			timer += Time.deltaTime;
			if (timer > offsetTime) {
				if (currentPos.x < lastPos.x) {
					if (currentVector == slideVector.left) {
						return;
					}
					//TODO trun Left event

					currentVector = slideVector.left;
					if (m_OnConrent) {
						OffContext ();
					} else {
						Exit ();
					}
				} 

				if (currentPos.x > lastPos.x) {
					if (currentVector == slideVector.right) {
						return;
					}
					//TODO trun right event

					currentVector = slideVector.right;

				}

				lastPos = currentPos;
				timer = 0;
			}		
		}

		if (Event.current.type == EventType.MouseUp) {//滑动结束  
			currentVector = slideVector.nullVector;  
		}  
	}
}

  