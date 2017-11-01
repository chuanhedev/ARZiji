using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonCircle : MonoBehaviour {
	private RectTransform rect;
	public float maxScale = 3;
	public float speed = 1;

	// Use this for initialization
	void Start () {
		rect = GetComponent<RectTransform> ();
	}
	
	// Update is called once per frame
	void Update () {
		float scale = rect.localScale.x + speed * Time.deltaTime;
		if (scale > maxScale) {
			scale = 1;
			gameObject.SetAlpha (1);
		}
		rect.localScale = Vector3.one * scale; 
		gameObject.SetAlpha (Mathf.Max(0.2f, gameObject.GetAlpha()-Time.deltaTime));
	}
}
