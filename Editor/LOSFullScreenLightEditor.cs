using UnityEngine;
using UnityEditor;
using System.Collections;

namespace LOS.Editor {
	
	[CustomEditor (typeof(LOSFullScreenLight))]
	public class LOSFullScreenLightEditor : LOSLightBaseEditor {
		protected SerializedProperty _invertMode;


		protected override void OnEnable () {
			base.OnEnable();

			_invertMode = serializedObject.FindProperty("invertMode");
		}

		public override void OnInspectorGUI () {
			var light = (LOSFullScreenLight) target;

			if (!SHelper.CheckWithinScreen(light.transform.position, LOSManager.instance.losCamera.unityCamera, 0)) {
				EditorGUILayout.HelpBox(
					"Full Screen Light requires its position within the camera. " +
					"Otherwise, wired shadow will be present. I'm working on the next version to better solve the issue.", 
					MessageType.Warning);
			}

			base.OnInspectorGUI();

			EditorGUILayout.PropertyField(_invertMode);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
