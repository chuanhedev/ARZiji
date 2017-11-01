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
	private List<string> scenes = new List<string> ();


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
				if (!(Xml.Attribute (item, "platform") == "ios" && Application.platform != RuntimePlatform.IPhonePlayer)) {
					string _desc = Xml.Attribute (item, "desc");
					string _icon = Xml.Attribute (item, "icon");
					string _title = Xml.Attribute (item, "title");
					//string sub = Xml.Attribute (item, "help");
					string _subtitle = Xml.Attribute (item, "subtitle");
					string _scene = Xml.Attribute (item, "scene", "Scan");
					titles.Add (_title);
					subtitles.Add (_subtitle);
					descs.Add (_desc);
					icons.Add (_icon);
					scenes.Add (_scene);
				}
			}
			slider.onIndexChanged = (idx) => {
				title.text = I18n.Translate(titles [idx]);
				description.text = I18n.Translate(descs [idx]);
				subtitle.text = I18n.Translate(subtitles [idx]);
				Configuration.instant.sceneName = scenes[idx];
			};
			Configuration.instant.sceneName = scenes [0];
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

	void LayoutLoaded (string str)
	{
		layout = XDocument.Parse (str).Root;
	}
}
