/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PuzzleBox
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class SceneViewText : PuzzleBoxBehaviour
    {
        [TextArea(1, 10)]
        public string text;
        public Color textColor = Color.white;
        public float fontSize = 10;
        public TextAnchor alignement = TextAnchor.UpperLeft;

        GUIStyle guiStyle = new GUIStyle();

        private void OnDrawGizmos()
        {
            float zoomLevel = 5f / Camera.current.orthographicSize;

            guiStyle.normal.textColor = textColor;
            guiStyle.alignment = alignement;
            guiStyle.fontSize = (int)(fontSize * zoomLevel);
            
            Handles.Label(transform.position, text, guiStyle);
        }
    }
}

