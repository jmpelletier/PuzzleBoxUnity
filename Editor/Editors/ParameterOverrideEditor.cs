/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

namespace PuzzleBox
{
    [CustomEditor(typeof(PuzzleBox.ParameterOverride))]
    public class ParameterOverrideEditor : PuzzleBoxBehaviourEditor
    {
        SerializedProperty targetProperty;
        SerializedProperty fieldNameProperty;
        SerializedProperty valueProperty;

        private void OnEnable()
        {
            targetProperty = serializedObject.FindProperty("target");
            fieldNameProperty = serializedObject.FindProperty("fieldName");
            valueProperty = serializedObject.FindProperty("value");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ParameterOverride parameterOverride = (ParameterOverride)target;

            if (targetProperty != null && fieldNameProperty != null)
            {
                SerializedProperty targetBehaviourProperty = targetProperty.FindPropertyRelative("behaviour");
                if (targetBehaviourProperty != null)
                {
                    FieldInfo[] fields = null;

                    // First, make sure this isn't a reference
                    BehaviourReference behaviourReference = targetBehaviourProperty.objectReferenceValue as BehaviourReference;

                    if (behaviourReference != null)
                    {
                        if (behaviourReference.behaviour != null)
                        {
                            fields = FieldAttribute.GetFields<OverridableAttribute>(behaviourReference.behaviour.GetClass());
                        }
                        else
                        {
                            fields = new FieldInfo[0];
                        }
                    }
                    else
                    {
                        PuzzleBoxBehaviour behaviour = targetBehaviourProperty.objectReferenceValue as PuzzleBoxBehaviour;
                        if (behaviour != null)
                        {
                            fields = FieldAttribute.GetFields<OverridableAttribute>(behaviour.GetType());
                        }
                        else
                        {
                            fields = new FieldInfo[0];
                        }
                    }
                    
                    if (fields != null && fields.Length > 0)
                    {
                        string[] fieldNames = fields.Select(f => f.Name).ToArray();

                        int selectedIndex = ArrayUtility.IndexOf(fieldNames, fieldNameProperty.stringValue);

                        selectedIndex = EditorGUILayout.Popup("Parameter", selectedIndex, fieldNames);
                        if (selectedIndex >= 0)
                        {
                            fieldNameProperty.stringValue = fieldNames[selectedIndex];
                            SerializedProperty typeProperty = valueProperty.FindPropertyRelative("type");
                            typeProperty.stringValue = fields[selectedIndex].FieldType.Name;

                            EditorGUILayout.PropertyField(valueProperty);
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
