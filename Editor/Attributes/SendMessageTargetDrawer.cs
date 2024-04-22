/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

namespace PuzzleBox
{
    [CustomPropertyDrawer(typeof(SendMessage.MessageTarget))]
    public class SendMessageTargetDrawer : ActionDelegateTargetDrawer
    {
        protected override void DrawProperty(Rect position, SerializedProperty property, GUIContent label)
        {
            base.DrawProperty(position, property, label);

            //Rect actionRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);

            //SerializedProperty actionProperty = property.FindPropertyRelative("action");
            //SerializedProperty methodNameProperty = property.FindPropertyRelative("methodName");

            //if (targetBehaviour != null )
            //{
            //    MethodInfo[] methods = ActionAttribute.GetMethods(targetBehaviour);
            //    string[] options = new string[methods.Length];

            //    for (int i = 0; i < methods.Length; i++)
            //    {
            //        options[i] = methods[i].Name;
            //    }

            //    int selectedIndex = System.Array.IndexOf(options, methodNameProperty.stringValue); ;
            //    selectedIndex = EditorGUI.Popup(actionRect, selectedIndex, options);
            //    if (selectedIndex >= 0)
            //    {
            //        methodNameProperty.stringValue = options[selectedIndex];
            //    }
            //}
        }

        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        //}
    }
}
