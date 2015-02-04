using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS.Event {

	public class LOSEventTrigger : MonoBehaviour {

		// Transform Info
		private Transform _trans;

		public Vector3 position {get {return _trans.position;}}


		// Events & Trigger Info
		public delegate void HandleSourceDelegate (LOSEventSource source);
		public delegate void HandleNoSourceDelegate ();
		public event HandleSourceDelegate OnTriggeredBySource;
		public event HandleSourceDelegate OnNotTriggeredBySource;
		public event HandleNoSourceDelegate OnTriggered;
		public event HandleNoSourceDelegate OnNotTriggered;
		public bool triggered {get {return _triggerSources.Count > 0;}}

		private List<LOSEventSource> _triggerSources;



		void Awake () {
			_trans = transform;
			_triggerSources = new List<LOSEventSource>();
		}

		void Start () {
			NotTriggered();
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

		public void TriggeredBySource (LOSEventSource source) {
			if (!_triggerSources.Contains(source)) {
				_triggerSources.Add(source);

				if (OnTriggeredBySource != null) {
					OnTriggeredBySource(source);
				}
				if (_triggerSources.Count == 1 && OnTriggered != null) {
					OnTriggered();
				}
			}
		}

		public void NotTriggeredBySource (LOSEventSource source) {
			if (_triggerSources.Contains(source)) {
				_triggerSources.Remove(source);

				if (OnNotTriggeredBySource != null) {
					OnNotTriggeredBySource(source);
				}
				if (_triggerSources.Count == 0 && OnNotTriggered != null) {
					OnNotTriggered();
				}
			}
		}

		public void NotTriggered () {
			_triggerSources.Clear();

			if (OnNotTriggered != null) {
				OnNotTriggered();
			}
		}
	}

}