using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	/// <summary>
	/// LOSObstacles provides mechanism for the system to tell if the system is changed.
	/// </summary>
	public class LOSObstacle : LOSObjectBase {

		[Tooltip("If the obstacle's center is offscreen, how much distance from the screen edge should be enough for the system" +
			"to consider it is fully offscreen?")]
		public float offScreenDistance = 3;
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
			bool withinScreen = SHelper.CheckWithinScreen(position, LOSManager.instance.losCamera.unityCamera, offScreenDistance) || !Application.isPlaying;
			return withinScreen && (base.CheckDirty () || gameObject.layer != _previousLayerMask);
		}

		public override void UpdatePreviousInfo () {
			base.UpdatePreviousInfo ();

			_previousLayerMask = gameObject.layer;
		}

	}

}