using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonClick : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		//Button btn = this.GetComponent<Button> ();
		//btn.OnPointerDown = OnPointerDown;
		EventTriggerListener.Get(gameObject).onDown = OnPointerDownHandler;
		EventTriggerListener.Get(gameObject).onClick = OnClickHandler;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
		
	public void OnPointerDownHandler(GameObject Obj){
		transform.DOScale (new Vector3(1.02f,1.02f,1.02f), 0.3f);
	}

	public void OnClickHandler(GameObject Obj){
		transform.DOScale (new Vector3(1.0f,1.0f,1.0f), 0.1f);
	}
}

