using UnityEngine;
using UnityEditor;
using System.Collections;

namespace LOS.Editor {

	public class LOSMenu : UnityEditor.Editor {

		[MenuItem("GameObject/LOS 2D Lighting/Radial Light", false, 50)]
		public static void CreateRadialLight () {
			CheckLOSCameraExistence();

			GameObject go = new GameObject("Radial Light");
			var light = go.AddComponent<LOSRadialLight>();

			light.material = Resources.Load<Material>("Materials/RadialLight");

			PlaceGameObjectAccordingToCamera(go);

			Selection.activeGameObject = go;
			Undo.RegisterCreatedObjectUndo(go, "Undo creating Radial Light");
		}

	
		[MenuItem("GameObject/LOS 2D Lighting/Full Screen Light", false, 51)]
		public static void CreateFullScreenForwardLight () {
			CheckLOSCameraExistence();
			
			GameObject go = new GameObject("Full Screen Light");
			var light = go.AddComponent<LOSFullScreenLight>();
			
			light.material = Resources.Load<Material>("Materials/Basic");

			PlaceGameObjectAccordingToCamera(go);

			Selection.activeGameObject = go;
			Undo.RegisterCreatedObjectUndo(go, "Undo creating Full Screen Light");
		}

		[MenuItem("GameObject/LOS 2D Lighting/Invert Full Screen Light", false, 52)]
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

			bool result = losCameras.Length == 1;

			if (losCameras.Length == 0) {
				Debug.LogWarning("No LOSCamera found! We'll add the LOSCamera in the Camera.main. Change it if you need to.");
				Camera.main.gameObject.AddComponent<LOSCamera>();
				result = true;
			}
			else if (losCameras.Length > 1) {
				Debug.LogWarning("More than 2 LOSCamera are found in the scene! Please keep only 1 active.");
			}

			return result;
		}

		private static void PlaceGameObjectAccordingToCamera (GameObject go) {
			Camera editorCamera = SceneView.currentDrawingSceneView.camera;

			Vector3 position = editorCamera.transform.position;
			position.z = 0;
			editorCamera.transform.position = position;
		}

		
	}




}