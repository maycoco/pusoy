using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;

using Msg;
using Google.Protobuf;
using Google.Protobuf.Collections;

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DiamondRecord{
	public uint 	Uid;
	public string 	Date;
	public string 	Time;
	public string 	Name;
	public string 	Avatar;
	public int 		Amount;
}

public class PrefileControl : MonoBehaviour
{
	public LobbyController	LobbyControl;

	public 	UICircle 	m_Avatar;
	//Prefab
	public GameObject 	m_Drecord;
	public GameObject 	m_Precord;
	public GameObject   m_Date;

	public Toggle		m_TogSent;
	public Toggle		m_TogReceived;

	private string 		m_UserID;
	private	string		m_Amount;
	private	string		m_InputType;
	private	Transform	m_DiamondContent;

	private Vector3 	m_UseridTagPos = new Vector3();
	private Vector3 	m_AmountTagPos = new Vector3();


	//Date
	private DateTime     		m_CurDate;
	private List<DayOfWeek>		m_weeks = new List<DayOfWeek> ();
	private List<string>		m_months = new List<string>();
	private string				m_BeginDate;
	private string				m_EndDate;

	// Use this for initialization
	void Start (){
	}
	
	// Update is called once per frame
	void Update (){
	}

	void Awake(){
		m_DiamondContent = transform.Find ("DiamondRecord/List/Viewport/Content");
		EventTriggerListener.Get (transform.Find ("SendDiamond/Mask").gameObject).onClick = onClickMaskHandler;
		EventTriggerListener.Get (transform.Find ("SendDiamond/UserIDInput/InputUser").gameObject).onClick = onClickInputHandler;
		EventTriggerListener.Get (transform.Find ("SendDiamond/AmountInput/InputAmount").gameObject).onClick = onClickInputHandler;


		m_UseridTagPos = transform.Find ("SendDiamond/UserIDInput/Tag").localPosition;
		m_AmountTagPos = transform.Find ("SendDiamond/AmountInput/Tag").localPosition;

		m_weeks.Add (DayOfWeek.Sunday);
		m_weeks.Add (DayOfWeek.Monday);
		m_weeks.Add (DayOfWeek.Tuesday);
		m_weeks.Add (DayOfWeek.Wednesday);
		m_weeks.Add (DayOfWeek.Thursday);
		m_weeks.Add (DayOfWeek.Friday);
		m_weeks.Add (DayOfWeek.Saturday);

		m_months.Add ("JANUARY");
		m_months.Add ("FEBRUARY");
		m_months.Add ("MARCH");

		m_months.Add ("APRIL");
		m_months.Add ("MAY");
		m_months.Add ("JUNE");

		m_months.Add ("JULY");
		m_months.Add ("AGUEST");
		m_months.Add ("SEPTEMBER");

		m_months.Add ("OCTOBER");
		m_months.Add ("NOVEMBER");
		m_months.Add ("DECEMBER");

		AdjustCalendar ();
	}

	public void Enter(){
		transform.localPosition = new Vector3(-640, 0, 0);
		Sequence s = DOTween.Sequence ();
		s.Append (transform.DOLocalMoveX (30, 0.2f));
		s.Append (transform.DOLocalMoveX (0, 0.2f));
		s.Play ();

		m_TogSent.isOn = true;
		m_TogReceived.isOn = true;

		transform.Find ("SelfInfo/Diamonds/Icon").gameObject.SetActive (false);
		transform.Find ("SelfInfo/Diamonds/Amount").gameObject.SetActive (false);

		ClearDiamondRecord ();
		HideCalendar ();

		LobbyControl.GetProfileServer ();
	}

	public void Exit(){
		LobbyControl.PlayerButtonEffect ();
		transform.DOLocalMoveX (-640, 0.15f);
	}

	public void UpdateSelfInfo(){
		transform.Find ("SelfInfo/ID").GetComponent<Text> ().text 	= Common.Uid.ToString();
		transform.Find ("SelfInfo/Name").GetComponent<Text> ().text = Common.FB_name;

		float width = Common.DiamondAmount.ToString ().Length * 12 + 45;
		float left = (640 - width) / 2;
		transform.Find ("SelfInfo/Diamonds/Icon").gameObject.SetActive (true);
		transform.Find ("SelfInfo/Diamonds/Amount").gameObject.SetActive (true);
		transform.Find ("SelfInfo/Diamonds/Amount").GetComponent<Text> ().text = Common.ToCarryNum((int)Common.DiamondAmount);

		transform.Find ("SelfInfo/Diamonds/Icon").localPosition = new Vector3 (left, 0, 0);
		transform.Find ("SelfInfo/Diamonds/Amount").localPosition = new Vector3 (left + 45, 0, 0);

		if (string.IsNullOrEmpty(Common.FB_avatar)) {
			m_Avatar.UseDefAvatar ();
		} else {
			StartCoroutine(Common.Load(m_Avatar, Common.FB_avatar));  
		}
	}


	public void ShowSendDiamond(){
		LobbyControl.PlayerButtonEffect ();

		transform.Find ("SendDiamond/UserIDInput/Tag").localPosition = m_UseridTagPos;
		transform.Find ("SendDiamond/AmountInput/Tag").localPosition = m_AmountTagPos;

		m_UserID 	= "";
		m_Amount 	= "";

		m_InputType = "userid";
		transform.Find ("SendDiamond/UserIDInput/Tag").gameObject.SetActive (true);
		transform.Find ("SendDiamond/AmountInput/Tag").gameObject.SetActive (false);


		transform.Find ("SendDiamond/UserIDInput/InputUser").GetComponent<Text> ().text = "";
		transform.Find ("SendDiamond/AmountInput/InputAmount").GetComponent<Text> ().text = "";

		transform.Find ("SendDiamond").gameObject.SetActive (true);
	}

	public void HideSendDiamond(){
		transform.Find ("SendDiamond").gameObject.SetActive (false);
	}

	public void InputNumber(int number){
		if(m_InputType == "userid"){
			if (m_UserID.Length < 11) {
				m_UserID = m_UserID + number.ToString();
			}
		}

		if(m_InputType == "amount"){
			if (m_Amount.Length < 10) {
				m_Amount = m_Amount + number.ToString();
			}
		}

		UpdateNumber ();
	}

	public void DeleteNumber(){
		if(m_InputType == "userid"){
			if (m_UserID.Length > 0) {
				m_UserID = m_UserID.Substring (0, m_UserID.Length - 1);
			}
		}

		if(m_InputType == "amount"){
			if (m_Amount.Length > 0) {
				m_Amount = m_Amount.Substring (0, m_Amount.Length - 1);
			}
		}

		UpdateNumber ();
	}

	public void UpdateNumber(){
		if(m_InputType == "userid"){
			transform.Find ("SendDiamond/UserIDInput/InputUser").GetComponent<Text> ().text = m_UserID;
			transform.Find ("SendDiamond/UserIDInput/Tag").localPosition = new Vector3 (m_UseridTagPos.x + (18 * m_UserID.Length) - (3.5f * (m_UserID.Length - 1)), m_UseridTagPos.y, m_UseridTagPos.z);
		}
		if(m_InputType == "amount"){
			string amt = Common.ToCarryNum (Convert.ToInt32(m_Amount));
			transform.Find ("SendDiamond/AmountInput/InputAmount").GetComponent<Text> ().text = amt;
			transform.Find ("SendDiamond/AmountInput/Tag").localPosition = new Vector3 (m_AmountTagPos.x + (17 * amt.Length) - (3.6f * (amt.Length - 1)), m_AmountTagPos.y, m_AmountTagPos.z);
		}
	}

	public void SendDiamonds(){
		if(!string.IsNullOrEmpty(m_UserID) && !string.IsNullOrEmpty(m_Amount)){
			if(uint.Parse(m_UserID) > 0 && uint.Parse(m_Amount) > 0){
				LobbyControl.SendDiamondsServer (uint.Parse(m_UserID), uint.Parse(m_Amount));
			}
		}
	}

	public void onClickInputHandler(GameObject obj){
		if(obj.name == "InputUser"){
			m_InputType = "userid";
			transform.Find ("SendDiamond/UserIDInput/Tag").gameObject.SetActive (true);
			transform.Find ("SendDiamond/AmountInput/Tag").gameObject.SetActive (false);
		}
		if(obj.name == "InputAmount"){
			m_InputType = "amount";
			transform.Find ("SendDiamond/UserIDInput/Tag").gameObject.SetActive (false);
			transform.Find ("SendDiamond/AmountInput/Tag").gameObject.SetActive (true);
		}
	}

	private void onClickMaskHandler(GameObject obj){
		HideSendDiamond ();
	}

	public DateTime ConvertStringToDateTime(string timeStamp)
	{
		DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
		long lTime = long.Parse(timeStamp + "0000000");
		TimeSpan toNow = new TimeSpan(lTime);
		return dateTimeStart.Add(toNow);
	}

	public void FormentDiamondRecord(RepeatedField<Msg.DiamondsRecordsItem> records, RepeatedField<Msg.UserNameAvatar> users){
		List<DiamondRecord> DList 	= new List<DiamondRecord>();

		foreach(Msg.DiamondsRecordsItem red in records){
			DiamondRecord c = new DiamondRecord ();
			c.Uid = red.Uid;

			DateTime date = ConvertStringToDateTime (red.Time.ToString());
			c.Date = date.Year + "-" + date.Month + "-" + date.Day;
			c.Time = date.Hour + ":" + date.Minute + ":" + date.Second;
			c.Amount = red.Diamonds;

			foreach(Msg.UserNameAvatar use in users){
				if(red.Uid == use.Uid){
					c.Name = use.Name;
					c.Avatar = use.Avatar;

					//StartCoroutine(Common.DownAvatar (use.Avatar));

					break;
				}
			}

			DList.Add (c);
		}

		UpdateDiamondRecord (DList);
	}

	public void UpdateDiamondRecord(List<DiamondRecord> m_DList){
		ClearDiamondRecord ();

		if(m_DList.Count <= 0){return;}

		for (int i = m_DList.Count - 1; i >= 0; i--) {  
			if(!m_TogSent.isOn && m_DList[i].Amount < 0){
				m_DList.Remove (m_DList[i]);
			}
		} 

		for (int i = m_DList.Count - 1; i >= 0; i--) {  
			if(!m_TogReceived.isOn && m_DList[i].Amount > 0){
				m_DList.Remove (m_DList[i]);
			}
		} 

		List<string> keys = new List<string>();
		foreach (DiamondRecord d in m_DList) {
			keys.Add (d.Date);
		}

		HashSet<string> keyst = new HashSet<string>(keys);
		float height = 40 * keyst.Count + 76 * m_DList.Count + ( 10 * keyst.Count - 2);
		if(height < 530){height = 530;}

		m_DiamondContent.GetComponent<RectTransform> ().sizeDelta = new Vector2 (m_DiamondContent.GetComponent<RectTransform> ().sizeDelta.x, height);
		m_DiamondContent.GetComponent<RectTransform> ().localPosition = new Vector3 (0, -height / 2, 0);

		keys = new List<string>(keyst);
		keys.Sort ();

		float top = height / 2;
		for(int i = 0; i < keys.Count; i++){
			//Date
			GameObject Drecord = Instantiate(m_Drecord) as GameObject;
			Drecord.transform.SetParent (m_DiamondContent);
			Drecord.GetComponent<RectTransform> ().localScale = new Vector3 (1,1,1);
			Drecord.GetComponent<RectTransform> ().localPosition = new Vector3 (0, top, 0);
			Drecord.transform.Find("Date").GetComponent<Text> ().text = keys [i];
			top -= 40;

			foreach(DiamondRecord d in m_DList){
				if(d.Date == keys[i]){
					//Time
					GameObject Precord = Instantiate(m_Precord) as GameObject;
					Precord.transform.SetParent (m_DiamondContent);
					Precord.GetComponent<RectTransform> ().localScale = new Vector3 (1,1,1);
					Precord.GetComponent<RectTransform> ().localPosition = new Vector3 (0, top, 0);
					Precord.transform.Find("Time").GetComponent<Text> ().text = d.Time;
					Precord.transform.Find("Text").GetComponent<Text> ().text = d.Name;
					Precord.transform.Find ("Amount").GetComponent<Text> ().text = Common.ToCarryNum (d.Amount);
					if (d.Amount >= 0) {
						Precord.transform.Find ("Amount").GetComponent<Text> ().color = new Color (0, 255.0f / 255, 6.0f / 255);
					} else {
						Precord.transform.Find ("Amount").GetComponent<Text> ().color = new Color (254.0f/ 255, 45.0f / 255, 23.0f / 255);
					}

					UICircle avatar = (UICircle)Instantiate(LobbyControl.PrefabAvatar);
					avatar.transform.SetParent (Precord.transform.Find("Avatar"));
					avatar.transform.localPosition = new Vector3 ();
					avatar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (61, 61);
					StartCoroutine(Common.Load(avatar, d.Avatar));

					top -= 76;
				}
			}

			top -= 14;
		}
	}

	public void ConfirmSearch(){
		HideCalendar ();
		if (!string.IsNullOrEmpty (m_BeginDate) && !string.IsNullOrEmpty (m_EndDate)) {
			m_BeginDate = m_BeginDate.Replace ("/", "-");
			m_EndDate = m_EndDate.Replace ("/", "-");

			LobbyControl.DiamondsRecordsServer(m_BeginDate, m_EndDate);
		}
	}

	public void ClearDiamondRecord(){
		for (int i = m_DiamondContent.childCount - 1; i >= 0; i--) {  
			Destroy (m_DiamondContent.GetChild(i).gameObject);
		}  
	}

	public void ShowCalendar(){
		LobbyControl.PlayerButtonEffect ();

		m_CurDate = DateTime.Now;
		UpdateCalendar ();
		transform.Find ("Date").gameObject.SetActive (true);
	}

	public void HideCalendar(){
		transform.Find ("Date").gameObject.SetActive (false);

		if(!string.IsNullOrEmpty (m_BeginDate) && !string.IsNullOrEmpty (m_EndDate)){
			transform.Find ("Console/Date/Text").GetComponent<Text> ().text = m_BeginDate + " ~ " + m_EndDate;
		}
	}

	public void AdjustCalendar(){
		float width 	= 0;
		float height 	= 0;
		float top 		= 255;

		for(int i = 0; i < 6; i++){
			float left = -322;

			for(int o = 0; o < 7; o++){
				GameObject Date = Instantiate(m_Date) as GameObject;
				Date.transform.SetParent (transform.Find ("Date/Calendar"));
				Date.transform.localScale = new Vector3 (1,1,1);
				Date.transform.localPosition = new Vector3 (left, top, 0);

				width = Date.GetComponent<RectTransform> ().sizeDelta.x;
				height = Date.GetComponent<RectTransform> ().sizeDelta.y;
				left += width - 2;
				Date.name = "null";
			}
			top -= height - 2;
		}
	}

	public void UpdateCalendar(){
		for (int i = 0; i < transform.Find ("Date/Calendar").childCount; i++) {
			Transform Date = transform.Find ("Date/Calendar").GetChild (i);
			Date.Find ("Text").GetComponent<Text> ().text = "";
			Date.Find ("Text").GetComponent<Text> ().color = new Color (99.0f / 255 ,136.0f / 255 ,141.0f / 255 );
			Date.Find ("Image").GetComponent<Image> ().color = new Color (9.0f / 255 ,35.0f / 255 ,39.0f / 255 );
			EventTriggerListener.Get (Date.gameObject).onClick = null;
			Date.name = "null";
		}


		transform.Find ("Date/BK/Month").GetComponent<Text> ().text = m_months [m_CurDate.Month - 1] + " " + m_CurDate.Year.ToString();

		int mounthdays = DateTime.DaysInMonth (m_CurDate.Year, m_CurDate.Month); 
		int weekindex = m_weeks.IndexOf(DateTime.Parse (m_CurDate.Year + "/" + m_CurDate.Month + "/1").DayOfWeek);
		int monthindex = 1;

		int	  index 	= 0;
		for(int i = 0; i < transform.Find ("Date/Calendar").childCount; i++){
			Transform Date = transform.Find ("Date/Calendar").GetChild(i);

			if(index >= weekindex && monthindex <= mounthdays){
				Date.name = m_CurDate.Year + "/" + m_CurDate.Month + "/" + monthindex.ToString ();
				Date.Find("Text").GetComponent<Text> ().text = monthindex.ToString();
				EventTriggerListener.Get(Date.gameObject).onClick = onClickButtonHandler;
				monthindex++;
			}

			if(Date.name == m_BeginDate){
				Date.Find ("Text").GetComponent<Text> ().color = new Color (1.0f / 255 ,56.0f / 255 ,64.0f / 255 );
				Date.Find ("Image").GetComponent<Image> ().color = new Color (135.0f / 255 ,185.0f / 255 ,191.0f / 255 );
			}

			if(Date.name == m_EndDate){
				Date.Find ("Text").GetComponent<Text> ().color = new Color (70.0f / 255, 56.0f / 255, 12.0f / 255);
				Date.Find ("Image").GetComponent<Image> ().color = new Color (191.0f / 255, 178.0f / 255, 135.0f / 255);
			}

			if(!string.IsNullOrEmpty (m_BeginDate) && !string.IsNullOrEmpty (m_EndDate) && Date.name != "null"){
				DateTime BeginDate = DateTime.Parse (m_BeginDate);
				DateTime EndDate = DateTime.Parse (m_EndDate);
				DateTime TempDate = DateTime.Parse (Date.name);

				if (DateTime.Compare (TempDate, EndDate) < 0 && DateTime.Compare (TempDate, BeginDate) > 0) {
					Date.Find ("Text").GetComponent<Text> ().color = Color.white;
					Date.Find ("Image").GetComponent<Image> ().color = new Color (16.0f / 255, 47.0f / 255, 52.0f / 255);
				}
			}

			index++;
		}
	}

	public void ChangeMonth(int type){
		if (type == 1) {
			m_CurDate = m_CurDate.AddMonths(1);
		} else {
			m_CurDate = m_CurDate.AddMonths(-1);
		}
		UpdateCalendar ();
	}

	public void onClickButtonHandler(GameObject obj){
		LobbyControl.PlayerButtonEffect ();

		if (string.IsNullOrEmpty (obj.name)) {
			return;
		}

		if (string.IsNullOrEmpty (m_BeginDate) && string.IsNullOrEmpty (m_EndDate)) {
			m_BeginDate = obj.name;
			UpdateCalendar ();
			return;
		}

		if (!string.IsNullOrEmpty (m_BeginDate) && string.IsNullOrEmpty (m_EndDate)) {
			DateTime BeginDate = DateTime.Parse (m_BeginDate);
			DateTime EndDate = DateTime.Parse (obj.name);

			if (DateTime.Compare (BeginDate, EndDate) < 0) {
				m_EndDate = obj.name;
				UpdateCalendar ();
				return;
			} else {
				m_BeginDate = obj.name;
				UpdateCalendar ();
				return;
			}
		} 

		if(!string.IsNullOrEmpty (m_BeginDate) && !string.IsNullOrEmpty (m_EndDate)){
			m_BeginDate = obj.name;
			m_EndDate = null;
			UpdateCalendar ();
			return;
		}
		return;
	}
}

