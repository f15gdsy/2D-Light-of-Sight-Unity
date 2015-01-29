using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	public class LOSLight : LOSObjectBase {

		public Material defaultMaterial;
		public float degreeStep = 0.1f;
		public bool invert = true;
		public LayerMask obstacleLayer;
		public float lightAngle = 0;
		public float faceAngle = 0;
		public Color color = new Color(1, 1, 1, 1);

		private MeshFilter _meshFilter;
		private float _previousFaceAngle;
		private float _previousLightAngle;
		private Color _previousColor;
		private float _raycastDistance;
		private float _startAngle;
		private float _endAngle;



		protected override void Awake () {
			base.Awake();

			lightAngle = SMath.ClampDegree0To360(lightAngle);
		}

		void Start () {
			_meshFilter = GetComponent<MeshFilter>();
			if (_meshFilter == null) {
				_meshFilter = gameObject.AddComponent<MeshFilter>();
			}

			if (renderer == null) {
//				gameObject.AddComponent<MeshRenderer>();
//				renderer.material = defaultMaterial;
			}

			renderer.material = defaultMaterial;

			Vector2 screenSize = SHelper.GetScreenSizeInWorld();
			_raycastDistance = Mathf.Sqrt(screenSize.x*screenSize.x + screenSize.y*screenSize.y);

			DoDraw();
		}

		private float time;

		void LateUpdate () {
			if (SHelper.CheckWithinScreen(position, LOSManager.instance.losCamera.camera) && ((LOSManager.instance.CheckDirty() || CheckDirty()))) {
				UpdatePreviousInfo();
				DoDraw();
			}
		}

		public override void UpdatePreviousInfo () {
			base.UpdatePreviousInfo ();
			_previousFaceAngle = faceAngle;
			_previousLightAngle = lightAngle;
			_previousColor = color;
		}

		public override bool CheckDirty () {
			return !_previousPosition.Equals(position) || 
				!_previousFaceAngle.Equals(faceAngle) || 
					!_previousLightAngle.Equals(lightAngle) ||
					!_previousColor.Equals(color);
		}

		private void DoDraw () {
			_startAngle = lightAngle == 0 ? 0 : faceAngle - lightAngle / 2;
			_endAngle = lightAngle == 0 ? 360 : faceAngle + lightAngle / 2;

			if (invert) {
				DrawInvertAngledImp();
			}
			else {
				DrawImp();
			}
		}

		private void Draw () {
			List<Vector3> meshVertices = new List<Vector3>();
			List<int> triangles = new List<int>();

			float distance = GetMinRaycastDistance();
			RaycastHit hit;
			int previousVectexIndex = 0;
			Vector3 hitPoint = Vector3.zero;

			meshVertices.Add(Vector3.zero);
		
			Vector2 viewboxSize = LOSManager.instance.halfViewboxSize;
			Vector3 upperRight = new Vector3(viewboxSize.x, viewboxSize.y) + LOSManager.instance.losCameraTrans.position;
			Vector3 upperLeft = new Vector3(-viewboxSize.x, viewboxSize.y) + LOSManager.instance.losCameraTrans.position;
			Vector3 lowerLeft = new Vector3(-viewboxSize.x, -viewboxSize.y) + LOSManager.instance.losCameraTrans.position;
			Vector3 lowerRight = new Vector3(viewboxSize.x, -viewboxSize.y) + LOSManager.instance.losCameraTrans.position;

			upperRight.z = 0;
			upperLeft.z = 0;
			lowerLeft.z = 0;
			lowerRight.z = 0;

			float degreeUpperRight =  SMath.ClampDegree0To360(SMath.VectorToDegree(upperRight - position));
			float degreeUpperLeft = SMath.ClampDegree0To360(SMath.VectorToDegree(upperLeft - position));
			float degreeLowerLeft = SMath.ClampDegree0To360(SMath.VectorToDegree(lowerLeft - position));
			float degreeLowerRight = SMath.ClampDegree0To360(SMath.VectorToDegree(lowerRight - position));

			for (float degree=_startAngle; degree<_endAngle; degree+=degreeStep) {
				Vector3 direction;

				if (degree < degreeUpperRight && degree+degreeStep > degreeUpperRight) {
					direction = upperRight - position;
				}
				else if (degree < degreeUpperLeft && degree+degreeStep > degreeUpperLeft) {
					direction = upperLeft - position;
				}
				else if (degree < degreeLowerLeft && degree+degreeStep > degreeLowerLeft) {
					direction = lowerLeft - position;
				}
				else if (degree < degreeLowerRight && degree+degreeStep > degreeLowerRight) {
					direction = lowerRight - position;
				}
				else {
					direction = SMath.DegreeToUnitVector(degree);
				}

				if (Physics.Raycast(position, direction, out hit, distance, obstacleLayer)) {
					hitPoint = hit.point;
				}
				else {
					LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref hitPoint);
				}

				meshVertices.Add(hitPoint - position);
				int currentVertexIndex = meshVertices.Count - 1;

				if (meshVertices.Count >= 3) {
					AddNewTriangle(ref triangles, 0, currentVertexIndex, previousVectexIndex);
				}
				previousVectexIndex = currentVertexIndex;
			}

			if (lightAngle == 0) {
				AddNewTriangle(ref triangles, 0, 1, previousVectexIndex);
			}
	
			Mesh mesh = new Mesh();

			mesh.vertices = meshVertices.ToArray();
			mesh.triangles = triangles.ToArray();

			_meshFilter.mesh = mesh;
		}

		private void DrawImp () {
			List<Vector3> meshVertices = new List<Vector3>();
			List<int> triangles = new List<int>();

			Vector3 direction = Vector3.zero;
			Vector3 previousNormal = Vector3.zero;
			Collider previousCollider = null;
			float distance = GetMinRaycastDistance();
			RaycastHit hit;
			int currentVertexIndex = -1;
			int previousVectexIndex = -1;
			Vector3 previousTempPoint = Vector3.zero;

			meshVertices.Add(Vector3.zero);

			// Add the four viewbox corners.
			foreach (Vector3 corner in LOSManager.instance.viewboxCorners) {
				meshVertices.Add(corner - position);
			}
			
			for (float degree=_startAngle; degree<_endAngle; degree+=degreeStep) {
				direction = SMath.DegreeToUnitVector(degree);
				
				if (Physics.Raycast(position, direction, out hit, distance, obstacleLayer) && CheckRaycastHit(hit)) {
					Vector3 hitPoint = hit.point;
					Collider hitCollider = hit.collider;
					Vector3 hitNormal = hit.normal;

					if (degree == _startAngle) {
						meshVertices.Add(hitPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
					}
					else if (previousCollider != hit.collider) {
						if (previousCollider == null) {
							Vector3 farPoint = Vector3.zero;
							LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref farPoint);

							meshVertices.Add(farPoint - position);
							previousVectexIndex = currentVertexIndex;
							currentVertexIndex = meshVertices.Count - 1;

							AddNewTrianglesBetweenPoints4Corners(ref triangles, meshVertices, previousVectexIndex, currentVertexIndex, 0);

							meshVertices.Add(hitPoint - position);
							previousVectexIndex = currentVertexIndex;
							currentVertexIndex = meshVertices.Count - 1;
						}
						else {
							meshVertices.Add(previousTempPoint - position);
							previousVectexIndex = currentVertexIndex;
							currentVertexIndex = meshVertices.Count - 1;
							AddNewTriangle (ref triangles, 0, currentVertexIndex, previousVectexIndex);

							meshVertices.Add(hitPoint - position);
							previousVectexIndex = currentVertexIndex;
							currentVertexIndex = meshVertices.Count - 1;
							AddNewTriangle (ref triangles, 0, currentVertexIndex, previousVectexIndex);
						}
					}
					else if (previousNormal != hitNormal) {
						meshVertices.Add(previousTempPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle (ref triangles, 0, currentVertexIndex, previousVectexIndex);
						
						meshVertices.Add(hitPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle (ref triangles, 0, currentVertexIndex, previousVectexIndex);
					}

					previousCollider = hitCollider;
					previousTempPoint = hitPoint;
					previousNormal = hitNormal;
				}
				else {
					if (degree == _startAngle) {
						Vector3 farPoint = Vector3.zero;
						LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref farPoint);

						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
					}
					else if (previousCollider != null) {
						meshVertices.Add(previousTempPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle (ref triangles, 0, currentVertexIndex, previousVectexIndex);

						Vector3 farPoint = Vector3.zero;
						LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref farPoint);

						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle(ref triangles, 0, currentVertexIndex, previousVectexIndex);


					}
					previousCollider = null;
//					previousTempPoint = farPoint;
				}
			}

			if (lightAngle == 0) {
				if (previousCollider == null) {
					AddNewTrianglesBetweenPoints4Corners(ref triangles, meshVertices, currentVertexIndex, 5, 0);
				}
				else {
					AddNewTriangle(ref triangles, 0, 5, currentVertexIndex);
				}
			}
			else {
				if (previousCollider == null) {
					Vector3 farPoint = Vector3.zero;
					LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref farPoint);

					meshVertices.Add(farPoint - position);
					previousVectexIndex = currentVertexIndex;
					currentVertexIndex = meshVertices.Count - 1;

					AddNewTrianglesBetweenPoints4Corners(ref triangles, meshVertices, previousVectexIndex, currentVertexIndex, 0);
				}
				else {
					meshVertices.Add(previousTempPoint - position);
					previousVectexIndex = currentVertexIndex;
					currentVertexIndex = meshVertices.Count - 1;
					AddNewTriangle(ref triangles, 0, currentVertexIndex, previousVectexIndex);
				}
			}

			DeployMesh(meshVertices, triangles, CalculateUVs(meshVertices), color);
		}

		private void DrawInvertAngled () {
			List<Vector3> invertAngledMeshVertices = new List<Vector3>();
			List<int> invertAngledTriangles = new List<int>();

			Collider previousCollider = null;
			Vector3 firstFarPoint = Vector3.zero;
			Vector3 lastFarPoint = Vector3.zero;
			int firstFarPointIndex = -1;
			int lastFarPointIndex = -1;
			int farPointIndex = -1;
			int previousFarPointIndex = -1;
			int closePointIndex = -1;
			int previousClosePointIndex = -1;
			int closePointAtDegree0Index = -1;
			int colliderClosePointCount = 0;
			RaycastHit hit;


			invertAngledMeshVertices.Add(Vector3.zero);		// Add the position of the light

			// Add the four viewbox corners.
			foreach (Vector3 corner in LOSManager.instance.viewboxCorners) {
				invertAngledMeshVertices.Add(corner - position);
			}

			for (float degree = _startAngle; degree<_endAngle; degree+=degreeStep) {
				Vector3 direction;
				
				direction = SMath.DegreeToUnitVector(degree);

				if (degree == _startAngle) {
					LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref firstFarPoint);
					invertAngledMeshVertices.Add(firstFarPoint - position);
					firstFarPointIndex = invertAngledMeshVertices.Count - 1;
				}
				else if (degree + degreeStep >= _endAngle) {		// degree == _endAngle - degreeStep
					LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref lastFarPoint);
					invertAngledMeshVertices.Add(lastFarPoint - position);
					lastFarPointIndex = invertAngledMeshVertices.Count - 1;
				}

				if (Physics.Raycast(position, direction, out hit, _raycastDistance, obstacleLayer) && CheckRaycastHit(hit)) {		// Hit a collider.
					Vector3 hitPoint = hit.point;
					Collider hitCollider = hit.collider;

					if (degree == _startAngle) {
						farPointIndex = firstFarPointIndex;

						invertAngledMeshVertices.Add(hitPoint - position);
						previousClosePointIndex = closePointIndex;
						closePointIndex = invertAngledMeshVertices.Count - 1;

						colliderClosePointCount++;

						if (degree == 0) {
							closePointAtDegree0Index = closePointIndex;
						}
					}
					else if (degree + degreeStep >= _endAngle) {
						previousFarPointIndex = farPointIndex;
						farPointIndex = lastFarPointIndex;

						invertAngledMeshVertices.Add(hitPoint - position);
						previousClosePointIndex = closePointIndex;
						closePointIndex = invertAngledMeshVertices.Count - 1;

						colliderClosePointCount++;
						if (previousCollider != hitCollider && null != previousCollider) {
							colliderClosePointCount = 1;

							if (_startAngle == 0 && _endAngle == 360) {
								AddNewTriangle(ref invertAngledTriangles, previousFarPointIndex, previousClosePointIndex, lastFarPointIndex);
								AddNewTriangle(ref invertAngledTriangles, previousClosePointIndex, closePointIndex, lastFarPointIndex);
								AddNewTrianglesBetweenPoints2Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, lastFarPointIndex);
							}
						}
					}
					else {
						if (null == previousCollider || hitCollider != previousCollider) {
							Vector3 farPoint = Vector3.zero;
							LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref farPoint);
							invertAngledMeshVertices.Add(farPoint - position);
							previousFarPointIndex = farPointIndex;
							farPointIndex = invertAngledMeshVertices.Count - 1;

							invertAngledMeshVertices.Add(hitPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;

							colliderClosePointCount++;

							if (hitCollider != previousCollider && previousCollider != null) {
								colliderClosePointCount = 1;

								AddNewTriangle(ref invertAngledTriangles, previousClosePointIndex, closePointIndex, farPointIndex);

								Vector3 previousClosePoint = invertAngledMeshVertices[previousClosePointIndex] + position;
								Vector3 previousFarPoint = invertAngledMeshVertices[previousFarPointIndex] + position;
								Vector3 previousClosePointToFar = farPoint - previousClosePoint;
								Vector3 previousCloseToPreviousFar = previousFarPoint - previousClosePoint;

								if (SMath.GetDegreeBetweenIndexVector(previousCloseToPreviousFar.normalized, previousClosePointToFar.normalized) < 0) {	// left
									AddNewTriangle(ref invertAngledTriangles, previousFarPointIndex, previousClosePointIndex, farPointIndex);
									AddNewTrianglesBetweenPoints2Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, farPointIndex);
								}
								else {	// right
									AddNewTrianglesBetweenPoints2Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, previousClosePointIndex);
								}
							}
						}
//						else if (hitCollider != previousCollider) {
//
//						}
						else {
							invertAngledMeshVertices.Add(hitPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;
						
							AddNewTriangle(ref invertAngledTriangles, previousClosePointIndex, closePointIndex, farPointIndex);

							colliderClosePointCount++;
						}
					}

					previousCollider = hitCollider;
				}
				else {
					if (null != previousCollider) {
						colliderClosePointCount = 0;

						Vector3 farPoint = Vector3.zero;
						LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref farPoint);
						invertAngledMeshVertices.Add(farPoint - position);
						previousFarPointIndex = farPointIndex;
						farPointIndex = invertAngledMeshVertices.Count - 1;

						Vector3 previousFarPoint = invertAngledMeshVertices[previousFarPointIndex] + position;
						Vector3 closePoint = invertAngledMeshVertices[closePointIndex] + position;
						List<Vector3> corners = LOSManager.instance.GetViewboxCornersBetweenPoints(previousFarPoint, closePoint, position);
						switch (corners.Count) {
						case 0:
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, farPointIndex, previousFarPointIndex);
							break;
						case 1:
							int cornerIndex0 = GetCorrectCornerIndex(invertAngledMeshVertices, corners[0], 1);
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, cornerIndex0, previousFarPointIndex);
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, farPointIndex, cornerIndex0);
							break;
						case 2:
							cornerIndex0 = GetCorrectCornerIndex(invertAngledMeshVertices, corners[0], 1);
							int cornerIndex1 = GetCorrectCornerIndex(invertAngledMeshVertices, corners[1], 1);
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, cornerIndex0, previousFarPointIndex);
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, cornerIndex1, cornerIndex0);
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, farPointIndex, cornerIndex1);
							break;
						}
					}

					previousCollider = null;
				}
			}



			if (_startAngle == 0 && _endAngle == 360) {
				if (previousCollider != null) {
					if (closePointAtDegree0Index == -1) {
						AddNewTrianglesBetweenPoints4Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, lastFarPointIndex, closePointIndex);
					}
					else {
						if (colliderClosePointCount > 1) {
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, closePointAtDegree0Index, firstFarPointIndex);
							AddNewTriangle(ref invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);
							AddNewTrianglesBetweenPoints4Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, firstFarPointIndex, closePointIndex);
						}
						else {
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, closePointAtDegree0Index, firstFarPointIndex);
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, firstFarPointIndex, lastFarPointIndex);
						}
					}
				}
			}
			else {	// Add triangles between outside of view.
				AddNewTrianglesBetweenPoints4Corners(ref invertAngledTriangles, invertAngledMeshVertices, lastFarPointIndex, firstFarPointIndex, 0);

				if (previousCollider != null) {
					if (colliderClosePointCount >= 2) {
						AddNewTrianglesBetweenPoints4Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, lastFarPointIndex, closePointIndex);
						AddNewTriangle(ref invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);
					}
					else if (previousFarPointIndex >= 0){
						AddNewTrianglesBetweenPoints4Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, lastFarPointIndex, previousClosePointIndex);
						AddNewTriangle(ref invertAngledTriangles, closePointIndex, lastFarPointIndex, previousClosePointIndex);
					}
				}
			}

			DeployMesh(invertAngledMeshVertices, invertAngledTriangles, CalculateUVs(invertAngledMeshVertices), color);
		}

		private void DrawInvertAngledImp () {
			List<Vector3> invertAngledMeshVertices = new List<Vector3>();
			List<int> invertAngledTriangles = new List<int>();

			Vector3 previousNormal = Vector3.zero;
			Collider previousCollider = null;
			Vector3 firstFarPoint = Vector3.zero;
			Vector3 lastFarPoint = Vector3.zero;
			Vector3 previousCloseTempPoint = Vector3.zero;
			int firstFarPointIndex = -1;
			int lastFarPointIndex = -1;
			int farPointIndex = -1;
			int previousFarPointIndex = -1;
			int closePointIndex = -1;
			int previousClosePointIndex = -1;
			int closePointAtDegree0Index = -1;
			int colliderClosePointCount = 0;
			RaycastHit hit;
			
			
			invertAngledMeshVertices.Add(Vector3.zero);		// Add the position of the light
			
			// Add the four viewbox corners.
			foreach (Vector3 corner in LOSManager.instance.viewboxCorners) {
				invertAngledMeshVertices.Add(corner - position);
			}
			
			for (float degree = _startAngle; degree<_endAngle; degree+=degreeStep) {
				Vector3 direction;
				
				direction = SMath.DegreeToUnitVector(degree);
				
				if (degree == _startAngle) {
					LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref firstFarPoint);
					invertAngledMeshVertices.Add(firstFarPoint - position);
					firstFarPointIndex = invertAngledMeshVertices.Count - 1;
				}
				else if (degree + degreeStep >= _endAngle) {		// degree == _endAngle - degreeStep
					LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref lastFarPoint);
					invertAngledMeshVertices.Add(lastFarPoint - position);
					lastFarPointIndex = invertAngledMeshVertices.Count - 1;
				}
				
				if (Physics.Raycast(position, direction, out hit, _raycastDistance, obstacleLayer) && CheckRaycastHit(hit)) {		// Hit a collider.
					Vector3 hitPoint = hit.point;
					Collider hitCollider = hit.collider;
					Vector3 hitNormal = hit.normal;
					
					if (degree == _startAngle) {
						farPointIndex = firstFarPointIndex;
						
						invertAngledMeshVertices.Add(hitPoint - position);
						previousClosePointIndex = closePointIndex;
						closePointIndex = invertAngledMeshVertices.Count - 1;
						
						colliderClosePointCount++;
						
						if (degree == 0) {
							closePointAtDegree0Index = closePointIndex;
						}
					}
					else if (degree + degreeStep >= _endAngle) {
						previousFarPointIndex = farPointIndex;
						farPointIndex = lastFarPointIndex;
						
						invertAngledMeshVertices.Add(hitPoint - position);
						previousClosePointIndex = closePointIndex;
						closePointIndex = invertAngledMeshVertices.Count - 1;
						
						colliderClosePointCount++;
						if (previousCollider != hitCollider && null != previousCollider) {
							colliderClosePointCount = 1;
							
							if (_startAngle == 0 && _endAngle == 360) {
								AddNewTriangle(ref invertAngledTriangles, previousFarPointIndex, previousClosePointIndex, lastFarPointIndex);
								AddNewTriangle(ref invertAngledTriangles, previousClosePointIndex, closePointIndex, lastFarPointIndex);
								AddNewTrianglesBetweenPoints2Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, lastFarPointIndex);
							}
						}
					}
					else {
						if (null == previousCollider) {
							Vector3 farPoint = Vector3.zero;
							LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref farPoint);
							invertAngledMeshVertices.Add(farPoint - position);
							previousFarPointIndex = farPointIndex;
							farPointIndex = invertAngledMeshVertices.Count - 1;
							
							invertAngledMeshVertices.Add(hitPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;
							
							colliderClosePointCount++;
						}
						else if (previousCollider != hitCollider) {
							colliderClosePointCount = 1;

							Vector3 farPoint = Vector3.zero;
							LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref farPoint);
							invertAngledMeshVertices.Add(farPoint - position);
							previousFarPointIndex = farPointIndex;
							farPointIndex = invertAngledMeshVertices.Count - 1;
							
							invertAngledMeshVertices.Add(previousCloseTempPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;
							
							AddNewTriangle(ref invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);

							invertAngledMeshVertices.Add(hitPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;

							AddNewTriangle(ref invertAngledTriangles, previousClosePointIndex, closePointIndex, farPointIndex);
							
							Vector3 previousClosePoint = invertAngledMeshVertices[previousClosePointIndex] + position;
							Vector3 previousFarPoint = invertAngledMeshVertices[previousFarPointIndex] + position;
							Vector3 previousClosePointToFar = farPoint - previousClosePoint;
							Vector3 previousCloseToPreviousFar = previousFarPoint - previousClosePoint;
							
							if (SMath.GetDegreeBetweenIndexVector(previousCloseToPreviousFar.normalized, previousClosePointToFar.normalized) < 0) {	// left
								AddNewTriangle(ref invertAngledTriangles, previousFarPointIndex, previousClosePointIndex, farPointIndex);
								AddNewTrianglesBetweenPoints2Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, farPointIndex);
							}
							else {	// right
								AddNewTrianglesBetweenPoints2Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, previousClosePointIndex);
							}
						}
						else {
							if (previousNormal != hitNormal) {
								invertAngledMeshVertices.Add(previousCloseTempPoint - position);
								previousClosePointIndex = closePointIndex;
								closePointIndex = invertAngledMeshVertices.Count - 1;
								AddNewTriangle(ref invertAngledTriangles, closePointIndex, farPointIndex, previousClosePointIndex);

								invertAngledMeshVertices.Add(hitPoint - position);
								previousClosePointIndex = closePointIndex;
								closePointIndex = invertAngledMeshVertices.Count - 1;
								AddNewTriangle(ref invertAngledTriangles, previousClosePointIndex, closePointIndex, farPointIndex);

							}
							
							colliderClosePointCount++;
						}
					}
					
					previousCollider = hitCollider;
					previousCloseTempPoint = hitPoint;
					previousNormal = hitNormal;
				}
				else {
					if (null != previousCollider) {
						colliderClosePointCount = 0;

						invertAngledMeshVertices.Add(previousCloseTempPoint - position);
						previousClosePointIndex = closePointIndex;
						closePointIndex = invertAngledMeshVertices.Count - 1;
						AddNewTriangle(ref invertAngledTriangles, closePointIndex, farPointIndex, previousClosePointIndex);
					
						Vector3 farPoint = Vector3.zero;
						LOSManager.instance.GetCollisionPointWithViewBox(position, direction, ref farPoint);
						invertAngledMeshVertices.Add(farPoint - position);
						previousFarPointIndex = farPointIndex;
						farPointIndex = invertAngledMeshVertices.Count - 1;
						
						Vector3 previousFarPoint = invertAngledMeshVertices[previousFarPointIndex] + position;
						Vector3 closePoint = invertAngledMeshVertices[closePointIndex] + position;
						List<Vector3> corners = LOSManager.instance.GetViewboxCornersBetweenPoints(previousFarPoint, closePoint, position);
						switch (corners.Count) {
						case 0:
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, farPointIndex, previousFarPointIndex);
							break;
						case 1:
							int cornerIndex0 = GetCorrectCornerIndex(invertAngledMeshVertices, corners[0], 1);
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, cornerIndex0, previousFarPointIndex);
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, farPointIndex, cornerIndex0);
							break;
						case 2:
							cornerIndex0 = GetCorrectCornerIndex(invertAngledMeshVertices, corners[0], 1);
							int cornerIndex1 = GetCorrectCornerIndex(invertAngledMeshVertices, corners[1], 1);
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, cornerIndex0, previousFarPointIndex);
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, cornerIndex1, cornerIndex0);
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, farPointIndex, cornerIndex1);
							break;
						}
					}
					
					previousCollider = null;
				}
			}
			
			
			
			if (_startAngle == 0 && _endAngle == 360) {
				if (previousCollider != null) {
					if (closePointAtDegree0Index == -1) {
						AddNewTrianglesBetweenPoints4Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, lastFarPointIndex, closePointIndex);
					}
					else {
						if (colliderClosePointCount > 1) {
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, closePointAtDegree0Index, firstFarPointIndex);
							AddNewTriangle(ref invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);
							AddNewTrianglesBetweenPoints4Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, firstFarPointIndex, closePointIndex);
						}
						else {
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, closePointAtDegree0Index, firstFarPointIndex);
							AddNewTriangle(ref invertAngledTriangles, closePointIndex, firstFarPointIndex, lastFarPointIndex);
						}
					}
				}
			}
			else {	// Add triangles between outside of view.
				AddNewTrianglesBetweenPoints4Corners(ref invertAngledTriangles, invertAngledMeshVertices, lastFarPointIndex, firstFarPointIndex, 0);
				
				if (previousCollider != null) {
					if (colliderClosePointCount >= 2) {
						AddNewTrianglesBetweenPoints4Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, lastFarPointIndex, closePointIndex);
						AddNewTriangle(ref invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);
					}
					else if (previousFarPointIndex >= 0){
						AddNewTrianglesBetweenPoints4Corners(ref invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, lastFarPointIndex, previousClosePointIndex);
						AddNewTriangle(ref invertAngledTriangles, closePointIndex, lastFarPointIndex, previousClosePointIndex);
					}
				}
			}
			
			DeployMesh(invertAngledMeshVertices, invertAngledTriangles, CalculateUVs(invertAngledMeshVertices), color);
		}

		private void AddNewTrianglesBetweenPoints2Corners (ref List<int> triangles, List<Vector3> vertices, int pointAIndex, int pointBIndex) {
			Vector3 pointA = vertices[pointAIndex] + position;
			Vector3 pointB = vertices[pointBIndex] + position;
			
			List<Vector3> corners = LOSManager.instance.GetViewboxCornersBetweenPoints(pointA, pointB, position);
			switch (corners.Count) {
			case 0:
				break;
			case 1:
				int cornerIndex0 = GetCorrectCornerIndex(vertices, corners[0], 1);
				AddNewTriangle(ref triangles, pointAIndex, pointBIndex, cornerIndex0);
				break;
			case 2:
				cornerIndex0 = GetCorrectCornerIndex(vertices, corners[0], 1);
				int cornerIndex1 = GetCorrectCornerIndex(vertices, corners[1], 1);
				AddNewTriangle(ref triangles, pointAIndex, pointBIndex, cornerIndex1);
				AddNewTriangle(ref triangles, pointAIndex, cornerIndex1, cornerIndex0);
				break;
			default:
				Debug.LogError("LOS Light: Invalid number of corners: " + corners.Count);
				break;
			}
		}

		private void AddNewTrianglesBetweenPoints4Corners (ref List<int> triangles, List<Vector3> vertices,int pointAIndex, int pointBIndex, int centerIndex) {
			Vector3 pointA = vertices[pointAIndex] + position;
			Vector3 pointB = vertices[pointBIndex] + position;

			List<Vector3> corners = LOSManager.instance.GetViewboxCornersBetweenPoints(pointA, pointB, position);
			switch (corners.Count) {
			case 0:
				AddNewTriangle(ref triangles, pointAIndex, centerIndex, pointBIndex);
				break;
			case 1:
				int cornerIndex0 = GetCorrectCornerIndex(vertices, corners[0], 1);
				AddNewTriangle(ref triangles, pointAIndex, centerIndex, pointBIndex);
				AddNewTriangle(ref triangles, pointAIndex, pointBIndex, cornerIndex0);
				break;
			case 2:
				cornerIndex0 = GetCorrectCornerIndex(vertices, corners[0], 1);
				int cornerIndex1 = GetCorrectCornerIndex(vertices, corners[1], 1);
				AddNewTriangle(ref triangles, pointAIndex, centerIndex, cornerIndex0);
				AddNewTriangle(ref triangles, centerIndex, cornerIndex1, cornerIndex0);
				AddNewTriangle(ref triangles, cornerIndex1, centerIndex, pointBIndex);
				break;
			case 3:
				cornerIndex0 = GetCorrectCornerIndex(vertices, corners[0], 1);
				cornerIndex1 = GetCorrectCornerIndex(vertices, corners[1], 1);
				int cornerIndex2 = GetCorrectCornerIndex(vertices, corners[2], 1);
				AddNewTriangle(ref triangles, pointAIndex, centerIndex, cornerIndex0);
				AddNewTriangle(ref triangles, centerIndex, cornerIndex1, cornerIndex0);
				AddNewTriangle(ref triangles, centerIndex, cornerIndex2, cornerIndex1);
				AddNewTriangle(ref triangles, cornerIndex2, centerIndex, pointBIndex);
				break;
			case 4:
				cornerIndex0 = GetCorrectCornerIndex(vertices, corners[0], 1);
				cornerIndex1 = GetCorrectCornerIndex(vertices, corners[1], 1);
				cornerIndex2 = GetCorrectCornerIndex(vertices, corners[2], 1);
				int cornerIndex3 = GetCorrectCornerIndex(vertices, corners[3], 1);
				AddNewTriangle(ref triangles, pointAIndex, centerIndex, cornerIndex0);
				AddNewTriangle(ref triangles, centerIndex, cornerIndex1, cornerIndex0);
				AddNewTriangle(ref triangles, centerIndex, cornerIndex2, cornerIndex1);
				AddNewTriangle(ref triangles, centerIndex, cornerIndex3, cornerIndex2);
				AddNewTriangle(ref triangles, cornerIndex3, centerIndex, pointBIndex);
				break;
			default:
				Debug.LogError("LOS Light: Invalid number of corners: " + corners.Count);
				break;
			}
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

		private int GetCorrectCornerIndex (List<Vector3> vertices, Vector3 corner, int cornersStartIndex) {
			for (int i=cornersStartIndex; i<cornersStartIndex+4; i++) {
				if (vertices[i] == corner - position) {
					return i;
				}
			}
			return -1;
		}

		private void DebugDraw (List<Vector3> meshVertices, List<int> triangles, Color color, float time) {
			for (int i=0; i<triangles.Count; i++) {
				Debug.DrawLine(meshVertices[triangles[i]], meshVertices[triangles[++i]], color, time);
				Debug.DrawLine(meshVertices[triangles[i]], meshVertices[triangles[++i]], color, time);
				Debug.DrawLine(meshVertices[triangles[i]], meshVertices[triangles[i-2]], color, time);
			}
		}

		private bool CheckRaycastHit (RaycastHit hit) {
			Vector3 hitPoint = hit.point;
			return LOSManager.instance.CheckPointWithinViewingBox(hitPoint);
		}

		private List<Vector2> CalculateUVs (List<Vector3> vertices) {
			List<Vector2> uvs = new List<Vector2>();

			foreach (Vector3 vertex in vertices) {
				float u = vertex.x / LOSManager.instance.halfViewboxSize.x / 2 + 0.5f;
				float v = vertex.y / LOSManager.instance.halfViewboxSize.x / 2 + 0.5f;
				uvs.Add(new Vector2(u, v));
			}
			return uvs;
		}

		private void DeployMesh (List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Color color) {
			Mesh mesh = new Mesh();
			
			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.uv = uvs.ToArray();

			Color32[] colors32 = new Color32[vertices.Count];
			for (int i=0; i<colors32.Length; i++) {
				colors32[i] = color;
			}
			mesh.colors32 = colors32;
			
			_meshFilter.mesh = mesh;
		}
	}
}

