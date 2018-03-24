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
	public GameObject 		Canvas;
	private AsyncOperation 	async = null;
	private int 			progress = 0;

	// Use this for initialization
	void Start () {
		//for demo
		Screen.SetResolution(448, 795, false);
	}

	void Awake(){
		//int id = Random.Range (100000, 999999);
		//Common.FB_id 			= "740176969651533";
		//Common.FB_access_token 	= "EAAY4utOSxIgBAJVCIFacnRuBMMnqzSLq7B35hDHFQDZAxWcuayFfqmZCjsr5Ri17vXX0xJRAp40We6XXG3eRLUMIJ0Ld3lH5wSSv70HhZBOUZCEsYzXtrUnCTZAesx8zq4jrUJC4uSsvKxAHUJNrlBRrozdPHpbacBHO8ZAti6C7Ch9YBmqYmdY17jCnH20NxOGvbXqWjqlSKeE9jWMlnjSocDYLukC70ZD";
		//Common.FB_name			= "123333";

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
		if(!string.IsNullOrEmpty(Common.FB_id) || !string.IsNullOrEmpty(Common.FB_access_token)){
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
			if(data.LoginRsp.Ret == 0){
				Common.Uid = data.LoginRsp.Uid;
				Common.FB_name = data.LoginRsp.Name;
				Common.CRoom_number = data.LoginRsp.RoomNumber;
				Common.FB_avatar = data.LoginRsp.Avatar;
				Loom.QueueOnMainThread(()=>{  
					GotoLobby ();
				}); 
			}
		}
	}

	public void LoginServer(){
		Protocol msg 			= new Protocol();
		msg.Msgid 				= MessageID.LoginReq;
		msg.LoginReq = new LoginReq();
		msg.LoginReq.Type		= LoginType.Facebook;
		msg.LoginReq.Fb 		= new LoginFBReq();
		msg.LoginReq.Fb.FbId 	= Common.FB_id;
		msg.LoginReq.Fb.Token 	= Common.FB_access_token;


		Debug.Log (Common.FB_id);
		Debug.Log (Common.FB_access_token);
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