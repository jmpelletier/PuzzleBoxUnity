/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

namespace PuzzleBox
{
    [CustomPropertyDrawer(typeof(ActionDelegate.Target))]
    public class ActionDelegateTargetDrawer : PropertyDrawer
    {
        protected GameObject target;
        protected PuzzleBoxBehaviour targetBehaviour;

        protected virtual void DrawProperty(Rect position, SerializedProperty property, GUIContent label)
        {
            target = null;
            targetBehaviour = null;

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float elementWidth = (position.width - spacing) * 0.5f;

            Rect gameObjectRect = new Rect(position.x, position.y, elementWidth, EditorGUIUtility.singleLineHeight);
            Rect behaviourRect = new Rect(position.x + elementWidth + spacing, position.y, elementWidth, EditorGUIUtility.singleLineHeight);

            SerializedProperty targetProperty = property.FindPropertyRelative("target");
            SerializedProperty behaviourProperty = property.FindPropertyRelative("behaviour");

            EditorGUI.PropertyField(gameObjectRect, targetProperty, GUIContent.none);

            target = (GameObject)targetProperty.objectReferenceValue;
            if (target != null)
            {
                PuzzleBoxBehaviour[] behaviours = target.GetComponents<PuzzleBoxBehaviour>();

                if (behaviours.Length > 0)
                {
                    if (behaviours.Length == 1)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        behaviourProperty.objectReferenceValue = behaviours[0];
                        targetBehaviour = behaviours[0];
                        EditorGUI.PropertyField(behaviourRect, behaviourProperty, GUIContent.none);
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        string[] options = new string[behaviours.Length];
                        for (int i = 0; i < behaviours.Length; i++)
                        {
                            options[i] = behaviours[i].GetType().Name;
                        }

                        int selectedIndex = System.Array.IndexOf(behaviours, behaviourProperty.objectReferenceValue);
                        selectedIndex = EditorGUI.Popup(behaviourRect, selectedIndex, options);
                        if (selectedIndex >= 0)
                        {
                            behaviourProperty.objectReferenceValue = behaviours[selectedIndex];
                            targetBehaviour = behaviours[selectedIndex];
                        }
                    }
                }
            }


            



            EditorGUI.indentLevel = indent;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            DrawProperty(position, property, label);

            EditorGUI.EndProperty();
        }
    }
}
