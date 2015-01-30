using UnityEngine;
using UnityEditor;
using System.Collections;

namespace LOS.Editor {
	
	[CustomEditor (typeof(LOSRadialLight))]
	public class LOSRadialLightEditor : LOSLightBaseEditor {
		protected SerializedProperty _radius;
		protected SerializedProperty _flashFrequency;
		protected SerializedProperty _flashOffset;

		private bool _enableFlash;
		private bool _previousEnableFlash;

		private int _previousEditorFrequency;
		private float _previousEditorOffset;
		

		protected override void OnEnable () {
			base.OnEnable ();

			_radius = serializedObject.FindProperty("radius");
			_flashFrequency = serializedObject.FindProperty("flashFrequency");
			_flashOffset = serializedObject.FindProperty("flashOffset");

			var target = (LOSRadialLight) serializedObject.targetObject;
			target.invertMode = false;

			serializedObject.ApplyModifiedProperties();
		}
	

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			EditorGUILayout.PropertyField(_radius);

			_enableFlash = EditorGUILayout.Toggle("Enable Flash", _enableFlash);

			if (_enableFlash) {
				if (CheckChangeToEnableFlash()) {
					_flashFrequency.intValue = _previousEditorFrequency;
					_flashOffset.floatValue = _previousEditorOffset;
				}
				EditorGUILayout.PropertyField(_flashFrequency);
				EditorGUILayout.PropertyField(_flashOffset);
			}
			else if (CheckChangeToDisableFlash()) {
				_previousEditorFrequency = _flashFrequency.intValue;
				_previousEditorOffset = _flashOffset.floatValue;
				_flashFrequency.intValue = 0;
				_flashOffset.floatValue = 0;
			}

			_previousEnableFlash = _enableFlash;

			serializedObject.ApplyModifiedProperties();
		}

		private bool CheckChangeToEnableFlash () {
			return _enableFlash && !_previousEnableFlash;
		}

		private bool CheckChangeToDisableFlash () {
			return !_enableFlash && _previousEnableFlash;
		}
	}
}
