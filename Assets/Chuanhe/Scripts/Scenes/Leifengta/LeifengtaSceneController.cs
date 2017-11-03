using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LeifengtaSceneController : MonoBehaviour {

	private GameObject canvas;
	private GameObject tips;

	void Awake(){
		StatusBar.Hide ();
		canvas = GameObject.Find ("Canvas");
		tips = canvas.GetChildByName ("tips");
		Invoke ("HideTips", 5);
	}

	private void HideTips(){
		tips.SetActive (false);
	}
	
	public void OnBackClicked(){
		SceneManager.LoadScene ("Start");
	}
}
