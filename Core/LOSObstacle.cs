using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	public class LOSObstacle : LOSObjectBase {

		public virtual List<Vector2> vertices {get; set;}
		private LayerMask _previousLayerMask;


		protected override void Awake () {
			base.Awake ();

			_previousLayerMask = gameObject.layer;
		}

		protected virtual void OnEnable () {
			LOSManager.instance.AddObstacle(this);
		}

		protected virtual void OnDisable () {
			if (LOSManager.TryGetInstance() != null) {		// Have to check in case the manager is destroyed when scene end.
				LOSManager.instance.RemoveObstacle(this);
			}
		}

		public override bool CheckDirty () {
			return base.CheckDirty () || gameObject.layer != _previousLayerMask;
		}

		public override void UpdatePreviousInfo () {
			base.UpdatePreviousInfo ();

			_previousLayerMask = gameObject.layer;
		}

	}

}