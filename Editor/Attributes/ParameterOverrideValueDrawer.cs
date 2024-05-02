/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PuzzleBox
{
    [CustomPropertyDrawer(typeof(ParameterOverride.Value))]
    public class ParameterOverrideValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty typeProperty = property.FindPropertyRelative("type");
            string type = typeProperty.stringValue;

            EditorGUI.BeginProperty(position, label, property);

            if (type == typeof(int).Name) EditorGUI.PropertyField(position, property.FindPropertyRelative("intValue"));
            else if (type == typeof(float).Name) EditorGUI.PropertyField(position, property.FindPropertyRelative("floatValue"));
            else if (type == typeof(bool).Name) EditorGUI.PropertyField(position, property.FindPropertyRelative("boolValue"));
            else if (type == typeof(string).Name) EditorGUI.PropertyField(position, property.FindPropertyRelative("stringValue"));
            else if (type == typeof(Vector2).Name) EditorGUI.PropertyField(position, property.FindPropertyRelative("vector2Value"));
            else if (type == typeof(Vector3).Name) EditorGUI.PropertyField(position, property.FindPropertyRelative("vector3Value"));

            EditorGUI.EndProperty();
        }
    }

}
