using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {


	/// <summary>
	/// Draws full screen light.
	/// </summary>
	public class LOSFullScreenLight : LOSLightBase {

		private float _tolerance;


		public override bool CheckDirty () {
			return base.CheckDirty () || _losCamera.CheckDirty();
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
			object previousHitGo = null;
			float distance = GetMinRaycastDistance();
			LOSRaycastHit hit;
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
				
				if (LOSRaycast(direction, out hit, distance) && CheckRaycastHit(hit, 0)) {
					Vector3 hitPoint = hit.point;
					object hitGo = hit.hitGo;
					Vector3 hitNormal = hit.normal;
					
					if (degree == _startAngle) {
						raycastHitAtStartAngle = true;
						meshVertices.Add(hitPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						
					}
					else if (previousHitGo != hitGo) {
						if (previousHitGo == null) {
							Vector3 farPoint = _losCamera.GetCollisionPointWithViewBox(position, direction);
							
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
					
					previousHitGo = hitGo;
					previousTempPoint = hitPoint;
					previousNormal = hitNormal;
				}
				else {
					if (degree == _startAngle) {
						Vector3 farPoint = _losCamera.GetCollisionPointWithViewBox(position, direction);
						
						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
					}
					else if (previousHitGo != null) {
						meshVertices.Add(previousTempPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle (triangles, 0, currentVertexIndex, previousVectexIndex);
						
						Vector3 farPoint = _losCamera.GetCollisionPointWithViewBox(position, direction);
						
						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle(triangles, 0, currentVertexIndex, previousVectexIndex);
						
						
					}
					previousHitGo = null;
					//					previousTempPoint = farPoint;
				}
			}
			
			if (coneAngle == 0) {
				if (previousHitGo == null) {
					if (raycastHitAtStartAngle) {
						Vector3 farPoint = _losCamera.GetCollisionPointWithViewBox(position, direction);
						
						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						
						AddNewTrianglesBetweenPoints4Corners(triangles, meshVertices, previousVectexIndex, currentVertexIndex, 0, true);
					}
					else {
						AddNewTrianglesBetweenPoints4Corners(triangles, meshVertices, currentVertexIndex, 5, 0, true);
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
				if (previousHitGo == null) {
					Vector3 farPoint = _losCamera.GetCollisionPointWithViewBox(position, direction);
					
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
			Vector3 normalAtDegree0 = Vector3.zero;
			GameObject previousHitGo = null;
			GameObject previousPreviousHitGo = null;
			GameObject hitGoAtDegree0 = null;
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
			LOSRaycastHit hit;
			LOSRaycastHit previousHit = new LOSRaycastHit();
			
			invertAngledMeshVertices.Add(Vector3.zero);		// Add the position of the light
			
			// Add the four viewbox corners.
			foreach (Vector3 corner in LOSManager.instance.viewboxCorners) {
				invertAngledMeshVertices.Add(corner - position);
			}
			
			for (float degree = _startAngle; degree<_endAngle; degree+=degreeStep) {
				Vector3 direction;
				
				direction = SMath.DegreeToUnitVector(degree);
				
				if (degree == _startAngle) {
					firstFarPoint = _losCamera.GetCollisionPointWithViewBox(position, direction);
					invertAngledMeshVertices.Add(firstFarPoint - position);
					firstFarPointIndex = invertAngledMeshVertices.Count - 1;
				}
				else if (degree + degreeStep >= _endAngle) {		// degree == _endAngle - degreeStep
					lastFarPoint = _losCamera.GetCollisionPointWithViewBox(position, direction);
					invertAngledMeshVertices.Add(lastFarPoint - position);
					lastFarPointIndex = invertAngledMeshVertices.Count - 1;
				}
				
				if (LOSRaycast(direction, out hit, _raycastDistance) && CheckRaycastHit(hit, 0)) {		// Hit a collider.
					Vector3 hitPoint = hit.point;
					GameObject hitGo = hit.hitGo;
					Vector3 hitNormal = hit.normal;
					
					if (degree == _startAngle) {
						farPointIndex = firstFarPointIndex;
						
						invertAngledMeshVertices.Add(hitPoint - position);
						previousClosePointIndex = closePointIndex;
						closePointIndex = invertAngledMeshVertices.Count - 1;
						
						colliderClosePointCount++;
						
						if (degree == 0) {
							closePointAtDegree0Index = closePointIndex;
							hitGoAtDegree0 = hitGo;
							normalAtDegree0 = hitNormal;
						}
					}
					else if (degree + degreeStep >= _endAngle) {
						previousFarPointIndex = farPointIndex;
						farPointIndex = lastFarPointIndex;
						
						if (previousHitGo != hitGo && null != previousHitGo) {
							colliderClosePointCount = 1;
							//							
							if (_startAngle == 0 && _endAngle == 360) {
								invertAngledMeshVertices.Add(previousCloseTempPoint - position);
								previousClosePointIndex = closePointIndex;
								closePointIndex = invertAngledMeshVertices.Count - 1;

								if (closePointAtDegree0Index != -1) {
									AddNewTriangle(invertAngledTriangles, closePointIndex, closePointAtDegree0Index, firstFarPointIndex);
								}
								AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, firstFarPointIndex);
								AddNewTrianglesBetweenPoints4Corners(invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, firstFarPointIndex, previousClosePointIndex);
							}
						}
						else if (previousNormal != hitNormal) {
							invertAngledMeshVertices.Add(hitPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;
							
//							AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);

							previousPreviousHitGo = previousHitGo;
							
//							Vector3 previouseClosePointTolerated = GetToleratedHitPointColliderOrNormalChange(previousDirection, previousHit, direction, hit);
//							invertAngledMeshVertices.Add(previouseClosePointTolerated - _trans.position);
//							previousClosePointIndex = closePointIndex;
//							closePointIndex = invertAngledMeshVertices.Count - 1;
						
//							AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);
						}
						else {
							invertAngledMeshVertices.Add(hitPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;
							
							colliderClosePointCount++;
						}
					}
					else {
						if (null == previousHitGo) {
							Vector3 farPoint = _losCamera.GetCollisionPointWithViewBox(position, previousDirection);
							invertAngledMeshVertices.Add(farPoint - position);
							previousFarPointIndex = farPointIndex;
							farPointIndex = invertAngledMeshVertices.Count - 1;

							hitPoint = GetToleratedHitPointColliderInOut(previousDirection, hit);
		
							invertAngledMeshVertices.Add(hitPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;
							
							colliderClosePointCount++;
						}
						else if (previousHitGo != hitGo) {
							colliderClosePointCount = 1;
							
							Vector3 farPoint = _losCamera.GetCollisionPointWithViewBox(position, direction);
							invertAngledMeshVertices.Add(farPoint - position);
							previousFarPointIndex = farPointIndex;
							farPointIndex = invertAngledMeshVertices.Count - 1;

							invertAngledMeshVertices.Add(previousCloseTempPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;
			
							AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);

							Vector3 previouseClosePointTolerated = GetToleratedHitPointColliderOrNormalChange(previousDirection, previousHit, direction, hit);
							invertAngledMeshVertices.Add(previouseClosePointTolerated - position);
							int previouseClosePointToleratedIndex = invertAngledMeshVertices.Count - 1;
							
							invertAngledMeshVertices.Add(hitPoint - position);
							previousClosePointIndex = closePointIndex;
							closePointIndex = invertAngledMeshVertices.Count - 1;

							AddNewTriangle(invertAngledTriangles, previousClosePointIndex, previouseClosePointToleratedIndex, closePointIndex);
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
								int closePointIndexBeforeNormalChange = closePointIndex;

								Vector3 toleratedHitpoint = GetToleratedHitPointColliderOrNormalChange(previousDirection, previousHit, direction, hit);
								invertAngledMeshVertices.Add(toleratedHitpoint - position);
								previousClosePointIndex = closePointIndex;
								closePointIndex = invertAngledMeshVertices.Count - 1;

								invertAngledMeshVertices.Add(hitPoint - position);
								previousClosePointIndex = closePointIndex;
								closePointIndex = invertAngledMeshVertices.Count - 1;
								AddNewTriangle(invertAngledTriangles, closePointIndexBeforeNormalChange, closePointIndex, farPointIndex);

								AddNewTriangle(invertAngledTriangles, closePointIndexBeforeNormalChange, previousClosePointIndex, closePointIndex);
							}
							
							colliderClosePointCount++;
						}
					}
					
					previousHitGo = hitGo;
					previousCloseTempPoint = hitPoint;
					previousNormal = hitNormal;
				}
				else {
					if (null != previousHitGo) {
						colliderClosePointCount = 0;

						Vector3 previousCloseTempPointTolerated = GetToleratedHitPointColliderInOut(direction, previousHit);
						
						invertAngledMeshVertices.Add(previousCloseTempPointTolerated - position);
						previousClosePointIndex = closePointIndex;
						closePointIndex = invertAngledMeshVertices.Count - 1;
						AddNewTriangle(invertAngledTriangles, closePointIndex, farPointIndex, previousClosePointIndex);
						
						Vector3 farPoint = _losCamera.GetCollisionPointWithViewBox(position, direction);
						invertAngledMeshVertices.Add(farPoint - position);
						previousFarPointIndex = farPointIndex;
						farPointIndex = invertAngledMeshVertices.Count - 1;
						
						Vector3 previousFarPoint = invertAngledMeshVertices[previousFarPointIndex] + position;
						Vector3 closePoint = invertAngledMeshVertices[closePointIndex] + position;
						List<Vector3> corners = _losCamera.GetViewboxCornersBetweenPoints(previousFarPoint, closePoint, position, false);
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
					
					previousHitGo = null;
				}
				previousDirection = direction;
				previousHit = hit;
			}
			
			if (_startAngle == 0 && _endAngle == 360) {
				if (previousHitGo != null) {
					if (closePointAtDegree0Index == -1) {
						Vector3 degree0FarPoint = _losCamera.GetCollisionPointWithViewBox(position, new Vector3(1, 0, 0));
						invertAngledMeshVertices.Add(degree0FarPoint - position);
						int degree0FarPointIndex = invertAngledMeshVertices.Count - 1;
					
						AddNewTrianglesBetweenPoints4Corners(invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, degree0FarPointIndex, closePointIndex);
						AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);
					}
					else {
						if (colliderClosePointCount > 1) {
//							AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);
							AddNewTriangle(invertAngledTriangles, closePointIndex, closePointAtDegree0Index, firstFarPointIndex);
							AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);
							AddNewTrianglesBetweenPoints4Corners(invertAngledTriangles, invertAngledMeshVertices, previousFarPointIndex, firstFarPointIndex, closePointIndex);
						}
						else {
							if (closePointAtDegree0Index != -1 && 
							    hitGoAtDegree0 == previousHitGo &&
							    previousPreviousHitGo == previousHitGo &&
							    normalAtDegree0 == previousNormal) {
								AddNewTriangle(invertAngledTriangles, previousClosePointIndex, closePointIndex, previousFarPointIndex);
								AddNewTriangle(invertAngledTriangles, closePointIndex, closePointAtDegree0Index, firstFarPointIndex);
								AddNewTriangle(invertAngledTriangles, closePointIndex, firstFarPointIndex, previousFarPointIndex);
							}
						}
					}
				}
			}
			else {	// Add triangles between outside of view.
				AddNewTrianglesBetweenPoints4Corners(invertAngledTriangles, invertAngledMeshVertices, lastFarPointIndex, firstFarPointIndex, 0);
				
				if (previousHitGo != null) {
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
		private Vector3 GetToleratedHitPointColliderInOut (Vector3 targetDirection, LOSRaycastHit hit) {
			return _trans.position + hit.distance * targetDirection.normalized;
		}

		private Vector3 GetToleratedHitPointColliderOrNormalChange (Vector3 direction1, LOSRaycastHit hit1, Vector3 direction2, LOSRaycastHit hit2) {
			float sqrDistance1 = (hit1.point - position).sqrMagnitude;
			float sqrDistance2 = (hit2.point - position).sqrMagnitude;

			if (sqrDistance1 > sqrDistance2) {
				return GetToleratedHitPointColliderInOut(direction1, hit2);
			}
			else {
				return GetToleratedHitPointColliderInOut(direction2, hit1);
			}
		}


		// Not used
		private Vector3 GetToleranceHitPointNormalChange (Vector3 previousNormal, Vector3 hitpoint, Vector3 previousHitpoint) {
			float delta = Mathf.Min(Mathf.Abs(previousHitpoint.x - hitpoint.x), Mathf.Abs(previousHitpoint.y - hitpoint.y));
			return hitpoint + previousNormal.normalized * delta;
		}
	}

}