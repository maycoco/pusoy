﻿using Google.Protobuf.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StateFinish : State {
	private Transform 		Layer 		= 	null;
	private int 			CountDowm	= 0;

	void Start () {
	}

	void Update () {
	}

	void Awake(){
		Layer = GameObject.Find ("Canvas").transform.Find ("FinishLayer");
	}

	public override void Enter(){
		ClearResultInfo ();
	
		Layer.gameObject.SetActive (true);
		ShowResultInfo (true);
		m_StateManage.m_StateSeat.UpdateAllSeatScore ();
		CountDowm = Common.ConfigFinishTime;
		ShowHandInfo ();

		if ((Common.CPlayed_hands + 1) < Common.CHands) {
			BeginCountDown ();
		} else {
			Layer.Find ("OK/CountDown").GetComponent<Text> ().text = "OK";
		}
	}
		
	public override void DisEnter(){
		Layer.gameObject.SetActive (true);

		m_StateManage.m_StateSeat.UpdateAllSeatScore ();
		CountDowm = Common.ConfigFinishTime;
		ShowHandInfo ();
		ShowResultInfo ();

		if ((Common.CPlayed_hands + 1) < Common.CHands) {
			BeginCountDown ();
		} else {
			Layer.Find ("OK/CountDown").GetComponent<Text> ().text = "OK";
		}
	}

	public void ResrtUI(){
		Layer.gameObject.SetActive (false);
	}

	public override void Exit(){
		ClearResultInfo ();
		Layer.gameObject.SetActive (false);
		CancelInvoke ();
		m_StateManage.m_StateSeat.ShowUI ();
	}
		
	public void BeginCountDown(){
		InvokeRepeating("UpdateSortingTime", 0f, 1.0f);
	}

	public void UpdateSortingTime(){
		if (CountDowm >= 0) {
			Layer.Find ("OK/CountDown").GetComponent<Text> ().text = "（" + CountDowm + "）OK";
		} 
		else {
			CancelInvoke ();
			Next ();
		}
		CountDowm--;
	}

	public void Next(){
		if ((Common.CPlayed_hands + 1) < Common.CHands) {
			if(m_GameController.m_SelfSeatID >= 0){
				m_GameController.CloseResultServer ();
			}

			m_StateManage.ChangeState (STATE.STATE_SEAT);
			m_StateManage.m_StateSeat.HideTipsWaitStart ();
		} else {
			HideLayer ();
			m_GameController.m_LastDialog.SetActive (true);
		}
	}

	public void HideLayer(){
		CancelInvoke ();
		ResrtUI ();
	}

	public void ShowHandInfo(){
		Layer.Find ("HandCount").GetComponent<Text>().text = "# " + (Common.CPlayed_hands + 1) + " / " + Common.CHands + " Hand";
	}

	public void ClearResultInfo(){
		for (int i = Layer.Find ("PreInfoCom").childCount - 1; i >= 0; i--) {  
			Destroy(Layer.Find ("PreInfoCom").GetChild(i).gameObject);
		}  
	}
		
	public void ShowResultInfo(bool hasAni = false){
		List<Vector3> poslist = new List<Vector3> ();
		poslist.Add (new Vector3(0,286,0));
		poslist.Add (new Vector3(0,54,0));
		poslist.Add (new Vector3(0,-165,0));
		poslist.Add (new Vector3(0,-386,0));

		List<int> Seats = new List<int> ();
		foreach (KeyValuePair<int, CSeatResult> pair in Common.CSeatResults) {
			Seats.Add (pair.Key);
		}
		Seats.Sort ();

		int index = 0;
		foreach (int seatid in Seats){
			CSeatResult hInfo = Common.CSeatResults[seatid];

			GameObject PreInfoObj = (GameObject)Instantiate(m_GameController.m_PrefabPreInfo);
			PreInfoObj.transform.SetParent (Layer.Find ("PreInfoCom"));

			if(hasAni){
				PreInfoObj.transform.localPosition = new Vector3 (0,-720,0);
			}
				
			UICircle avatar = (UICircle)Instantiate(m_GameController.m_PrefabAvatar);
			avatar.transform.SetParent (PreInfoObj.transform.Find ("Avatar"));
			avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 ();
			avatar.transform.localPosition = new Vector3 ();
			avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (60, 60);

			if (string.IsNullOrEmpty(hInfo.Avatar)) {avatar.UseDefAvatar ();}
			else {StartCoroutine(Common.Load(avatar, hInfo.Avatar));}

			if (seatid == 0) {
				PreInfoObj.transform.Find ("BBorder").gameObject.SetActive (true);
				PreInfoObj.transform.Find ("BName").GetComponent<Text> ().text = hInfo.Name;
			} else {
				PreInfoObj.transform.Find ("PBorder").gameObject.SetActive (true);
				PreInfoObj.transform.Find ("PName").GetComponent<Text> ().text = hInfo.Name;
			}


			for(int i = 0; i < hInfo.Pres.Count; i++){
                RepeatedField<uint> pInfo = hInfo.Pres [i];

				for(int o = 0; o < pInfo.Count; o++){
					Transform Poker = PreInfoObj.transform.Find ("Hand" + i + "/Poker" + o);
					Image image = Poker.GetComponent<Image>();
					image.sprite = Resources.Load("Image/Poker/" + pInfo[o], typeof(Sprite)) as Sprite;
				}
			}

			Transform number = PreInfoObj.transform.Find ("Amount");
			string	typestr = "";

			if (hInfo.Win < 0) {typestr = "lost";}
			else {typestr = "win";}

			string amount = Mathf.Abs (hInfo.Win).ToString ();
			float left = 0;
			for (int c = amount.Length - 1; c >= 0; c--) {  
				GameObject t = new GameObject ();
				t.AddComponent<Image> ();
				t.GetComponent<Image>().sprite = Resources.Load ("Image/Game/"+ typestr + amount[c] , typeof(Sprite)) as Sprite;
				t.transform.SetParent(number);
				t.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (20, 24);
				t.transform.localPosition = new Vector3 (left,0,0);
				left = left - 18;

				if(c == 0){
					GameObject icon = new GameObject ();
					icon.AddComponent<Image> ();
					icon.GetComponent<Image>().sprite = Resources.Load ("Image/Game/"+ typestr + "icon" , typeof(Sprite)) as Sprite;
					icon.transform.SetParent(number);
					icon.transform.GetComponent<RectTransform> ().sizeDelta = new Vector2 (20, 24);
					icon.transform.localPosition = new Vector3 (left,0,0);
				}
			}

			float right = number.localPosition.x - 18 * amount.Length;
			if (hInfo.autowin) {
				PreInfoObj.transform.Find ("GetLucky").gameObject.SetActive (true);
				PreInfoObj.transform.Find ("GetLucky").localPosition = new Vector3 (right, PreInfoObj.transform.Find ("GetLucky").localPosition.y, 0);
			}

			if (hInfo.foul) {
				PreInfoObj.transform.Find ("Foul").gameObject.SetActive (true);
				PreInfoObj.transform.Find ("Foul").localPosition = new Vector3( right, PreInfoObj.transform.Find ("GetLucky").localPosition.y, 0);
			}

			if (hasAni) {
				PreInfoObj.transform.DOLocalMoveY (poslist [index].y, 0.25f).SetDelay ((index  + 1)* 0.17f);
			} else {
				PreInfoObj.transform.localPosition = poslist [index];
			}

			PreInfoObj.transform.localScale = new Vector3 (1, 1, 1);

			index++;
		}
	}
}
