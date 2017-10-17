﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using System;
using System.Xml.Linq;

public enum Environment{
	Production, Development
}

public class Configuration : MonoBehaviour {
	public static Configuration instant;
	public string serverVersion;
	public string version;
	[Header("Production")]
	public string prodRemoteUrl = "http://www.iyoovr.com/hsyx";
	[Header("Development")]
	public string devRemoteUrl = "http://www.iyoovr.com/hsyx";

	public string language = Language.Chinese;
	public bool enablePopupVideo = true;
	public Text message;
	public Environment environment;
	public Color mainColor;
	public Color uiGrey;

	public Image bg;
	private bool loaded = false;
	private bool timeToLoad = true;
	private string configStr;
	private bool versionLoaded = false;
	public GameObject slider;
	private AsyncOperation loadSceneAsync;
	private bool loadingNextScene;
	public static bool initialized = false;
	private GameObject loadingPanel;
	private GameObject loadingImage;

	// Use this for initialization
	//	void Start () {
	//		StartCoroutine (NextSceneAfterSeconds (5));
	//	}

	void Awake(){
		StatusBar.Show ();
		loadingNextScene = false;
		loadingPanel = GameObject.Find ("Canvas").GetChildByName ("LoadingPanel");
		loadingImage = loadingPanel.GetChildByName ("Loader");
		Director.version = new Version (version);
		Director.environment = environment;
		Configuration.instant = this;
		if (environment == Environment.Development) {
			Request.RemoteUrl = devRemoteUrl;
		} else {
			Request.RemoteUrl = prodRemoteUrl;
		}
		if (!string.IsNullOrEmpty(serverVersion))
			Request.RemoteUrl += "/" + serverVersion;
	}

	// Use this for initialization
	void Start () {
		//slider.AddComponent<Button> ();
		//slider.GetComponent<Button> ().onClick.AddListener ();
		StartCoroutine (readConfig ());
		//message.text = "";
	}

	void Update(){
		if (loadingNextScene && !loadSceneAsync.isDone) {
			//Logger.Log (loadSceneAsync.progress.ToString(), "green");
			//message.text = loadSceneAsync.progress.ToString ();
			loadingImage.transform.Rotate(0,0,-10);
		}
	}

	IEnumerator readConfig ()
	{ 
		if (initialized) {
			OnLoaded ();
			yield return null;
		}
		yield return Request.ReadRemote ("version.xml", (str)=>{
			Debug.Log(str);
			XElement verXml = XDocument.Parse(str).Root;
			Version v = new Version(Xml.Attribute(verXml, "version"));
			if(v.GreaterThan(Director.version)){
				InitVersionPanel(Xml.Attribute(verXml, "forceupdate")=="true",
				Application.platform == RuntimePlatform.IPhonePlayer?Xml.Attribute(verXml, "ios"):Xml.Attribute(verXml, "android"));
			}else{
				versionLoaded = true;
			}
		}, ()=>	versionLoaded = true);
		while (!versionLoaded)
			yield return null;
		ConfigLoader loader = new ConfigLoader ();
		//StartCoroutine (PrepareForStart (2));
		yield return loader.LoadConfig ("ui/config.xml");
		yield return Request.ReadPersistent ("ui/config.xml", str=>configStr = str);
		if (!String.IsNullOrEmpty (configStr)) {
			yield return I18n.Initialise (language);
			Director.Initialize (XDocument.Parse(configStr).Root);
			yield return Director.Load ();
			Director.style.mainColor = mainColor;
			Director.style.uiGrey = uiGrey;
			OnLoaded();
		} else {
			if (I18n.language == Language.Chinese) {
				message.text = "初始化失败，请检查网络连接";
			}else
				message.text = "Failed to initialise, please check your Internet Connection";
		}
	}

	public void OnScanClick(){
		//SceneManager.LoadScene ("Guanyin");
		loadingPanel.SetActive(true);
		loadSceneAsync = SceneManager.LoadSceneAsync("Guanyin");
		loadingNextScene = true;
	}

	public void NextScene(){
		//if (loaded && timeToLoad)
		//	SceneManager.LoadScene ("Selection");
	}

	public void OnClick(){
		//if (loaded)
		//	SceneManager.LoadScene ("Selection");
	}

	public IEnumerator PrepareForStart(int second){
		yield return new WaitForSeconds (second);
		timeToLoad = true;
		NextScene ();
	}

	public void OnLoaded(){
		initialized = true;
		loaded = true;
		ZijiController ziji = GetComponent<ZijiController> ();
		StartCoroutine (ziji.Initialize ());
		//WWW www = new WWW (Request.Read ());
		//StartCoroutine(LoadBg());
		//NextScene ();
	}

	public void InitVersionPanel(bool forceUpdate, string url){
		OKCancelPanel panel = Director.LoadPrefab ("OKCancelPanel", GameObject.Find("Canvas")).GetComponent<OKCancelPanel>();
		panel.autoTranslate = false;
		//panel.gameObject.SetActive (false);
		if (forceUpdate) {
			panel.okText.text = "更新";
			panel.cancelText.text = "关闭";
			panel.onCancelHandler = () => {
				Application.Quit ();
			};
		} else {
			panel.okText.text = "更新";
			panel.cancelText.text = "取消";
			panel.onCancelHandler = () => {
				panel.gameObject.SetActive (false);
				versionLoaded = true;
			};
		}
		panel.description.text = "发现新版本,是否进行更新";
		panel.onOKHandler = () => {
			Application.OpenURL (url);
		};
	}

	IEnumerator LoadBg(){
		WWW www = new WWW (Request.GetPersistentPath("/ui/bg.jpg"));
		yield return www;
		if (string.IsNullOrEmpty (www.error)) {
			Texture2D tex = www.texture;
			bg.sprite = Sprite.Create (tex, new Rect (0.0f, 0.0f, tex.width, tex.height), new Vector2 (0.5f, 0.5f));
		}
	}
}
