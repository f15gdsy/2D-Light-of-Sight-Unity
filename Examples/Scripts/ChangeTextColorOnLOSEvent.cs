using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LOS.Event;

public class ChangeTextColorOnLOSEvent : MonoBehaviour {
	
	public Color litColor = new Color(1, 1, 1, 1);
	public Color notLitColor = new Color(1, 1, 1, 1);
	
	
	private Text _text;
	
	void Start () {
		_text = GetComponent<Text>();
		
		LOSEventTrigger trigger = GetComponent<LOSEventTrigger>();
		trigger.OnNotTriggered += OnNotLit;
		trigger.OnTriggered += OnLit;

		OnNotLit();
	}
	
	private void OnNotLit () {
		_text.color = notLitColor;
	}
	
	private void OnLit () {
		_text.color = litColor;
	}
}
