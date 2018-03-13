using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class PageView : MonoBehaviour, IBeginDragHandler, IEndDragHandler {
	public List<UICircle> lightObj;


    private ScrollRect rect;                   
    private float targethorizontal = 0;        
    private bool isDrag = false;                 
    private List<float> posList = new List<float> ();
    private int currentPageIndex = -1;
    public Action<int> OnPageChanged;

	public Text Title;

    private bool stopMove = true;
    public float smooting = 4; 
    public float sensitivity = 0;
    private float startTime;

    private float startDragHorizontal; 


    void Awake () {
        rect = transform.GetComponent<ScrollRect> ();
        float horizontalLength = rect.content.rect.width - GetComponent<RectTransform> ().rect.width;
        posList.Add (0);
        for(int i = 1; i < rect.content.transform.childCount - 1; i++) {
            posList.Add (GetComponent<RectTransform> ().rect.width * i / horizontalLength);
        }
        posList.Add (1);
    }

    void Update () {
        if(!isDrag && !stopMove) {
            startTime += Time.deltaTime;
            float t = startTime * smooting;
            rect.horizontalNormalizedPosition = Mathf.Lerp (rect.horizontalNormalizedPosition , targethorizontal , t);
            if(t >= 1)
                stopMove = true;
        }
    }

    public void pageTo (int index) {
        if(index >= 0 && index < posList.Count) {
            rect.horizontalNormalizedPosition = posList[index];
            SetPageIndex(index);
        } else {
            Debug.LogWarning ("页码不存在");
        }
    }

    private void SetPageIndex (int index) {
        if(currentPageIndex != index) {
            currentPageIndex = index;
            if(OnPageChanged != null)
                OnPageChanged (index);

			if(index == 0){
				Title.text = "7 Days winning +1000";
			}
			else if(index == 1){
				Title.text = "30 Days winning +1000";
			}
			else if(index == 2){
				Title.text = "all Days winning +1000";
			}
        }
    }

    public void OnBeginDrag (PointerEventData eventData) {
        isDrag = true;
        startDragHorizontal = rect.horizontalNormalizedPosition; 
    }

    public void OnEndDrag (PointerEventData eventData) {
        float posX = rect.horizontalNormalizedPosition;
        posX += ((posX - startDragHorizontal) * sensitivity);
        posX = posX < 1 ? posX : 1;
        posX = posX > 0 ? posX : 0;
        int index = 0;
        float offset = Mathf.Abs (posList[index] - posX);
        for(int i = 1; i < posList.Count; i++) {
            float temp = Mathf.Abs (posList[i] - posX);
            if(temp < offset) {
                index = i;
                offset = temp;
            }
        }
        SetPageIndex (index);
		setLight (index);

        targethorizontal = posList[index];
        isDrag = false;
        startTime = 0;
        stopMove = false;
    } 

	public void setLight(int index){
		for(int i = 0; i < lightObj.Count; i++){
			lightObj [i].color = new Color (8.0f / 255, 42.0f / 255, 50.0f / 255);
		}

		lightObj [index].color = new Color (136.0f / 255, 229.0f / 255, 240.0f / 255);
	}
}
