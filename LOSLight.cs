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
				DrawImp();
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
			List<Vector3> meshVertices = new List<Vector3>();
			List<Vector3> invertMeshVertices = new List<Vector3>();
			List<int> invertTriangles = new List<int>();

			bool raycastHitAtDegree0 = false;
			bool raycastHitPreviously = false;
			int invertRectBaseVertexIndex = 0;
			
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
			
			for (float degree=0; degree<360; degree+=degreeStepRough) {
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

					if (degree == 0) {
						raycastHitAtDegree0 = true;
						raycastHitPreviously = true;
						invertMeshVertices.Add(hitPoint - _trans.position);
						LOSManager.instance.GetCollisionPointWithViewBox(_trans.position, direction, ref hitPoint);
						invertMeshVertices.Add(hitPoint - _trans.position);
					}
					else if (!raycastHitPreviously) {
						invertMeshVertices.Add(hitPoint - _trans.position);
						invertMeshVertices.Add(meshVertices[meshVertices.Count-1]);		// previous added mesh vertex
						raycastHitPreviously = true;
					}
				}
				else {
					LOSManager.instance.GetCollisionPointWithViewBox(_trans.position, direction, ref hitPoint);
					
					if (raycastHitPreviously) {
						invertMeshVertices.Add(hitPoint - _trans.position);
						invertMeshVertices.Add(meshVertices[meshVertices.Count-1]);
						
						AddNewTriangle(ref invertTriangles, invertRectBaseVertexIndex, invertRectBaseVertexIndex+2, invertRectBaseVertexIndex+1);
						AddNewTriangle(ref invertTriangles, invertRectBaseVertexIndex, invertRectBaseVertexIndex+3, invertRectBaseVertexIndex+2);
						invertRectBaseVertexIndex += 4;
						raycastHitPreviously = false;
					}
				}
				
				meshVertices.Add(hitPoint - _trans.position);
				int currentVertexIndex = meshVertices.Count - 1;

				previousVectexIndex = currentVertexIndex;
			}

			if (raycastHitPreviously) {
				if (raycastHitAtDegree0) {
					AddNewTriangle(ref invertTriangles, 0, 1, invertRectBaseVertexIndex);
					AddNewTriangle(ref invertTriangles, 1, invertRectBaseVertexIndex, invertRectBaseVertexIndex+1);
				}
				else {
					// TODO
				}
			}

			Mesh mesh = new Mesh();

			mesh.vertices = invertMeshVertices.ToArray();
			mesh.triangles = invertTriangles.ToArray();
			
			_meshFilter.mesh = mesh;
		}

		private void DrawImp () {
			List<Vector3> meshVertices = new List<Vector3>();
			List<int> triangles = new List<int>();
			
			bool raycastHitPreviously = false;
			
			float distance = GetMinRaycastDistance();
			RaycastHit hit;
			
			int previousVectexIndex = 0;
			Vector3 hitPoint = Vector3.zero;
			Vector3 previousHitpointRough = Vector3.zero;
			Vector3 previousHitpointDetail = Vector3.zero;
			
			meshVertices.Add(Vector3.zero);

			for (float degreeRough=0, previousDegreeRough=0; degreeRough<360; previousDegreeRough=degreeRough, degreeRough+=degreeStepRough) {
				Vector3 direction = SMath.DegreeToUnitVector(degreeRough);

				if (Physics.Raycast(_trans.position, direction, out hit, distance)) {
					Vector3 hitpointRough = hit.point;

					if (!raycastHitPreviously) {	// Then find the vertex in detail
						bool detailRaycastHit = false;

						for (float degreeDetail=previousDegreeRough; degreeDetail<degreeRough; degreeDetail+=degreeStepDetail) {
							direction = SMath.DegreeToUnitVector(degreeDetail);

							if (Physics.Raycast(_trans.position, direction, out hit, distance)) {
								hitPoint = hit.point;
								detailRaycastHit = true;
								break;
							}
						}

						if (!detailRaycastHit) {
							hitPoint = hitpointRough;
						}

						meshVertices.Add(hitPoint - _trans.position);

						raycastHitPreviously = true;
					}

					previousHitpointRough = hitpointRough;
				}
				else {
					if (raycastHitPreviously) {
						bool detailRaycastHit = false;

						for (float degreeDetail=previousDegreeRough; degreeDetail<degreeRough; degreeDetail+=degreeStepDetail) {
							direction = SMath.DegreeToUnitVector(degreeDetail);

							if (Physics.Raycast(_trans.position, direction, out hit, distance)) {
								previousHitpointDetail = hit.point;
								detailRaycastHit = true;
							}
							else {
								break;
							}
						}

						if (detailRaycastHit) {
							hitPoint = previousHitpointDetail;
						}
						else {
							hitPoint = previousHitpointRough;
						}

						meshVertices.Add(hitPoint - _trans.position);
						AddNewTriangle(ref triangles, 0, meshVertices.Count-1, meshVertices.Count-2);

						raycastHitPreviously = false;
					}
				}
			}

			if (raycastHitPreviously) {
				AddNewTriangle(ref triangles, 0, 1, meshVertices.Count-1);
			}

			Mesh mesh = new Mesh();
			
			mesh.vertices = meshVertices.ToArray();
			mesh.triangles = triangles.ToArray();
			
			_meshFilter.mesh = mesh;
		}

		private void DrawInvertImp () {

		}

		private float GetMinRaycastDistance () {
			Vector3 screenSize = SHelper.GetScreenSizeInWorld();
			Vector2 screenSize2 = SMath.Vec3ToVec2(screenSize);
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

