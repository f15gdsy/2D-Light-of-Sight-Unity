using UnityEngine;
using System.Collections;

public class MoveAccordingToDirection : MonoBehaviour {

	public Vector2 offset;

	private Transform _trans;
	private Vector3 _originPosiiton;

	void Start () {
		_trans = transform;
		_originPosiiton = _trans.localPosition;
	}

	void Update () {
		Vector3 direction = Vector3.zero;
		
		if (Input.GetKey(KeyCode.W)) {
			direction.y = 1;
		}
		else if (Input.GetKey(KeyCode.S)) {
			direction.y = -1;
		}
		
		if (Input.GetKey(KeyCode.A)) {
			direction.x = -1;
		}
		else if (Input.GetKey(KeyCode.D)) {
			direction.x = -1;
		}

		direction.x *= offset.x;
		direction.y *= offset.y;
		_trans.localPosition = _originPosiiton + direction;
	}
}
