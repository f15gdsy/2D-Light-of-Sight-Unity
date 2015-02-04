using UnityEngine;
using System.Collections;
using LOS.Event;

public class DisappearWhenNotLit : MonoBehaviour {

	private SpriteRenderer _renderer;

	void Start () {
		_renderer = (SpriteRenderer) renderer;

		LOSEventTrigger trigger = GetComponent<LOSEventTrigger>();
		trigger.OnNotTriggered += Disappear;
		trigger.OnTriggered += Appear;
	}

	private void Disappear () {
		_renderer.color = Color.blue;
	}

	private void Appear (LOSEventSource source) {
		_renderer.color = Color.red;
	}
}
