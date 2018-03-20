using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	public string 		FB_id     = "";
	public Sprite 		FB_avatar = null;
	public uint			Bet		  = 0;
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
	public  static Sprite   FB_avatar		= 	null;

	//UserInfo
	public static int		DiamondAmount	= 0;	

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

	private Common() {}
}
