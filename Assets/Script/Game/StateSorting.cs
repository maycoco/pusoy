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
	private bool AlreadyComfim				= true;
	private bool GetLuckyConfim				= false;

	Dictionary<Msg.CardRank, List<uint[]>> RankResult = new Dictionary<Msg.CardRank, List<uint[]>> ();

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

		RanksText.Add ("high card");
		RanksText.Add ("one pair");
		RanksText.Add ("two pair");
		RanksText.Add ("three of a kind");
		RanksText.Add ("straight");
		RanksText.Add ("flush");
		RanksText.Add ("full house");
		RanksText.Add ("four of a kind");
		RanksText.Add ("straight flush");
	}

	// Update is called once per frame
	void Update () {
	}

	public override void Enter(){
		Debug.Log ("==============================state sorting===================================");
		RoateAniIndex = 0;
		RoateCAniIndex = 0;

		m_GameController.HideSeatLayer ();
		m_GameController.HideTableInfo ();
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

		AdjustUI ();
		AdjustPokers ();
		CountDownTime = Common.ConfigSortTime;
		BeginCountDown ();
	}

	public override void DisEnter(){
		RoateAniIndex = 0;
		RoateCAniIndex = 0;

		m_GameController.HideSeatLayer ();
		m_GameController.HideTableInfo ();
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

		AdjustUI ();
		AdjustPokers (true);
		CountDownTime = Common.ConfigSortTime;
		BeginCountDown ();
		ShowControlUI ();
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
		if (Effect_BaoPai != null) {Destroy (Effect_BaoPai);Effect_BaoPai = null;} 
		if (Effect_DX != null) {Destroy (Effect_DX);Effect_DX = null;} 

		GameObject[] SortHandObjs = GameObject.FindGameObjectsWithTag("SortHand");
		for(int i = 0; i < SortHandObjs.Length; i++){
			Destroy (SortHandObjs[i]);
		}

		Layer.gameObject.SetActive (true);
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
		ShowConfim ();
		Layer1.Find ("GetLucky").gameObject.SetActive (false);
		Layer1.Find ("Battle").gameObject.SetActive (false);
		Layer2.Find ("Types").gameObject.SetActive (false);
		GetLuckyConfim = true;
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

			if(CountDownTime == 5){
				Effect_DX = Instantiate(m_GameController.m_PrefabEffectED) as GameObject;
				Effect_DX.transform.SetParent (Layer.Find("EffectDX").transform);
				Effect_DX.transform.localScale = new Vector3 (1, 1, 1);
				Effect_DX.transform.localPosition = new Vector3 (0, 0, 0);
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
		float iv = 0.03f;
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
		Pokers[HandPokers[RoateAniIndex]].transform.DORotate (new Vector3 (0, 180, 0), 0.08f).OnComplete(RoateAniCallBack);
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
				//Pokers[PokerID].AnimeMove(m_GameController.DefaultHandPisitions [i], Time);
				Pokers[PokerID].ReseatSelected ();
				Pokers[PokerID].Belong = "Hand";
				Pokers[PokerID].SetBelongPos (m_GameController.DefaultHandPisitions [i]);
				Pokers[PokerID].SiblingIndex = i;
				Pokers[PokerID].transform.SetSiblingIndex(i);
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
				Pokers[pokers[i]].transform.SetSiblingIndex (Pokers [pokers[i]].SiblingIndex);
				Pokers[pokers[i]].ResetBelongPos();
				Pokers[pokers[i]].RiwerSelected ();
			}
			break;

		case "Upper":
			if (UpperPokers.Count >= 3) {
				MovePokers (belong, "Other", pokers);
				return;
			}

			if (UpperPokers.Count + pokers.Length <= 3) {
				DeletePokerFromTag (belong, pokers);
				AddPokerToTag (tag, pokers);
				UpdateUpperPokers (0.2f);
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
				UpdateMiddlePokers (0.2f);
				UpdateBelongPokets (belong);
				UpdateControlButton ();
				UpdatePokerTips ();
			}

			break;

		case "Under":
			if(UnderPokers.Count >= 5){MovePokers(belong, "Other", pokers);return;}

			if(UnderPokers.Count + pokers.Length <= 5){
				DeletePokerFromTag (belong, pokers);
				AddPokerToTag (tag, pokers);
				UpdateUnderPokers (0.2f);
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
				UpdateHandPokers (0.2f);
				UpdateBelongPokets (belong);
				UpdateControlButton ();
			}
			break;
		}
			
		UpdateRanks ();
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
			UpdateHandPokers (0.2f);
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

		//UpdateRanks ();
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
		if(AlreadyComfim){return;}
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
		//UpdateRanks ();
		UpdatePokerTips ();
	}

	public void BatchRemoveControl(string Tag){
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
		//UpdateRanks ();
		UpdatePokerTips ();
	}

	public void EndDragSelects(){
		foreach (int id in SelectedPokers) {
			Color temp = Pokers [id].transform.GetComponent<Image> ().color ;
			Pokers [id].transform.GetComponent<Image> ().color = new Color (temp.r, temp.g, temp.b, 1);
		}
	}

	public void DragSelects(int PokerID){
		if(SelectedPokers.Count <= 1){return;}

		foreach (int id in SelectedPokers) {
			if(id != PokerID){
				Color temp = Pokers [id].transform.GetComponent<Image> ().color ;
				Pokers [id].transform.GetComponent<Image> ().color = new Color (temp.r, temp.g, temp.b, 70.0f/255);
			}
		}
	}

	public void SortConfrm(){
		AlreadyComfim = true;
		DisPokers ();
		HideConfim ();
		if (TweenColor != null) {TweenColor.Kill ();}

		if (!PlayBaoPai ()) {
			SortConfrmServer ();
		} else {
			Invoke("SortConfrmServer", 2.0f);
		}
	}

	public void SortConfrmServer(){
		Msg.CardGroup upper_pokers = new Msg.CardGroup();
		Msg.CardGroup middle_pokers = new Msg.CardGroup();
		Msg.CardGroup under_pokers = new Msg.CardGroup();

		//upper_pokers.Cards = new uint[UpperPokers.Count];
		for(int i = 0; i < UpperPokers.Count; i++){
			upper_pokers.Cards.Add((uint)UpperPokers[i]);
		}

		//middle_pokers.Cards = new uint[MiddlePokers.Count];
		for(int i = 0; i < MiddlePokers.Count; i++){
			middle_pokers.Cards.Add((uint)MiddlePokers[i]);
		}

		//under_pokers.Cards = new uint[UnderPokers.Count];
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
		Layer1.Find ("HelpLayer").gameObject.SetActive (true);
	}

	public void CloseHelpLayer(){
		Layer1.Find ("HelpLayer").gameObject.SetActive (false);
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
		if (HandPokers.Count <= 0) {
			ShowConfim ();
		} else {
			HideConfim ();
		}
			
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
			Rank.transform.DOScale (new Vector3(1,1,1), 0.09f);
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
			}
		}
	}

	public void SelectPokers(uint[] pokers){
		UpdateHandPokers (0);
		SelectedPokers.Clear ();

		foreach(uint p in pokers){
			int id = (int)p;
			if(Pokers[id]){
				SelectedPokers.Add (id);
				Pokers [id].Selected ();
			}
		}
	}

	public Msg.CardRank GetRanksType(List<int> pokers){

		Dictionary<Msg.CardRank, List<uint[]>> result = GetRanks (pokers);
	
		if(result.Count <= 0){
			return  Msg.CardRank.HighCard;
		}

		List<Msg.CardRank> ranks = new List<Msg.CardRank> ();
		foreach (KeyValuePair<Msg.CardRank, List<uint[]>> pair in result){
			ranks.Add (pair.Key);
		}

		ranks.Sort ();

		return ranks[ranks.Count - 1];
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
			Layer1.Find ("UnderTips").GetComponent<Text> ().text = RanksText [(int)GetRanksType (UnderPokers)];
		}

		if (UpperPokers.Count > 0 && MiddlePokers.Count > 0) {
			if((int)GetRanksType (UpperPokers) > (int)GetRanksType (MiddlePokers)){
				Layer1.Find ("UpperTips").GetComponent<Text> ().color = rcolor;
			}
		}

		if (MiddlePokers.Count > 0 && UnderPokers.Count > 0) {
			if((int)GetRanksType (MiddlePokers) > (int)GetRanksType (UnderPokers)){
				Layer1.Find ("MiddleTips").GetComponent<Text> ().color = rcolor;
			}
		}
	}

	public bool PlayBaoPai(){
		bool baopai = false;

		if (UpperPokers.Count > 0 && MiddlePokers.Count > 0) {
			if((int)GetRanksType (UpperPokers) > (int)GetRanksType (MiddlePokers)){
				baopai = true;
			}
		}

		if (MiddlePokers.Count > 0 && UnderPokers.Count > 0) {
			if((int)GetRanksType (MiddlePokers) > (int)GetRanksType (UnderPokers)){
				baopai = true;
			}
		}

		if(baopai){
			if (Effect_BaoPai != null) {
				Destroy (Effect_BaoPai);
				Effect_BaoPai = null;
			} 

			Effect_BaoPai = Instantiate(m_GameController.m_PrefabEffectSL) as GameObject;
			Effect_BaoPai.transform.SetParent (Layer);
			Effect_BaoPai.transform.localScale = new Vector3 (1, 1, 1);
			Effect_BaoPai.transform.localPosition = new Vector3 (0, 0, 0);
		}

		return baopai;
	}
}
