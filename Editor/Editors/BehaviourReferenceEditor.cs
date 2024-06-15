/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PuzzleBox
{
    [CustomEditor(typeof(PuzzleBox.BehaviourReference))]
    public class BehaviourReferenceEditor : PuzzleBoxBehaviourEditor
    {
        SerializedProperty sampleObject;
        SerializedProperty behaviour;
        SerializedProperty typeName;


        private void OnEnable()
        {
            sampleObject = serializedObject.FindProperty("sampleObject");
            behaviour = serializedObject.FindProperty("behaviour");
            typeName = serializedObject.FindProperty("typeName");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            BehaviourReference behaviourReference = (BehaviourReference)target;


            if (sampleObject != null && sampleObject.objectReferenceValue != null)
            {
                GameObject go = sampleObject.objectReferenceValue as GameObject;
                MonoScript script = behaviour.objectReferenceValue as MonoScript;

                string[] behaviourNames;
                MonoBehaviour[] behaviours;

                if (go != null)
                {
                    behaviours = go.GetComponents<MonoBehaviour>().Where(component => Reflection.IsEnumerable(component.GetType())).ToArray();
                    behaviourNames = behaviours.Select(b => b.GetType().Name).ToArray();                    
                }
                else
                {
                    behaviourNames = new string[0];
                    behaviours = new MonoBehaviour[0];
                }

                int selectedIndex = -1;
                if (script != null && behaviourNames.Length > 0)
                {
                    for (int i = 0; i < behaviourNames.Length; i++)
                    {
                        if (behaviourNames[i] == script.GetClass().Name)
                        {
                            selectedIndex = i;
                            break;
                        }
                    }

                    if (selectedIndex < 0)
                    {
                        selectedIndex = 0;
                        behaviourReference.behaviour = MonoScript.FromMonoBehaviour(behaviours[selectedIndex]);
                        System.Type type = behaviourReference.behaviour != null ? behaviourReference.behaviour.GetClass() : null;
                        if (type != null)
                        {
                            typeName.stringValue = type.FullName;
                        }
                    }
                }
                
                selectedIndex = EditorGUILayout.Popup("Behaviour", selectedIndex, behaviourNames);
                if (selectedIndex >= 0)
                {
                    behaviourReference.behaviour = MonoScript.FromMonoBehaviour(behaviours[selectedIndex]);
                    System.Type type = behaviourReference.behaviour != null ? behaviourReference.behaviour.GetClass() : null;
                    if (type != null)
                    {
                        typeName.stringValue = type.FullName;
                    }
                }
            }
            else
            {
                EditorGUILayout.PropertyField(behaviour);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

