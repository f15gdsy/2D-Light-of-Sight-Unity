using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {

	public float distanceToPlayer;

	private Transform _playerTrans;
	private Transform _trans;

	private Vector3 _originalAngles;


	void Start () {
		GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
		_playerTrans = playerGo.transform;

		_trans = transform;

		_originalAngles = _trans.eulerAngles;
	}
	
	void Update () {

		Vector3 direction = _playerTrans.position - _trans.position;
		direction.z = 0;

		if (direction.sqrMagnitude <= distanceToPlayer * distanceToPlayer) {
			_trans.eulerAngles = new Vector3(0, 0, SMath.VectorToDegree(direction));
		}
		else {
//			_trans.eulerAngles = _originalAngles;
		}
	}
}
