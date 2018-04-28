
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

public class LoginController : MonoBehaviour { 
	public GameObject 				Canvas;
	public GameObject 				PrefabDialog;


	// Use this for initialization
	void Start () {
		#if UNITY_STANDALONE_WIN || UNITY_EDITOR
			Screen.SetResolution(448, 795, false);
			Application.runInBackground = true;
		#endif
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
}