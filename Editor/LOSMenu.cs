using UnityEngine;
using UnityEditor;
using System.Collections;

namespace LOS.Editor {

	public class LOSMenu : UnityEditor.Editor {

		void Update () {
			Debug.Log("Update");
		}

		void LateUpdate () {
			Debug.Log("Late Update");
		}

		[MenuItem("GameObject/LOS Lighting/Create Radial Light", false, 90)]
		public static void CreateRadialLight () {
			CheckLOSCameraExistence();

			GameObject go = new GameObject("Radial Light");
			var light = go.AddComponent<LOSRadialLight>();

			light.material = Resources.Load<Material>("Materials/RadialLight");

			PlaceGameObjectAccordingToCamera(go);

			Selection.activeGameObject = go;
			Undo.RegisterCreatedObjectUndo(go, "Undo creating Radial Light");
		}

	
		[MenuItem("GameObject/LOS Lighting/Create Full Screen Light", false, 91)]
		public static void CreateFullScreenForwardLight () {
			CheckLOSCameraExistence();
			
			GameObject go = new GameObject("Full Screen Light");
			var light = go.AddComponent<LOSFullScreenLight>();
			
			light.material = Resources.Load<Material>("Materials/Basic");

			PlaceGameObjectAccordingToCamera(go);

			Selection.activeGameObject = go;
			Undo.RegisterCreatedObjectUndo(go, "Undo creating Full Screen Light");
		}

		[MenuItem("GameObject/LOS Lighting/Create Invert Full Screen Light", false, 92)]
		public static void CreateInvertFullScreenForwardLight () {
			CheckLOSCameraExistence();
			
			GameObject go = new GameObject("Invert Full Screen Light");
			var light = go.AddComponent<LOSFullScreenLight>();
			
			light.material = Resources.Load<Material>("Materials/Basic");
			light.invertMode = true;
			
			PlaceGameObjectAccordingToCamera(go);

			Selection.activeGameObject = go;
			Undo.RegisterCreatedObjectUndo(go, "Undo creating Invert Full Screen Light");
		}

		private static bool CheckLOSCameraExistence () {
			var losCameras = GameObject.FindObjectsOfType<LOSCamera>();
			if (losCameras.Length == 0) {
				Debug.LogWarning("No LOSCamera is found in the scene! Please remember to attach LOSCamera to the camera gameobjeect.");
			}
			else if (losCameras.Length > 1) {
				Debug.LogWarning("More than 1 LOSCamera are found in the scene! Please only keep one active.");
			}

			return losCameras.Length == 1;
		}

		private static void PlaceGameObjectAccordingToCamera (GameObject go) {
			Camera editorCamera = SceneView.currentDrawingSceneView.camera;

			Vector3 position = editorCamera.transform.position;
			position.z = 0;
			editorCamera.transform.position = position;
		}

		
	}




}