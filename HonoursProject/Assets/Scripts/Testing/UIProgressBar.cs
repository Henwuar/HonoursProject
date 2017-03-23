using UnityEngine;
using System.Collections;

public class UIProgressBar : MonoBehaviour
{
    private RectTransform bar;

	// Use this for initialization
	void Start ()
    {
        bar = transform.FindChild("Image").GetComponent<RectTransform>();
	}

    public void SetPercentage(float percentage)
    {
        float width = GetComponent<RectTransform>().sizeDelta.x; 
        bar.sizeDelta = new Vector2(percentage * width, bar.sizeDelta.y);
    }
}
