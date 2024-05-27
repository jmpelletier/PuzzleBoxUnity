/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PuzzleBox
{
    [CustomPropertyDrawer(typeof(ObservableProperty))]
    public class ObservablePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty gameObjectProperty = property.FindPropertyRelative("gameObject");
            SerializedProperty behaviourProperty = property.FindPropertyRelative("behaviour");
            SerializedProperty nameProperty = property.FindPropertyRelative("propertyName");

            GameObject gameObject = gameObjectProperty.objectReferenceValue as GameObject;
            MonoBehaviour behaviour = behaviourProperty.objectReferenceValue as MonoBehaviour;
            string propertyName = nameProperty.stringValue;

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float elementWidth = (position.width - spacing) * 0.5f;

            Rect gameObjectRect = new Rect(position.x, position.y, elementWidth, EditorGUIUtility.singleLineHeight);
            Rect propertyRect = new Rect(position.x + elementWidth + spacing, position.y, elementWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(gameObjectRect, gameObjectProperty, GUIContent.none);

            if (gameObject != null)
            {
                PuzzleBoxBehaviour[] behaviours = gameObject.GetComponents<PuzzleBoxBehaviour>();
                List<string> behaviourNames = new List<string>();
                foreach (PuzzleBoxBehaviour b in behaviours)
                {
                    string path = string.Empty;

                    if (b != null)
                    {
                        string behaviourName = b.GetType().Name;
                        path = behaviourName;
                        int index = 0;
                        while (behaviourNames.IndexOf(path) != -1)
                        {
                            index++;
                            path = $"{behaviourName} ({index})";
                        }
                    }

                    behaviourNames.Add(path);
                }

                List<string> options = new List<string>();
                List<(MonoBehaviour, string)> pairs = new List<(MonoBehaviour, string)>();

                string selection = string.Empty;

                for (int i = 0; i < behaviours.Length; i++)
                {
                    MonoBehaviour b = behaviours[i];
                    string behaviourName = behaviourNames[i];
                    var fields = b.GetType().GetFields().Where(f => typeof(PuzzleBox.IObservable).IsAssignableFrom(f.FieldType));
                    foreach (FieldInfo field in fields)
                    {
                        string fieldName = field.Name;
                        string path = $"{behaviourName}/{fieldName}";
                        options.Add(path);
                        pairs.Add((b, fieldName));

                        if (b == behaviour && fieldName.Equals(propertyName))
                        {
                            selection = path;
                        }
                    }
                }

                int selectedIndex = options.IndexOf(selection);

                selectedIndex = EditorGUI.Popup(propertyRect, selectedIndex, options.ToArray());

                if (selectedIndex >= 0)
                {
                    var pair = pairs[selectedIndex];
                    behaviourProperty.objectReferenceValue = pair.Item1;
                    nameProperty.stringValue = pair.Item2;
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                behaviourProperty.objectReferenceValue = null;
                nameProperty.stringValue = string.Empty;
                EditorGUI.Popup(propertyRect, 0, new string[0]);
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}
