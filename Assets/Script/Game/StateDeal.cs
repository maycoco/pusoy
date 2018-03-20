using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateDeal : State {
	private Transform Layer = null;
	private GameObject Deal = null;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	void Awake(){
		Layer = GameObject.Find("Canvas").transform.Find("DealLayer");
	}

	public override void Enter(){
		Debug.Log ("==============================state deal===================================");
		AdjustUI ();
	}

	public override void DisEnter(){
		AdjustUI ();
	}

	public override void Exit(){
		Layer.gameObject.SetActive (false);
	}

	public override void AdjustUI(){
		m_StateManage.m_StateSeat.HideAutoBanker ();
		Layer.gameObject.SetActive (true);
		if(Deal != null){
			Destroy (Deal);
		}
		Deal = Instantiate(m_GameController.m_PrefabEffectDeal) as GameObject;

		for(int i = 0; i < Common.CSeats.Count; i++){
			int index = Common.ConfigSeatOrder.IndexOf (Common.CSeats[i]);
			Deal.transform.GetChild(index).gameObject.SetActive (true);
		}

		Deal.transform.SetParent (Layer);
		Deal.transform.localPosition = new Vector3 (0,0,0);
		Deal.transform.localScale	= new Vector3(1,1,1);
	}
}
