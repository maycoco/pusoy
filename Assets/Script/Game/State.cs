using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State : MonoBehaviour {
	[HideInInspector]  public GameController 	m_GameController;	
	[HideInInspector]  public StateManage 		m_StateManage;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public virtual void Enter (){
	}

	public virtual void DisEnter (){
	}

	public virtual void Exit (){
	}

	public virtual void AdjustUI(){
	}

	public virtual void UpdateSeatOrder(){
	}
}
