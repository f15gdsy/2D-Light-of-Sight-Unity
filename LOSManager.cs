using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	public class LOSManager {

		public Vector2 viewboxSize;

		private static LOSManager _instance;

		private List<LOSObstacle> _obstacles;
		private List<LOSObstacleLine> _viewbox;


		public static LOSManager instance {
			get {
				if (_instance == null) {
					_instance = new LOSManager();
				}
				return _instance;
			}
		}

		public List<LOSObstacle> obstacles {get {return _obstacles;}}


		private LOSManager () {
			_obstacles = new List<LOSObstacle>();

			_viewbox = new List<LOSObstacleLine>();
			for (int i=0; i<4; i++) {
				GameObject lineGo = new GameObject();
				LOSObstacleLine line = lineGo.AddComponent<LOSObstacleLine>();
				_viewbox.Add(line);
			}
		}

		public void UpdateViewingBox (Vector2 center) {
			Vector2 upperRight = new Vector2(viewboxSize.x, viewboxSize.y) + center;
			Vector2 upperLeft = new Vector2(-viewboxSize.x, viewboxSize.y) + center;
			Vector2 lowerLeft = new Vector2(-viewboxSize.x, -viewboxSize.y) + center;
			Vector2 lowerRight = new Vector2(viewboxSize.x, -viewboxSize.y) + center;

			_viewbox[0].SetStartEnd(upperRight, upperLeft);		// up
			_viewbox[1].SetStartEnd(upperLeft, lowerLeft);	// left
			_viewbox[2].SetStartEnd(lowerLeft, lowerRight);	// down
			_viewbox[3].SetStartEnd(lowerRight, upperRight);		// right
		}

		public bool GetCollisionPointWithViewBox (Vector3 origin, Vector3 direction, ref Vector3 point) {
			foreach (LOSObstacleLine line in _viewbox) {
				Vector2 q = line.start;
				Vector2 s = line.end - line.start;

				Vector2 p = SMath.Vec3ToVec2(origin);
				Vector2 r = SMath.Vec3ToVec2(direction).normalized;

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
	