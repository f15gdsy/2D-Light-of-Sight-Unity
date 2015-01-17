using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	public class LOSObstacle : MonoBehaviour {

		public bool isStatic = true;

		protected Transform _trans;
		protected Vector3 _previousPosition;
		protected Quaternion _previousRotation;


		public virtual List<Vector2> vertices {get; set;}


		protected virtual void Awake () {
			_trans = transform;
			_previousPosition = _trans.position;
		}

		protected virtual void Start () {}

		protected virtual void OnEnable () {
			LOSManager.instance.AddObstacle(this);
		}

		protected virtual void OnDisable () {
			LOSManager.instance.RemoveObstacle(this);
		}

		public void UpdatePositionAndRotation () {
			_previousPosition = _trans.position;
			_previousRotation = _trans.rotation;
		}

		public bool CheckDirty () {
			return !_trans.position.Equals(_previousPosition) || !_trans.rotation.Equals(_previousRotation);
		}
	}

}