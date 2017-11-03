using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;


public class HorizontalSlider : MonoBehaviour
{
	public bool draging = false;

	private float startDragX;
	private float startTransformX;
	private RectTransform transform;
	private int itemsCount;
	public float itemWidth;
	public float itemGap;
	private float minX;
	private float maxX;
	public bool enabled;
	public int index;
	public Action<int> onIndexChanged;

//	public void Start ()
//	{
//		transform = GetComponent<RectTransform> ();
//		maxX = transform.localPosition.x + itemGap;
//		minX = transform.localPosition.x - (itemsCount - 1) * (itemGap + itemWidth) - itemGap;
//	}

	public IEnumerator Initialize(List<String> icons){
		itemsCount = icons.Count;
		index = 0;
		transform = GetComponent<RectTransform> ();
		maxX = transform.localPosition.x + itemGap;
		minX = transform.localPosition.x - (itemsCount - 1) * (itemGap + itemWidth) - itemGap;
		for (int i = 0; i < itemsCount; i++) {
			GameObject obj = null;
			yield return Request.LoadImage (Application.persistentDataPath +"/" + icons [i], (o) => obj = o);
			RectTransform rect = obj.GetComponent<RectTransform> ();
			rect.localPosition = rect.localPosition.SetX((itemWidth + itemGap) * i);
			obj.transform.SetParent (transform, false);
		}
		enabled = true;
		Select (0);
	}

	public void SliderBeginDrag(){
		draging = true;
		startDragX = Input.mousePosition.x;
		startTransformX = transform.localPosition.x;
		Debug.Log (Input.mousePosition.ToString());
	}

	public void SliderEndDrag(){
		draging = false;
		//int portion = Mathf.FloorToInt (-transform.localPosition.x * 2/ (itemGap + itemWidth));
		//int itemIdx = Mathf.FloorToInt ((portion + 1) / 2);
		//GetComponent<Transform>().D
		int origIdx = index;
		float flipDistance = 100;
		if (transform.localPosition.x - startTransformX > flipDistance) {
			index = Mathf.Max (index - 1, 0);
		}else if (transform.localPosition.x - startTransformX < -flipDistance) {
			index = Mathf.Min (index + 1, itemsCount - 1);
		}

		transform.DOLocalMoveX(-index * (itemGap + itemWidth), .1f);
		if (origIdx != index && onIndexChanged != null)
			onIndexChanged.Invoke (index);
		Debug.Log (transform.localPosition.x + " " + startTransformX  + " " + index + " SliderEndDrag");
	}

	public void Select(int idx){
		index = idx;
		if (onIndexChanged != null)
			onIndexChanged.Invoke (index);
	}

	public void SliderDrag(){
		//Debug.Log ("SliderDrag");
	}


	void Update(){
		if (!enabled)
			return;
		if (draging) {
			transform.localPosition = transform.localPosition.SetX(Mathf.Clamp( startTransformX + Input.mousePosition.x - startDragX , minX, maxX ) );  
		}
	}
}
