using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	public class LOSLight : MonoBehaviour {

		public Material defaultMaterial;
		public float degreeStepRough = 1;
		public float degreeStepDetail = 0.1f;

		public bool invert = true;

		private Transform _trans;
		private MeshFilter _meshFilter;
		private Vector2 _previousPosition;

		private static float _tolerance = 0.001f;

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

				if (Physics.Raycast(_trans.position, direction, out hit, distance)) {
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
			RaycastHit hit;
			int vertexIndex = 0;
			int farVertexIndex = 0;
			Vector3 previousHitPoint = Vector3.zero;

			for (float degree=0; degree<360; degree+=degreeStepRough) {
				Vector3 direction;
				
				direction = SMath.DegreeToUnitVector(degree);
				
				if (Physics.Raycast(_trans.position, direction, out hit, 8)) {		// Hit a collider.
					Vector3 hitPoint = hit.point;
					Collider hitCollider = hit.collider;

					if (degree == 0) {
						raycastHitAtDegree0 = true;
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

						farVertexIndex = vertexIndex + 2;

						vertexIndex += 2;
						AddNewTriangle(ref invertTriangles, vertexIndex-1, vertexIndex+1, farVertexIndex);
					}
					else {
						invertMeshVertices.Add(hitPoint - _trans.position);
						
						vertexIndex++;
						AddNewTriangle(ref invertTriangles, vertexIndex, vertexIndex+1, farVertexIndex);
					}

					previousHitPoint = hitPoint;
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
						}
					}

					previousHitCollider = null;
				}
			}

			if (null != previousHitCollider) {
				if (raycastHitAtDegree0) {
					AddNewTriangle(ref invertTriangles, vertexIndex+1, 1, farVertexIndex);
					AddNewTriangle(ref invertTriangles, 0, 1, farVertexIndex);
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

		private void DrawInvertImp () {
			List<Vector3> invertMeshVertices = new List<Vector3>();
			List<int> invertTriangles = new List<int>();

			RaycastHit hit;
			Collider currentHitCollider = null;
			Vector3 currentHitNormal = Vector3.zero;
			bool raycastHitAtDegree0 = false;
			int vertexIndex = 0;
			Vector3 previousHitPoint = Vector3.zero;
			bool biggerBase = false;

			for (float degreeRough=0; degreeRough<360; degreeRough+=degreeStepRough) {
				Vector3 direction;

				direction = SMath.DegreeToUnitVector(degreeRough);

				if (Physics.Raycast(_trans.position, direction, out hit, 8)) {		// Hit a collider.
					Vector3 hitPoint = hit.point;
					Collider hitCollider = hit.collider;
					Vector3 hitNormal = hit.normal;

					if (degreeRough == 0) {
						raycastHitAtDegree0 = true;
						invertMeshVertices.Add(hitPoint - _trans.position);
						LOSManager.instance.GetCollisionPointWithViewBox(_trans.position, direction, ref hitPoint);
						invertMeshVertices.Add(hitPoint - _trans.position);

						currentHitCollider = hitCollider;
						currentHitNormal = hitNormal;
					}
					else if (currentHitCollider == null) {
						for (float degreeDetail=degreeRough-degreeStepRough; degreeDetail<degreeRough; degreeDetail+=degreeStepDetail) {
							direction = SMath.DegreeToUnitVector(degreeDetail);
							
							if (Physics.Raycast(_trans.position, direction, out hit, 8)) {
								hitPoint = hit.point;
								break;
							}
						}

						invertMeshVertices.Add(hitPoint - _trans.position);
						LOSManager.instance.GetCollisionPointWithViewBox(_trans.position, direction, ref hitPoint);
						invertMeshVertices.Add(hitPoint - _trans.position);
					}
					else if (hitCollider != currentHitCollider) {
						Vector3 previousHitPointDetail = Vector3.zero;
						for (float degreeDetail=degreeRough-degreeStepRough; degreeDetail<degreeRough; degreeDetail+=degreeStepDetail) {
							direction = SMath.DegreeToUnitVector(degreeDetail);
							
							if (Physics.Raycast(_trans.position, direction, out hit, 8)) {
								hitPoint = hit.point;

								if (hit.collider != currentHitCollider) {
									break;
								}
								previousHitPointDetail = hitPoint;
							}
						}

						invertMeshVertices.Add(previousHitPointDetail - _trans.position);
						invertMeshVertices.Add(hitPoint - _trans.position);		// The hit point on the new collider.
						LOSManager.instance.GetCollisionPointWithViewBox(_trans.position, direction, ref previousHitPointDetail);
						invertMeshVertices.Add(previousHitPointDetail - _trans.position);


						if (biggerBase) {
							AddNewTriangle(ref invertTriangles, vertexIndex, vertexIndex+1, vertexIndex-1);
							AddNewTriangle(ref invertTriangles, vertexIndex-1, vertexIndex+1, vertexIndex+3);
							AddNewTriangle(ref invertTriangles, vertexIndex+1, vertexIndex+2, vertexIndex+3);
							biggerBase = false;
							vertexIndex += 2;
						}
						else {
							AddNewTriangle(ref invertTriangles, vertexIndex, vertexIndex+2, vertexIndex+1);
							AddNewTriangle(ref invertTriangles, vertexIndex+1, vertexIndex+2, vertexIndex+4);
							AddNewTriangle(ref invertTriangles, vertexIndex+3, vertexIndex+2, vertexIndex+4);
							vertexIndex += 3;
						}
					}
					else if (hitNormal != currentHitNormal) {
						for (float degreeDetail=degreeRough-degreeStepRough; degreeDetail<degreeRough; degreeDetail+=degreeStepDetail) {
							direction = SMath.DegreeToUnitVector(degreeDetail);
							
							if (Physics.Raycast(_trans.position, direction, out hit, 8)) {
								hitPoint = hit.point;

								if (hitNormal != currentHitNormal) {
									break;
								}
							}
						}

						invertMeshVertices.Add(hitPoint - _trans.position);

						AddNewTriangle(ref invertTriangles, vertexIndex, vertexIndex+2, vertexIndex+1);
						biggerBase = true;

						vertexIndex += 2;
					}

					currentHitCollider = hitCollider;
					currentHitNormal = hitNormal;

					previousHitPoint = hitPoint;
				}
				else {		// Hit nothing
					if (currentHitCollider != null) {
						Vector3 hitPoint = previousHitPoint;
						
							// Detail scan
						for (float degreeDetail=degreeRough-degreeStepRough; degreeDetail<degreeRough; degreeDetail+=degreeStepDetail) {
							direction = SMath.DegreeToUnitVector(degreeDetail);
							
							if (!Physics.Raycast(_trans.position, direction, out hit, 8)) {
								break;
							}
							else {
								hitPoint = hit.point;
							}
						}

						invertMeshVertices.Add(hitPoint - _trans.position);
						LOSManager.instance.GetCollisionPointWithViewBox(_trans.position, direction, ref hitPoint);
						invertMeshVertices.Add(hitPoint - _trans.position);

						if (biggerBase) {
							AddNewTriangle(ref invertTriangles, vertexIndex, vertexIndex+1, vertexIndex-1);
							AddNewTriangle(ref invertTriangles, vertexIndex-1, vertexIndex+1, vertexIndex+2);
							biggerBase = false;
							vertexIndex += 3;
						}
						else {
							AddNewTriangle(ref invertTriangles, vertexIndex, vertexIndex+2, vertexIndex+1);
							AddNewTriangle(ref invertTriangles, vertexIndex+1, vertexIndex+2, vertexIndex+3);
							vertexIndex += 4;
						}
					}
					currentHitCollider = null;
				}
			}

			if (currentHitCollider != null && raycastHitAtDegree0) {
				AddNewTriangle(ref invertTriangles, 0, 1, vertexIndex);
				AddNewTriangle(ref invertTriangles, 1, vertexIndex, vertexIndex+1);
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

