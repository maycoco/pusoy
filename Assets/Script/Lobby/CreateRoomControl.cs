using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CreateRoomControl : MonoBehaviour {
	public LobbyController	LobbyControl;
	public GameObject 		Commingsoon;

	private List<uint>	MinBetArray 		= new List<uint>();
	private List<uint>	HandsArray 			= new List<uint>();
	private List<uint> 	CreditPointsArray 	= new List<uint>();

	private int 		CurMinBet;
	private int			CurCreditPoints;
	private int 		CurHands;
	private bool		is_share;

	// Use this for initialization
	void Start () {
	}

	void Awake(){
	}

	// Update is called once per frame
	void Update () {
	}

	public void Enter(){
		CurMinBet 		= 0;
		CurHands 		= 0;

		MinBetArray = new List<uint> (Common.ConfigMinBet);
		HandsArray 	= new List<uint> (Common.ConfigHands);

		AdjustUI ();

		transform.localPosition = new Vector3(-640, 0, 0);
		Sequence s = DOTween.Sequence ();
		s.Append (transform.DOLocalMoveX (30, 0.2f));
		s.Append (transform.DOLocalMoveX (0, 0.2f));
		s.Play ();
	}

	public void Exit(){
		transform.DOLocalMoveX (-640, 0.15f);
		LobbyControl.PlayerButtonEffect ();
	}

	void AdjustUI(){
		transform.Find ("InputName").GetComponent<InputField> ().text = Common.FB_name + "'s room";
		transform.Find ("BetSize/Slider").GetComponent<Slider> ().value = CurMinBet;
		transform.Find ("MaxHands/Slider").GetComponent<Slider> ().value = CurHands;

		for(int i = 0; i < HandsArray.Count; i++){
			Text Handcount = this.transform.Find ("MaxHands/Hands/HandAmount" + i + "/Text").GetComponent<Text> ();
			if (HandsArray [i] == 0) {
				Handcount.text = "∞";
			} else {
				Handcount.text = HandsArray [i].ToString();
			}
		}

		UpdateHands ();
		UpdateBetSize ();
	}

	void UpdateCreditPoints(){
		if (CreditPointsArray[CurCreditPoints] == 0) {
			transform.Find ("CreditPoints/Amount").GetComponent<Text> ().text = "Unlimited";
		} else {
			transform.Find ("CreditPoints/Amount").GetComponent<Text> ().text = Common.ToCarryNum ((int)CreditPointsArray[CurCreditPoints]);
		}
	}

	public void CreditPointsControl(int type){
		LobbyControl.PlayerButtonEffect ();

		if(type == 0){
			if(CurCreditPoints > 0){
				CurCreditPoints--;
			}
		}
		else if(type == 1){
			if(CurCreditPoints < (CreditPointsArray.Count - 1)){
				CurCreditPoints++;
			}
		}

		UpdateCreditPoints ();
	}

	public void UpdateBetSize(){
		transform.Find ("BetSize/MinBet").GetComponent<Text> ().text = MinBetArray [CurMinBet].ToString ();
		transform.Find ("BetSize/MaxBet").GetComponent<Text> ().text = (MinBetArray [CurMinBet] * 20).ToString ();

		CreditPointsArray.Clear ();

		uint maxbet = MinBetArray [CurMinBet] * 20;
		for(int i = 0; i < Common.ConfigCredit.Length; i++){
			CreditPointsArray.Add (maxbet * Common.ConfigCredit[i]);
		}

		CurCreditPoints = CreditPointsArray.Count - 1;
		UpdateCreditPoints ();
	}

	public void UpdateHands(){
		for (int i = 0; i < HandsArray.Count; i++) {
			Text Handcount = this.transform.Find ("MaxHands/Hands/HandAmount" + i + "/Text").GetComponent<Text> ();
			Handcount.color = new Color (55.0f / 255, 220.0f / 255, 240.0f / 255);
		}

		this.transform.Find ("MaxHands/Hands/HandAmount" + CurHands + "/Text").GetComponent<Text> ().color = new Color (1,1,1);
		int amount = (int)HandsArray [CurHands] * 2;
		this.transform.Find ("RoomRate/Amount").GetComponent<Text> ().text = amount.ToString ();
	}

	public void ChangeMinBet(){
		CurMinBet = (int)transform.Find ("BetSize/Slider").GetComponent<Slider> ().value;
		UpdateBetSize ();
	}

	public void ChangeHands(){
		CurHands = (int)transform.Find ("MaxHands/Slider").GetComponent<Slider> ().value;
		UpdateHands ();
	}

	public void OnChangeHands(int index){
		transform.Find ("MaxHands/Slider").GetComponent<Slider> ().value = index;
		UpdateHands ();
	}

	public void CheckIn(){
		LobbyControl.PlayerButtonEffect ();

		string roomname 	= this.transform.Find ("InputName").GetComponent<InputField> ().text;

		if(string.IsNullOrEmpty(roomname)){
			return;
		}

		uint min_bet 		= MinBetArray [CurMinBet];
		uint max_bet 		= min_bet * 20;
		uint hands 			= HandsArray [CurHands];
		uint creditpoints = Common.ConfigCredit[CurCreditPoints];
		bool is_share 		= this.transform.Find ("RoomRate/Shared").GetComponent<Toggle> ().isOn;

		if(!string.IsNullOrEmpty(roomname)){
			Common.CRoom_name 	= roomname;
			Common.CMin_bet   	= min_bet;
			Common.CMax_bet		= max_bet;
			Common.CHands		= hands;
			Common.CCredit_points= creditpoints;
			Common.CIs_share	= is_share;
			Common.CState 		= Msg.GameState.Ready;

			LobbyControl.CreatRoomServer (roomname, min_bet, max_bet, hands, creditpoints, is_share);
		}
	}

	public void OpenCommingSoon(){
		//Commingsoon.SetActive (true);
		Common.TipsOn (LobbyControl.PrefabTips, LobbyControl.Canvas, Common.TipsComingSoon);
	}

	public void CloseCommingSoon(){
		//Commingsoon.SetActive (false);
	}

	//滑动退出
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
					Exit ();
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