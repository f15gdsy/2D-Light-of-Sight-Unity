using UnityEngine;
using System.Collections;

namespace LOS {

	public class LOSEdge {

		private Vector2 _start;
		private Vector2 _end;

		private float _distStartEnd;

		// Line properties
		private float _k;
		private float _c;


		public Vector2 start {
			get {return _start;}
			set {
				if (!_start.Equals(value)) {
					_start = value;
					CalculateLineProperties();
				}
			}
		}

		public Vector2 end {
			get {return _end;} 
			set {
				if (!_end.Equals(value)) {
					_end = value;
					CalculateLineProperties();
				}
			}
		}


		public LOSEdge (Vector2 start, Vector2 end) {
			_start = start;
			_end = end;

			CalculateLineProperties();
		}

		public bool CheckPointOnLineSegment (Vector2 point, float tolerance) {
			// Check point within line segment range
			if (point.x > Mathf.Max(start.x, end.x) + tolerance || point.x < Mathf.Min(start.x, end.x) - tolerance || 
			    point.y > Mathf.Max(start.y, end.y) + tolerance || point.y < Mathf.Min(start.y, end.y) - tolerance) {
				return false;
			}

			// Check if vertical
			if (start.x - end.x == 0) {
				return true;
			}

			// Check if point on line
			return point.y - _k * point.x - _c <= tolerance;
		}

		public bool CheckPointOnLine (Vector2 point, float tolerance) {
			// Check if vertical
			if (start.x - end.x == 0 && Mathf.Abs(point.x - start.x) <= tolerance) {
				return true;
			}

			float distStartPoint = (start - point).magnitude;
			float distEndPoint = (end - point).magnitude;
			_distStartEnd = (start - end).magnitude;

			float max = Mathf.Max(Mathf.Max(distStartPoint, distEndPoint), _distStartEnd);
			float min = Mathf.Min(Mathf.Min(distStartPoint, distEndPoint), _distStartEnd);
			float mid = 0;
			if (distStartPoint != max && distStartPoint != min) {
				mid = distStartPoint;
			}
			else if (distEndPoint != max && distEndPoint != min) {
				mid = distEndPoint;
			}
			else {
				mid = _distStartEnd;
			}

			if (min + mid <= max + tolerance) {
				Debug.Log("true");
				return true;
			}
			return false;
		}

		private void CalculateLineProperties () {
			// Not vertical
			if (start.x - end.x != 0) {
				_k = (start.y - end.y) / (start.x - end.x);
				_c = start.y - _k * start.x;

				_distStartEnd = (start - end).magnitude;
			}
		}

		public override string ToString () {
			return string.Format ("[LOSEdge: start={0} end={1} func: y={2}x+{3}", start, end, _k, _c);
		}
	}

}