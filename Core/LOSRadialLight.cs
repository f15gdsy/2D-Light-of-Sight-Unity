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

		// Cache
		private bool _withinScreen;


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
			bool withinScreen = SHelper.CheckWithinScreen(position, _losCamera.unityCamera, _radius) || !Application.isPlaying;
			bool dirty = withinScreen && ((base.CheckDirty () || _radius != _previousRadius || _radius != radius) && radius > 0);
			dirty = dirty || (withinScreen && !_withinScreen);
			_withinScreen = withinScreen;
			return dirty;
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
			object previousHitGo = null;
			float distance = _radius;
			LOSRaycastHit hit;
			int currentVertexIndex = -1;
			int previousVectexIndex = -1;
			Vector3 previousTempPoint = Vector3.zero;
			
			meshVertices.Add(Vector3.zero);
			
			for (float degree=_startAngle; degree<_endAngle; degree+=degreeStep) {
				direction = SMath.DegreeToUnitVector(degree);

				float distanceForCheck = Application.isPlaying ? _radius : float.MaxValue - 100;	// 100000 for big number

				if (LOSRaycast(direction, out hit, distance) && CheckRaycastHit(hit, distanceForCheck)) {
					Vector3 hitPoint = hit.point;
					GameObject hitGo = hit.hitGo;
					Vector3 hitNormal = hit.normal;
					
					if (degree == _startAngle) {
						raycastHitAtStartAngle = true;
						meshVertices.Add(hitPoint - position);
						previousVectexIndex = currentVertexIndex;
						currentVertexIndex = meshVertices.Count - 1;
						
					}
					else if (previousHitGo != hitGo) {
						if (previousHitGo == null) {
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
					
					previousHitGo = hitGo;
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
					else if (previousHitGo != null) {
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
					previousHitGo = null;
					//					previousTempPoint = farPoint;
				}
			}
			
			if (coneAngle == 0) {
				if (previousHitGo == null) {
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
				if (previousHitGo == null) {
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