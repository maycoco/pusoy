using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Result{
	public string user_id;
	public string access_token;
}

public class AccountResult{
	public string name;
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

public class Common
{
	private static volatile Common _instance;
	private static object _lock = new object();

	// Data
	public 	static string 	FB_id			=	"";
	public  static string 	FB_access_token	=	"";
	public  static string 	FB_name			=	"";
	public  static uint 	Uid				= 	0;
	public  static string   FB_avatar		= 	"";

	//UserInfo
	public static uint		DiamondAmount	= 0;	

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
	public static bool		CAutoBanker 	= true;
	public static Msg.GameState CState			= Msg.GameState.Ready;
	public static List<PlayerInfo> CPlayers	= new List<PlayerInfo>();
	public static List<int>	CPokers			= new List<int>();
	public static List<int>	CSeats			= new List<int>();

	//Career
	public static List<uint> CareerWins 	= new List<uint>();

	//Room Config
	public 	static uint[] 	ConfigMinBet 	= new uint[]{5, 20, 100, 500};
	public 	static uint[] 	ConfigHands 	= new uint[]{20, 50, 100, 200, 500, 0};
	public 	static uint[] 	ConfigCredit 	= new uint[]{0, 20, 40, 60, 80, 100};
	public  static List<int> ConfigSeatOrder= new List<int> ();

	public  static int 		ConfigBetTime 	= 10;
	public  static int 		ConfigSortTime	= 45;
	public  static int      ConfigFinishTime= 8;

	//Chips Config
	public static int[] 	ConfigChips 	= {1, 2 ,10, 20};
	public static int 		ConfigMaxChips	= 20;

	//CareerReqDays
	public static uint 		ConfigCareerDays = 30;

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

//	public static IEnumerator Load(Image img, string url)
//	{
//		double startTime = (double) Time.time;
//
//		WWW www = new WWW(url);
//		yield return www;
//		if (www!=null && string.IsNullOrEmpty(www.error))
//		{
//
//			Texture2D texture = www.texture;
//			Sprite sprite = Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(0.5f,0.5f) );
//
//			img.sprite = sprite;
//			double time = (double)Time.time - startTime;
//		} 
//	}

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


	private Common() {}
}

