using UnityEngine;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;

namespace LOS.Editor {

	[CustomEditor (typeof(LOSLightBase))]
	public class LOSLightBaseEditor : UnityEditor.Editor {

		protected SerializedProperty _isStatic;
		protected SerializedProperty _color;
		protected SerializedProperty _degreeStep;
		protected SerializedProperty _coneAngle;
		protected SerializedProperty _faceAngle;
		protected SerializedProperty _obstacleLayer;
		protected SerializedProperty _material;
		protected SerializedProperty _orderInLayer;
		protected SerializedProperty _sortingLayer;

		protected virtual void OnEnable () {
			serializedObject.Update();

			var light = (LOSLightBase) target;

			EditorUtility.SetSelectedWireframeHidden(light.GetComponent<Renderer>(), !LOSManager.instance.debugMode);

			_isStatic = serializedObject.FindProperty("isStatic");
			_obstacleLayer = serializedObject.FindProperty("obstacleLayer");
			_degreeStep = serializedObject.FindProperty("degreeStep");
			_coneAngle = serializedObject.FindProperty("coneAngle");
			_faceAngle = serializedObject.FindProperty("faceAngle");
			_color = serializedObject.FindProperty("color");
			_sortingLayer = serializedObject.FindProperty("sortingLayer");
			_orderInLayer = serializedObject.FindProperty("orderInLayer");
			_material = serializedObject.FindProperty("material");
		}


		public override void OnInspectorGUI () {
			serializedObject.Update();

			EditorGUILayout.PropertyField(_isStatic);

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(_obstacleLayer);
			EditorGUILayout.Slider(_degreeStep, 0.1f, 2f);
//			EditorGUILayout.PropertyField(_degreeStep);
			EditorGUILayout.PropertyField(_coneAngle);
			if (_coneAngle.floatValue != 0) {
				EditorGUILayout.PropertyField(_faceAngle);
			}

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(_color);
			
		    // Start of Sorting Layer Popup
            Rect firstHoriz = EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(firstHoriz, GUIContent.none, _sortingLayer);

            string[] layerNames = GetSortingLayerNames();
            int[] layerID = GetSortingLayerUniqueIDs();

            int selected = -1;
            
            // What is selected?
            int sID = _sortingLayer.intValue;
            for (int i = 0; i < layerID.Length; i++)
            {
                if (sID == layerID[i])
                {
                    selected = i;
                }
            }

            // Select Default
            if (selected == -1)
            {
                for (int i = 0; i < layerID.Length; i++)
                {
                    if (layerID[i] == 0)
                    {
                        selected = i;
                    }
                }
            }

            selected = EditorGUILayout.Popup("Sorting Layer", selected, layerNames);

            //Translate to ID
            _sortingLayer.intValue = layerID[selected];

            EditorGUI.EndProperty();
            EditorGUILayout.EndHorizontal();
            // End of Sorting Layer Popup
			
			EditorGUILayout.PropertyField(_orderInLayer);
			EditorGUILayout.PropertyField(_material);
		}
		
		public string[] GetSortingLayerNames()
        {
            var internalEditorUtilityType = typeof(InternalEditorUtility);
            PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
            return (string[])sortingLayersProperty.GetValue(null, new object[0]);
        }

        public int[] GetSortingLayerUniqueIDs()
        {
            var internalEditorUtilityType = typeof(InternalEditorUtility);
            PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
            return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
        }
	}

}