using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {
	
	public class LOSRadialLight : LOSLightBase {

		public float radius = 10;


		protected override void ForwardDraw () {
			List<Vector3> meshVertices = new List<Vector3>();
			List<int> triangles = new List<int>();
			
			bool raycastHitAtStartAngle = false;
			Vector3 direction = Vector3.zero;
			Vector3 previousNormal = Vector3.zero;
			Collider previousCollider = null;
			float distance = radius;
			RaycastHit hit;
			int currentVertexIndex = -1;
			int previousVectexIndex = -1;
			Vector3 previousTempPoint = Vector3.zero;
			
			meshVertices.Add(Vector3.zero);
			
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
							meshVertices.Add(hitPoint - position);
							previousVectexIndex = currentVertexIndex;
							currentVertexIndex = meshVertices.Count - 1;
							AddNewTriangle(ref triangles, 0, currentVertexIndex, previousVectexIndex);
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
						Vector3 farPoint = LOSManager.instance.GetPointForRadius(position, direction, radius);
						
						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
					}
					else if (previousCollider != null) {
						meshVertices.Add(previousTempPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle (ref triangles, 0, currentVertexIndex, previousVectexIndex);
						
						Vector3 farPoint = LOSManager.instance.GetPointForRadius(position, direction, radius);
						
						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle(ref triangles, 0, currentVertexIndex, previousVectexIndex);
					}
					else {
						Vector3 farPoint = LOSManager.instance.GetPointForRadius(position, direction, radius);
						
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
					if (raycastHitAtStartAngle) {
						Vector3 farPoint = LOSManager.instance.GetPointForRadius(position, direction, radius);
						
						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						
					}
					else {
						AddNewTriangle(ref triangles, 0, 1, currentVertexIndex);
					}
				}
				else {
					meshVertices.Add(previousTempPoint - position);
					previousVectexIndex = currentVertexIndex;
					currentVertexIndex = meshVertices.Count - 1;
					AddNewTriangle (ref triangles, 0, currentVertexIndex, previousVectexIndex);
					AddNewTriangle(ref triangles, 0, 1, currentVertexIndex);
				}
			}
			else {
				if (previousCollider == null) {
					Vector3 farPoint = LOSManager.instance.GetPointForRadius(position, direction, radius);
					
					meshVertices.Add(farPoint - position);
					previousVectexIndex = currentVertexIndex;
					currentVertexIndex = meshVertices.Count - 1;
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
	
		protected override void InvertDraw () {
			Debug.LogError("Invert mode is not available in radial light");
		}
	}

}