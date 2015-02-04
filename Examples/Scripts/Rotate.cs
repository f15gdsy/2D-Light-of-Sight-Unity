using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {

	public Transform _trans;
	public float speed;


	void Start () {
		_trans = transform;
	}


	void Update () {
		Vector3 angle = _trans.eulerAngles;
		angle.z += speed * Time.deltaTime;
		_trans.eulerAngles = angle;
	}
}
