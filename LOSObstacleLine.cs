using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	public class LOSObstacleLine : LOSObstacle {

		public Vector2 start {get; set;}
		public Vector2 end {get; set;}

		public override List<Vector2> vertices {
			get {
				List<Vector2> result = new List<Vector2>();
				result.Add(start);
				result.Add(end);
				return result;
			}
			set {
				start = value[0];
				end = value[1];
			}
		}


		public void SetStartEnd (Vector2 start, Vector2 end) {
			this.start = start;
			this.end = end;
		}

//		public bool CheckPointOnLine (Vector2 point) {
//			SMath.
//		}

		protected override void OnEnable () {}

		public bool CheckPointOnLine (Vector2 point, float tolerance) {
			// Check if vertical
			if (start.x - end.x == 0 && Mathf.Abs(point.x - start.x) <= tolerance) {
				return true;
			}
			
			float distStartPoint = (start - point).magnitude;
			float distEndPoint = (end - point).magnitude;
			float distStartEnd = (start - end).magnitude;
			
			float max = Mathf.Max(Mathf.Max(distStartPoint, distEndPoint), distStartEnd);
			float min = Mathf.Min(Mathf.Min(distStartPoint, distEndPoint), distStartEnd);
			float mid = 0;
			if (distStartPoint != max && distStartPoint != min) {
				mid = distStartPoint;
			}
			else if (distEndPoint != max && distEndPoint != min) {
				mid = distEndPoint;
			}
			else {
				mid = distStartEnd;
			}
			
			if (min + mid <= max + tolerance) {
				return true;
			}
			return false;
		}
	}

}