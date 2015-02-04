using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour {

	public Transform targetTrans;
	public bool freezeZ;

	private Transform _trans;
	private bool _firstTime;

	// Use this for initialization
	void Start () {
		_trans = transform;

		_firstTime = true;
	}

	// Update is called once per frame
	void LateUpdate () {
		if (freezeZ) {
			Vector3 pos = targetTrans.position;
			pos.z = _trans.position.z;
			_trans.position = pos;
		}
		else {
			_trans.position = targetTrans.position;
		}
	}
}
