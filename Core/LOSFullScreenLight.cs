using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	public class LOSFullScreenLight : LOSLightBase {

		private float _tolerance;


		public override bool CheckDirty () {
			return base.CheckDirty () || LOSManager.instance.losCamera.CheckDirty();
		}

		protected override float GetMaxLightLength () {
			return LOSManager.instance.halfViewboxSize.x;
		}

		protected override void ForwardDraw () {
			List<Vector3> meshVertices = new List<Vector3>();
			List<int> triangles = new List<int>();
			
			bool raycastHitAtStartAngle = false;
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
						raycastHitAtStartAngle = true;
						meshVertices.Add(hitPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						
					}
					else if (previousCollider != hit.collider) {
						if (previousCollider == null) {
							Vector3 farPoint = LOSManager.instance.GetCollisionPointWithViewBox(position, direction);
							
							meshVertices.Add(farPoint - position);
							previousVectexIndex = currentVertexIndex;
							currentVertexIndex = meshVertices.Count - 1;
							
							AddNewTrianglesBetweenPoints4Corners(triangles, meshVertices, previousVectexIndex, currentVertexIndex, 0);
							
							meshVertices.Add(hitPoint - position);
							previousVectexIndex = currentVertexIndex;
							currentVertexIndex = meshVertices.Count - 1;
						}
						else {
							meshVertices.Add(previousTempPoint - position);
							previousVectexIndex = currentVertexIndex;
							currentVertexIndex = meshVertices.Count - 1;
							AddNewTriangle (triangles, 0, currentVertexIndex, previousVectexIndex);
							
							meshVertices.Add(hitPoint - position);
							previousVectexIndex = currentVertexIndex;
							currentVertexIndex = meshVertices.Count - 1;
							AddNewTriangle (triangles, 0, currentVertexIndex, previousVectexIndex);
						}
					}
					else if (previousNormal != hitNormal) {
						meshVertices.Add(previousTempPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle (triangles, 0, currentVertexIndex, previousVectexIndex);
						
						meshVertices.Add(hitPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle (triangles, 0, currentVertexIndex, previousVectexIndex);
					}
					
					previousCollider = hitCollider;
					previousTempPoint = hitPoint;
					previousNormal = hitNormal;
				}
				else {
					if (degree == _startAngle) {
						Vector3 farPoint = LOSManager.instance.GetCollisionPointWithViewBox(position, direction);
						
						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
					}
					else if (previousCollider != null) {
						meshVertices.Add(previousTempPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle (triangles, 0, currentVertexIndex, previousVectexIndex);
						
						Vector3 farPoint = LOSManager.instance.GetCollisionPointWithViewBox(position, direction);
						
						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle(triangles, 0, currentVertexIndex, previousVectexIndex);
						
						
					}
					previousCollider = null;
					//					previousTempPoint = farPoint;
				}
			}
			
			if (coneAngle == 0) {
				if (previousCollider == null) {
					if (raycastHitAtStartAngle) {
						Vector3 farPoint = LOSManager.instance.GetCollisionPointWithViewBox(position, direction);
						
						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						
						AddNewTrianglesBetweenPoints4Corners(triangles, meshVertices, previousVectexIndex, currentVertexIndex, 0);
					}
					else {
						AddNewTrianglesBetweenPoints4Corners(triangles, meshVertices, currentVertexIndex, 5, 0);
					}
				}
				else {
					meshVertices.Add(previousTempPoint - position);
					previousVectexIndex = currentVertexIndex;
					currentVertexIndex = meshVertices.Count - 1;
					AddNewTriangle (triangles, 0, currentVertexIndex, previousVectexIndex);
					AddNewTriangle(triangles, 0, 5, currentVertexIndex);
				}
			}
			else {
				if (previousCollider == null) {
					Vector3 farPoint = LOSManager.instance.GetCollisionPointWithViewBox(position, direction);
					
					meshVertices.Add(farPoint - position);
					previousVectexIndex = currentVertexIndex;
					currentVertexIndex = meshVertices.Count - 1;
					
					AddNewTrianglesBetweenPoints4Corners(triangles, meshVertices, previousVectexIndex, currentVertexIndex, 0);
				}
				else {
					meshVertices.Add(previousTempPoint - position);
					previousVectexIndex = currentVertexIndex;
					currentVertexIndex = meshVertices.Count - 1;
					AddNewTriangle(triangles, 0, currentVertexIndex, previousVectexIndex);
				}
			}
			
			DeployMesh(meshVertices, triangles);
		}

		protected override void InvertDraw () {
			List<Vector3> invertAngledMeshVertices = new List<Vector3>();
			List<int> invertAngledTriangles = new List<int>();
			
			Vector3 previousNormal = Vector3.zero;
			Collider previousCollider = null;
			Vector3 firstFarPoint = Vector3.zero;
			Vector3 lastFarPoint = Vector3.zero;
			Vector3 previousCloseTempPoint = Vector3.zero;
			Vector3 previousDirection = Vector3.zero;
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
					firstFarPoint = LOSManager.instance.GetCollisionPointWithViewBox(position, direction);
					invertAngledMeshVertices.Add(firstFarPoint - position);
					firstFarPointIndex = invertAngledMeshVertices.Count - 1;
				}
				else if (degree + degreeStep >= _endAngle) {		// degree == _endAngle - degreeStep
					lastFarPoint = LOSManager.instance.GetCollisionPointWithViewBox(position, direction);
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
						
						if (previousCollider != hitCollider && null != previousCollider) {
							colliderClosePointCount = 1;
							//							
							if (_startAngle == 0 && _endAngle == 360) {
								invertAngledMeshVertices.Add(previousCloseTempPoint - position);
								previousClosePointIndex = closePointIndex;
								closePointIndex = invertAngledMeshVertices.Count - 1;
								
								AddNewTriangle(invertAngledTriangles, closePointIndex, closePointAtDegree0Index, firstFarPointIndex);
								AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, firstFarPointIndex);
								AddNewTrianglesBetweenPoints4Corners(invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, firstFarPointIndex, previousClosePointIndex);
							}
						}
						else {
							invertAngledMeshVertices.Add(hitPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;
							
							colliderClosePointCount++;
						}
					}
					else {
						if (null == previousCollider) {
							Vector3 farPoint = LOSManager.instance.GetCollisionPointWithViewBox(position, previousDirection);
							invertAngledMeshVertices.Add(farPoint - position);
							previousFarPointIndex = farPointIndex;
							farPointIndex = invertAngledMeshVertices.Count - 1;

							hitPoint = GetToleratedHitPointColliderInOut(previousDirection, hit);
		
							invertAngledMeshVertices.Add(hitPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;
							
							colliderClosePointCount++;
						}
						else if (previousCollider != hitCollider) {
							colliderClosePointCount = 1;
							
							Vector3 farPoint = LOSManager.instance.GetCollisionPointWithViewBox(position, direction);
							invertAngledMeshVertices.Add(farPoint - position);
							previousFarPointIndex = farPointIndex;
							farPointIndex = invertAngledMeshVertices.Count - 1;
							
							invertAngledMeshVertices.Add(previousCloseTempPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;
							
							AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);
							
							invertAngledMeshVertices.Add(hitPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;
							
							AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, farPointIndex);
							
							Vector3 previousClosePoint = invertAngledMeshVertices[previousClosePointIndex] + position;
							Vector3 previousFarPoint = invertAngledMeshVertices[previousFarPointIndex] + position;
							Vector3 previousClosePointToFar = farPoint - previousClosePoint;
							Vector3 previousCloseToPreviousFar = previousFarPoint - previousClosePoint;
							
							if (SMath.GetDegreeBetweenIndexVector(previousCloseToPreviousFar.normalized, previousClosePointToFar.normalized) < 0) {	// left
								AddNewTriangle(invertAngledTriangles, previousFarPointIndex, previousClosePointIndex, farPointIndex);
								AddNewTrianglesBetweenPoints2Corners(invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, farPointIndex);
							}
							else {	// right
								AddNewTrianglesBetweenPoints2Corners(invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, previousClosePointIndex);
							}
						}
						else {
							if (previousNormal != hitNormal) {
								invertAngledMeshVertices.Add(previousCloseTempPoint - position);
								previousClosePointIndex = closePointIndex;
								closePointIndex = invertAngledMeshVertices.Count - 1;
								AddNewTriangle(invertAngledTriangles, closePointIndex, farPointIndex, previousClosePointIndex);

//								if (SMath.GetDegreeBetweenIndexVector(previousNormal.normalized, hitNormal.normalized) > 0) {
//									hitPoint = GetToleranceHitPointNormalChange(previousNormal, hitPoint, previousCloseTempPoint);
//								}

								invertAngledMeshVertices.Add(hitPoint - position);
								previousClosePointIndex = closePointIndex;
								closePointIndex = invertAngledMeshVertices.Count - 1;
								AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, farPointIndex);
								
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
						AddNewTriangle(invertAngledTriangles, closePointIndex, farPointIndex, previousClosePointIndex);
						
						Vector3 farPoint = LOSManager.instance.GetCollisionPointWithViewBox(position, direction);
						invertAngledMeshVertices.Add(farPoint - position);
						previousFarPointIndex = farPointIndex;
						farPointIndex = invertAngledMeshVertices.Count - 1;
						
						Vector3 previousFarPoint = invertAngledMeshVertices[previousFarPointIndex] + position;
						Vector3 closePoint = invertAngledMeshVertices[closePointIndex] + position;
						List<Vector3> corners = LOSManager.instance.GetViewboxCornersBetweenPoints(previousFarPoint, closePoint, position);
						switch (corners.Count) {
						case 0:
							AddNewTriangle(invertAngledTriangles, closePointIndex, farPointIndex, previousFarPointIndex);
							break;
						case 1:
							int cornerIndex0 = GetCorrectCornerIndex(invertAngledMeshVertices, corners[0], 1);
							AddNewTriangle(invertAngledTriangles, closePointIndex, cornerIndex0, previousFarPointIndex);
							AddNewTriangle(invertAngledTriangles, closePointIndex, farPointIndex, cornerIndex0);
							break;
						case 2:
							cornerIndex0 = GetCorrectCornerIndex(invertAngledMeshVertices, corners[0], 1);
							int cornerIndex1 = GetCorrectCornerIndex(invertAngledMeshVertices, corners[1], 1);
							AddNewTriangle(invertAngledTriangles, closePointIndex, cornerIndex0, previousFarPointIndex);
							AddNewTriangle(invertAngledTriangles, closePointIndex, cornerIndex1, cornerIndex0);
							AddNewTriangle(invertAngledTriangles, closePointIndex, farPointIndex, cornerIndex1);
							break;
						}
					}
					
					previousCollider = null;
				}
				previousDirection = direction;
			}
			
			
			
			if (_startAngle == 0 && _endAngle == 360) {
				if (previousCollider != null) {
					if (closePointAtDegree0Index == -1) {
						AddNewTrianglesBetweenPoints4Corners(invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, lastFarPointIndex, closePointIndex);
					}
					else {
						if (colliderClosePointCount > 1) {
							AddNewTriangle(invertAngledTriangles, closePointIndex, closePointAtDegree0Index, firstFarPointIndex);
							AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);
							AddNewTrianglesBetweenPoints4Corners(invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, firstFarPointIndex, closePointIndex);
						}
						else {
							// NOTE: Fixed at last far point
						}
					}
				}
			}
			else {	// Add triangles between outside of view.
				AddNewTrianglesBetweenPoints4Corners(invertAngledTriangles, invertAngledMeshVertices, lastFarPointIndex, firstFarPointIndex, 0);
				
				if (previousCollider != null) {
					if (colliderClosePointCount >= 2) {
						AddNewTrianglesBetweenPoints4Corners(invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, lastFarPointIndex, closePointIndex);
						AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);
					}
					else if (previousFarPointIndex >= 0){
						AddNewTrianglesBetweenPoints4Corners(invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, lastFarPointIndex, previousClosePointIndex);
						AddNewTriangle(invertAngledTriangles, closePointIndex, lastFarPointIndex, previousClosePointIndex);
					}
				}
			}
			
			DeployMesh(invertAngledMeshVertices, invertAngledTriangles);
		}


		/// <summary>
		/// This function is used to help the shadow tocover the colliders better
		/// </summary>
		/// <returns>The hitpoint after calculation. </returns>
		/// <param name="targetDirection">Target direction.</param>
		/// <param name="hit">Hit.</param>
		private Vector3 GetToleratedHitPointColliderInOut (Vector3 targetDirection, RaycastHit hit) {
			return _trans.position + hit.distance * targetDirection.normalized;
		}


		// Not used
		private Vector3 GetToleranceHitPointNormalChange (Vector3 previousNormal, Vector3 hitpoint, Vector3 previousHitpoint) {
			float delta = Mathf.Min(Mathf.Abs(previousHitpoint.x - hitpoint.x), Mathf.Abs(previousHitpoint.y - hitpoint.y));
			return hitpoint + previousNormal.normalized * delta;
		}
	}

}