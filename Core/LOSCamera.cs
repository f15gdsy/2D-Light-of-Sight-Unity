using UnityEngine;
using System.Collections;

namespace LOS {

	[RequireComponent (typeof(Camera))]
	public class LOSCamera : LOSObjectBase {
		private Camera _camera;

		public Camera unityCamera {
			get {
				if (_camera == null) {
					_camera = GetComponent<Camera>();
				}
				return _camera;
			}
		}
	}

}