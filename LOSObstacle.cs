using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	public class LOSObstacle : MonoBehaviour {

		public bool isStatic = true;

		protected Transform _trans;
		protected Vector3 _previousPosition;
		// TODO: Rotation



		public virtual List<Vector2> vertices {get; set;}

		public virtual List<LOSEdge> edges {get; set;}


		public LOSEdge GetCollideEdge (Vector2 point, float tolerance) {
			foreach (LOSEdge edge in edges) {
				if (edge.CheckPointOnLineSegment(point, tolerance)) {
					return edge;
				}
			}
			return null;
		}

		public LOSEdge GetEdgeEndAtVertex (Vector2 vertex, float tolerance) {
			foreach (LOSEdge edge in edges) {
				if (SMath.CheckSamePoint(edge.end, vertex, tolerance)) {
					return edge;
				}
			}
			return null;
		}

		public LOSEdge GetEdgeStartAtVertex (Vector2 vertex, float tolerance) {
			foreach (LOSEdge edge in edges) {
				if (SMath.CheckSamePoint(edge.start, vertex, tolerance)) {
					return edge;
				}
			}
			return null;
		}

		public bool CheckDirty () {
			return !_trans.position.Equals(_previousPosition);
		}


		protected virtual void Awake () {
			_trans = transform;
			_previousPosition = _trans.position;
		}

		protected virtual void Start () {}

		protected virtual void OnEnable () {
			LOSManager.instance.AddObstacle(this);
		}

		protected virtual void OnDisable () {
			LOSManager.instance.RemoveObstacle(this);
		}

		protected virtual void LateUpdate () {
			_previousPosition = _trans.position;
		}
	}

}