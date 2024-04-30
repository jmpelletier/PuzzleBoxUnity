/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Animations;

namespace PuzzleBox
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PuzzleBox.PuzzleBoxBehaviour), true)]
    public class PuzzleBoxBehaviourEditor : Editor
    {
  
        public override void OnInspectorGUI()
        {
            if (PuzzleBox.EditorUtilities.logo != null)
            {
                Color oldColor = GUI.color;
                GUI.color = EditorGUIUtility.isProSkin ? PuzzleBox.EditorUtilities.lightColor : PuzzleBox.EditorUtilities.darkColor;
                GUILayout.Label(PuzzleBox.EditorUtilities.logo.Get(), GUILayout.MaxHeight(16));
                GUI.color = oldColor;
            }
            base.OnInspectorGUI();
        }
    }
}


