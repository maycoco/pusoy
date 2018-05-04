using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class Notice : MonoBehaviour
{
	public GameObject bk;
	public Text message;

	// Use this for initialization
	void Start ()
	{
		//bk.SetActive (false);
		message.text = "";
		message.transform.localPosition = new Vector3 (320, 0 , 0);
	}

	// Update is called once per frame
	void Update ()
	{
	
	}
}

