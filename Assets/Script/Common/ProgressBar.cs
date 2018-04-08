using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour {
    public GameObject imgLoading;
	
    public void SetPercent(float percent)
    {
        Image image = imgLoading.GetComponent<Image>();
        image.fillAmount = percent;
    }
}
