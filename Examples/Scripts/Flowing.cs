using UnityEngine;
using System.Collections;

public class Flowing : MonoBehaviour {
	public float interval;
	public float force;

	private Rigidbody _rigid;
	private float _time;
	private Vector3 _direction;


	void Start () {
		_time = interval / 2;
		_rigid = GetComponent<Rigidbody>();
		_direction = new Vector3(0, 1, 0);
	}
	
	void FixedUpdate () {
		_time += Time.fixedDeltaTime;

		if (_time >= interval) {
			_time = 0;
			_direction.y *= -1;
		}

		_rigid.AddForce(_direction * force);
	}
}
