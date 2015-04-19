using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour {

	public Transform targetTrans;
	public bool freezeZ;

	private Transform _trans;


	// Use this for initialization
	void Start () {
		_trans = transform;
	}

	// Update is called once per frame
	void Update () {
		if (targetTrans == null) return ;

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
