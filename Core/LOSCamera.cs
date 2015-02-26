using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	/// <summary>
	/// LOS camera. 
	/// Needs to be placed in a camera for the system to work.
	/// </summary>
	[RequireComponent (typeof(Camera))]
	public class LOSCamera : LOSObjectBase {

		public bool mainLOSCamera;

		[HideInInspector]
		public Vector2 halfViewboxSize;

		private List<ViewBoxLine> _viewbox;
		private Camera _camera;


		public List<ViewBoxLine> viewbox {
			get {
				if (_viewbox == null) {
					_viewbox = new List<ViewBoxLine>();
					for (int i=0; i<4; i++) {
						_viewbox.Add(new ViewBoxLine());
					}
					UpdateViewingBox();
				}
				return _viewbox;
			}
		}

		public List<Vector3> viewboxCorners {
			get {
				List<Vector3> result = new List<Vector3>();
				foreach (var line in viewbox) {
					result.Add(line.end);
				}
				return result;
			}
		}

		public void UpdateViewingBox () {
			Vector2 screenSize = SHelper.GetScreenSizeInWorld();
			halfViewboxSize = screenSize / 2 * LOSManager.instance.viewboxExtension;
			
			Vector2 upperRight = new Vector2(halfViewboxSize.x, halfViewboxSize.y) + SMath.Vec3ToVec2(_trans.position);
			Vector2 upperLeft = new Vector2(-halfViewboxSize.x, halfViewboxSize.y) + SMath.Vec3ToVec2(_trans.position);
			Vector2 lowerLeft = new Vector2(-halfViewboxSize.x, -halfViewboxSize.y) + SMath.Vec3ToVec2(_trans.position);
			Vector2 lowerRight = new Vector2(halfViewboxSize.x, -halfViewboxSize.y) + SMath.Vec3ToVec2(_trans.position);
			
			viewbox[0].SetStartEnd(lowerRight, upperRight);		// right
			viewbox[1].SetStartEnd(upperRight, upperLeft);		// up
			viewbox[2].SetStartEnd(upperLeft, lowerLeft);	// left
			viewbox[3].SetStartEnd(lowerLeft, lowerRight);	// down
		}

		public Camera unityCamera {
			get {
				if (_camera == null) {
					_camera = GetComponent<Camera>();
				}
				return _camera;
			}
		}


		public class ViewBoxLine {
			public Vector2 start {get; set;}
			public Vector2 end {get; set;}
			
			public void SetStartEnd (Vector2 start, Vector2 end) {
				this.start = start;
				this.end = end;
			}
		}
	}

}