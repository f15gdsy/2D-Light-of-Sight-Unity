using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	[ExecuteInEditMode]
	public class LOSManager : MonoBehaviour {
//		public LOSCamera losCamera;
		public float viewboxExtension = 1.01f;
		public float collidersExtension = 1.001f;

		[HideInInspector]
		public Vector2 halfViewboxSize;

		public static float _tolerance = 0.1f;

		private static LOSManager _instance;

		private List<LOSObstacle> _obstacles;
		private List<LOSLightBase> _lights;
		private List<ViewBoxLine> _viewbox;
		private Transform _losCameraTrans;
		private LOSCamera _losCamera;
		private bool _isDirty;


		public static LOSManager instance {
			get {
				if (_instance == null) {
					_instance = FindObjectOfType<LOSManager>();

					if (_instance == null) {
						var go = new GameObject();
						go.name = "LOSManager";
						_instance = go.AddComponent<LOSManager>();
						DontDestroyOnLoad(go);

						_instance.Init();
					}
				}
				return _instance;
			}
		}

		public List<LOSObstacle> obstacles {
			get {
				if (_obstacles == null) {
					_obstacles = new List<LOSObstacle>();
				}
				return _obstacles;
			}
		}

		private List<LOSLightBase> lights {
			get {
				if (_lights == null) {
					_lights = new List<LOSLightBase>();
				}
				return _lights;
			}
		}

		private List<ViewBoxLine> viewbox {
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

		public LOSCamera losCamera {
			get {
				if (_losCamera == null) {
					var losCameras = FindObjectsOfType<LOSCamera>();
					if (losCameras.Length == 0) {
						Debug.LogError("No LOSCamera found! Please add the LOSCamera component to a camera.");
					}
					else if (losCameras.Length > 1) {
						Debug.LogError("More than 1 LOSCamera found!");
					}
					else {
						_losCamera = losCameras[0];
					}
				}
				return _losCamera;
			}
		}
		public Transform losCameraTrans {
			get {
				if (_losCameraTrans == null) {
					_losCameraTrans = losCamera.transform;
				}
				return _losCameraTrans;
			}
		}


		void Start () {
			Init();
		}


		void LateUpdate () {
			UpdateLights();
		}

		public void UpdateLights () {
			foreach (var light in lights) {
				light.TryDraw();
			}
			
			UpdatePreviousInfo();
		}

		public static LOSManager TryGetInstance () {
			return _instance;
		}

		private void Init () {
			_instance = this;

			UpdateViewingBox();
		}

		public void UpdateViewingBox () {
			Vector2 screenSize = SHelper.GetScreenSizeInWorld();
			halfViewboxSize = screenSize / 2 * viewboxExtension;

			Vector2 upperRight = new Vector2(halfViewboxSize.x, halfViewboxSize.y) + SMath.Vec3ToVec2(losCameraTrans.position);
			Vector2 upperLeft = new Vector2(-halfViewboxSize.x, halfViewboxSize.y) + SMath.Vec3ToVec2(losCameraTrans.position);
			Vector2 lowerLeft = new Vector2(-halfViewboxSize.x, -halfViewboxSize.y) + SMath.Vec3ToVec2(losCameraTrans.position);
			Vector2 lowerRight = new Vector2(halfViewboxSize.x, -halfViewboxSize.y) + SMath.Vec3ToVec2(losCameraTrans.position);

			viewbox[0].SetStartEnd(lowerRight, upperRight);		// right
			viewbox[1].SetStartEnd(upperRight, upperLeft);		// up
			viewbox[2].SetStartEnd(upperLeft, lowerLeft);	// left
			viewbox[3].SetStartEnd(lowerLeft, lowerRight);	// down
		}

		public void UpdatePreviousInfo () {
			_isDirty = false;

			losCamera.UpdatePreviousInfo();

			foreach (LOSObstacle obstacle in obstacles) {
				obstacle.UpdatePreviousInfo();
			}

			UpdateViewingBox();
		}

		public Vector3 GetPointForRadius (Vector3 origin, Vector3 direction, float radius) {
			float c = direction.magnitude;

			float x = radius * direction.x / c + origin.x;
			float y = radius * direction.y / c + origin.y;
			return new Vector3(x, y, 0);
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
			if (degreeA > degreeB + 0.0005f || (degreeA > degreeB && degreeA < degreeB + 0.0005f && give4CornersWhenAEqualB)) {	// 0.001f is the tolerance
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
		
		public void AddObstacle (LOSObstacle obstacle) {
			if (!obstacles.Contains(obstacle)) {
				_isDirty = true;
				obstacles.Add(obstacle);
			}
		}

		public void RemoveObstacle (LOSObstacle obstacle) {
			obstacles.Remove(obstacle);
			_isDirty = true;
		}

		public void AddLight (LOSLightBase light) {
			if (!lights.Contains(light)) {
				lights.Add(light);
			}
		}

		public void RemoveLight (LOSLightBase light) {
			lights.Remove(light);
		}

		public bool CheckDirty () {
			if (_isDirty) return true;

			bool result = false;
			foreach (LOSObstacle obstacle in obstacles) {
				if (!obstacle.isStatic && obstacle.CheckDirty()) {
					result = true;
				}
			}

			if (!Application.isPlaying) {
				UpdatePreviousInfo();
			}

			return result;
		}

		public bool CheckPointWithinViewingBox (Vector2 point) {
			return !(point.x <= -halfViewboxSize.x + losCameraTrans.position.x || point.x >= halfViewboxSize.x + losCameraTrans.position.x ||
			         point.y <= -halfViewboxSize.y + losCameraTrans.position.y || point.y >= halfViewboxSize.y + losCameraTrans.position.y);
		}

		private float ClampDegree (float start, float degree) {
			while (degree < start) {
				degree += 360;
			}
			while (degree > start + 360) {
				degree -= 360;
			}
			return degree;
		}


		private class ViewBoxLine {
			public Vector2 start {get; set;}
			public Vector2 end {get; set;}

			public void SetStartEnd (Vector2 start, Vector2 end) {
				this.start = start;
				this.end = end;
			}
		}
	}
}
	