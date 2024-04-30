/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PuzzleBox
{
    [PuzzleBox.HideInEnumeration]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class SceneViewImage : Utility
    {
        public Texture texture;
        public float size = 5f;

        public Color color = Color.white;

        private void OnDrawGizmos()
        {
            if (texture != null)
            {
                bool previousState = GL.sRGBWrite;
                GL.sRGBWrite = false;
                Handles.BeginGUI();
                float w = size;
                float h = (size * texture.height) / texture.width;
                Vector2 topLeft = HandleUtility.WorldToGUIPoint(transform.position);
                Vector2 bottomRight = HandleUtility.WorldToGUIPoint(transform.position + new Vector3(w, -h));
                Rect rect = new Rect(topLeft, bottomRight - topLeft);
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleAndCrop, false, 0, color, 0, 0);
                Handles.EndGUI();

                GL.sRGBWrite = previousState;
            }
        }
    }
}

