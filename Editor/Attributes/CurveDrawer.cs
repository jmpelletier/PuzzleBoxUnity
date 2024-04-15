/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using UnityEngine.UIElements;

namespace PuzzleBox
{
    [CustomPropertyDrawer(typeof(CurveAttribute))]
    public class CurveDrawer : PropertyDrawer
    {
        const float PROPERTY_HEIGHT = 50f;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CurveAttribute curve = attribute as CurveAttribute;
            if (property.propertyType == SerializedPropertyType.AnimationCurve)
            {
                Rect r = position;
                r.height = PROPERTY_HEIGHT;
                EditorGUI.CurveField(r, property, curve.color, curve.range);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.AnimationCurve)
            {
                return PROPERTY_HEIGHT;
            }
            else
            {
                return EditorGUI.GetPropertyHeight(property);
            }
        }
    }
}

