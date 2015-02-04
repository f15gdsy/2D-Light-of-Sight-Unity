using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {
	
	/// <summary>
	/// Base class for lights.
	/// </summary>
	public abstract class LOSLightBase : LOSObjectBase {

		// Cone Light
		[Tooltip("The angle of the light cone. 0 means every direction.")]
		public float coneAngle = 0;

		[Tooltip("The direction that the light is facing. Measured in degrees, counting from direction (1, 0)")]
		public float faceAngle = 0;

		protected float _previousFaceAngle;
		protected float _previousLightAngle;
		protected float _startAngle;
		protected float _endAngle;


		// Light Settings
		[Tooltip("The precision of the light collision test. Measured in degrees.")]
		public float degreeStep = 1f;

		[Tooltip("Draws the light in invert mode or not.")]
		public bool invertMode = false;

		[Tooltip("The layers that the light will interact with.")]
		public LayerMask obstacleLayer;

		protected float _raycastDistance;


		// Outlook
		public Color color = Color.yellow;
		public Material material;
		public int orderInLayer;
		public int sortingLayer;

		protected Color _previousColor;
		protected int _previousOrderInLayer;
		protected int _previousSortingLayer;


		// Cache
		protected Mesh _mesh;
		protected LayerMask _previousObstacleLayer;
		protected bool _previousInvertMode;
		protected float _previousDegreeStep;



		protected override void Awake () {
			base.Awake();

			coneAngle = SMath.ClampDegree0To360(coneAngle);

			var meshFilter = GetComponent<MeshFilter>();
			if (meshFilter == null) {
				meshFilter = gameObject.AddComponent<MeshFilter>();
			}

			if (_mesh == null) {
				_mesh = new Mesh();
				meshFilter.mesh = _mesh;
			}
			
			Vector2 screenSize = SHelper.GetScreenSizeInWorld();
			_raycastDistance = Mathf.Sqrt(screenSize.x*screenSize.x + screenSize.y*screenSize.y);
		}

		protected virtual void OnEnable () {
			LOSManager.instance.AddLight(this);
		}

		protected virtual void OnDisable () {
			if (LOSManager.TryGetInstance() != null) {
				LOSManager.instance.RemoveLight(this);
			}
		}


		void Start () {
			if (renderer == null) {
				gameObject.AddComponent<MeshRenderer>();
			}

			UpdateSortingOrder();
			UpdateSortingLayer();
			renderer.material = material;

			TryDraw();
		}


		public void TryDraw () {
			if (LOSManager.instance.CheckDirty() || CheckDirty()) {
				UpdatePreviousInfo();
				DoDraw();
			}
			if (CheckColorDirty()) {
				UpdateColor();
			}
			if (CheckSortingLayerDirty()) {
				UpdateSortingLayer();
			}
			if (CheckRenderQueueDirty()) {
				UpdateSortingOrder();
			}
		}


		protected virtual void ForwardDraw () {}
		
		protected virtual void InvertDraw () {}
		
		protected virtual float GetMaxLightLength () {return 0;}


		public override void UpdatePreviousInfo () {
			base.UpdatePreviousInfo ();

			_previousFaceAngle = faceAngle;
			_previousLightAngle = coneAngle;
			_previousColor = color;
			_previousObstacleLayer = obstacleLayer;
			_previousInvertMode = invertMode;
			_previousDegreeStep = degreeStep;
		}

		public override bool CheckDirty () {
			return !isStatic && (!_previousPosition.Equals(position) || 
				!_previousFaceAngle.Equals(faceAngle) || 
					!_previousLightAngle.Equals(coneAngle)) ||
				_previousObstacleLayer != obstacleLayer ||
					_previousInvertMode != invertMode || 
					_previousDegreeStep != degreeStep;
		}

		private bool CheckColorDirty () {
			return _previousColor != color;
		}

		private bool CheckSortingLayerDirty () {
			return _previousSortingLayer != sortingLayer;
		}

		private bool CheckRenderQueueDirty () {
			return _previousOrderInLayer != orderInLayer;
		}

		private void DoDraw () {
			_startAngle = coneAngle == 0 ? 0 : faceAngle - coneAngle / 2;
			_endAngle = coneAngle == 0 ? 360 : faceAngle + coneAngle / 2;

			if (invertMode) {
				InvertDraw();
			}
			else {
				ForwardDraw();
			}
		}

		protected void AddNewTrianglesBetweenPoints2Corners (List<int> triangles, List<Vector3> vertices, int pointAIndex, int pointBIndex, bool give4CornersWhenAEqualsB = false) {
			Vector3 pointA = vertices[pointAIndex] + position;
			Vector3 pointB = vertices[pointBIndex] + position;
			
			List<Vector3> corners = LOSManager.instance.GetViewboxCornersBetweenPoints(pointA, pointB, position, give4CornersWhenAEqualsB);
			switch (corners.Count) {
			case 0:
				break;
			case 1:
				int cornerIndex0 = GetCorrectCornerIndex(vertices, corners[0], 1);
				AddNewTriangle(triangles, pointAIndex, pointBIndex, cornerIndex0);
				break;
			case 2:
				cornerIndex0 = GetCorrectCornerIndex(vertices, corners[0], 1);
				int cornerIndex1 = GetCorrectCornerIndex(vertices, corners[1], 1);
				AddNewTriangle(triangles, pointAIndex, pointBIndex, cornerIndex1);
				AddNewTriangle(triangles, pointAIndex, cornerIndex1, cornerIndex0);
				break;
			default:
				Debug.LogError("LOS Light: Invalid number of corners: " + corners.Count);
				break;
			}
		}

		protected void AddNewTrianglesBetweenPoints4Corners (List<int> triangles, List<Vector3> vertices,int pointAIndex, int pointBIndex, int centerIndex, bool give4CornersWhenAEqualB = false) {
			Vector3 pointA = vertices[pointAIndex] + position;
			Vector3 pointB = vertices[pointBIndex] + position;

			List<Vector3> corners = LOSManager.instance.GetViewboxCornersBetweenPoints(pointA, pointB, position, give4CornersWhenAEqualB);
			switch (corners.Count) {
			case 0:
				AddNewTriangle(triangles, pointAIndex, centerIndex, pointBIndex);
				break;
			case 1:
				int cornerIndex0 = GetCorrectCornerIndex(vertices, corners[0], 1);
				AddNewTriangle(triangles, pointAIndex, centerIndex, pointBIndex);
				AddNewTriangle(triangles, pointAIndex, pointBIndex, cornerIndex0);
				break;
			case 2:
				cornerIndex0 = GetCorrectCornerIndex(vertices, corners[0], 1);
				int cornerIndex1 = GetCorrectCornerIndex(vertices, corners[1], 1);
				AddNewTriangle(triangles, pointAIndex, centerIndex, cornerIndex0);
				AddNewTriangle(triangles, centerIndex, cornerIndex1, cornerIndex0);
				AddNewTriangle(triangles, cornerIndex1, centerIndex, pointBIndex);
				break;
			case 3:
				cornerIndex0 = GetCorrectCornerIndex(vertices, corners[0], 1);
				cornerIndex1 = GetCorrectCornerIndex(vertices, corners[1], 1);
				int cornerIndex2 = GetCorrectCornerIndex(vertices, corners[2], 1);
				AddNewTriangle(triangles, pointAIndex, centerIndex, cornerIndex0);
				AddNewTriangle(triangles, centerIndex, cornerIndex1, cornerIndex0);
				AddNewTriangle(triangles, centerIndex, cornerIndex2, cornerIndex1);
				AddNewTriangle(triangles, cornerIndex2, centerIndex, pointBIndex);
				break;
			case 4:
				cornerIndex0 = GetCorrectCornerIndex(vertices, corners[0], 1);
				cornerIndex1 = GetCorrectCornerIndex(vertices, corners[1], 1);
				cornerIndex2 = GetCorrectCornerIndex(vertices, corners[2], 1);
				int cornerIndex3 = GetCorrectCornerIndex(vertices, corners[3], 1);
				AddNewTriangle(triangles, pointAIndex, centerIndex, cornerIndex0);
				AddNewTriangle(triangles, centerIndex, cornerIndex1, cornerIndex0);
				AddNewTriangle(triangles, centerIndex, cornerIndex2, cornerIndex1);
				AddNewTriangle(triangles, centerIndex, cornerIndex3, cornerIndex2);
				AddNewTriangle(triangles, cornerIndex3, centerIndex, pointBIndex);
				break;
			default:
				Debug.LogError("LOS Light: Invalid number of corners: " + corners.Count);
				break;
			}
		}

		protected float GetMinRaycastDistance () {
			Vector3 screenSize = SHelper.GetScreenSizeInWorld();
			return screenSize.magnitude;
		}

		protected void AddNewTriangle (List<int> triangles, int v0, int v1, int v2) {
			triangles.Add(v0);
			triangles.Add(v1);
			triangles.Add(v2);
		}

		protected int GetCorrectCornerIndex (List<Vector3> vertices, Vector3 corner, int cornersStartIndex) {
			for (int i=cornersStartIndex; i<cornersStartIndex+4; i++) {
				if (vertices[i] == corner - position) {
					return i;
				}
			}
			return -1;
		}

		protected void DebugDraw (List<Vector3> meshVertices, List<int> triangles, Color color, float time) {
			for (int i=0; i<triangles.Count; i++) {
				Debug.DrawLine(meshVertices[triangles[i]], meshVertices[triangles[++i]], color, time);
				Debug.DrawLine(meshVertices[triangles[i]], meshVertices[triangles[++i]], color, time);
				Debug.DrawLine(meshVertices[triangles[i]], meshVertices[triangles[i-2]], color, time);
			}
		}

		protected bool CheckRaycastHit (RaycastHit hit) {
			Vector3 hitPoint = hit.point;
			return LOSManager.instance.CheckPointWithinViewingBox(hitPoint);
		}

		protected void UpdateColor () {
			_previousColor = color;

			Color32[] colors32 = new Color32[_mesh.vertices.Length];
			for (int i=0; i<colors32.Length; i++) {
				colors32[i] = color;
			}
			_mesh.colors32 = colors32;
		}

		protected void UpdateSortingLayer () {
			renderer.sortingLayerID = sortingLayer;
			_previousSortingLayer = sortingLayer;
		}

		protected void UpdateSortingOrder () {
			renderer.sortingOrder = orderInLayer;
			_previousOrderInLayer = orderInLayer;
		}

		protected void UpdateUVs (Vector3[] vertices) {
			Vector2[] uvs = new Vector2[vertices.Length];

			for (int i=0; i<uvs.Length; i++) {
				float u = vertices[i].x / GetMaxLightLength() / 2 + 0.5f;
				float v = vertices[i].y / GetMaxLightLength() / 2 + 0.5f;
				
				u = SMath.Clamp(0, u, 1);
				v = SMath.Clamp(0, v, 1);

				uvs[i] = new Vector2(u, v);
			}

			_mesh.uv = uvs;
		}

		protected void DeployMesh (List<Vector3> vertices, List<int> triangles) {
			_mesh.Clear();

			Vector3[] verticesArray = vertices.ToArray();
			
			_mesh.vertices = verticesArray;
			_mesh.triangles = triangles.ToArray();

			UpdateColor();
			UpdateUVs(verticesArray);
		}
	}
}

