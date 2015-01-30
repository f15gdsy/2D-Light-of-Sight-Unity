using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS {

	public class LOSLightBase : LOSObjectBase {

		public float lightAngle = 0;
		public float faceAngle = 0;
		public float degreeStep = 0.1f;
		public bool invert = true;
		public LayerMask obstacleLayer;
		public Color color = new Color(1, 1, 1, 1); 
		public Material defaultMaterial;

		protected MeshFilter _meshFilter;
		protected float _previousFaceAngle;
		protected float _previousLightAngle;
		protected Color _previousColor;
		protected float _raycastDistance;
		protected float _startAngle;
		protected float _endAngle;



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
				InvertDraw();
			}
			else {
				ForwardDraw();
			}
		}

		protected virtual void ForwardDraw () {}

		protected virtual void InvertDraw () {}

		protected void AddNewTrianglesBetweenPoints2Corners (ref List<int> triangles, List<Vector3> vertices, int pointAIndex, int pointBIndex) {
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

		protected void AddNewTrianglesBetweenPoints4Corners (ref List<int> triangles, List<Vector3> vertices,int pointAIndex, int pointBIndex, int centerIndex) {
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

		protected float GetMinRaycastDistance () {
			Vector3 screenSize = SHelper.GetScreenSizeInWorld();
			return screenSize.magnitude;
		}

		protected void AddNewTriangle (ref List<int> triangles, int v0, int v1, int v2) {
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

		protected List<Vector2> CalculateUVs (List<Vector3> vertices) {
			List<Vector2> uvs = new List<Vector2>();

			foreach (Vector3 vertex in vertices) {
				float u = vertex.x / LOSManager.instance.halfViewboxSize.x / 2 + 0.5f;
				float v = vertex.y / LOSManager.instance.halfViewboxSize.x / 2 + 0.5f;
				u = SMath.Clamp(0, u, 1);
				v = SMath.Clamp(0, v, 1);
				uvs.Add((new Vector2(u, v)));
			}
			return uvs;
		}

		protected void DeployMesh (List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Color color) {
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

