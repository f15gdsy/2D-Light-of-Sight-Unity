using UnityEngine;
using UnityEditor;
using System.Collections;

namespace LOS.Editor {
	
	[CustomEditor (typeof(LOSRadialLight))]
	public class LOSRadialLightEditor : LOSLightBaseEditor {
		protected SerializedProperty _radius;
		protected SerializedProperty _flashFrequency;
		protected SerializedProperty _flashOffset;

		private static float _previousTime;
		

		protected override void OnEnable () {
			base.OnEnable ();

			_radius = serializedObject.FindProperty("radius");
			_flashFrequency = serializedObject.FindProperty("flashFrequency");
			_flashOffset = serializedObject.FindProperty("flashOffset");

			var light = (LOSRadialLight) target;
			light.invertMode = false;

			serializedObject.ApplyModifiedProperties();
		}


	

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			EditorGUILayout.PropertyField(_radius);

			EditorGUILayout.PropertyField(_flashFrequency);
			if (_flashFrequency.intValue > 0) {
				EditorGUILayout.PropertyField(_flashOffset);

				if (_flashOffset.floatValue < 0) {
					EditorGUILayout.HelpBox("Flash offset should not be less than 0. Make it positive to work.", MessageType.Error);
				}
			}
			else if (_flashFrequency.intValue < 0) {
				EditorGUILayout.HelpBox("Flash frequency should not be less than 0. Make it positive to work.", MessageType.Error);
			}

			serializedObject.ApplyModifiedProperties();

			if (_flashFrequency.intValue > 0 && _flashOffset.floatValue > 0) {
				EditorUtility.SetDirty(target);
				var light = (LOSRadialLight) target;
				light.timeFromLastFlash += Time.realtimeSinceStartup - _previousTime;
				_previousTime = Time.realtimeSinceStartup;
			}
		}
	}
}
