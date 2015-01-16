using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	public class LOSLight : MonoBehaviour {

		public Material defaultMaterial;
		public float degreeStepRough = 1;
		public float degreeStepDetail = 0.1f;
		public bool invert = true;
		public LayerMask obstacleLayer;

		private Transform _trans;
		private MeshFilter _meshFilter;
		private Vector2 _previousPosition;

		public Vector2 position2 {get {return new Vector2(_trans.position.x, _trans.position.y);}}


		void Awake () {
			_trans = transform;

			Vector2 screenSize = SHelper.GetScreenSizeInWorld();
			LOSManager.instance.viewboxSize = screenSize;
			LOSManager.instance.UpdateViewingBox(Camera.main.transform.position);
		}

		void Start () {
			_meshFilter = GetComponent<MeshFilter>();
			if (_meshFilter == null) {
				_meshFilter = gameObject.AddComponent<MeshFilter>();
			}

			if (renderer == null) {
				gameObject.AddComponent<MeshRenderer>();
				renderer.material = defaultMaterial;
			}

			DoDraw();
		}
		
		void LateUpdate () {
			if (SHelper.CheckWithinScreen(_trans.position) && (LOSManager.instance.CheckDirty() || !_previousPosition.Equals(position2))) {
				_previousPosition = position2;

				DoDraw();
			}
		}

		private void DoDraw () {
			if (invert) {
				DrawInvert();
			}
			else {
				Draw();
			}
		}

		private void Draw () {
			List<Vector3> meshVertices = new List<Vector3>();
			List<int> triangles = new List<int>();

			bool raycastHitPreviously = false;

			float distance = GetMinRaycastDistance();
			RaycastHit hit;

			int previousVectexIndex = 0;
			Vector3 hitPoint = Vector3.zero;

			meshVertices.Add(Vector3.zero);
		
			Vector2 viewboxSize = LOSManager.instance.viewboxSize;
			Vector3 upperRight = new Vector3(viewboxSize.x, viewboxSize.y) + Camera.main.transform.position;
			Vector3 upperLeft = new Vector3(-viewboxSize.x, viewboxSize.y) + Camera.main.transform.position;
			Vector3 lowerLeft = new Vector3(-viewboxSize.x, -viewboxSize.y) + Camera.main.transform.position;
			Vector3 lowerRight = new Vector3(viewboxSize.x, -viewboxSize.y) + Camera.main.transform.position;

			upperRight.z = 0;
			upperLeft.z = 0;
			lowerLeft.z = 0;
			lowerRight.z = 0;

			float degreeUpperRight =  SMath.ClampDegree0To360(SMath.VectorToDegree(upperRight - _trans.position));
			float degreeUpperLeft = SMath.ClampDegree0To360(SMath.VectorToDegree(upperLeft - _trans.position));
			float degreeLowerLeft = SMath.ClampDegree0To360(SMath.VectorToDegree(lowerLeft - _trans.position));
			float degreeLowerRight = SMath.ClampDegree0To360(SMath.VectorToDegree(lowerRight - _trans.position));

			float degreeStep = degreeStepRough;

			for (float degree=0; degree<360; degree+=degreeStep) {
				Vector3 direction;

				if (degree < degreeUpperRight && degree+degreeStepRough > degreeUpperRight) {
					direction = upperRight - _trans.position;
				}
				else if (degree < degreeUpperLeft && degree+degreeStepRough > degreeUpperLeft) {
					direction = upperLeft - _trans.position;
				}
				else if (degree < degreeLowerLeft && degree+degreeStepRough > degreeLowerLeft) {
					direction = lowerLeft - _trans.position;
				}
				else if (degree < degreeLowerRight && degree+degreeStepRough > degreeLowerRight) {
					direction = lowerRight - _trans.position;
				}
				else {
					direction = SMath.DegreeToUnitVector(degree);
				}

				if (Physics.Raycast(_trans.position, direction, out hit, distance, obstacleLayer)) {
					hitPoint = hit.point;

					if (!raycastHitPreviously) {

					}
				}
				else {
					LOSManager.instance.GetCollisionPointWithViewBox(_trans.position, direction, ref hitPoint);

					if (raycastHitPreviously) {

					}
				}

				meshVertices.Add(hitPoint - _trans.position);
				int currentVertexIndex = meshVertices.Count - 1;

				if (meshVertices.Count >= 3) {
					AddNewTriangle(ref triangles, 0, currentVertexIndex, previousVectexIndex);
				}
				previousVectexIndex = currentVertexIndex;
			}

			AddNewTriangle(ref triangles, 0, 1, previousVectexIndex);
	
			Mesh mesh = new Mesh();

			mesh.vertices = meshVertices.ToArray();
			mesh.triangles = triangles.ToArray();

			_meshFilter.mesh = mesh;
		}

		private void DrawInvert () {
			List<Vector3> invertMeshVertices = new List<Vector3>();
			List<int> invertTriangles = new List<int>();
			
			bool raycastHitAtDegree0 = false;
			Collider previousHitCollider = null;
			Collider colliderAtDegree0 = null;
			RaycastHit hit;
			int vertexIndex = 0;
			int farVertexIndex = 0;

			for (float degree=0; degree<360; degree+=degreeStepRough) {
				Vector3 direction;
				
				direction = SMath.DegreeToUnitVector(degree);
				
				if (Physics.Raycast(_trans.position, direction, out hit, 8, obstacleLayer)) {		// Hit a collider.
					Vector3 hitPoint = hit.point;
					Collider hitCollider = hit.collider;

					if (degree == 0) {
						raycastHitAtDegree0 = true;
						colliderAtDegree0 = hitCollider;
					}

					if (null == previousHitCollider) {
						Vector3 farPoint = Vector3.zero;;
						LOSManager.instance.GetCollisionPointWithViewBox(_trans.position, direction, ref farPoint);
						invertMeshVertices.Add(farPoint - _trans.position);
						invertMeshVertices.Add(hitPoint - _trans.position);
						
						farVertexIndex = vertexIndex;
					}
					else if (hitCollider != previousHitCollider) {
						Vector3 farPoint = Vector3.zero;;
						LOSManager.instance.GetCollisionPointWithViewBox(_trans.position, direction, ref farPoint);
						invertMeshVertices.Add(farPoint - _trans.position);
						invertMeshVertices.Add(hitPoint - _trans.position);

						AddNewTriangle(ref invertTriangles, vertexIndex+1, vertexIndex+2, farVertexIndex);

						int previousFarVertexIndex = farVertexIndex;
						farVertexIndex = vertexIndex + 2;

						// TODO: Add viewbox corner

						vertexIndex += 2;
						AddNewTriangle(ref invertTriangles, vertexIndex-1, vertexIndex+1, farVertexIndex);

					}
					else {
						invertMeshVertices.Add(hitPoint - _trans.position);
						
						vertexIndex++;
						AddNewTriangle(ref invertTriangles, vertexIndex, vertexIndex+1, farVertexIndex);
					}

					previousHitCollider = hitCollider;
				}
				else {
					if (previousHitCollider != null) {
						if (vertexIndex > 0) {
							Vector3 farPoint = Vector3.zero;
							LOSManager.instance.GetCollisionPointWithViewBox(_trans.position, direction, ref farPoint);
							invertMeshVertices.Add(farPoint - _trans.position);

							AddNewTriangle(ref invertTriangles, vertexIndex+1, vertexIndex+2, farVertexIndex);
							vertexIndex += 3;
						}
						else {
							invertMeshVertices.Clear();
							raycastHitAtDegree0 = false;
							colliderAtDegree0 = null;
						}
					}

					previousHitCollider = null;
				}
			}

			if (null != previousHitCollider) {
				if (raycastHitAtDegree0) {

					if (colliderAtDegree0 == previousHitCollider) {
						AddNewTriangle(ref invertTriangles, vertexIndex+1, 1, farVertexIndex);
						AddNewTriangle(ref invertTriangles, 0, farVertexIndex, 1);
					}
					else {
						AddNewTriangle(ref invertTriangles, vertexIndex+1, 1, 0);
						AddNewTriangle(ref invertTriangles, vertexIndex+1, 0, farVertexIndex);
					}
				}
				else {
					Vector3 farPoint = Vector3.zero;
					LOSManager.instance.GetCollisionPointWithViewBox(_trans.position, new Vector3(1, 0, 0), ref farPoint);
					invertMeshVertices.Add(farPoint - _trans.position);

					AddNewTriangle(ref invertTriangles, vertexIndex+1, invertMeshVertices.Count-1, farVertexIndex);
				}
			}
			
			Mesh mesh = new Mesh();
			
			mesh.vertices = invertMeshVertices.ToArray();
			mesh.triangles = invertTriangles.ToArray();
			
			_meshFilter.mesh = mesh;
		}

		private float GetMinRaycastDistance () {
			Vector3 screenSize = SHelper.GetScreenSizeInWorld();
			return screenSize.magnitude;
		}

		private void AddNewTriangle (ref List<int> triangles, int v0, int v1, int v2) {
			triangles.Add(v0);
			triangles.Add(v1);
			triangles.Add(v2);
		}

		private void DebugDraw (List<Vector3> meshVertices, List<int> triangles) {
			for (int i=0; i<triangles.Count; i++) {
				Debug.DrawLine(meshVertices[triangles[i]], meshVertices[triangles[++i]], Color.yellow, 100);
				Debug.DrawLine(meshVertices[triangles[i]], meshVertices[triangles[++i]], Color.yellow, 100);
				Debug.DrawLine(meshVertices[triangles[i]], meshVertices[triangles[i-2]], Color.yellow, 100);
			}
		}
	}
}

