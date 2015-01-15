using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	public class LOSObstacleLine : LOSObstacle {

		private LOSEdge _edge;

		public Vector2 start {get{return _edge.start;}}
		public Vector2 end {get{return _edge.end;}}

		public override List<Vector2> vertices {
			get {
				List<Vector2> result = new List<Vector2>();
				result.Add(_edge.start);
				result.Add(_edge.end);
				return result;
			}
			set {
				_edge.start = value[0];
				_edge.end = value[1];
			}
		}

		public override List<LOSEdge> edges {
			get {
				List<LOSEdge> result = new List<LOSEdge>();
				result.Add(_edge);
				return result;
			}
			set {
				_edge = value[0];
			}
		}

		public void SetStartEnd (Vector2 start, Vector2 end) {
			_edge = new LOSEdge(start, end);
		}

		protected override void OnEnable () {}
	}

}