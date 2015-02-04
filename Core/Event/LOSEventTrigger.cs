using UnityEngine;
using System.Collections;

namespace LOS.Event {

	public class LOSEventTrigger : MonoBehaviour {

		// Transform Info
		private Transform _trans;

		public Vector3 position {get {return _trans.position;}}


		// Events & Trigger Info
		public delegate void HandleLightDelegate (LOSEventSource source);
		public delegate void HandleNoLightDelegate ();
		public event HandleLightDelegate OnTriggered;
		public event HandleNoLightDelegate OnNotTriggered;
		private bool _triggered;
		public bool triggered {get {return _triggered;}}


		void Awake () {
			_trans = transform;
		}

		void OnEnable () {
			LOSEventManager.instance.AddEventTrigger(this);
		}

		void OnDisable () {
			if (LOSEventManager.TryGetInstance() != null) {
				LOSEventManager.instance.RemoveEventTrigger(this);
			}
		}

		public bool CheckWithinScreen () {
			return SHelper.CheckWithinScreen(_trans.position, LOSManager.instance.losCamera.unityCamera, 5);
		}

		public void TriggeredByLight (LOSEventSource source) {
			if (!_triggered) {
				_triggered = true;

				if (OnTriggered != null) {
					OnTriggered(source);
				}
			}
		}

		public void NotTriggered () {
			if (_triggered) {
				_triggered = false;

				if (OnNotTriggered != null) {
					OnNotTriggered();
				}
			}
		}
	}

}