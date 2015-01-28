using UnityEngine;
using System.Collections;

namespace LOS {

	public class LOSEventTrigger : MonoBehaviour {

		public float interval;

		private float _time;

		public delegate void LOSEventTriggerDelegate (GameObject lightGo);
		public event LOSEventTriggerDelegate LOSLightHitEnter;
		public event LOSEventTriggerDelegate LOSLightHitStay;
		public event LOSEventTriggerDelegate LOSLightHitExit;


		void Update () {
			_time += Time.deltaTime;
		}


	}

}