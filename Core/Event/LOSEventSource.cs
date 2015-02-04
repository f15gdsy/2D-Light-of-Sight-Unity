using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS.Event {

	public class LOSEventSource : MonoBehaviour {

		private Transform _trans;

		public LayerMask triggerLayers;
		public LayerMask obstacleLayers;
		public float distance;


		void Awake () {
			_trans = transform;
		}

		void OnEnable () {
			LOSEventManager.instance.AddEventSource(this);
		}
		
		void OnDisable () {
			if (LOSEventManager.TryGetInstance() != null) {
				LOSEventManager.instance.RemoveEventSource(this);
			}
		}

		public void Process (List<LOSEventTrigger> triggers) {
			RaycastHit hit;

			List<LOSEventTrigger> triggered = new List<LOSEventTrigger>();
			List<LOSEventTrigger> notTriggered = new List<LOSEventTrigger>();

			LayerMask mask = triggerLayers | obstacleLayers;

			foreach (LOSEventTrigger trigger in triggers) {
				Vector3 direction = trigger.position - _trans.position;

				if (direction.sqrMagnitude <= distance) {	// Within distance
					if (triggered.Contains(trigger)) continue;		// May be added previously

					if (Physics.Raycast(_trans.position, direction, out hit, distance, mask)) {
						GameObject hitGo = hit.collider.gameObject;

						if (hitGo == trigger.gameObject) {
							triggered.Add(trigger);
						}
						else {
							notTriggered.Add(trigger);

							if (SHelper.CheckGameObjectInLayer(hitGo, triggerLayers)) {
								LOSEventTrigger triggerToAdd = hitGo.GetComponentInChildren<LOSEventTrigger>();
								if (triggerToAdd == null) {
									triggerToAdd = hitGo.GetComponentInParent<LOSEventTrigger>();
								}
								triggered.Add(triggerToAdd);
							}
						}
					}
				}
			}

			foreach (LOSEventTrigger trigger in triggered) {
				trigger.TriggeredByLight(this);
			}
			foreach (LOSEventTrigger trigger in notTriggered) {
				trigger.NotTriggered();
			}
		}
	}

}