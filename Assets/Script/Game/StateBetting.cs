using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StateBetting : State {
	private int 		BettingTime 	= 0;
	private Transform 	Layer 			= null;
	private	Vector3		CMPos			= new Vector3();	
	private bool 		CanBet			= false;
	private List<int> 	m_Chips			= new List<int>();
	private List<Vector3> m_ChipsPos 	= new List<Vector3> ();
	private List<Vector3> m_ChipsBeginPos 	= new List<Vector3> ();

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	void Awake(){
		Layer = GameObject.Find("Canvas").transform.Find("BettingLayer");
		CMPos = Layer.Find ("ChipsMask").localPosition;
	}

	public override void Enter (){
		Debug.Log ("==============================state betting===================================");
		Layer.gameObject.SetActive (true);

		BettingTime = Common.ConfigBetTime;
		BeginCountDown ();

		if (m_GameController.m_SelfSeatID != -1 && m_GameController.m_SelfSeatID != 0) {
			AdjustUI ();
		}
		m_Chips.Clear ();
		m_ChipsPos.Clear ();
		m_ChipsBeginPos.Clear ();
		CreateChipPos ();
	}

	public void CreateChipPos(){
		for(int i = 0; i < 30; i++){
			m_ChipsPos.Add (RandomChipsPos());
		}
		m_ChipsPos [0] = new Vector3 (0,0,0);
	}

	public override void AdjustUI (){
		Layer.Find ("ChipsMask").localPosition =  CMPos;
		for(int i = 0; i < m_GameController.m_ChipsType.Count; i++){
			Layer.Find ("ChipsMask/Chip" + i + "/Value").GetComponent<Text> ().text = m_GameController.m_ChipsType [i].ToString ();
		}
		ShowBeetingArea ();
	}

	public override void Exit (){
		UpdateChipsUI (0,0);
		UpdateChipsUI (0,1);
		UpdateChipsUI (0,2);
		UpdateChipsUI (0,3);
		ClearChipsButton ();
		ClearCountDown ();
		Layer.gameObject.SetActive (false);
	}

	public void BeginCountDown(){
		Layer.Find ("CountDown").gameObject.SetActive (true);
		InvokeRepeating("UpdateBettingTime", 0f, 1.0f);
	}

	public void ShowBeetingArea(){
		Tweener tween = Layer.Find ("ChipsMask").DOLocalMove (new Vector3 (CMPos.x, CMPos.y + 110, CMPos.z), 0.3f, true);
		tween.onComplete = CanBetCall;
	}
		
	public void UpdateBettingTime(){
		
		if (BettingTime > 0) {

			if (BettingTime.ToString ().Length <= 1) {
				Image imageCD0 = Layer.Find ("CountDown/CD0").GetComponent<Image> ();
				imageCD0.sprite = Resources.Load ("Image/Game/btnumber0", typeof(Sprite)) as Sprite;

				Image imageCD1 = Layer.Find ("CountDown/CD1").GetComponent<Image> ();
				imageCD1.sprite = Resources.Load ("Image/Game/btnumber" + BettingTime.ToString (), typeof(Sprite)) as Sprite;
			} else {
				Image imageCD0 = Layer.Find ("CountDown/CD0").GetComponent<Image> ();
				imageCD0.sprite = Resources.Load ("Image/Game/btnumber" + BettingTime.ToString ()[0], typeof(Sprite)) as Sprite;

				Image imageCD1 = Layer.Find ("CountDown/CD1").GetComponent<Image> ();
				imageCD1.sprite = Resources.Load ("Image/Game/btnumber" + BettingTime.ToString ()[1], typeof(Sprite)) as Sprite;
			}
		} 
		else {
			CanBet = false;
			ClearCountDown ();
			ClearChipsButton ();
		}
		BettingTime--;
	}

	public int GetChipsAmoun(){
		int sum = 0;
		foreach(int t in m_Chips){
			sum += t;
		}
		return sum;
	}
		
	public void Betting(int Index){
		if(m_StateManage.GetCulState() != STATE.STATE_BETTING){return;}

		if(!CanBet){return;}

		int BettingAmount = m_GameController.m_ChipsType[Index];

		if(GetChipsAmoun() + BettingAmount > m_GameController.m_MaxChips){return;}

		m_Chips.Add(BettingAmount);

		//anime
		GameObject Chip = (GameObject)Instantiate(m_GameController.m_PrefabChip);  
		Chip.transform.SetParent (Layer.Find ("SeatCom/Seat" + m_GameController.m_SelfSeatID + "/ChipsAssets"));
		Vector3 beginpos = Layer.Find ("ChipsMask/Chip" + Index).position;
		Chip.transform.position = beginpos;
		EventTriggerListener.Get(Chip).onClick = onClickButtonHandler;
		Chip.name = (m_Chips.Count - 1).ToString ();

		m_ChipsBeginPos.Add (Chip.transform.localPosition);

		Image image = Chip.GetComponent<Image>();
		image.sprite = Resources.Load("Image/Game/chip"+Index , typeof(Sprite)) as Sprite;
		Chip.transform.Find ("Value").GetComponent<Text> ().text = BettingAmount.ToString();
		Color color = Layer.Find ("ChipsMask/Chip" + Index + "/Value").GetComponent<Outline> ().effectColor;
		Chip.transform.Find ("Value").GetComponent<Outline> ().effectColor = color;

		Tweener tween = Chip.transform.DOLocalMove (m_ChipsPos[m_Chips.Count - 1], 0.2f, true);
		tween.onComplete =BettingCall;
		//anime
	}

	public void BettingCall(){
//		foreach(int t in m_GameController.m_ChipsType){
//			int amount = 0;
//			int count = 0;
//
//			foreach(int c in m_Chips){
//				if(c == t){
//					count++;
//				}
//			}
//
//			if(count > 1){
//				amount = t * count;
//				for(int o = 1;  o <  m_GameController.m_ChipsType.Count; o++){
//					if(m_GameController.m_ChipsType[o] == amount){
//						for (int i = m_Chips.Count - 1; i >= 0; i--) {
//							if(m_Chips[i] == t){
//								m_Chips.Remove (m_Chips[i]);
//							}
//						}
//
//						m_Chips.Add (amount);
//					}
//				}
//			}
//		}

		//UpdateSelfChips (m_GameController.m_SelfSeatID);
		UpdateChipsAmount (m_GameController.m_SelfSeatID , GetChipsAmoun());
	}

public void UpdateSelfChips(int SeatID){
		Transform Obj = Layer.Find ("SeatCom/Seat" + SeatID + "/ChipsAssets");

		for (int i = Obj.childCount - 1; i >= 0; i--) {  
			Destroy(Obj.GetChild(i).gameObject);  
		}  

		for (int i = 0; i < m_Chips.Count; i++) {
			GameObject Chip = (GameObject)Instantiate(m_GameController.m_PrefabChip);  
			Chip.transform.SetParent (Obj.transform);
			Chip.transform.localPosition = m_ChipsPos [i];
			Chip.name = (m_Chips.Count - 1).ToString ();

			EventTriggerListener.Get(Chip).onClick = onClickButtonHandler;

			Image image = Chip.GetComponent<Image>();
			image.sprite = Resources.Load("Image/Game/chip" + m_GameController.m_ChipsType.IndexOf(m_Chips[i]), typeof(Sprite)) as Sprite;
			Chip.transform.Find ("Value").GetComponent<Text> ().text = m_Chips[i].ToString ();

			Color c = Layer.Find ("ChipsMask/Chip" + m_GameController.m_ChipsType.IndexOf(m_Chips[i]) + "/Value").GetComponent<Outline> ().effectColor;
			Chip.transform.Find ("Value").GetComponent<Outline> ().effectColor = c;
		}
	}

	public void UpdateChipsUI(int Amount, int SeatID){
		List<int> 	TableChips = CountingChips(Amount);
		Transform Obj = Layer.Find ("SeatCom/Seat" + SeatID + "/ChipsAssets");

		for (int i = Obj.childCount - 1; i >= 0; i--) {  
			Destroy(Obj.GetChild(i).gameObject);  
		}  

		for(int i = 0; i < TableChips.Count; i++){
			GameObject Chip = (GameObject)Instantiate(m_GameController.m_PrefabChip);  
			Chip.transform.SetParent (Obj.transform);
			Chip.transform.name = TableChips [i].ToString ();
			Chip.transform.localPosition = RandomChipsPos();

			int index  = m_GameController.m_ChipsType.IndexOf (TableChips[i]);
			Image image = Chip.GetComponent<Image>();
			image.sprite = Resources.Load("Image/Game/chip" + index, typeof(Sprite)) as Sprite;
			Chip.transform.Find ("Value").GetComponent<Text> ().text = TableChips [i].ToString ();

			Color c = Layer.Find ("ChipsMask/Chip" + index + "/Value").GetComponent<Outline> ().effectColor;
			Chip.transform.Find ("Value").GetComponent<Outline> ().effectColor = c;
		}

		UpdateChipsAmount (SeatID, Amount);
	}

	public void UpdateChipsAmount(int SeatID, int Amount){
		Layer.Find ("SeatCom/Seat" + SeatID + "/AmountBack").gameObject.SetActive (true);
		Layer.Find ("SeatCom/Seat" + SeatID + "/AmountBack/ChipsAmount").GetComponent<Text> ().text = Amount.ToString ();

		if(Amount <= 0){
			Layer.Find ("SeatCom/Seat" + SeatID + "/AmountBack").gameObject.SetActive (false);
			Layer.Find ("SeatCom/Seat" + SeatID + "/AmountBack/ChipsAmount").GetComponent<Text> ().text = "";
		}
	}

	public override void UpdateSeatOrder(){
		for(int seatid = 0; seatid < Common.ConfigSeatOrder.Count; seatid++){
			Transform SeatObject = Layer.Find ("SeatCom/Seat" + Common.ConfigSeatOrder[seatid]);
			SeatObject.transform.localPosition = m_GameController.DefaultBetPositions [seatid];
		}
	}

	private void onClickButtonHandler(GameObject obj)
	{
		if(m_StateManage.GetCulState() != STATE.STATE_BETTING){return;}
		if(!CanBet){return;}

		if(m_Chips.Count > 0){
			int count = m_Chips.Count - 1;
			Transform tr = Layer.Find ("SeatCom/Seat" + m_GameController.m_SelfSeatID + "/ChipsAssets/" + count.ToString ());
			Tweener tween = tr.DOLocalMove (m_ChipsBeginPos[count], 0.2f, true);
			tween.onComplete =CancelBettingCall;
		}
	}

	public void CancelBettingCall(){
		CanBet = true;
		int count = m_Chips.Count - 1;

		Transform tr = Layer.Find ("SeatCom/Seat" + m_GameController.m_SelfSeatID + "/ChipsAssets/" + count.ToString ());
		Destroy (tr.gameObject);

		m_ChipsBeginPos.RemoveAt (count);
		m_Chips.RemoveAt (count);
		//UpdateSelfChips (m_GameController.m_SelfSeatID);
		UpdateChipsAmount (m_GameController.m_SelfSeatID, GetChipsAmoun());
	}

	public void CanBetCall(){
		CanBet = true;
	}

	public void DCanBetCall(){
		CanBet = false;
	}

	public void ClearChipsButton(){
		Layer.Find ("ChipsMask").DOLocalMove (CMPos, 0.3f, true);
	}

	public List<int> CountingChips(int  CAmount){
		List<int> temp = new List<int>();
		int amount = CAmount;

		for(int i = m_GameController.m_ChipsType.Count - 1; i >= 0; i--){

			int c = amount / m_GameController.m_ChipsType [i];

			if(c > 0){
				for(int o = 0; o < c; o++){
					temp.Add (m_GameController.m_ChipsType [i]);
				}
			}

			amount -= c * m_GameController.m_ChipsType [i];
		}

		temp.Sort ();
		return temp;
	}

	public Vector3 RandomChipsPos(){
		float x = Random.Range (-45, 45);
		float y = Random.Range (-21, 21);
		float z = 0;
		return new Vector3(x, y, z);
	}

	public void BetConfim(){
		//for demo
		//m_StateManage.ChangeState(STATE.STATE_SORTING);

		if(m_StateManage.GetCulState() != STATE.STATE_BETTING){return;}
		if(GetChipsAmoun() > 0){
			CanBet = false;
			m_GameController.BetServer ((uint)GetChipsAmoun());
		}
	}

	public void ShowConfimBet(){
		DCanBetCall ();
		ClearCountDown ();
		ClearChipsButton ();
	}

	public void ClearCountDown(){
		CancelInvoke ();
		Layer.Find ("CountDown").gameObject.SetActive (false);
	}
}
