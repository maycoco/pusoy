using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StateSorting : State {
	private int 	  CountDownTime			= 0;
	private Transform Layer 				= null;
	private Transform Layer1 				= null;
	private Transform Layer2				= null;
	private Tweener   TweenColor			= null;

	private List<int> HandPokers 			= new List<int>();
	private List<int> UpperPokers 			= new List<int>();
	private List<int> MiddlePokers 			= new List<int>();
	private List<int> UnderPokers 			= new List<int>();
	private List<int> SelectedPokers		= new List<int>();

	private int RoateAniIndex 				= 0;
	private int RoateCAniIndex 				= 0;

	private	GameObject Effect_BaoPai		= null;
	private	GameObject Effect_DX			= null;

	private Dictionary<int, Poker> Pokers	= new Dictionary<int, Poker>();
	private bool AlreadyComfim				= false;
	private bool GetLuckyConfim				= false;

	private bool PokerMoving 				= false;

	Dictionary<Msg.CardRank, List<uint[]>> RankResult = new Dictionary<Msg.CardRank, List<uint[]>> ();

	private GameObject PokerSelectObj	= null;

	//config
	List<Vector3> postype0 = new List<Vector3> ();
	List<Vector3> postype1 = new List<Vector3> ();
	List<string>  RanksText= new List<string>();

	// Use this for initialization
	void Start () {
	}

	void Awake(){
		Layer = GameObject.Find("Canvas").transform.Find("SortingLayer");
		Layer1 = Layer.Find ("SortingLayer01");
		Layer2 = Layer.Find ("SortingLayer02");

	
		postype0.Add (new Vector3( -252, -328, 0));
		postype0.Add (new Vector3( -160, -272, 0));
		postype0.Add ( new Vector3(-55, -244, 0));
		postype0.Add ( new Vector3(55, -244, 0));
		postype0.Add (new Vector3( 160, -272, 0));
		postype0.Add (new Vector3( 252, -328, 0));

		postype1.Add (new Vector3 (-252, -328, 0));
		postype1.Add ( new Vector3(-128, -270, 0));
		postype1.Add ( new Vector3(-0, -244, 0));
		postype1.Add (new Vector3( 128, -270, 0));
		postype1.Add (new Vector3 (252, -328, 0));

		RanksText.Add ("High Card");
		RanksText.Add ("One Pair");
		RanksText.Add ("Two Pair");
		RanksText.Add ("Three Of A Kind");
		RanksText.Add ("Straight");
		RanksText.Add ("Flush");
		RanksText.Add ("Full House");
		RanksText.Add ("Four Of A Kind");
		RanksText.Add ("Straight Flush");
	}

	// Update is called once per frame
	void Update () {
	}

	public void InitData(){
		RoateAniIndex = 0;
		RoateCAniIndex = 0;

		m_GameController.HideSeatLayer ();
		//m_GameController.HideTableInfo ();
		m_GameController.HideGameConsole ();
		Layer.gameObject.SetActive (true);

		RankResult.Clear ();
		Pokers.Clear ();
		HandPokers = new List<int> (Common.CPokers);
		HandPokers.Sort ();

		UpperPokers.Clear ();
		MiddlePokers.Clear ();
		UnderPokers.Clear ();
		SelectedPokers.Clear ();
		GetLuckyConfim = false;
	}

	public override void Enter(){
		Debug.Log ("==============================state sorting===================================");
		m_StateManage.m_StateFinish.ResrtUI ();
		m_GameController.m_GameConsole.CloseMenu ();

		InitData ();

		AdjustUI ();

		CountDownTime = Common.ConfigSortTime;
		BeginCountDown ();

		AdjustPokers ();
	}

	public override void DisEnter(){
		InitData ();
		AdjustUI ();

		CountDownTime = Common.ConfigSortTime;
		BeginCountDown ();
	
		AdjustPokers (true);
		ShowControlUI ();

		if (Common.CCPokers.Count > 0) {
			Layer.Find ("Waiting").gameObject.SetActive (true);
			AlreadyComfim = true;
			StopSorting ();
			Invoke ("ShowSynch", 1.5f);
		}
	}

	public void ShowSynch(){
		SynchPoker (Common.CCPokers);
		Common.CCPokers.Clear ();
	}

	public void ConfimCombin(){
		Layer.Find ("Waiting").gameObject.SetActive (false);
		Layer1.Find ("GetLucky").gameObject.SetActive (false);
		Layer1.Find ("Battle").gameObject.SetActive (false);
		Layer2.Find ("Types").gameObject.SetActive (false);

		if(Common.CCPokers.Count <= 0 && !AlreadyComfim){
			AutoSortConfrm ();
		}
	}

	public override void Exit(){
		CancelInvoke ();

		GameObject[] SortHandObjs = GameObject.FindGameObjectsWithTag("SortHand");
		for(int i = 0; i < SortHandObjs.Length; i++){
			Destroy (SortHandObjs[i]);
		}
		Layer.gameObject.SetActive (false);
	}

	 public override void AdjustUI(){
		m_StateManage.m_StateSeat.HideAutoBanker ();
		Layer.gameObject.SetActive (true);

		if (Effect_BaoPai != null) {Destroy (Effect_BaoPai);Effect_BaoPai = null;} 
		if (Effect_DX != null) {Destroy (Effect_DX);Effect_DX = null;} 

		GameObject[] SortHandObjs = GameObject.FindGameObjectsWithTag("SortHand");
		for(int i = 0; i < SortHandObjs.Length; i++){
			Destroy (SortHandObjs[i]);
		}

		CloseHelpLayer ();
		Layer.Find ("Waiting").gameObject.SetActive (false);
		Layer1.Find ("CountDown").gameObject.SetActive (false);
		HideConfim ();

		UpdateControlButton ();
		UpdatePokerTips ();
	}

	public void ShowControlUI(){
		//get lucky or battle
		Dictionary<Msg.CardRank, List<uint[]>> result = GetRanks(HandPokers);
		bool getlucky = false;
		foreach (KeyValuePair<Msg.CardRank, List<uint[]>> pair in result){
			if(pair.Key == Msg.CardRank.FourOfAKind || pair.Key == Msg.CardRank.StraightFlush){
				getlucky = true;
			}
		}

		if (getlucky) {
			Layer1.Find ("GetLucky").gameObject.SetActive (true);
			Layer1.Find ("Battle").gameObject.SetActive (true);
			Layer2.Find ("Types").gameObject.SetActive (false);
		} else {
			Battle ();
		}
	}

	public void HideConfim(){
		Layer2.Find ("Confim").gameObject.SetActive (false);
	}

	public void ShowConfim(){
		Layer2.Find ("Confim").gameObject.SetActive (true);
	}
		
	public void Battle(){
		HideConfim ();
		Layer1.Find ("GetLucky").gameObject.SetActive (false);
		Layer1.Find ("Battle").gameObject.SetActive (false);
		Layer2.Find ("Types").gameObject.SetActive (true);
		UpdateRanks ();
		AlreadyComfim = false;
		EnPokers ();
	}

	public void GetLucky(){
		HideConfim ();
		Layer1.Find ("GetLucky").gameObject.SetActive (false);
		Layer1.Find ("Battle").gameObject.SetActive (false);
		Layer2.Find ("Types").gameObject.SetActive (false);
		GetLuckyConfim = true;
		SortConfrmServer ();
	}

	public void BeginCountDown(){
		Layer1.Find ("CountDown/Clock").GetComponent<Image> ().color = new Color (53.0f / 255, 1.0f, 104.0f / 255);
		Layer1.Find ("CountDown/Clock").GetComponent<Image> ().fillAmount = 1.0f;
		Layer1.Find ("CountDown").gameObject.SetActive (true);

		TweenColor = Layer1.Find ("CountDown/Clock").GetComponent<Image> ().DOColor (new Color(255.0f / 255, 17.0f / 255, 17.0f / 255), Common.ConfigSortTime);
		InvokeRepeating("UpdateSortingTime", 0f, 1.0f);
	}

	public void UpdateSortingTime(){
		if (CountDownTime >= 0) {
			float temp = 1.0f /Common.ConfigSortTime;
			Layer1.Find ("CountDown/Clock/Text").GetComponent<Text> ().text = CountDownTime.ToString() + "s";
			Layer1.Find ("CountDown/Clock").GetComponent<Image> ().fillAmount = CountDownTime * temp;

			if(CountDownTime == 5 && !AlreadyComfim){
				m_GameController.PlayEffect (Effect.EIDA);
				Effect_DX = Instantiate(m_GameController.m_PrefabEffectED) as GameObject;
				Effect_DX.transform.SetParent (Layer.Find("EffectDX").transform);
				Effect_DX.transform.localScale = new Vector3 (1, 1, 1);
				Effect_DX.transform.localPosition = new Vector3 (0, 0, 0);
			}

			if(CountDownTime == 3){
				m_GameController.PlayEffect (Effect.CLOCK);
			}
		} 
		else {
			CancelInvoke ();
			Layer1.Find ("CountDown").gameObject.SetActive (false);
			SortConfrm ();
		}
		CountDownTime--;
	}

	public void AdjustPokers(bool ini = false){
		float iv = 0.15f;
		for (int i = 0; i <HandPokers.Count; i++) {
			Poker Poker = Instantiate(m_GameController.m_PrefabPoker) as Poker;
			Poker.canvas = GameObject.Find("Canvas").GetComponent<RectTransform>();
			Poker.StateSorting = this;
			Poker.PokerID = HandPokers [i];
			Poker.Belong  = "Hand";
			Poker.SiblingIndex = i;
			Poker.transform.name = "Poker" + Poker.PokerID;
			Poker.transform.tag = "SortHand";
			Poker.transform.SetParent(GameObject.Find("Pokers").transform);

			if (!ini) {
				Poker.ShowBack ();
			} else {
				Poker.ShowFace ();
			}

			Poker.transform.localPosition = m_GameController.DefaultHandPisitions [i];
			Poker.SetBelongPos (m_GameController.DefaultHandPisitions [i]);
			Poker.transform.localScale = new Vector3 (1, 1, 1);
			Pokers.Add (Poker.PokerID, Poker);

			if(!ini){
				Invoke ("RoateAni", iv*(i+1));
			}
		}
	}
		
	public void RoateAni(){
		m_GameController.PlayEffect (Effect.FAN);
		Pokers[HandPokers[RoateAniIndex]].transform.DORotate (new Vector3 (0, 180, 0), 0.12f).OnComplete(RoateAniCallBack);
		RoateAniIndex++;
	}

	public void RoateAniCallBack(){
		Pokers [HandPokers [RoateCAniIndex]].ShowFace ();
		Pokers [HandPokers [RoateCAniIndex]].transform.localRotation = new Quaternion (0,0,0,0);
		RoateCAniIndex++;

		if(RoateCAniIndex == HandPokers.Count){
			ShowControlUI ();
		}
	}

	public void UpdateHandPokers(float Time){
		ReseatSelectedState ();
		for (int i = 0; i < HandPokers.Count; i++) {
			int PokerID = HandPokers [i];
			if(Pokers.ContainsKey (PokerID)){
				Pokers[PokerID].ReseatSelected ();
				Pokers[PokerID].Belong = "Hand";
				Pokers[PokerID].SetBelongPos (m_GameController.DefaultHandPisitions [i]);
				Pokers[PokerID].SiblingIndex = i;
				Pokers[PokerID].transform.SetSiblingIndex(i);
				//Pokers[PokerID].AnimeMove(m_GameController.DefaultHandPisitions [i], Time);
				Pokers[PokerID].transform.localPosition = m_GameController.DefaultHandPisitions [i];
				Pokers[PokerID].transform.localScale = new Vector3 (1, 1, 1);
			}
		}
	}

	public void UpdateUpperPokers(float Time){
		float posx = Layer1.Find ("UpperPos").transform.localPosition.x;
		float posy = Layer1.Find ("UpperPos").transform.localPosition.y + 2;

		for (int i = 0; i < UpperPokers.Count; i++) {
			int PokerID = UpperPokers [i];
			if(Pokers.ContainsKey (PokerID)){
				Pokers[PokerID].AnimeMove(new Vector3 (posx, posy ,0), Time);
				Pokers[PokerID].ReseatSelected ();
				Pokers[PokerID].Belong = "Upper";
				Pokers[PokerID].SetBelongPos (new Vector3 (posx, posy ,0));
				//Pokers[PokerID].transform.localPosition = new Vector3(posx, posy, 0);
				Pokers[PokerID].transform.localScale = new Vector3 (1, 1, 1);
				posx += 93;
			}
		}
	}

	public void UpdateMiddlePokers(float Time){
		float posx = Layer1.Find ("MiddlePos").transform.localPosition.x;
		float posy = Layer1.Find ("MiddlePos").transform.localPosition.y+ 2;

		for (int i = 0; i < MiddlePokers.Count; i++) {
			int PokerID = MiddlePokers [i];
			if(Pokers.ContainsKey (PokerID)){
				Pokers[PokerID].AnimeMove(new Vector3 (posx, posy ,0), Time);
				Pokers[PokerID].ReseatSelected ();
				Pokers[PokerID].Belong = "Middle";
				Pokers[PokerID].SetBelongPos (new Vector3 (posx, posy ,0));
				//Pokers[PokerID].transform.localPosition = new Vector3(posx, posy, 0);
				Pokers[PokerID].transform.localScale = new Vector3 (1, 1, 1);
				posx += 93;
			}
		}
	}

	public void UpdateUnderPokers(float Time){
		float posx = Layer1.Find ("UnderPos").transform.localPosition.x;
		float posy = Layer1.Find ("UnderPos").transform.localPosition.y+ 2;
	
		for (int i = 0; i < UnderPokers.Count; i++) {
			int PokerID = UnderPokers [i];
			if(Pokers.ContainsKey (PokerID)){
				Pokers[PokerID].AnimeMove(new Vector3 (posx, posy ,0), Time);
				Pokers[PokerID].ReseatSelected ();
				Pokers[PokerID].Belong = "Under";
				Pokers[PokerID].SetBelongPos (new Vector3 (posx, posy ,0));
				//Pokers[PokerID].transform.localPosition = new Vector3(posx, posy, 0);
				Pokers[PokerID].transform.localScale = new Vector3 (1, 1, 1);
				posx += 93;
			}
		}
	}

	public void DragMovePokers(string belong, string tag, int[] pokers){
		if(tag == "Upper"){
			if (SelectedPokers.Count > 3 || (SelectedPokers.Count + UpperPokers.Count) > 3) {
				MovePokers(belong, "Other", pokers);
			} else {
				BatchAddControl(tag);
			}

		}
		else if(tag == "Middle"){
			if (SelectedPokers.Count > 5 || (SelectedPokers.Count + MiddlePokers.Count) > 5) {
				MovePokers(belong, "Other", pokers);
			} else {
				BatchAddControl(tag);
			}
		}
		else if(tag == "Under"){
			if (SelectedPokers.Count > 5 || (SelectedPokers.Count + UnderPokers.Count) > 5) {
				MovePokers(belong, "Other", pokers);
			} else {
				BatchAddControl(tag);
			}
		}
		else{
			MovePokers(belong, "Other", pokers);
		}

		UpdateControlButton ();
	}

	 public void MovePokers(string belong, string tag, int[] pokers){
		if(pokers.Length <= 0){return;}

		if(belong == tag){
			for(int i = 0; i < pokers.Length; i++){
				Pokers [pokers[i]].ResetBelongPos ();
				Pokers [pokers[i]].transform.SetSiblingIndex (Pokers [pokers[i]].SiblingIndex);
				return;
			}
		}

		switch (tag) {
		case "Other":
			for (int i = 0; i < pokers.Length; i++) {
				Pokers [pokers [i]].transform.SetSiblingIndex (Pokers [pokers [i]].SiblingIndex);
				Pokers [pokers [i]].ResetBelongPos ();
				Pokers [pokers [i]].RiwerSelected ();
			}
			break;


		case "Upper":
			if (UpperPokers.Count >= 3) {MovePokers (belong, "Other", pokers);return;}

			if (UpperPokers.Count + pokers.Length <= 3) {
				DeletePokerFromTag (belong, pokers);
				AddPokerToTag (tag, pokers);
				PokerMoving = true;
				UpdateUpperPokers (0.2f);
				DisControlButton (0.2f);
				UpdateBelongPokets (belong);
				UpdateControlButton ();
				UpdatePokerTips ();
			} 
			break;

		case "Middle":
			if(MiddlePokers.Count >= 5){MovePokers(belong, "Other", pokers);return;}

			if(MiddlePokers.Count + pokers.Length <= 5){
				DeletePokerFromTag (belong, pokers);
				AddPokerToTag (tag, pokers);
				PokerMoving = true;
				UpdateMiddlePokers (0.2f);
				DisControlButton (0.2f);
				UpdateBelongPokets (belong);
				UpdateControlButton ();
				UpdatePokerTips ();
			}

			break;

		case "Under":
			if (UnderPokers.Count >= 5) {
				MovePokers (belong, "Other", pokers);
				return;
			}

			if(UnderPokers.Count + pokers.Length <= 5){
				DeletePokerFromTag (belong, pokers);
				AddPokerToTag (tag, pokers);
				PokerMoving = true;
				UpdateUnderPokers (0.2f);
				DisControlButton (0.2f);
				UpdateBelongPokets (belong);
				UpdateControlButton ();
				UpdatePokerTips ();
			}
			break;

		case "Hand":
			if(HandPokers.Count >= 13){MovePokers(belong, "Other", pokers);return;}

			if(HandPokers.Count + pokers.Length <= 13){
				DeletePokerFromTag (belong, pokers);
				AddPokerToTag (tag, pokers);
				UpdateHandPokers (0.1f);
				UpdateBelongPokets (belong);
				UpdateControlButton ();
				UpdatePokerTips ();
			}
			break;
		}

		AutoSelected ();
		UpdateRanks ();
		if (HandPokers.Count <= 0) {ShowConfim ();} else {HideConfim ();}
	}

	public void EnbleControlButton(){
		PokerMoving = false;
	}

	public void DisControlButton(float time){
		PokerMoving = true;
		Invoke("EnbleControlButton", time);
	}

	public void AutoSelected(){
		if(UpperPokers.Count + HandPokers.Count <= 3 ){
			List<uint> temp = new List<uint> ();

			foreach(int p in HandPokers){
				temp.Add ((uint)p);
			}

			SelectPokers (temp.ToArray());
		}
		else if(MiddlePokers.Count + HandPokers.Count <= 5 ){
			List<uint> temp = new List<uint> ();

			foreach(int p in HandPokers){
				temp.Add ((uint)p);
			}

			SelectPokers (temp.ToArray());
		}
		else if(UnderPokers.Count + HandPokers.Count <= 5 ){
			List<uint> temp = new List<uint> ();

			foreach(int p in HandPokers){
				temp.Add ((uint)p);
			}

			SelectPokers (temp.ToArray());
		}
	}

	public void UpdateBelongPokets(string belong){
		switch (belong) {
		case "Upper":
			UpdateUpperPokers (0.2f);
			break;

		case "Middle":
			UpdateMiddlePokers (0.2f);
			break;

		case "Under":
			UpdateUnderPokers (0.2f);
			break;

		case "Hand":
			UpdateHandPokers (0.1f);
			break;
		}
	}

	public void AddPokerToTag(string tag, int[] pokers){
		
		switch (tag) {
		case "Upper":
			foreach(int p in pokers){
				UpperPokers.Add (p);
			}
			break;

		case "Middle":
			foreach(int p in pokers){
				MiddlePokers.Add (p);
			}
			break;

		case "Under":
			foreach(int p in pokers){
				UnderPokers.Add (p);
			}
			break;

		case "Hand":
			foreach (int p in pokers) {
				HandPokers.Add (p);
			}
			HandPokers.Sort ();
			break;
		}

		m_GameController.PlayEffect (Effect.SEKECT);
	}

	 public void DeletePokerFromTag(string tag, int[] pokers){
		switch (tag) {
		case "Upper":
			foreach(int p in pokers){
				for (int i = UpperPokers.Count - 1; i >= 0; i--) {
					if(p == UpperPokers[i]){
						UpperPokers.RemoveAt (i);
					}
				}
			}
			break;

		case "Middle":
			foreach(int p in pokers){
				for (int i = MiddlePokers.Count - 1; i >= 0; i--) {
					if(p == MiddlePokers[i]){
						MiddlePokers.RemoveAt (i);
					}
				}
			}
			break;

		case "Under":
			foreach(int p in pokers){
				for (int i = UnderPokers.Count - 1; i >= 0; i--) {
					if(p == UnderPokers[i]){
						UnderPokers.RemoveAt (i);
					}
				}
			}
			break;

		case "Hand":
			foreach(int p in pokers){
				for (int i = HandPokers.Count - 1; i >= 0; i--) {
					if(p == HandPokers[i]){
						HandPokers.RemoveAt (i);
					}
				}
			}
			break;
		}
	}

	public void AddSelectPokers(int PokerID){
		if(Pokers [PokerID].Belong != "Hand"){
			MovePokers (Pokers [PokerID].Belong, "Hand", new int[]{PokerID});
			return;
		}

		if(SelectedPokers.Count >= 5){
			Pokers [PokerID].ReseatSelected ();
			return;
		}

		SelectedPokers.Add (PokerID);
		m_GameController.PlayEffect (Effect.SEKECT);
		Pokers [PokerID].Selected ();
	}

	public void RemoveSelectPokers(int PokerID){
		foreach(int value in SelectedPokers){
			if(value == PokerID){
				Pokers [PokerID].CancelSelect ();
				SelectedPokers.Remove (value);
				return;
			}
		}
	}

	public int GetSelectedPokerCount(){
		return SelectedPokers.Count;
	}

	public void ReseatSelectedState(){
		SelectedPokers.Clear ();
	}
		
	public void UpdateControlButton(){
		if (UpperPokers.Count < 3) {
			Layer1.Find ("UpperAddControl").gameObject.SetActive (true);
			Layer1.Find ("UpperRemoveControl").gameObject.SetActive (false);
		} else {
			Layer1.Find ("UpperRemoveControl").gameObject.SetActive (true);
			Layer1.Find ("UpperAddControl").gameObject.SetActive (false);
		}

		if (MiddlePokers.Count < 5) {
			Layer1.Find ("MiddleAddControl").gameObject.SetActive (true);
			Layer1.Find ("MiddleRemoveControl").gameObject.SetActive (false);
		} else {
			Layer1.Find ("MiddleRemoveControl").gameObject.SetActive (true);
			Layer1.Find ("MiddleAddControl").gameObject.SetActive (false);
		}

		if (UnderPokers.Count < 5) {
			Layer1.Find ("UnderAddControl").gameObject.SetActive (true);
			Layer1.Find ("UnderRemoveControl").gameObject.SetActive (false);
		} else {
			Layer1.Find ("UnderRemoveControl").gameObject.SetActive (true);
			Layer1.Find ("UnderAddControl").gameObject.SetActive (false);
		}
	}

	public void BatchAddControl(string Tag){
		if(PokerMoving){return;}
		if(AlreadyComfim){return;}
		if(SelectedPokers.Count <= 0){return;}

		if(Tag == "Upper"){
			if (UpperPokers.Count + SelectedPokers.Count > 3) {
				if( SelectedPokers.Count <= 3){
					int[] selects = SelectedPokers.ToArray();
					BatchRemoveControl("Upper");
					MovePokers("Hand", Tag, selects);
				}
			} else {
				MovePokers("Hand", Tag, SelectedPokers.ToArray());
			}

		}
		else if(Tag == "Middle"){
			if (MiddlePokers.Count + SelectedPokers.Count > 5) {
				if (SelectedPokers.Count <= 5) {
					int[] selects = SelectedPokers.ToArray ();
					BatchRemoveControl ("Middle");
					MovePokers ("Hand", Tag, selects);
				}
			} else {
				MovePokers("Hand", Tag, SelectedPokers.ToArray());
			}
		}
		else if(Tag == "Under"){
			if (UnderPokers.Count + SelectedPokers.Count > 5) {
				if (SelectedPokers.Count <= 5) {
					int[] selects = SelectedPokers.ToArray ();
					BatchRemoveControl ("Under");
					MovePokers ("Hand", Tag, selects);
				}
			} else {
				MovePokers ("Hand", Tag, SelectedPokers.ToArray ());
			}
		}

		UpdateControlButton ();
		UpdatePokerTips ();
	}

	public void BatchRemoveControl(string Tag){
		if(PokerMoving){return;}
		if(AlreadyComfim){return;}
		if(Tag == "Upper"){
			MovePokers(Tag, "Hand", UpperPokers.ToArray());
		}
		else if(Tag == "Middle"){
			MovePokers(Tag, "Hand", MiddlePokers.ToArray());
		}
		else if(Tag == "Under"){
			MovePokers(Tag, "Hand", UnderPokers.ToArray());
		}

		UpdateControlButton ();
		UpdatePokerTips ();
	}

	public void EndDragSelects(){
		foreach (int id in SelectedPokers) {
			Pokers [id].Border.SetActive (true);
		}

		if(PokerSelectObj != null){Destroy (PokerSelectObj);PokerSelectObj = null;}

		foreach (int id in SelectedPokers) {
			Color temp = Pokers [id].transform.GetComponent<Image> ().color ;
			Pokers [id].transform.GetComponent<Image> ().color = new Color (temp.r, temp.g, temp.b, 1);
		}
	}

	public void DragSelects(int PokerID){
		foreach (int id in SelectedPokers) {
			if(id != PokerID){
				Pokers [id].Border.SetActive (false);
			}
		}

		if(SelectedPokers.Count <= 1){return;}
		if(PokerSelectObj != null){Destroy (PokerSelectObj);PokerSelectObj = null;}

		GameObject selected = Instantiate(m_GameController.m_PrefabSelectAll) as GameObject;
		PokerSelectObj = selected;
		selected.transform.SetParent (Pokers [PokerID].transform);
		selected.transform.localScale = new Vector3(1,1,1);
		selected.transform.localPosition = new Vector3 (0,0,0);

		int index = 0;
		foreach (int id in SelectedPokers) {
			if(id != PokerID){
				selected.transform.Find ("Poker" + index).gameObject.SetActive (true);
				selected.transform.Find("Poker"+index).GetComponent<Image>().sprite = Resources.Load ("Image/Poker/" + id, typeof(Sprite)) as Sprite;
				index++;
			}
		}

		foreach (int id in SelectedPokers) {
			if(id != PokerID){
				Color temp = Pokers [id].transform.GetComponent<Image> ().color ;
				Pokers [id].transform.GetComponent<Image> ().color = new Color (temp.r, temp.g, temp.b, 70.0f/255);
			}
		}
	}

	public void DisPokers(){
		foreach (KeyValuePair<int, Poker> pair in Pokers){
			pair.Value.CloseTouchSwitch ();
		}
	}

	public void EnPokers(){
		foreach (KeyValuePair<int, Poker> pair in Pokers){
			pair.Value.OpenTouchSwitch ();
		}
	}

	public void ShowHelpLayer(){
		Layer.Find ("HelpLayer").gameObject.SetActive (true);
	}

	public void CloseHelpLayer(){
		Layer.Find ("HelpLayer").gameObject.SetActive (false);
	}

	public Dictionary<Msg.CardRank, List<uint[]>> GetRanks(List<int> pokers){
		Dictionary<Msg.CardRank, List<uint[]>> result = new Dictionary<Msg.CardRank, List<uint[]>> ();

		if(pokers.Count > 0){
			uint[] t = new uint[pokers.Count];
			for(int i = 0; i < pokers.Count; i++){
				t [i] = (uint)pokers [i];
			}
			result = Pusoy.CardRankFinder.Find(t);
		}

		List<Msg.CardRank> ranks = new List<Msg.CardRank> ();
		foreach (KeyValuePair<Msg.CardRank, List<uint[]>> pair in result){
			ranks.Add (pair.Key);
		}

		int c = 0;
		foreach (Msg.CardRank r in ranks){
			if(c >= 6){
				result.Remove (r);
			}
			c++;
		}
		return result;
	}

	public void UpdateRanks(){
		for (int i = Layer2.Find("Types/PokerType").childCount - 1; i >= 0; i--) {
			Destroy (Layer2.Find ("Types/PokerType").GetChild (i).gameObject);
		}

		RanksIndexs.Clear ();
		RankResult.Clear ();
		RankResult = GetRanks (HandPokers);

		if (RankResult.Count <= 0) {
			Layer2.Find ("Types/Line").gameObject.SetActive (false);
			return;
		} else {
			Layer2.Find ("Types/Line").gameObject.SetActive (true);
		}


		int index = 0;
		int posindex = 0;

		List<Vector3> pos = new List<Vector3> ();
		bool jo = (RankResult.Count % 2 == 1) ? true : false;
		if (jo) {pos = postype1;} else {pos = postype0;}
		posindex = (pos.Count - RankResult.Count) / 2;

		List<Msg.CardRank> ranks = new List<Msg.CardRank> ();
		foreach (KeyValuePair<Msg.CardRank, List<uint[]>> pair in RankResult) {
			ranks.Add (pair.Key);
		}
		ranks.Sort ();

		for(int i = 0; i < ranks.Count; i++){
			GameObject Rank = Instantiate(m_GameController.m_PrefabRank) as GameObject;
			Rank.GetComponent<Image>().sprite = Resources.Load ("Image/Game/poker_type" + index, typeof(Sprite)) as Sprite;
			Rank.transform.Find("Text").GetComponent<Text> ().text = RanksText[(int)ranks[i]];
			Rank.transform.SetParent (Layer2.Find("Types/PokerType"));
			Rank.transform.localScale = new Vector3 (0,0,0);
			Sequence mySequence = DOTween.Sequence();  
			mySequence.Append (Rank.transform.DOScale (new Vector3 (1.2f, 1.2f, 1.2f), 0.13f));
			mySequence.Append (Rank.transform.DOScale (new Vector3 (1, 1, 1), 0.05f));
			mySequence.Play ();
			//Rank.transform.DOScale (new Vector3 (1, 1, 1), 0.13f);
			Rank.transform.localPosition = pos [posindex];
			Rank.name = ((int)(ranks[i])).ToString();
			EventTriggerListener.Get(Rank).onClick = onClickRanksHandler;
			index++;
			posindex++;
		}
	}

	private List<int> RanksIndexs = new List<int>();

	public void onClickRanksHandler(GameObject obj){
		if(AlreadyComfim){return;}

		foreach (KeyValuePair<Msg.CardRank, List<uint[]>> pair in RankResult) {
			if(((int)pair.Key).ToString() == obj.name){
				
				int index = RanksIndexs.IndexOf ((int)pair.Key);
				if (index == -1) {
					RanksIndexs.Clear ();
					RanksIndexs.Add ((int)pair.Key);
				} else {
					if (pair.Value.Count == RanksIndexs.Count) {
						RanksIndexs.Clear ();
						RanksIndexs.Add ((int)pair.Key);
					} else {
						RanksIndexs.Add ((int)pair.Key);
					}

				}
					
				if(RanksIndexs.Count > 0){
					SelectPokers (pair.Value [RanksIndexs.Count - 1]);
				}
				return;
			}
		}
	}

	public void SelectPokers(uint[] pokers){
		UpdateHandPokers (0);
		SelectedPokers.Clear ();

		foreach(uint p in pokers){
			int id = (int)p;
			if(Pokers[id] && !Pokers[id].IsSelected){
				SelectedPokers.Add (id);
				Pokers [id].Selected ();
			}
		}
	}

	public Msg.CardRank GetRanksType(List<int> pokers){
		List<uint> t = new List<uint> ();
		foreach(int p in pokers){
			t.Add ((uint)p);
		}
        var arr = t.ToArray();
        return Pusoy.CardRankFinder.GetCardRank(ref arr);
	}

	public void UpdatePokerTips(){
		Color gcolor = new Color (0, 255.0f / 255, 90.0f / 255);
		Color rcolor = new Color (255.0f / 255, 72.0f / 255, 0);

		if (UpperPokers.Count <= 0) {
			Layer1.Find ("UpperTips").GetComponent<Text> ().text = "Good";
			Layer1.Find ("UpperTips").GetComponent<Text> ().color = gcolor;
		} else {
			Layer1.Find ("UpperTips").GetComponent<Text> ().text = RanksText [(int)GetRanksType (UpperPokers)];
		}

		if (MiddlePokers.Count <= 0) {
			Layer1.Find ("MiddleTips").GetComponent<Text> ().text = "Better";
			Layer1.Find ("MiddleTips").GetComponent<Text> ().color = gcolor;
		} else {
			Layer1.Find ("MiddleTips").GetComponent<Text> ().text = RanksText [(int)GetRanksType (MiddlePokers)];
		}

		if (UnderPokers.Count <= 0) {
			Layer1.Find ("UnderTips").GetComponent<Text> ().text = "Best";
			Layer1.Find ("UnderTips").GetComponent<Text> ().color = gcolor;
		} else {
			Layer1.Find ("UnderTips").GetComponent<Text> ().color = gcolor;
			Layer1.Find ("UnderTips").GetComponent<Text> ().text = RanksText [(int)GetRanksType (UnderPokers)];
		}

		if (UpperPokers.Count > 0 && MiddlePokers.Count > 0) {
			List<uint> t = new List<uint> ();
			foreach(int p in UpperPokers){
				t.Add ((uint)p);
			}

			List<uint> t1 = new List<uint> ();
			foreach(int p in MiddlePokers){
				t1.Add ((uint)p);
			}

			if (Pusoy.CardRankFinder.Compare (t.ToArray (), t1.ToArray ()) > 0) {
				Layer1.Find ("UpperTips").GetComponent<Text> ().color = rcolor;
			} else {
				Layer1.Find ("UpperTips").GetComponent<Text> ().color = gcolor;
			}
		}

		if (MiddlePokers.Count > 0 && UnderPokers.Count > 0) {
			
			List<uint> t = new List<uint> ();
			foreach(int p in MiddlePokers){
				t.Add ((uint)p);
			}

			List<uint> t1 = new List<uint> ();
			foreach(int p in UnderPokers){
				t1.Add ((uint)p);
			}


			if (Pusoy.CardRankFinder.Compare (t.ToArray (), t1.ToArray ()) > 0) {
				Layer1.Find ("MiddleTips").GetComponent<Text> ().color = rcolor;
			} else {
				Layer1.Find ("MiddleTips").GetComponent<Text> ().color = gcolor;
			}
		}
	}

	public bool PlayBaoPai(){
		bool baopai = false;

		if (UpperPokers.Count > 0 && MiddlePokers.Count > 0) {
			List<uint> t = new List<uint> ();
			foreach(int p in UpperPokers){
				t.Add ((uint)p);
			}

			List<uint> t1 = new List<uint> ();
			foreach(int p in MiddlePokers){
				t1.Add ((uint)p);
			}

			if(Pusoy.CardRankFinder.Compare (t.ToArray(), t1.ToArray()) > 0){
				baopai = true;
			}
		}

		if (MiddlePokers.Count > 0 && UnderPokers.Count > 0) {
			List<uint> t = new List<uint> ();
			foreach(int p in MiddlePokers){
				t.Add ((uint)p);
			}

			List<uint> t1 = new List<uint> ();
			foreach(int p in UnderPokers){
				t1.Add ((uint)p);
			}

			if(Pusoy.CardRankFinder.Compare (t.ToArray(), t1.ToArray()) > 0){
				baopai = true;
			}
		}

		if(baopai){
			if (Effect_BaoPai != null) {
				Destroy (Effect_BaoPai);
				Effect_BaoPai = null;
			} 

			m_GameController.PlayEffect (Effect.BOLI);
			Effect_BaoPai = Instantiate(m_GameController.m_PrefabEffectSL) as GameObject;
			Effect_BaoPai.transform.SetParent (Layer);
			Effect_BaoPai.transform.localScale = new Vector3 (1, 1, 1);
			Effect_BaoPai.transform.localPosition = new Vector3 (0, 0, 0);
		}

		return baopai;
	}

	public void StopSorting(){
		AlreadyComfim = true;
		DisPokers ();
		HideConfim ();
		HandPokers.Clear ();
		UpdateRanks ();
	}

	public void SortConfrm(){
		StopSorting ();

		Layer.Find ("Waiting").gameObject.SetActive (true);

		if (!PlayBaoPai ()) {SortConfrmServer ();
		} else {Invoke("SortConfrmServer", 2.0f);}
	}

	public void AutoSortConfrm(){
		StopSorting ();
		CancelInvoke ();
		Layer1.Find ("CountDown").gameObject.SetActive (false);

		if(!AlreadyComfim){
			SortConfrmServer ();
		}
	}

	public void SortConfrmServer(){
		if (TweenColor != null) {TweenColor.Kill (); TweenColor = null;}
		AlreadyComfim = true;

		Msg.CardGroup upper_pokers = new Msg.CardGroup();
		Msg.CardGroup middle_pokers = new Msg.CardGroup();
		Msg.CardGroup under_pokers = new Msg.CardGroup();

		for(int i = 0; i < UpperPokers.Count; i++){
			upper_pokers.Cards.Add((uint)UpperPokers[i]);
		}

		for(int i = 0; i < MiddlePokers.Count; i++){
			middle_pokers.Cards.Add((uint)MiddlePokers[i]);
		}

		for(int i = 0; i < UnderPokers.Count; i++){
			under_pokers.Cards.Add((uint)UnderPokers[i]);
		}

		List<Msg.CardGroup> cards = new List<Msg.CardGroup> ();
		cards.Add (upper_pokers);
		cards.Add (middle_pokers);
		cards.Add (under_pokers);

		m_GameController.CombineServer (cards, GetLuckyConfim);
		HideConfim ();
	}

	public void SynchPoker(List<List<uint>> cards){
		UpperPokers.Clear ();
		MiddlePokers.Clear ();
		UnderPokers.Clear ();
		HandPokers.Clear ();

		foreach(uint c in cards[0]){
			UpperPokers.Add ((int)c);
			UpdateUpperPokers (0.3f);
		}

		foreach(uint c in cards[1]){
			MiddlePokers.Add ((int)c);
			UpdateMiddlePokers (0.3f);
		}

		foreach(uint c in cards[2]){
			UnderPokers.Add ((int)c);
			UpdateUnderPokers (0.3f);
		}

		UpdatePokerTips ();
		DisPokers ();
	}
}
