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

		protected override void OnEnable () {}
	}

}