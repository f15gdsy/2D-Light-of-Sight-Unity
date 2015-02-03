using UnityEngine;
using System.Collections;

public class FlipX : MonoBehaviour {

	private Transform _trans;

	// Use this for initialization
	void Start () {
		_trans = transform;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 direction = _trans.localScale;

		if (Input.GetKey(KeyCode.A)) {
			direction.x = 1;
		}
		else if (Input.GetKey(KeyCode.D)) {
			direction.x = -1;
		}

		_trans.localScale = direction;
	}
}
