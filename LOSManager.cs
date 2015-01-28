using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	public class LOSManager : MonoBehaviour {

		public Vector2 viewboxSize;
		public Camera lightCamera;

		public static float _tolerance = 0.1f;

		private static LOSManager _instance;
		private static bool _sceneEnd;

		private List<LOSObstacle> _obstacles;
		private List<LOSObstacleLine> _viewbox;
		private Transform _lightCameraTrans;


		public static LOSManager instance {
			get {
				if (_sceneEnd) {
					return null;
				}

				if (_instance == null && ((_instance = FindObjectOfType<LOSManager>()) == null)) {
					var go = new GameObject();
					go.name = "LOSManager";
					_instance = go.AddComponent<LOSManager>();
				}
				return _instance;
			}
		}

		public List<LOSObstacle> obstacles {get {return _obstacles;}}

		public List<Vector3> viewboxCorners {
			get {
				List<Vector3> result = new List<Vector3>();
				foreach (LOSObstacleLine line in _viewbox) {
					result.Add(line.end);
				}
				return result;
			}
		}

		public Transform lightCameraTrans {get {return _lightCameraTrans;}}


		void Awake () {
			_instance = this;

			_obstacles = new List<LOSObstacle>();
			
			_viewbox = new List<LOSObstacleLine>();
			for (int i=0; i<4; i++) {
				GameObject lineGo = new GameObject();
				LOSObstacleLine line = lineGo.AddComponent<LOSObstacleLine>();
				_viewbox.Add(line);
			}

			if (lightCamera == null) {
				lightCamera = Camera.main;
			}
			_lightCameraTrans = lightCamera.transform;
			
			Vector2 screenSize = SHelper.GetScreenSizeInWorld();
			LOSManager.instance.viewboxSize = screenSize;
			UpdateViewingBox();
		}

		void Start () {

		}

		void LateUpdate () {
			UpdateObstaclesTransformData();
			UpdateViewingBox();
		}

		void OnLevelWasLoaded (int level) {
			_sceneEnd = false;
		}

		void OnDestroy () {
			_sceneEnd = true;
		}

		public void UpdateViewingBox () {
			Vector2 upperRight = new Vector2(viewboxSize.x, viewboxSize.y) + SMath.Vec3ToVec2(_lightCameraTrans.position);
			Vector2 upperLeft = new Vector2(-viewboxSize.x, viewboxSize.y) + SMath.Vec3ToVec2(_lightCameraTrans.position);
			Vector2 lowerLeft = new Vector2(-viewboxSize.x, -viewboxSize.y) + SMath.Vec3ToVec2(_lightCameraTrans.position);
			Vector2 lowerRight = new Vector2(viewboxSize.x, -viewboxSize.y) + SMath.Vec3ToVec2(_lightCameraTrans.position);

			_viewbox[0].SetStartEnd(lowerRight, upperRight);		// right
			_viewbox[1].SetStartEnd(upperRight, upperLeft);		// up
			_viewbox[2].SetStartEnd(upperLeft, lowerLeft);	// left
			_viewbox[3].SetStartEnd(lowerLeft, lowerRight);	// down
		}

		public void UpdateObstaclesTransformData () {
			foreach (LOSObstacle obstacle in _obstacles) {
				obstacle.UpdatePreviousInfo();
			}
		}

		public bool GetCollisionPointWithViewBox (Vector3 origin, Vector3 direction, ref Vector3 point) {
			foreach (LOSObstacleLine line in _viewbox) {
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
						point = SMath.Vec2ToVec3(q + u * s);
						return true;
					}
				}
			}
			return false;
		}

		// Works in counter-clock wise, pointA is the one with smaller angle against vector (1, 0)
		public List<Vector3> GetViewboxCornersBetweenPoints (Vector3 pointA, Vector3 pointB, Vector3 origin) {
			pointA.z = 0;
			pointB.z = 0;
			origin.z = 0;

			float degreeA = SMath.VectorToDegree(pointA - origin);
			float degreeB = SMath.VectorToDegree(pointB - origin);

			if (degreeA == 360) {
				degreeA = 0;
			}
			if (degreeA > degreeB) {
				degreeA -= 360;
			}
		
			Dictionary<float, Vector3> tempResults = new Dictionary<float, Vector3>();

			foreach (LOSObstacleLine line in _viewbox) {
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
			if (!_obstacles.Contains(obstacle)) {
				_obstacles.Add(obstacle);
			}
		}

		public void RemoveObstacle (LOSObstacle obstacle) {
			_obstacles.Remove(obstacle);
		}

		public bool CheckDirty () {
			foreach (LOSObstacle obstacle in _obstacles) {
				if (!obstacle.isStatic && obstacle.CheckDirty()) {
					return true;
				}
			}
			return false;
		}

		public bool CheckPointWithinViewingBox (Vector2 point, Vector2 center) {
			return !(point.x <= -viewboxSize.x + center.x || point.x >= viewboxSize.x + center.x ||
			        point.y <= -viewboxSize.y + center.y || point.y >= viewboxSize.y + center.y);
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
	}
}
	