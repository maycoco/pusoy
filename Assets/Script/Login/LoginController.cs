
using Facebook.Unity;

using Google.Protobuf;
using Msg;
using networkengine;

using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using cn.sharesdk.unity3d;

public class JsonFor  
{  
	public string server;
	public int port; 
	public int update; 
	public string notice;
}  


public class LoginController : MonoBehaviour { 
	public GameObject 				Canvas;
	public GameObject 				PrefabDialog;


	// Use this for initialization
	void Start () {
		#if UNITY_STANDALONE_WIN || UNITY_EDITOR
			Screen.SetResolution(448, 795, false);
			Application.runInBackground = false;
		#endif

		StartCoroutine(RequestQuickLogin ());
	}

	void Awake(){
		Loom.Initialize ();
		InitCallbackForNet ();
		if (!FB.IsInitialized) {Init ();}
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void GotoLobby(){
		SceneManager.LoadScene ("Scene/Lobby");
	}

	public void Test1(){
		Common.FB_id 			= "W";
		Common.FB_access_token 	= "EAAY4u0bsgXPsnZBHcIztcgDybpAI1ua2yKoYDoukx8nbRWg0GUxKaLWZCOKUQ2A6x4psVEu4JLvLuRZCYf7EjmmwnZCW41kljPsEVREsnGx2MUefBrZAFsFLop1rp26s59EjaXiaVEiS3av3U3unglcx73Xf6bGuETEAPZBzSrj697m2XrZBPgpfOHlgSAAlJ73oRclYGHPL4ZD";
		Common.FB_name			= "Test1";
		ConnectServer ();
	}

	public void Test2(){
		Common.FB_id 			= "A";
		Common.FB_access_token 	= "EAAY4utOSxIgBemjfkEu6rKKZCr0bsgXPsnZBHcIztcgDybpAI1uKUQ2A6x4psVEu4JLvLuRZCYf7EjmmwnZCW41kljPsEVREsnGx2MUefBrZAFsFLop1rp26s59EjaXiaVEiS3av3U3unglcx73Xf6bGuETEAPZBzSrj697m2XrZBPgpfOHlgSAAlJ73oRclYGHPL4ZD";
		Common.FB_name			= "Test2";
		ConnectServer ();
	}

	public void Test3(){
		Common.FB_id 			= "P";
		Common.FB_access_token 	= "EAAY4utOSxIgBemjfkEu6rKKZCr0bsgXPsnZBHcKoYDoukx8nbRWg0GUxKaLWZCOKUQ2A6x4psVEu4JLvLuRZCYf7EjmmwnZCW41kljPsEVREsnGx2MUefBrZAFsFLop1rp26s59EjaXiaVEiS3av3U3unglcx73Xf6bGuETEAPZBzSrj697m2XrZBPgpfOHlgSAAlJ73oRclYGHPL4ZD";
		Common.FB_name			= "Test3";
		ConnectServer ();
	}

	public void Test4(){
		Common.FB_id 			= "B";
		Common.FB_access_token 	= "EAAY4utOSxIgBemjfkEu6rKKZCr0bsgXPsnZBHcIztcgDybpUxKaLWZCOKUQ2A6x4psVEu4JLvLuRZCYf7EjmmwnZCW41kljPsEVREsnGx2MUefBrZAFsFLop1rp26s59EjaXiaVEiS3av3U3unglcx73Xf6bGuETEAPZBzSrj697m2XrZBPgpfOHlgSAAlJ73oRclYGHPL4ZD";
		Common.FB_name			= "Test4";
		ConnectServer ();
	}

	//===================================connect=================================
	public void InitCallbackForNet(){
		protonet.SetConnectedCall(Connected);
		protonet.SetFailConnectedCall(FailedConnect);
		protonet.SetDisConnectedCall(DisConnect);
		protonet.SetDataCall(Data);	
	} 

	public void ConnectServer(){ 
		if(string.IsNullOrEmpty(Common.SServer) || Common.SPort == 0){
			Common.ErrorDialog (PrefabDialog, Canvas, Common.ErrorLogin);
			return;
		}
		if(!string.IsNullOrEmpty(Common.FB_id) || !string.IsNullOrEmpty(Common.FB_access_token)){
			protonet.ConnectServer ();
		}
	}

	public void Connected(){
		Loom.QueueOnMainThread(()=>{  
			LoginServer ();
		}); 
	}

	public void FailedConnect(){
		Debug.Log ("FailedConnect");
	}

	public void DisConnect(){
		Common.IsOnline = false;
	}

	public void Data(Protocol data){
		Loom.QueueOnMainThread(()=>{  
			Common.EndCalling ();
		}); 

		if(data == null){return;}

		if(data.Msgid  == MessageID.LoginRsp){
			if (data.LoginRsp.Ret == 0) {
				Loom.QueueOnMainThread (() => {
					Common.Uid = data.LoginRsp.Uid;
					Common.FB_name = data.LoginRsp.Name;
					Common.FB_avatar = data.LoginRsp.Avatar;
					Common.IsOnline = true;
					GotoLobby ();
				}); 
			} else {
				Loom.QueueOnMainThread (() => {  
					Common.ErrorDialog (PrefabDialog, Canvas, Common.ErrorLogin);
				}); 
			}
		}
	}

	public void LoginServer(){
		Common.Calling (Canvas);
		Protocol msg 			= new Protocol();
		msg.Msgid 				= MessageID.LoginReq;
		msg.LoginReq = new LoginReq();
		msg.LoginReq.Type		= LoginType.Facebook;
		msg.LoginReq.Fb 		= new LoginFBReq();
		msg.LoginReq.Fb.FbId 	= Common.FB_id;
		msg.LoginReq.Fb.Token 	= Common.FB_access_token;

		using (var stream = new MemoryStream())
		{
            msg.WriteTo(stream);
			Client.Instance.Send(stream.ToArray());
		}
	}

	//===================================facebook=================================
	public void Init(){
		FB.Init(this.OnInitComplete, this.OnHideUnity);
	}

	public void LoginFacebook(){
		if(FB.IsInitialized){
			this.CallFBLogin();
		}
	}

	private void CallFBLogin(){
		FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, this.LoginHandleResult);
	}

	private void OnInitComplete(){
		if(FB.IsLoggedIn){
			FB.API("/me", HttpMethod.GET, this.InfoHandleResult);
		}
	}

	private void OnHideUnity(bool isGameShown){
	}

	protected void LoginHandleResult(ILoginResult result){
		if (result == null){return;}
		if (result.Cancelled){return;}
		if (!string.IsNullOrEmpty(result.Error)){return;}

		if (!string.IsNullOrEmpty(result.RawResult)){
			Result res = JsonUtility.FromJson<Result> (result.RawResult);
			Common.FB_id 				= res.user_id;
			Common.FB_access_token   	= res.access_token;

			if(FB.IsLoggedIn){
				//FB.API("/me", HttpMethod.GET, this.InfoHandleResult);
				ConnectServer ();
			}
		}
	}

	protected void InfoHandleResult(IResult result){
		if (result == null){return;}
		if (result.Cancelled){return;}
		if (!string.IsNullOrEmpty(result.Error)){return;}
		 
		if (!string.IsNullOrEmpty (result.RawResult)) {
			AccountResult acres = JsonUtility.FromJson<AccountResult> (result.RawResult);
			Common.FB_name = acres.name;
			ConnectServer ();
		}
	}

	public IEnumerator RequestQuickLogin()
	{
		WWW www = new WWW(Common.SJsonHttp);
		yield return www;

		if (www!=null && string.IsNullOrEmpty(www.error))
		{
			JsonFor td = JsonUtility.FromJson<JsonFor> (www.text);  
			Common.SServer 	= td.server;
			Common.SPort	= td.port;
			Common.SUpdate	= td.update;

			if(Common.SUpdate == 1){
				Common.CloseServerDialog (PrefabDialog, Canvas, td.notice);
			}
		} 
	}

	public void OnShareClicked1() 
	{
		#if UNITY_ANDROID
		// Get the required Intent and UnityPlayer classes.
		AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
		AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

		// Construct the intent.
		AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
		intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
		intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), "请点击http://www.baidu.com");
		intent.Call<AndroidJavaObject>("setPackage","com.facebook.orca");
		intent.Call<AndroidJavaObject>("setType", "*/*");


		// Display the chooser.
		AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, "Share");
		currentActivity.Call("startActivity", chooser);
		#endif

	}

	public void OnShareClicked2(){
		StartCoroutine(ShareScreenshot());
	}

	private	ShareSDK			ssdk;
	public void OnsShareClick3(){
		ssdk = gameObject.GetComponent<ShareSDK>();
		ssdk.shareHandler = ShareResultHandler;

		ShareContent content = new ShareContent();
		content.SetText("this is a test string.");
		content.SetImageUrl("https://f1.webshare.mob.com/code/demo/img/1.jpg");
		content.SetTitle("test title");
		content.SetTitleUrl("http://www.mob.com");
		content.SetSite("Mob-ShareSDK");
		content.SetSiteUrl("http://www.mob.com");
		content.SetUrl("http://www.mob.com");
		content.SetComment("test description");
		content.SetMusicUrl("http://mp3.mwap8.com/destdir/Music/2009/20090601/ZuiXuanMinZuFeng20090601119.mp3");
		content.SetShareType(ContentType.Webpage);

		ssdk.ShowPlatformList(null, content, 100, 100);
		ssdk.Authorize(PlatformType.FacebookMessenger);
	}

	void ShareResultHandler (int reqID, ResponseState state, PlatformType type, Hashtable result)
	{
		if (state == ResponseState.Success)
		{
			print ("share result :");
			print (MiniJSON.jsonEncode(result));
		}
		else if (state == ResponseState.Fail)
		{
			print ("fail! throwable stack = " + result["stack"] + "; error msg = " + result["msg"]);
		}
		else if (state == ResponseState.Cancel) 
		{
			print ("cancel !");
		}
	}
		
	private bool isProcessing = false;
	private bool isFocus = false;

	public IEnumerator ShareScreenshot()
	{
		isProcessing = true;
		yield return new WaitForEndOfFrame();
		ScreenCapture.CaptureScreenshot("screenshot.png", 2);
		string destination = Path.Combine(Application.persistentDataPath, "screenshot.png");
		yield return new WaitForSeconds(0.3f); //WaitForSecondsRealtime(0.3f);
		if (!Application.isEditor)
		{
			AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
			AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
			intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
			AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
//			AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + destination);
//			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"),
//				uriObject);


			//AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "http://www.baidu.com");
//			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"),
//				uriObject);

			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"),
				"<a href='https://www.baidu.com/'>woshibaidu</a>");
			intentObject.Call<AndroidJavaObject>("setType", "text/html");

			AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser",
				intentObject, "Share your new score");
			currentActivity.Call("startActivity", chooser);
			yield return new WaitForSeconds(1f); //WaitForSecondsRealtime(1f);
		}
		yield return new WaitUntil(() => isFocus);
		isProcessing = false;
	}

	public  void OnApplicationFocus(bool focus)
	{
		isFocus = focus;
	}
}