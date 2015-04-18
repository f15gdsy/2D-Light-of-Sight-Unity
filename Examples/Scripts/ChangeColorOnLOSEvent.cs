using UnityEngine;
using System.Collections;
using LOS.Event;

public class ChangeColorOnLOSEvent : MonoBehaviour {

	public Color litColor;
	public Color notLitColor;


	private SpriteRenderer _renderer;

	void Start () {
		_renderer = (SpriteRenderer) GetComponent<Renderer>();

		LOSEventTrigger trigger = GetComponent<LOSEventTrigger>();
		trigger.OnNotTriggered += OnNotLit;
		trigger.OnTriggered += OnLit;

		OnNotLit();
	}

	private void OnNotLit () {
		_renderer.color = notLitColor;
	}

	private void OnLit () {
		_renderer.color = litColor;
	}
}
