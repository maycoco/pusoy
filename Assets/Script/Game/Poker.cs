using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class Poker : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IDragHandler, IEndDragHandler, IBeginDragHandler{
	public 	int 				PokerID;
	public 	string 				Belong;
	public 	StateSorting		StateSorting;
	public 	RectTransform 		canvas;
	private RectTransform 		imgRect;

	public bool 				TouchSwitch;
	public  bool				IsSelected;

	public  int 				SiblingIndex;

	private Vector3 BelongPos	= new Vector3();
	Vector2 offset 				= new Vector3();

	private Vector3	TouchPos	= new Vector3();

	// Use this for initialization
	void Start () {
	}

	void Awake(){
		imgRect = GetComponent<RectTransform>();
		TouchSwitch = false;
		IsSelected 	= false;
	}

	// Update is called once per frame
	void Update () {
	}
		
	public void ReseatSelected(){
		IsSelected = false;
	}

	public void Selected(){
		IsSelected = true;
		Vector3 pos = transform.localPosition;
		transform.localPosition = new Vector3 (pos.x, pos.y + 30, pos.z);
	}

	public void CancelSelect(){
		IsSelected = false;
		transform.localPosition = BelongPos;
	}

	public void RiwerSelected(){
		if (IsSelected) {
			Selected ();
		} else {
			CancelSelect ();
		}
	}

	public void OnPointerDown(PointerEventData eventData){
		if(!TouchSwitch){return;}
		TouchPos = transform.localPosition;

		Vector2 mouseDown = eventData.position; 
		Vector2 mouseUguiPos = new Vector2 ();

		bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle (canvas, mouseDown, eventData.enterEventCamera, out mouseUguiPos);
		if (isRect) {
			offset = imgRect.anchoredPosition - mouseUguiPos;
		}
	}
		
	public void OnEndDrag(PointerEventData eventData){
		offset = Vector2.zero;
	}

	public void OnBeginDrag(PointerEventData eventData){
		if(IsSelected){
			StateSorting.DragSelects (PokerID);
		}
	}
		
	public void OnDrag(PointerEventData eventData){
		if(!TouchSwitch){return;}

		transform.SetAsLastSibling ();
		Vector2 mouseDrag = eventData.position;
		Vector2 uguiPos = new Vector2();

		bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, mouseDrag, eventData.enterEventCamera, out uguiPos);

		if (isRect)
		{
			imgRect.anchoredPosition = offset + uguiPos;
		}
	}

	public void OnPointerUp(PointerEventData eventData){
		if(!TouchSwitch){return;}

		if(CheckSelected ()){
			return;
		}

		offset = Vector2.zero;
		Vector3 pos =  gameObject.transform.position; 
		string	tag	= "";

		if (InRect (pos, GameObject.Find ("Upper"))) {
			tag = "Upper";
		} 
		else if (InRect (pos, GameObject.Find ("Middle"))) {
			tag = "Middle";
		} 
		else if (InRect (pos, GameObject.Find ("Under"))) {
			tag = "Under";
		}
		else if (InRect (pos, GameObject.Find ("Hand"))) {
			tag = "Hand";
		} 
		else {
			tag = "Other";
		}


		if (IsSelected) {
			StateSorting.EndDragSelects ();
			StateSorting.DragMovePokers (Belong, tag, new int[]{PokerID});
		} else {
			StateSorting.MovePokers (Belong, tag, new int[]{PokerID});
		}
	}

	public bool CheckSelected(){
		if (transform.localPosition == TouchPos) {
			if (!IsSelected) {
				IsSelected = true;
				StateSorting.AddSelectPokers (PokerID);
			} else {
				StateSorting.RemoveSelectPokers (PokerID);
			}
			return true;
		}
		return false;
	}

	public bool InRect(Vector3 pos, GameObject obj){

		float left = obj.transform.position.x - obj.GetComponent<RectTransform> ().sizeDelta.x / 2 ;
		float right = obj.transform.position.x + obj.GetComponent<RectTransform> ().sizeDelta.x / 2 ;
		float top = obj.transform.position.y + obj.GetComponent<RectTransform> ().sizeDelta.y / 2;
		float btm = obj.transform.position.y - obj.GetComponent<RectTransform> ().sizeDelta.y / 2;
		float[] rect = new float[]{ left, right, top, btm };

		if (pos.x >= rect [0] && pos.x <= rect [1] && pos.y <= rect [2] && pos.y >= rect [3]) {
			return true;
		} 
		else {
			return false;
		}
	}

	public void SetBelongPos(Vector3 pos){
		BelongPos = pos;
	}

	public Vector3 GetBelongPos(){
		return BelongPos;
	}

	public void ResetBelongPos(){
		transform.localPosition = BelongPos;
	}

	public void AnimeMove(Vector3 Pos, float Time){
		TouchSwitch = false;
		transform.DOLocalMove(Pos, Time, true);
		InvokeRepeating("OpenTouchSwitch", Time, 0f);
	}

	public void OpenTouchSwitch(){
		CancelInvoke ();
		TouchSwitch = true;
	}

	public void CloseTouchSwitch(){
		CancelInvoke ();
		TouchSwitch = false;
	}

	public void ShowFace(){
		Image image = transform.GetComponent<Image>();
		image.sprite = Resources.Load("Image/Poker/" + PokerID, typeof(Sprite)) as Sprite;
	}

	public void ShowBack(){
		Image image = transform.GetComponent<Image>();
		image.sprite = Resources.Load("Image/Poker/poker_back", typeof(Sprite)) as Sprite;
	}
}