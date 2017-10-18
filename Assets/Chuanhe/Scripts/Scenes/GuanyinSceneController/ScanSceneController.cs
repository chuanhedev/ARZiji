using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;

namespace Guanyin
{

	public enum Direction
	{
		Up,
		Down,
		Null
	}

	public enum ScanSceneState
	{
		GuanyinShow,
		GuanyinMove,
		EnterName,
		AnimationAfterName,
		FuShown,
		ShowScroll
	}

	public class ScanSceneController : MonoBehaviour
	{

		public static ScanSceneController instant;
		public static GameObject currentTrackableObject;
		public StartLightController startLightController;
		public GameObject cloudRight;
		public GameObject cloudLeft;
		private Vector3 cloudRightPosition;
		private Vector3 cloudLeftPosition;
		private Vector3 guanyinPosition;
		//private Vector3 dizuoPosition;
		public float cloudSpeed = 1f;
		private bool cloudMoving = false;
		public GameObject guanyin;
		public bool playing;
		public GameObject raysContainer;

		public Text text;
		private float moveAplitude = 10f;
		private int count = 0;
		private float lastGyroY;
		private float startGyroY;
		private string gyroHistory = "";
		private Direction lastDirection = Direction.Null;
		private int historyLength = 3;
		private List<Direction> histories = new List<Direction> ();
		private DelayCall delayCall = new DelayCall ();
		private DelayCall inputDelayCall = new DelayCall ();
		//private DelayCall falunDelayCall = new DelayCall ();
		private bool guanyinMoving = false;
		private Tweener tweener;
		public InputField inputField;
		public GameObject canvas;
		//public GameObject falun;
		private Camera ARCamera;
		public GameObject enterNameObj;
		//private ISceneComponent enterNameScroll;
		public GameObject namePanel;
		public GameObject fuPanel;
		//private bool enterNameMoving = false;
		private ScanSceneState state;
		private GameObject fuContainer;
		private List<Fu> fus = new List<Fu> ();
		public List<Texture> fuTexs;
		private Dictionary<string, ISceneComponent> sceneComponents = new Dictionary<string, ISceneComponent> ();
		//public GameObject dizuo;
		private float guanyinScale;
		private Quaternion guanyinRotation;
		public Light headLight;
		private GameObject guanyinCloud;
		public BezierSpline guanyinPath;
		public float guanyinMoveDuration = 5;
		private float guanyinMoveTime = 0;
		public MovingPathData guanyinPathData;

		//qi fu
		public GameObject qifuPanel;
		public Text qifuText;

		private AudioClip dabeizhouClip;
		public string dabeizhouPath;
		public GameObject sparkle;
		private GameObject scanner;
		private GameObject btnBack;
		private GameObject btnInfo;
		private GameObject infoPanel;
		[HideInInspector]
		public string sceneName = "item1";

		void Awake ()
		{
			StatusBar.Hide ();
			instant = this;
			ARCamera = Camera.main;
			cloudRightPosition = cloudRight.transform.position;
			cloudLeftPosition = cloudLeft.transform.position;
			guanyinPosition = guanyin.transform.localPosition;
			guanyinScale = guanyin.transform.localScale.x;
			guanyinRotation = guanyin.transform.localRotation;
			guanyinCloud = guanyin.GetChildByNameInChildren ("Cloud_GuanYin");
			scanner = canvas.GetChildByName ("Scanner");
			btnBack = canvas.GetChildByName ("BtnBack");
			btnInfo = canvas.GetChildByName ("BtnInfo");
			infoPanel = canvas.GetChildByName ("InfoPanel");

//		dizuoPosition = dizuo.transform.localPosition;
//		if (SystemInfo.supportsGyroscope) {
//			Input.gyro.enabled = true;
//			text.text = "gyro supported";
//			startGyroY = Input.gyro.attitude.eulerAngles.y;
//		}else
//			text.text = "gyro not supported";
			startGyroY = Camera.main.transform.localRotation.eulerAngles.y;
			raysContainer.SetActive (false);
			//sceneComponents.Add("scroll", new Scroll (enterNameObj));
			//sceneComponents.Add ("falun", new Falun (falun));
			sceneComponents.Add ("sparkle", new Sparkle (sparkle));
			//dizuo.transform.localScale = Vector3.one * .001f;
			headLight.enabled = false;
			//qifuPanel.SetActive (false);
			GetRandomFu ();
			StartCoroutine (LoadAudio ());
		}



		private AudioSource GetAudioSource ()
		{
			AudioSource audio = GetComponent<AudioSource> ();
			if (audio != null)
				return audio;
			gameObject.AddComponent<AudioSource> ();
			audio = GetComponent<AudioSource> ();
			return audio;
		}


		IEnumerator LoadAudio ()
		{
			WWW www = new WWW (ResolvePath (Application.streamingAssetsPath + "/" + dabeizhouPath));
			yield return www;
			dabeizhouClip = www.GetAudioClip ();
		}

		static string ResolvePath (string path, bool addFilePrefix = true)
		{
			if (!addFilePrefix)
				return path;
			string str = "file:///" + path;
			str = str.Replace ("file:////", "file:///");
			return str;
		}

		// Use this for initialization
		void Start ()
		{
			//QifuClicked ();
		}

		public void Reset ()
		{
			headLight.enabled = false;
			guanyinMoveTime = 0;
			guanyin.transform.localPosition = new Vector3 (-1.25f, 1.97f, 1.07f);
			guanyin.transform.localScale = Vector3.one * 3;
			guanyin.transform.localRotation = Quaternion.Euler (0, 90, 0);
			qifuPanel.SetActive (false);


			for (int i = fus.Count - 1; i >= 0; i--) {
				Destroy (fus [i].gameObject);
			}
			foreach (string key in sceneComponents.Keys) {
				sceneComponents [key].Reset ();
			}
			fus = new List<Fu> ();
			fuPanel.SetActive (false);
			namePanel.SetActive (false);
			raysContainer.SetActive (false);
			startLightController.Reset ();
			cloudRight.transform.position = cloudRightPosition;
			cloudLeft.transform.position = cloudLeftPosition;
			delayCall.Cancel ();
			inputDelayCall.Cancel ();
			guanyinMoving = false;
			cloudMoving = false;
			tweener.Kill ();
			//guanyin.transform.localPosition = new Vector3 (guanyinPosition.x, guanyinPosition.y, guanyinPosition.z + 2);
			guanyin.SetActive (false);
		}

		public void PlayAnimation ()
		{
			Debug.Log ("PlayAnimation");
			btnBack.SetActive (false);
			infoPanel.SetActive (false);
			btnInfo.SetActive (false);
			scanner.SetActive (false);
			//StartCoroutine(startLightController.ShowLight());
			playing = true;
			//state = ScanSceneState.AnimationAfterTrack;
			delayCall.Call (1f, () => {
				//startLightController.Play ();
				sceneComponents ["sparkle"].Play ();
			});

			delayCall.Call (3.5f, () => {
				state = ScanSceneState.GuanyinShow;
				guanyin.SetActive (true);
				guanyin.transform.position = guanyinPath.transform.position;// - new Vector3(0, .1f, 0);
				guanyin.transform.localScale = Vector3.zero;
				guanyin.transform.DOScale (3, 3f).SetEase (Ease.OutQuad);
			});
			//dizuo.transform.localPosition = new Vector3 (dizuoPosition.x, dizuoPosition.y - 0.5f, dizuoPosition.z);
			Vector3 cameraDir = Camera.main.transform.forward;
			//guanyin.transform.parent.LookAt (new Vector3(-cameraDir.x, 0, -cameraDir.z));
			delayCall.Call (4f, () => {
				headLight.enabled = true;
				state = ScanSceneState.GuanyinMove;
				ParticleSystem guanyinCloudParticle = guanyinCloud.GetComponent<ParticleSystem> ();
				guanyinCloudParticle.Play ();

				//guanyinCloud = guanyin.GetChildByNameInChildren ("Cloud_GuanYin");

				//this.guanyinMoving = true;

				Debug.Log ("delayCall guanyin");
				//tweener = guanyin.transform.DOLocalMove(guanyinPosition, 5f).SetEase(Ease.OutQuad);
				//guanyin.transform.DOLocalMove(guanyinPosition, 5f).SetEase(Ease.Linear);

				/////////////////////wo xie de
				/*List<GameObject> pathPointList = guanyinPathData.pathPointData;
			int pathPointIndex = 0;
			foreach(GameObject pathPoint in pathPointList){
			//	delayCall.Call(pathPointIndex, ()=>{
					guanyin.transform.DOLocalMove(pathPoint.transform.localPosition, 1f).SetEase(Ease.Linear);
				});
				pathPointIndex++;
			}*/
				/////////////////////wo xie de


				guanyin.transform.DOScale (guanyinScale, 5f).SetEase (Ease.OutQuad);
				guanyin.transform.DOLocalRotate (new Vector3 (0, 180, 0), 5f).SetEase (Ease.OutQuad);
			});

//			delayCall.Call (2f, () => {
//				dizuo.transform.DOScale (Vector3.one, 2f).SetEase (Ease.OutQuart);
//			});
//		inputDelayCall.Call(9f, ()=>{
//			canvas.SetActive (true);
//		});
//		delayCall.Call(5f, ()=>{
//			falun.transform.SetParent(ARCamera.gameObject.transform);
//			falun.transform.DOLocalMove(  ARCamera.gameObject.transform.InverseTransformPoint(ARCamera.gameObject.transform.forward) * .1f, 5f).SetEase(Ease.OutQuad);
//		});

			delayCall.Call (9f, () => {
				namePanel.SetActive (true);
				//enterNameMoving = true;
				//state = ScanSceneState.ShowScroll;
				//sceneComponents["scroll"].Play();
				UpdateState (ScanSceneState.EnterName);
			});
			cloudMoving = true;
		}

		Direction GetDirection ()
		{
			if (histories.Count < historyLength)
				return Direction.Null;
			Direction b = histories [0];
			for (int i = 0; i < histories.Count; i++)
				if (histories [i] != b)
					return  Direction.Null;
			return b;
		}


		void GyroUpdate ()
		{
			text.text = gyroHistory; //angles.x.ToString () + " " + angles.y.ToString () + " " + angles.z.ToString () + " " + gyroHistory;
			float currentY = Camera.main.transform.localRotation.eulerAngles.y;
			if (lastGyroY == currentY)
				return;
			Direction dir = currentY - lastGyroY > 0 ? Direction.Up : Direction.Down;
			//Debug.Log (currentY + " " + lastGyroY + " " + Mathf.Abs (currentY - startGyroY) + " " + dir + " " + gyroUp);
			Direction dirFull = dir;
			//Debug.Log (dir.ToString() + " " + dirFull.ToString());
			if (dirFull != Direction.Null) {
				if (lastDirection != Direction.Null && lastDirection != dirFull) {
					Debug.Log (dir.ToString () + " " + dirFull.ToString () + " " + startGyroY + " " + currentY + " " + Mathf.Abs (currentY - startGyroY));
					if (Mathf.Abs (currentY - startGyroY) > moveAplitude) {
						if (dirFull == Direction.Up)
							gyroHistory += "U";
						else if (dirFull == Direction.Down)
							gyroHistory += "D";
					}
					startGyroY = currentY;
				}
				lastDirection = dirFull;
			}
			lastGyroY = currentY;
			histories.Add (dir);
			if (histories.Count > historyLength)
				histories.RemoveAt (0);
		}

		public void QifuAgainClicked ()
		{
			btnBack.SetActive (true);
			UpdateState (ScanSceneState.AnimationAfterName);
		}

		public void QifuClicked ()
		{
			UpdateState (ScanSceneState.AnimationAfterName);
		}

		public void Qifu2Clicked ()
		{
			UpdateState (ScanSceneState.FuShown);
		}

		private void GetRandomFu ()
		{
			Debug.Log ("GetRandomFu");
			Texture tex = fuTexs [UnityEngine.Random.Range (0, fuTexs.Count - 1)];
			fuPanel.GetChildByNameInChildren ("Content").GetComponent<Image> ().sprite = Sprite.Create (tex as Texture2D, new Rect (0, 0, tex.width, tex.height), new Vector2 (0, 0));
			//fuPanel.GetChildByNameInChildren ("Content").GetComponent<Image> ().material.mainTexture = tex;
			//material.mainTexture = fus[UnityEngine.Random.Range(0, fus.Count -1 )];
		}

		void UpdateState (ScanSceneState s)
		{
			state = s;
			if (s == ScanSceneState.FuShown) {
				//state = ScanSceneState.FuShown;
				qifuPanel.SetActive (false);
				fuPanel.SetActive (true);
				int year = DateTime.Now.Year;
				Text yTxt = fuPanel.GetChildByNameInChildren ("Year").GetComponent<Text> ();
				Text yTxt2 = fuPanel.GetChildByNameInChildren ("Year2").GetComponent<Text> ();
				if (year == 2018) {
					yTxt.text = "二零一八";
					yTxt2.text = "戊戌年";
				} else if (year == 2019) {
					yTxt.text = "二零一九";
					yTxt2.text = "己亥年";
				} else {
					yTxt.text = "二零一七";
					yTxt2.text = "丁酉年";
				}
				GetRandomFu ();

			} else if (s == ScanSceneState.EnterName) {
				//sceneComponents["scroll"].Reset();
				namePanel.SetActive (true);
				inputField.Select ();
				inputField.ActivateInputField ();
			} else if (s == ScanSceneState.AnimationAfterName) {
				namePanel.SetActive (false);
//			if (fuContainer == null) {
//				fuContainer = new GameObject ("FuContainer");
//				fuContainer.transform.SetParent (ARCamera.gameObject.transform, false);
//			}
				//sceneComponents ["falun"].Play ();
				raysContainer.SetActive (true);
				qifuPanel.SetActive (true);
				fuPanel.SetActive (false);
				//delayCall.Call (10f, ()=>UpdateState (ScanSceneState.FuShown));
				qifuText.text = inputField.text;
				AudioSource audio = GetAudioSource ();
				audio.clip = dabeizhouClip;
				if (!audio.isPlaying) {
					audio.Play ();
					audio.volume = 0;
				}
			}
		}



		public void OnBackClicked ()
		{
			SceneManager.LoadSceneAsync ("Start");
		}


		public void OnInfoClick ()
		{
			ScanSceneController.instant.infoPanel.SetActive (!infoPanel.activeSelf);
		}

		public void OnInfoLinkClick ()
		{
			Application.OpenURL (Request.RemoteUrl + I18n.Translate (sceneName + "_infolink"));
		}

		public void OnInfoCloseClick ()
		{
			ScanSceneController.instant.infoPanel.SetActive (false);
		}

		void Update ()
		{
			foreach (string key in sceneComponents.Keys) {
				sceneComponents [key].Update ();
			}
			if ((Input.touches.Length > 0 || Input.GetMouseButtonDown (0)) && state == ScanSceneState.EnterName) {
				Debug.Log ("Mouse Down");
				inputField.Select ();
			}
			if (state == ScanSceneState.GuanyinMove) {
				headLight.range = guanyin.transform.localScale.x * guanyin.transform.localScale.x * 0.0004f;
				//guanyinCloud.transform.localScale = Vector3.one * guanyin.transform.localScale.x * 0.0003f;


				ParticleSystem guanyinCloudParticle = guanyinCloud.GetComponent<ParticleSystem> ();
				ParticleSystem.ShapeModule shape = guanyinCloudParticle.shape;
				shape.meshScale = guanyin.transform.localScale.x * guanyin.transform.localScale.x * 0.004f;
				//guanyinCloudParticle.shape = shape;


				//} else if (state == ScanSceneState.GuanyinMove) {
				guanyinMoveTime += Time.deltaTime / guanyinMoveDuration;
				//Debug.Log (guanyinMoveTime.ToString());
				Vector3 v3 = guanyinPath.GetPoint (EaseCurve.OneMinusCos (Mathf.Min (1, guanyinMoveTime)));
				//v3 = guanyin.transform.parent.InverseTransformPoint (v3);
				Debug.Log (guanyinMoveTime.ToString () + " " + v3.x.ToString () + " " + v3.y + " " + v3.z);
				guanyin.transform.position = v3;//guanyinPath.GetPoint (Mathf.Max(1, Time.deltaTime / guanyinMoveTime));
			} else if (state == ScanSceneState.AnimationAfterName) {
				AudioSource audio = GetAudioSource ();
				if (audio.volume < 1) {
					audio.volume += 0.0013f;
				} else {
					audio.volume = 1;
				}
			}
//		if (state == ScanSceneState.AnimationAfterName || state == ScanSceneState.FuShown) {
//			if (Time.frameCount % 10 == 0) {
//				Fu fu = new Fu(fuContainer);
//				fus.Add (fu);
//			}
//			for (int i = fus.Count - 1; i >= 0; i--) {
//				fus [i].Update ();
//				if (fus [i].gameObject.transform.localPosition.y < -10) {
//					Destroy (fus [i].gameObject);
//					fus.RemoveAt (i);
//					i++;
//				}
//			}
//		}
			delayCall.Update ();
			//inputDelayCall.Update ();
			//Vector3 cameraDir = Camera.main.transform.forward;
			//raysContainer.transform.LookAt (new Vector3(-cameraDir.x, 0, -cameraDir.z));
			//if (Input.gyro.enabled) {
			//Vector3 angles = Input.gyro.attitude.eulerAngles;
			



			if (!cloudMoving)
				return;
		
			Vector3 cloudRightPos = cloudRight.transform.localPosition;
			cloudRight.transform.localPosition = new Vector3 (cloudRightPos.x + cloudSpeed * Time.deltaTime * 2, cloudRightPos.y, cloudRightPos.z);
			Vector3 cloudLeftPos = cloudLeft.transform.localPosition;
			cloudLeft.transform.localPosition = new Vector3 (cloudLeftPos.x - cloudSpeed * Time.deltaTime, cloudLeftPos.y, cloudLeftPos.z);
//		if (guanyinMoving) {
//			Debug.Log ("update");
//			Vector3 gyPos = guanyin.transform.localPosition;
//			guanyin.transform.localPosition = new Vector3 (gyPos.x, gyPos.y - (gyPos.y - guanyinPosition.y) * Time.deltaTime, gyPos.z - (gyPos.z - guanyinPosition.z) * Time.deltaTime);
//		}
			//Vector3 dizuoPos = dizuo.transform.localPosition;
			//dizuo.transform.localPosition = new Vector3 (dizuoPos.x, dizuoPos.y -  (dizuoPos.y - dizuoPosition.y)  * Time.deltaTime, dizuoPos.z -  (dizuoPos.z - dizuoPosition.z)  * Time.deltaTime );
			//dizuo.transform.Rotate (0, Time.deltaTime, 0);
		}
	}

	public class Vector3Extension
	{

		public static Vector3 GetRandomVector3 ()
		{
			return new Vector3 (UnityEngine.Random.Range (0f, 1f), UnityEngine.Random.Range (0f, 1f), UnityEngine.Random.Range (0f, 1f));
		}

		//	public static Vector3 GetRandomVector3(){
		//		return new Vector3(UnityEngine.Random.Range (0f, 1f), UnityEngine.Random.Range (0f, 1f), UnityEngine.Random.Range (0f, 1f));
		//	}
	}

	interface ISceneComponent
	{
		GameObject gameObject { get; set; }

		void Reset ();

		void Update ();

		void Play ();
	}

	public class Falun: ISceneComponent
	{
		public GameObject gameObject { get; set; }

		private Vector3 origPosition;
		private Quaternion origRotation;
		private Transform origParent;
		private float origScale;
		private bool playing;

		public Falun (GameObject obj)
		{
			gameObject = obj;
			origScale = obj.transform.localScale.x;
			origPosition = obj.transform.localPosition;
			origRotation = obj.transform.localRotation;
			origParent = obj.transform.parent;
		}

		public void Play ()
		{
			playing = true;
			//gameObject.transform.SetParent(Camera.main.gameObject.transform);
			//gameObject.transform.DOLocalMove(Camera.main.gameObject.transform.InverseTransformPoint(Camera.main.gameObject.transform.forward) * .1f, 5f).SetEase(Ease.OutQuad);
			gameObject.transform.DOLocalMove (new Vector3 (origPosition.x, origPosition.y, origPosition.z + .015f), 5f).SetEase (Ease.OutQuad);
			gameObject.transform.DOScale (Vector3.one * origScale * 1.5f, 5f).SetEase (Ease.OutQuad);
		}

		public void Reset ()
		{
			gameObject.transform.SetParent (origParent);
			gameObject.transform.localPosition = origPosition;
			gameObject.transform.localRotation = origRotation;
			gameObject.transform.localScale = Vector3.one * origScale;
		}

		public void Update ()
		{
			if (!playing)
				return;
			gameObject.transform.Rotate (0, 0, Time.deltaTime * 500);
		}
	}

	public class FalunAttachedToCenter: ISceneComponent
	{
		public GameObject gameObject { get; set; }

		private Vector3 origPosition;
		private Quaternion origRotation;
		private Transform origParent;
		private bool playing;

		public FalunAttachedToCenter (GameObject obj)
		{
			gameObject = obj;
			origPosition = obj.transform.localPosition;
			origRotation = obj.transform.localRotation;
			origParent = obj.transform.parent;
		}

		public void Play ()
		{
			playing = true;
			gameObject.transform.SetParent (Camera.main.gameObject.transform);
			gameObject.transform.DOLocalMove (Camera.main.gameObject.transform.InverseTransformPoint (Camera.main.gameObject.transform.forward) * .1f, 5f).SetEase (Ease.OutQuad);
		}

		public void Reset ()
		{
			gameObject.transform.SetParent (origParent);
			gameObject.transform.localPosition = origPosition;
			gameObject.transform.localRotation = origRotation;
		}

		public void Update ()
		{
			if (!playing)
				return;
			gameObject.transform.Rotate (0, 0, Time.deltaTime * 500);
		}
	}


	public class Sparkle: ISceneComponent
	{
		public ParticleSystem particleSystem { get; set; }

		public GameObject gameObject { get; set; }
		//	private Vector3 origPosition;
		//	private Quaternion origRotation;
		//	private Transform origParent;
		private bool playing;
		private float size = 0.001f;
		private float sizeInc = 0.00015f;

		public Sparkle (GameObject obj)
		{
			gameObject = obj;
			particleSystem = obj.GetComponent<ParticleSystem> ();
			gameObject.SetActive (false);
			//origPosition = obj.transform.localPosition;
			//origRotation = obj.transform.localRotation;
			//origParent = obj.transform.parent;
		}

		public void Play ()
		{
			playing = true;
			gameObject.SetActive (true);
			//gameObject.transform.SetParent(Camera.main.gameObject.transform);
			//gameObject.transform.DOLocalMove(Camera.main.gameObject.transform.InverseTransformPoint(Camera.main.gameObject.transform.forward) * .1f, 5f).SetEase(Ease.OutQuad);
		}

		public void Reset ()
		{
//		gameObject.transform.SetParent (origParent);
//		gameObject.transform.localPosition = origPosition;
//		gameObject.transform.localRotation = origRotation;
			playing = false;
			gameObject.SetActive (false);
		}

		public void Update ()
		{
			if (!playing)
				return;
			ParticleSystem.MainModule main = particleSystem.main;
			size += sizeInc;
			if (size > .06f && sizeInc > 0)
				sizeInc = -sizeInc;
			else if (sizeInc < 0 && size < .001f)
				Reset ();
			main.startSize = new ParticleSystem.MinMaxCurve (size / 5, size);
		}
	}
	//public class Scroll: ISceneComponent{
	//	public GameObject gameObject { get; set;}
	//	private float origWidth;
	//	private float origHeight;
	//	private bool playing;
	//	public float scrollingSpeed = 10f;
	//	public float scrollingMax = 830;
	//
	//	public Scroll(GameObject obj){
	//		gameObject = obj;
	//		origWidth = obj.GetComponent<RectTransform> ().rect.width;
	//		origHeight = obj.GetComponent<RectTransform> ().rect.height;
	//	}
	//
	//	public void Play(){
	//		playing = true;
	//		gameObject.SetActive (true);
	//	}
	//
	//	public void Reset(){
	//		gameObject.GetComponent<RectTransform> ().sizeDelta = new Vector2 (origWidth, origHeight);
	//		Hide ();
	//	}
	//
	//	public void Hide(){
	//		gameObject.SetActive (false);
	//	}
	//
	//	public void Update(){
	//		if (!playing)
	//			return;
	//		float width = gameObject.GetComponent<RectTransform> ().rect.width + scrollingSpeed;
	//		if (width > scrollingMax)
	//			playing = false;
	//		gameObject.GetComponent<RectTransform> ().sizeDelta = new Vector2 (width, origHeight);
	//	}
	//}

	public class Fu
	{
		public GameObject gameObject;
		public float speed;

		public Fu (GameObject parent)
		{
			speed = UnityEngine.Random.Range (3f, 5f);
			GameObject fu = new GameObject ();
			int fuIdx = UnityEngine.Random.Range (0, ScanSceneController.instant.fuTexs.Count - 1);
			GameObject f1 = GameObject.CreatePrimitive (PrimitiveType.Quad);
			f1.GetComponent<MeshRenderer> ().material.mainTexture = ScanSceneController.instant.fuTexs [fuIdx];
			GameObject f2 = GameObject.CreatePrimitive (PrimitiveType.Quad);
			f2.GetComponent<MeshRenderer> ().material.mainTexture = ScanSceneController.instant.fuTexs [fuIdx];
			f1.transform.SetParent (fu.transform);
			f2.transform.SetParent (fu.transform);
			f2.transform.localRotation = Quaternion.Euler (0, 180, 0);
			//fus.Add (fu);
			fu.transform.SetParent (parent.transform);
			fu.transform.localRotation = Quaternion.Euler (Vector3Extension.GetRandomVector3 () * 180);
			fu.transform.localScale = Vector3.one * .2f + Vector3Extension.GetRandomVector3 () * .2f;
			fu.transform.localPosition = new Vector3 (UnityEngine.Random.Range (-3f, 3f), UnityEngine.Random.Range (3f, 5f), UnityEngine.Random.Range (5f, 10f));
			gameObject = fu;
		}

		public void Update ()
		{
			Vector3 localpos = gameObject.transform.localPosition;
			gameObject.transform.localPosition = new Vector3 (localpos.x, localpos.y - speed * Time.deltaTime, localpos.z);
			gameObject.transform.Rotate (new Vector3 (UnityEngine.Random.Range (0f, 1f), UnityEngine.Random.Range (0f, 1f), UnityEngine.Random.Range (0f, 1f)) * 10);
		}
	}


	public class DelayCall
	{
		private List<Action> handlers = new List<Action> ();
		private float timer;
		private List<float> delayTimes = new List<float> ();
		private bool enabled;

		public DelayCall ()
		{

		}

		public void Call (float time, Action a, bool relative = true)
		{
			//handler = a;
			if (enabled) {
				delayTimes.Add (relative ? (delayTimes [delayTimes.Count - 1] + time) : time);
			} else {
				timer = 0;
				enabled = true;
				delayTimes.Add (time);
			}
			handlers.Add (a);
		}

		public void Cancel ()
		{
			Debug.Log ("DelayCall canceled");
			enabled = false;
			handlers = new List<Action> ();
			delayTimes = new List<float> ();
		}

		public void Update ()
		{
			if (enabled) {
				timer += Time.deltaTime;
				for (int i = delayTimes.Count - 1; i >= 0; i--) {
					if (timer >= delayTimes [i] && handlers [i] != null) {
						Debug.Log ("Delay Update " + i);
						handlers [i].Invoke ();
						handlers.RemoveAt (i);
						delayTimes.RemoveAt (i);
						if (handlers.Count > 0) {
							//i++;
						} else {
							Debug.Log ("Delay Canceled ");
							Cancel ();
						}
					}
				}
			}
		}
	}


	public static class GameObjectExtension
	{

		public static GameObject GetChildByName (this GameObject o, string name)
		{
			for (int i = 0; i < o.transform.childCount; i++) {
				GameObject obj = o.transform.GetChild (i).gameObject;
				if (obj.name == name)
					return obj;
			}
			return null;
		}


		public static GameObject GetChildByNameInChildren (this GameObject o, string name)
		{
			for (int i = 0; i < o.transform.childCount; i++) {
				GameObject obj = o.transform.GetChild (i).gameObject;
				if (obj.name == name)
					return obj;
				else {
					obj = obj.GetChildByNameInChildren (name);
					if (obj != null)
						return obj;
				}
			}
			return null;
		}
	}


	public static class EaseCurve
	{

		public static float Sine (float f)
		{
			return Mathf.Sin (f * Mathf.PI / 2);
		}

		public static float OneMinusSine (float f)
		{
			return 1 - Sine (f);
		}

		public static float Cos (float f)
		{
			return Mathf.Cos (f * Mathf.PI / 2);
		}

		public static float OneMinusCos (float f)
		{
			return 1 - Cos (f);
		}
	}
}