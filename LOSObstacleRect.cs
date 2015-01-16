using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	public class LOSObstacleRect : LOSObstacle {

		private BoxCollider _collider;

		private List<Vector2> _vertices;

		
		public override List<Vector2> vertices {
			get {
				if (!isStatic && CheckDirty()) {
					FillVertices();
				}
				return _vertices;
			}
			set {
				Debug.LogError("Invalid Operation");
			}
		}


		protected override void Awake () {
			base.Awake ();

			Collider myCollider = collider;
			if (myCollider == null || !myCollider.GetType().Equals(typeof(BoxCollider))) {
				Debug.LogError("LOS.LOSObstacleRect: BoxCollider not found");
			}
			_collider = (BoxCollider) myCollider;

			FillVertices();
		}

		protected void FillVertices () {
			_vertices = new List<Vector2>();

			Vector2 center = new Vector2(_collider.center.x, _collider.center.y);
			Vector2 extents = new Vector2(_collider.bounds.extents.x / _trans.localScale.x, _collider.bounds.extents.y / _trans.localScale.y);

			Vector2 p0 = new Vector2(center.x-extents.x, center.y+extents.y);	// left up
			Vector2 p1 = new Vector2(center.x-extents.x, center.y-extents.y);	// left down
			Vector2 p2 = new Vector2(center.x+extents.x, center.y-extents.y);	// right down
			Vector2 p3 = new Vector2(center.x+extents.x, center.y+extents.y);	// right up
		
			p0 = _trans.TransformPoint(p0);
			p1 = _trans.TransformPoint(p1);
			p2 = _trans.TransformPoint(p2);
			p3 = _trans.TransformPoint(p3);

			_vertices.Add(p0);
			_vertices.Add(p1);
			_vertices.Add(p2);
			_vertices.Add(p3);
		}
	}

}