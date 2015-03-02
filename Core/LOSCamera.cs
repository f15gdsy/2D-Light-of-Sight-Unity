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
			Vector2 screenSize = SHelper.GetScreenSizeInWorld(unityCamera);
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


		public Vector3 GetCollisionPointWithViewBox (Vector3 origin, Vector3 direction) {
			Vector3 point = Vector3.zero;
			foreach (var line in viewbox) {
				Vector2 q = line.start;
				Vector2 s = line.end - line.start;
				
				Vector2 p = SMath.Vec3ToVec2(origin);
				Vector2 r = SMath.Vec3ToVec2(direction);
				
				// The intersection is where q + u*s == p + t*r, and 0 <= u <= 1 && 0 <= t
				// t = (q − p) × s / (r × s)
				// u = (q − p) × r / (r × s)
				
				float crossRS = SMath.CrossProduct2D(r, s);
				float crossQP_S = SMath.CrossProduct2D(q - p, s);
				float crossQP_R = SMath.CrossProduct2D(q - p, r);
				
				if (crossRS == 0) {
					// TODO: other situations
				}
				else {
					float t = crossQP_S / crossRS;
					float u = crossQP_R / crossRS;
					
					if (0 <= u && u <= 1 && 0 <= t) {
						point = q + u * s;
						break;
					}
				}
			}
			return point;
		}
		
		// Works in counter-clock wise, pointA is the one with smaller angle against vector (1, 0)
		public List<Vector3> GetViewboxCornersBetweenPoints (Vector3 pointA, Vector3 pointB, Vector3 origin, bool give4CornersWhenAEqualB) {
			pointA.z = 0;
			pointB.z = 0;
			origin.z = 0;
			
			float degreeA = SMath.VectorToDegree(pointA - origin);
			float degreeB = SMath.VectorToDegree(pointB - origin);
			
			if (degreeA == 360) {
				degreeA = 0;
			}
			if (degreeA > degreeB + 0.0005f || (degreeA >= degreeB && degreeA <= degreeB + 0.0005f && give4CornersWhenAEqualB)) {	// 0.0005f is the tolerance
				degreeA -= 360;
			}
			
			Dictionary<float, Vector3> tempResults = new Dictionary<float, Vector3>();
			
			foreach (var line in viewbox) {
				Vector3 corner = line.end;
				
				float degreeToA = 0;
				float degreeCorner = SMath.VectorToDegree(corner - origin);
				if (((degreeToA = (degreeCorner - degreeA)) > 0 && degreeCorner < degreeB) ||
				    ((degreeToA = (degreeCorner - 360 - degreeA)) > 0 && degreeCorner - 360 < degreeB) ||
				    ((degreeToA = (degreeCorner + 360 - degreeA)) > 0 && degreeCorner + 360 < degreeB)) {
					tempResults.Add(degreeToA, corner);
				}
			}
			
			List<float> degreesToA = new List<float>();
			
			foreach (float degreeToA in tempResults.Keys) {
				degreesToA.Add(degreeToA);
			}
			degreesToA.Sort();
			
			List<Vector3> results = new List<Vector3>();
			foreach (float degreeToA in degreesToA) {
				results.Add(tempResults[degreeToA]);
			}
			
			return results;
		}
		
		public bool CheckPointWithinViewingBox (Vector2 point, float distance) {
			return !(point.x <= -halfViewboxSize.x + _trans.position.x - distance || point.x >= halfViewboxSize.x + _trans.position.x + distance||
			         point.y <= -halfViewboxSize.y + _trans.position.y - distance|| point.y >= halfViewboxSize.y + _trans.position.y + distance);
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