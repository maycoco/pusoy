using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

using Google.Protobuf.Collections;

using System.IO;

public class Result{
	public string user_id;
	public string access_token;
}

public class AccountResult{
	public string name;
}
	
public class NoticeMessage{
	public string 	text;
	public int 		times;
}

public class CSeatResult{
	public int 		SeatID;
	public string	Name;
	public string 	Avatar;
	public string 	Uid;
	public bool 	autowin;
	public bool 	foul;
	public int 		Bet;
	public int 		Win;
	public int 		BWin;
	public List<int> 							score	= new List<int> ();
	public RepeatedField<RepeatedField<uint>> 	Pres 	= new RepeatedField<RepeatedField<uint>>();
	public RepeatedField<global::Msg.CardRank> 	Ranks 	= new RepeatedField<global::Msg.CardRank>();
}

public enum GameState:int{
	Ready,
	Bet,
	Confirm_bet,
	Deal,
	Combine,
	Result
}

public class PlayerInfo{
	public uint 		Uid		  = 0;
	public string 		Name	  = "";
	public int 			SeatID	  = -1;
	public string 		FB_avatar = "";
	public uint			Bet		  = 0;
	public int			Score	  = 0;
}

public class Common: MonoBehaviour
{
	private static volatile Common _instance;
	private static object _lock = new object();
	//online state
	public 	static bool		IsOnline		= false;
	public  static int 		MaxTrying 		= 2;
	public 	static int      Trying			= 0;

	// Data
	public 	static string 	FB_id			=	"";
	public  static string 	FB_access_token	=	"";
	public  static string 	FB_name			=	"";
	public  static uint 	Uid				= 	0;
	public  static string   FB_avatar		= 	"";

	//UserInfo
	public static uint		DiamondAmount	= 0;	

	//setver Info
	public static string	SJsonHttp 		= "http://pusoy.apppark.xyz/json.txt" ;
	public static string	SServer			= "";
	public static int		SPort			= 0;
	public static int		SUpdate			= 0;

	//Room Data
	public  static uint 	CRoom_id		=	0;
	public  static string 	CRoom_number	= 	"";
	public static string 	CRoom_name		=   "";
	public static uint 		CMin_bet 		= 	0;
	public static uint 		CMax_bet 		= 	0;
	public static uint 		CHands			= 	0;
	public static uint 		CPlayed_hands	= 	0;
	public static uint 		CCredit_points	= 	0;
	public static bool 		CIs_share		= 	true;
	public static bool		CAutoBanker 	= 	true;
	public static Msg.GameState CState		= 	Msg.GameState.Ready;
	public static List<PlayerInfo> CPlayers	= new List<PlayerInfo>();
	public static List<int>	CPokers			= new List<int>();
	public static List<int>	CSeats			= new List<int>();

	//Players Info
	public static Dictionary<int,  CSeatResult> CSeatResults 	= new Dictionary<int, CSeatResult>();
	public static List<List<uint>>	CCPokers= new List<List<uint>>();

	//GameObject
	public static GameObject CallingObj		= null;		
	public static GameObject TipsOnObj		= null;

	//Career
	public static List<int> CareerWins 	= new List<int>();

	//Room Config
	public 	static uint[] 	ConfigMinBet 	= new uint[]{5, 20, 100, 500};
	public 	static uint[] 	ConfigHands 	= new uint[]{20, 50, 100, 200, 500};
	public 	static uint[] 	ConfigCredit 	= new uint[]{0, 20, 40, 60, 80, 100};
	public  static List<int> ConfigSeatOrder= new List<int> ();

	public  static int 		ConfigBetTime 	= 10;
	public  static int 		ConfigSortTime	= 45;
	public  static int      ConfigFinishTime= 8;

	//Chips Config
	public static int[] 	ConfigChips 	= {1, 2 ,10, 20};
	public static int 		ConfigMaxChips	= 20;

	//Music Config
	public static bool 	 	ConfigMusicOn	= true;

	//CareerReqDays
	public static uint 		ConfigCareerDays = 30;

	//Focus Time
	public static int       PauseTimeOut		= 300;
	public static int       PauseTimeOutLong	= 300;

	public static long		PauseTime			= 0;
	public static bool      needReConnect		= true;

	//Notices
	public static List<NoticeMessage> GameNotices	= new List<NoticeMessage>();


	public static bool	 	Sumbiting = false;

	//DialogText
	public static string 	TipsCantStandUp		= "You can’t stand up right now";
	public static string    TipsSendSuccess		= "Successfully Sent";
	public static string	TipsCreareRoom 		= "Failed to create room";
	public static string	TipsJoinRoom 		= "Failed to joining room";
	public static string 	TipsCantLeave 		= "On going ation, can't quit the room";
	public static string	TipsCantCloseRoom 	= "Failed to disband room:On going action";
	public static string	TipsInvalidUserID 	= "Invalid user ID";
	public static string	TipsCantSendSelf 	= "Can't send diamond to yourself";
	public static string	TipsInsufficient 	= "Unable to send out diamond under 100 diamond balance";

	public static string	TipsCreateRoomMax	= "You can’t create more then 10 rooms";
	public static string	TipsComingSoon		= "Coming Soon";
	public static string 	TipsSeatWasToken	= "Seat was taken";
	public static string 	TipsMaxBet			= "Maximum bet is ";
	public static string 	TipsCreditMax		= "Credit max out";

	public static string 	ErrorGameCreditMax	= "Game credit max out";
	public static string	ErrorOutdue			= "Out due to no action for 3 hands";
	public static string    ErrorNoConnect		= "Connection Failed, Please check Your Network";

	public static string    ErrorLogin			= "Login failed";
	public static string    ErrorInsufficient	= "Insufficient diamonds,please require to your agent";
	public static string    ErrorCantGame		= "Failed to continue the game: Player has insufficient diamond";

	public static string    ErrorKickGame		= "Forced to quit due to maintenance, sorry for the inconvenience";

	public static Common Instance
	{
		get
		{
			if (_instance == null)
			{
				lock(_lock)
				{
					if (_instance == null) 
						_instance = new Common();
				}
			}
			return _instance;
		}
	}

	public static Dictionary<string,  Texture> AvatarCache 	= new Dictionary<string, Texture>();

	public static IEnumerator DownAvatar(string url){
		if(!string.IsNullOrEmpty(url)){
			if(!AvatarCache.ContainsKey(url)){
				WWW www = new WWW(url);
				yield return www;
				if (www!=null && string.IsNullOrEmpty(www.error))
				{
					AvatarCache.Add (url, www.texture as Texture);
				} 
			}
		}
	}

	public static IEnumerator Load(UICircle av, string url)
	{
		if (!string.IsNullOrEmpty (url)) {
			if (AvatarCache.ContainsKey (url)) {
				av.texture = AvatarCache [url];
			} else {
				WWW www = new WWW(url);
				yield return www;
				if (www!=null && string.IsNullOrEmpty(www.error))
				{
					if (AvatarCache.ContainsKey (url)) {
						av.texture = AvatarCache [url];
					} else {
						AvatarCache.Add (url, www.texture as Texture);
						av.texture = www.texture as Texture;
					}
				} 
			}
		}
	}

	public static bool Sumbit(GameObject Tips,  GameObject Obj){
		if(Sumbiting){
			TipsOn(Tips, Obj, "Please wait.");
			return false;
		}
		Sumbiting = true;
		return true;
	}

	public static void Calling(GameObject Obj){
		EndCalling ();

		CallingObj = new GameObject ();
		CallingObj.AddComponent<RectTransform>();
		CallingObj.GetComponent<RectTransform> ().sizeDelta = new Vector2 (640, 1136);
		CallingObj.name = "Calling";
		CallingObj.transform.SetParent (Obj.transform);


		GameObject Mask = new GameObject ();
		Mask.AddComponent<Image> ();
		Mask.GetComponent<Image> ().color = new Color (0,0,0, 25.0f / 255);
		Mask.GetComponent<RectTransform> ().sizeDelta = new Vector2 (640, 1136);
		Mask.transform.SetParent (CallingObj.transform);


		GameObject Anime = new GameObject ();
		Anime.AddComponent<Image> ();
		Anime.GetComponent<Image>().sprite =  Resources.Load("Image/Common/calling", typeof(Sprite)) as Sprite;
		Anime.GetComponent<RectTransform> ().sizeDelta = new Vector2 (100, 100);
		Anime.transform.SetParent (CallingObj.transform);


		CallingObj.transform.localScale = new Vector3 (1,1,1);
		CallingObj.transform.localPosition = new Vector3 (0,0,0);

		Mask.transform.localScale = new Vector3 (1,1,1);
		Mask.transform.localPosition = new Vector3 (0,0,0);

		Anime.transform.localScale = new Vector3 (1,1,1);
		Anime.transform.localPosition = new Vector3 (0,0,0);

		Anime.transform.DOBlendableLocalRotateBy (new Vector3 (0, 0, -180), 0.8f).SetLoops(-1, LoopType.Incremental);
	}

	public static void EndCalling(){
		if(CallingObj != null){
			Destroy(CallingObj);
			CallingObj = null;
		}
	}

	public static void ErrorDialog(GameObject Obj, GameObject Parent, string text, EventTriggerListener.VoidDelegate call = null){
		GameObject Dialog = Instantiate(Obj) as GameObject;
		Dialog.transform.Find ("Conten").GetComponent<Text> ().text = text;
		Dialog.transform.SetParent (Parent.transform);
		Dialog.transform.localScale = new Vector3 (1,1,1);
		Dialog.transform.localPosition = new Vector3 (0,0,0);

		EventTriggerListener.Get(Dialog.transform.Find("Close").gameObject).onClick = CloseErrorDialog;

		if (call != null) {
			EventTriggerListener.Get (Dialog.transform.Find ("OK").gameObject).onClick = call;
		} else {
			EventTriggerListener.Get(Dialog.transform.Find("OK").gameObject).onClick = CloseErrorDialog;
		}
	}

	public static void CloseErrorDialog(GameObject Obj){
		Destroy (Obj.transform.parent.gameObject);
	}

	public static void CloseServerDialog(GameObject Obj, GameObject Parent, string text, EventTriggerListener.VoidDelegate call = null){
		GameObject Dialog = Instantiate(Obj) as GameObject;
		Dialog.transform.Find ("Conten").GetComponent<Text> ().text = text;
		Dialog.transform.SetParent (Parent.transform);
		Dialog.transform.localScale = new Vector3 (1,1,1);
		Dialog.transform.localPosition = new Vector3 (0,0,0);

		Dialog.transform.Find ("Close").gameObject.SetActive (false);
		EventTriggerListener.Get(Dialog.transform.Find("OK").gameObject).onClick = CloseServerDialog;
	}

	public static void CloseServerDialog(GameObject Obj){
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}

	public static void TipsOn(GameObject Obj, GameObject Parent, string text){
		GameObject Dialog = Instantiate(Obj) as GameObject;
		Dialog.transform.Find ("Text").GetComponent<Text> ().text = text;
		Dialog.transform.SetParent (Parent.transform);
		Dialog.transform.localScale = new Vector3 (1,1,1);
		Dialog.transform.localPosition = new Vector3 (0,-568,0);

		Sequence s = DOTween.Sequence ();
		s.Append (Dialog.transform.DOLocalMoveY (-480, 0.2f));
		s.Append (Dialog.transform.GetComponent<Image>().DOFade(0, 3f)).OnComplete(()=>TipsOff(Dialog));
		Dialog.transform.Find ("Text").GetComponent<Text> ().DOFade (0, 2f);
	}

	public static void TipsOff(GameObject d){
		Destroy (d);
	}

	public static string ToCarryNum(int num){
		string t = (Math.Abs(num)).ToString ();
		string temp = "";

		int index = 1;
		for(int i = t.Length - 1; i >= 0; i--){
			temp = t [i] + temp;

			if(index < t.Length){
				if(index % 3 == 0){
					temp = "," + temp;
				}
			}
			index++;
		}

		if(num < 0){
			temp = "-" + temp;
		}
		return temp;
	}

	public static long GetTimeStamp(bool bflag = true)
	{
		TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
		long ret;
		if (bflag)
			ret = Convert.ToInt64(ts.TotalSeconds);
		else
			ret = Convert.ToInt64(ts.TotalMilliseconds);
		return ret;
	}

	public static Notice InitNotices(Notice Obj, GameObject Parent){
		Notice nitice = Instantiate(Obj) as Notice;
		nitice.transform.SetParent (Parent.transform);
		nitice.transform.localScale = new Vector3 (1,1,1);
		nitice.transform.localPosition = new Vector3 (0,1136/2,0);
		return nitice;
	}

	public static void SetMusicConfig(bool music){
		StreamWriter sw;  
		string file = Application.persistentDataPath + "Config.txt";

		if(!File.Exists(file))  
		{  
			sw = File.CreateText(file);
		}  
		else  
		{  
			File.Delete (file);
			sw = File.CreateText(file);
		}  

		string con = "1";
		if(!music){con = "0";}

		sw.Write(con);
		sw.Close ();  
		sw.Dispose ();

		ConfigMusicOn = music;
	}

	public static bool GetMusicConfig(){
		
		StreamReader sr;

		string file = Application.persistentDataPath + "Config.txt";

		if(!File.Exists(file))  
		{  
			SetMusicConfig (true);
			return true;
		}  
			
		sr = File.OpenText(file);

		string str;  
		while ((str = sr.ReadLine ()) != null) {
			sr.Close ();  
			sr.Dispose ();
			if (str == "1") {return true;} else {return false;}
		}

		return true;
	}

	private Common() {}
}