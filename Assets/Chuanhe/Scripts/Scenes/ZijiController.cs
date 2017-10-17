using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Xml.Linq;
using System.IO;
using UnityEngine.UI;
using System;

public class ZijiController : MonoBehaviour
{
	public Text title;
	public Text subtitle;
	public Text description;
	public HorizontalSlider slider;
	private XElement layout;
	private List<string> titles = new List<string> ();
	private List<string> subtitles = new List<string> ();
	private List<string> descs = new List<string> ();


//	void Start ()
//	{
//		StartCoroutine (initScene ());
//	}

	public IEnumerator Initialize ()
	{
		yield return Request.ReadPersistent ("ui/ui.xml", LayoutLoaded);
		if (layout != null) {
			XElement itemsEle = layout.Element ("items");
			var items = itemsEle.Elements ();
			List<string> icons = new List<string> ();
			foreach (XElement item in items) {
				string _desc = Xml.Attribute (item, "desc");
				string _icon = Xml.Attribute (item, "icon");
				string _title = Xml.Attribute (item, "title");
				//string sub = Xml.Attribute (item, "help");
				string _subtitle = Xml.Attribute (item, "subtitle");
				titles.Add (_title);
				subtitles.Add (_subtitle);
				descs.Add (_desc);
				icons.Add (_icon);
			}
			slider.onIndexChanged = (idx) => {
				title.text = I18n.Translate(titles [idx]);
				description.text = I18n.Translate(descs [idx]);
				subtitle.text = I18n.Translate(subtitles [idx]);
			};
			yield return slider.Initialize (icons);
		}
	}

	bool Enabled{
		get{
			return enabled;
		}
		set{
			enabled = value;
		}
	}

//	void OnItemClick(SelectionItem item){
//		//item.gameObject.GetComponent<Button> ().interactable = false;
//		StartCoroutine (OnItemClickHandler (item));
//	}
//
//	IEnumerator OnItemClickHandler(SelectionItem item){
//		string name = item.name;
//		Logger.Log (name + " clicked");
//		okCancelPanel.Reset ();
//		Enabled = false;
//		configLoader = new ConfigLoader ();
//		//configLoader.loadedHandler = FileLoaded;
//		configLoader.progressHandler = FileProgressing;
//		configLoader.okCancelPanel = okCancelPanel;
//		yield return configLoader.LoadConfig (name + "/config.xml");
//		progressPanel.Hide ();
//		Enabled = true;
//		if (!configLoader.forceBreak && !okCancelPanel.isCancel) {
//			Hashtable arg = new Hashtable ();
//			arg.Add ("type", item.type);
//			arg.Add ("name", name);
//			arg.Add ("data", Xml.GetChildByAttribute(layout.Element ("items"), "title", name));
//			SceneManagerExtension.LoadScene ("Scan", arg);
//		}
//	}


//	void SelectTabButton(int idx, bool shown = true){
//		GameObject btn = tabButtons [idx];
//		Text text = btn.GetChildByName ("Text").GetComponent<Text> ();
//		Image icon = btn.GetChildByName ("Image").GetComponent<Image> ();
//		text.color = icon.color = shown ? Director.style.mainColor : Director.style.uiGrey;
//	}
//
//	void OnTabClicked(GameObject button){
//		Debug.Log (button.name);
//		int idx = tabButtons.IndexOf (button);
//		if (idx == activeTabIndex)
//			return;
//		activeTabIndex = idx;
//		for (int i = 0; i < tabs.Count; i++) {
//			tabs [i].SetActive (false);
//			SelectTabButton (i, false);
//		}
//		tabs [idx].SetActive (true);
//		SelectTabButton (idx);
//		normalCamera.gameObject.SetActive (idx != 1);
//		ARCamera.gameObject.SetActive (idx == 1);
//		header.GetComponent<Image> ().enabled = idx != 1;
//		header.GetComponentInChildren<Text> ().text = I18n.Translate ("select_title" + idx);
//		StatusBar.Show (idx != 1);
//	}

	void LayoutLoaded (string str)
	{
		layout = XDocument.Parse (str).Root;
	}
}
