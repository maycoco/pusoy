using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpringFramework.UI;
using System;

public class Wol{
	public int Win;
	public int Lose;
	public double WinPro;
	public double LosePro;
}

public class CareerControl : MonoBehaviour {
	public PieGraph			Pie7Day;
	public PieGraph			Pie30Day;
	public PieGraph			PieAllDay;
	public List<PieGraph>	Borders;
	public List<Wol>		WolData = new List<Wol>();

	// Use this for initialization
	void Start () {
	}

	void Awake(){
		GetRecordData ();
		UpdateRecordData ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Enter(){
		this.gameObject.SetActive (true);
	}

	public void Exit(){
		this.gameObject.SetActive (false);
	}

	public void GetRecordData(){
		for(int i = 0; i < 3; i++){
			Wol temp = new Wol ();
			temp.Win = 22;
			temp.Lose =55;
		
			double winper = Convert.ToDouble (temp.Win) / Convert.ToDouble (temp.Win + temp.Lose);
			double loseper = Convert.ToDouble (temp.Lose) / Convert.ToDouble (temp.Win + temp.Lose);

			temp.WinPro = Convert.ToInt32( Math.Round ((decimal)winper , 2, MidpointRounding.AwayFromZero) * 100 );
			temp.LosePro = Convert.ToInt32( Math.Round ((decimal)loseper , 2, MidpointRounding.AwayFromZero) * 100 );

			WolData.Add (temp);
		}
	}

	public void UpdateRecordData(){
		Color LostColor = new Color (0.0f / 255, 0.0f / 255, 0.0f / 255, 0.2f);
		Color WinColor = new Color (27.0f / 255, 116.0f / 255, 126.0f / 255);

		Pie7Day.SetPie (70, 30, LostColor, WinColor);
		Pie30Day.SetPie (80, 20, LostColor, WinColor);
		PieAllDay.SetPie (10, 90, LostColor, WinColor);

		for(int i = 0; i < Borders.Count; i++){
			Color Border = new Color (74.0f / 255, 184.0f / 255, 197.0f / 255);
			Borders[i].SetPie (100, 100, Border, Border);
		}
	}
}
