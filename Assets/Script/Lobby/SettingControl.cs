using UnityEngine;
using System.Collections;

public class SettingControl : MonoBehaviour
{

	// Use this for initialization
	void Start (){
	
	}
	
	// Update is called once per frame
	void Update (){
	
	}

	public void Enter(){
		this.gameObject.SetActive (true);
	}

	public void Exit(){
		this.gameObject.SetActive (false);
	}
}

  