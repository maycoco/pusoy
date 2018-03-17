using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum STATE {
	STATE_SEAT,
	STATE_BETTING,
	STATE_DEAL,
	STATE_SORTING,
	STATE_SHOWHAND,
	STATE_FINISH,
	STATE_COUNT
};

public class StateManage : MonoBehaviour {
	public StateSeat 		m_StateSeat;
	public StateBetting 	m_StateBetting;
	public StateDeal 	m_StateDeal;
	public StateSorting 	m_StateSorting;
	public StateShowHand 	m_StateShowHand;
	public StateFinish 		m_StateFinish;


	public   List<State> 	m_StateList;
	private  STATE m_CurState;
	private  STATE m_LastState;

	void Start () {
	}

	void Update () {
	}

	void Awake(){
		m_StateList.Add (m_StateSeat);
		m_StateList.Add (m_StateBetting);
		m_StateList.Add (m_StateDeal);
		m_StateList.Add (m_StateSorting);
		m_StateList.Add (m_StateShowHand);
		m_StateList.Add (m_StateFinish);
	}
		
	public void initialize(GameController g){
		for(int i = 0; i < m_StateList.Count; i++){
			m_StateList [i].m_GameController 	= g;
			m_StateList [i].m_StateManage 		= this;
		}
	}

	public void SetState(STATE state, bool dis = false){
		m_CurState = state;

		if (!dis) {
			m_StateList [(int)m_CurState].Enter ();
		} else {
			m_StateList [(int)m_CurState].DisEnter ();
		}
	}

	public void ChangeState(STATE state,  bool dis = false){
		if(m_CurState != state){
			m_StateList [(int)m_CurState].Exit ();
			m_LastState = m_CurState;
			m_CurState = state;

			if (!dis) {
				m_StateList [(int)m_CurState].Enter ();
			} else {
				m_StateList [(int)m_CurState].DisEnter ();
			}
		}
	}

	public State GetStateInstance(STATE state){
		return m_StateList [(int)state];
	}

	public STATE GetCulState(){
		return m_CurState;
	}

	public STATE GetLastState(){
		return m_LastState;
	}

	public State GetCulStateObj(){
		return m_StateList [(int)m_CurState];
	}
}
