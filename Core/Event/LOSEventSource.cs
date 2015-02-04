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

			List<LOSEventTrigger> triggeredTriggers = new List<LOSEventTrigger>();
			List<LOSEventTrigger> notTriggeredTriggers = new List<LOSEventTrigger>();

			foreach (LOSEventTrigger trigger in triggers) {

				if (!SHelper.CheckGameObjectInLayer(trigger.gameObject, triggerLayers)) continue;

				bool triggered = false;
				Vector3 direction = trigger.position - _trans.position;

				if (direction.sqrMagnitude <= distance * distance) {	// Within distance
					if (triggeredTriggers.Contains(trigger)) continue;		// May be added previously

					LayerMask mask = 1 << trigger.gameObject.layer | obstacleLayers;

					if (Physics.Raycast(_trans.position, direction, out hit, distance, mask)) {
						GameObject hitGo = hit.collider.gameObject;

						if (hitGo == trigger.gameObject) {
							triggered = true;
						}
						else if (hitGo.layer == trigger.gameObject.layer) {
							LOSEventTrigger triggerToAdd = hitGo.GetComponentInChildren<LOSEventTrigger>();
							if (triggerToAdd == null) {
								triggerToAdd = hitGo.GetComponentInParent<LOSEventTrigger>();
							}
							triggeredTriggers.Add(triggerToAdd);
						}
					}
				}

				if (triggered) {
					triggeredTriggers.Add(trigger);
				}
				else {
					notTriggeredTriggers.Add(trigger);
				}

			}

			foreach (LOSEventTrigger trigger in triggeredTriggers) {
				trigger.TriggeredBySource(this);
			}

			foreach (LOSEventTrigger trigger in notTriggeredTriggers) {
				trigger.NotTriggeredBySource(this);
			}
		}
	}

}