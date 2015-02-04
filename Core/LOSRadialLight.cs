using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	/// <summary>
	/// Draws radial light.
	/// </summary>
	public class LOSRadialLight : LOSLightBase {

		// Radius
		public float radius = 5;

		private float _previousRadius;
		private float _radius;


		// Flash
		[Tooltip("How frequent the light flashes. Measured by times / second. Should be positive")]
		public int flashFrequency = 0;
		[Tooltip("How much will the light change by during flash. Should be positive")]
		public float flashOffset = 0;

		private float _timeFromLastFlash;

		public float timeFromLastFlash {
			get {
				return _timeFromLastFlash;
			}
			set {
				if (!Application.isPlaying) {
					_timeFromLastFlash = value;
				}
				else {
					// Not supposed to use this API in play mode.
				}
			}
		}


		protected override void Awake () {
			base.Awake ();

			_radius = radius;
		}

		void Update () {
			if (flashFrequency > 0 && flashOffset > 0) {
					_timeFromLastFlash += Time.deltaTime;

				if (_timeFromLastFlash > 1f / flashFrequency) {		// flashFrequency is int
					_timeFromLastFlash = 0;
					_radius = Random.Range(radius - flashOffset, radius + flashOffset);
				}
			}
		}

		public override bool CheckDirty () {
			bool withinScreen = SHelper.CheckWithinScreen(position, LOSManager.instance.losCamera.unityCamera, _radius);
			return withinScreen && ((base.CheckDirty () || _radius != _previousRadius || _radius != radius) && radius > 0);
		}

		public override void UpdatePreviousInfo () {
			base.UpdatePreviousInfo ();

			_previousRadius = _radius;
			if (flashFrequency <= 0 || flashOffset <= 0) {
				_radius = radius;
			}
		}

		protected override float GetMaxLightLength () {
			return _radius;
		}

		protected override void ForwardDraw () {
			List<Vector3> meshVertices = new List<Vector3>();
			List<int> triangles = new List<int>();
			
			bool raycastHitAtStartAngle = false;
			Vector3 direction = Vector3.zero;
			Vector3 previousNormal = Vector3.zero;
			Collider previousCollider = null;
			float distance = _radius;
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
							AddNewTriangle(triangles, 0, currentVertexIndex, previousVectexIndex);
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
						Vector3 farPoint = LOSManager.instance.GetPointForRadius(position, direction, distance);
						
						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
					}
					else if (previousCollider != null) {
						meshVertices.Add(previousTempPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle (triangles, 0, currentVertexIndex, previousVectexIndex);
						
						Vector3 farPoint = LOSManager.instance.GetPointForRadius(position, direction, distance);
						
						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle(triangles, 0, currentVertexIndex, previousVectexIndex);
					}
					else {
						Vector3 farPoint = LOSManager.instance.GetPointForRadius(position, direction, distance);
						
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
						Vector3 farPoint = LOSManager.instance.GetPointForRadius(position, direction, distance);
						
						meshVertices.Add(farPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						AddNewTriangle(triangles, 0, 1, currentVertexIndex);
					}
					else {
						AddNewTriangle(triangles, 0, 1, currentVertexIndex);
					}
				}
				else {
					meshVertices.Add(previousTempPoint - position);
					previousVectexIndex = currentVertexIndex;
					currentVertexIndex = meshVertices.Count - 1;
					AddNewTriangle (triangles, 0, currentVertexIndex, previousVectexIndex);
					AddNewTriangle(triangles, 0, 1, currentVertexIndex);
				}
			}
			else {
				if (previousCollider == null) {
					Vector3 farPoint = LOSManager.instance.GetPointForRadius(position, direction, distance);
					
					meshVertices.Add(farPoint - position);
					previousVectexIndex = currentVertexIndex;
					currentVertexIndex = meshVertices.Count - 1;
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
			Debug.LogError("Invert mode is not available in radial light");
		}
	}

}