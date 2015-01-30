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
			base.OnInspectorGUI();

			EditorGUILayout.PropertyField(_invertMode);
		}
	}
}
