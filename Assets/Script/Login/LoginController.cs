using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Facebook.Unity;

using networkengine;
using ProtoBuf;
using Msg;
using System.IO;

public class LoginController : MonoBehaviour { 
	
	// Use this for initialization
	void Start () {
		//for demo
		Screen.SetResolution(448, 795, false);
	}

	void Awake(){
		int id = Random.Range (100000, 999999);
		Common.FB_id 			= id.ToString();
		Common.FB_access_token 	= "dasdasdasndkasndlkasjdlkasjasdasd";
		Common.FB_name			= "P"+id.ToString();

		Loom.Initialize ();
		InitCallbackForNet ();
		if (!FB.IsInitialized) {Init ();}
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void GotoLobby(){
		SceneManager.LoadScene (1);
	}

	//===================================connect=================================
	public void InitCallbackForNet(){
		protonet.SetConnectedCall(Connected);
		protonet.SetFailConnectedCall(FailedConnect);
		protonet.SetDisConnectedCall(DisConnect);
		protonet.SetDataCall(Data);
	} 

	public void ConnectServer(){
		if(!string.IsNullOrEmpty(Common.FB_id) || !string.IsNullOrEmpty(Common.FB_access_token) || !string.IsNullOrEmpty(Common.FB_name)){
			protonet.ConnectServer ();
		}
	}

	public void Connected(){
		LoginServer ();
	}

	public void FailedConnect(){
		Debug.Log ("FailedConnect");
	}

	public void DisConnect(){
		Debug.Log ("DisConnect");
	}

	public void Data(Protocol data){
		if(data == null){
			return;
		}

		if(data.Msgid  == MessageID.LoginRsp){
			if(data.loginRsp.Ret == 0){
				Common.Uid = data.loginRsp.Uid;
				Common.CRoom_id = data.loginRsp.RoomId;
				Loom.QueueOnMainThread(()=>{  
					GotoLobby ();
				}); 
			}
		}
	}

	public void LoginServer(){
		Protocol msg 			= new Protocol();
		msg.Msgid 				= MessageID.LoginReq;
		msg.loginReq 			= new LoginReq();
		msg.loginReq.Type		= LoginType.Facebook;
		msg.loginReq.Fb 		= new LoginFBReq();
		msg.loginReq.Fb.FbId 	= Common.FB_id;
		msg.loginReq.Fb.Token 	= Common.FB_access_token;
		msg.loginReq.Fb.Name 	= Common.FB_name;
		using (var stream = new MemoryStream())
		{
			Serializer.Serialize<Protocol>(stream, msg);
			Client.Instance.Send(stream.ToArray());
		}
	}

	//===================================facebook=================================
	public void Init(){
		FB.Init(this.OnInitComplete, this.OnHideUnity);
	}

	public void LoginFacebook(){
		//for demo
//		GotoLobby();
//		return;

//		if(FB.IsInitialized){
//			this.CallFBLogin();
//		}

		ConnectServer ();
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
				FB.API("/me", HttpMethod.GET, this.InfoHandleResult);
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