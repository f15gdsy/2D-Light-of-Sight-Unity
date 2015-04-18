using UnityEngine;
using System.Collections;

public class MovePhysics : MonoBehaviour {
	
	public float force;
	
	private Vector3 _direction;
	private Rigidbody _rigid;
	
	// Use this for initialization
	void Start () {
		_rigid = GetComponent<Rigidbody>();
	}
	
	void FixedUpdate () {
		_rigid.AddForce(_direction * force);
	}
	
	// Update is called once per frame
	void Update () {
		_direction = Vector3.zero;
		
		if (Input.GetKey(KeyCode.W)) {
			_direction.y = 1;
		}
		else if (Input.GetKey(KeyCode.S)) {
			_direction.y = -1;
		}
		
		if (Input.GetKey(KeyCode.A)) {
			_direction.x = -1;
		}
		else if (Input.GetKey(KeyCode.D)) {
			_direction.x = 1;
		}
		//		_trans.position = _trans.position + direction;
	}
}
