using UnityEngine;
using UnityEditor;
using System.Collections;

namespace LOS.Editor {
	
	[CustomEditor (typeof(LOSRadialLight))]
	public class LOSRadialLightEditor : LOSLightBaseEditor {
		protected SerializedProperty _radius;
		

		protected override void OnEnable () {
			base.OnEnable ();

			_radius = serializedObject.FindProperty("radius");

			var light = (LOSRadialLight) serializedObject.targetObject;
			light.invertMode = false;

			EditorUtility.SetDirty(light);
		}
	

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			EditorGUILayout.PropertyField(_radius);
		}
	}
}
