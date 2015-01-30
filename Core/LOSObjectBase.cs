using UnityEngine;
using System.Collections;

public class LOSObjectBase : MonoBehaviour {

	[Tooltip("Will not be considered in the LOS system.")]
	public bool isStatic;

	protected Transform _trans;
	protected Vector3 _previousPosition;
	protected Quaternion _previousRotation;


	public Vector3 position {get {return _trans.position;}}
	public Quaternion rotation {get {return _trans.rotation;}}


	protected virtual void Awake () {
		_trans = transform;
	}

	public virtual bool CheckDirty () {
		return _previousPosition != _trans.position || _previousRotation != _trans.rotation;
	}

	public virtual void UpdatePreviousInfo () {
		_previousPosition = position;
		_previousRotation = rotation;
	}
}
