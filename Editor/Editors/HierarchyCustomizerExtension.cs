/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PuzzleBox
{
    public class HierarchyCustomizerExtension
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        enum Alignment
        {
            Left,
            Right
        }

        private static Vector2 DrawTexture(Texture texture, Rect selectionRect, float offset, Alignment alignment)
        {
            if (texture != null)
            {
                float wh_ratio = (float)texture.width / (float)texture.height;
                float w = wh_ratio * selectionRect.height;
                Rect rect = new Rect();
                rect.size = new Vector2(w, selectionRect.height);
                if (alignment == Alignment.Left)
                {
                    rect.x = selectionRect.x + offset;
                }
                else if (alignment == Alignment.Right)
                {
                    rect.x = selectionRect.x + (selectionRect.width - w - offset);
                }
                
                rect.y = selectionRect.y;

                GUI.DrawTexture(rect, texture);

                return rect.size;
            }

            return Vector2.zero;
        }


        private static void HierarchyWindowItemOnGUI(int instanceId, Rect selectionRect)
        {
            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (!gameObject)
            {
                return;
            }

            HierarchyCustomizer customizer = gameObject.GetComponent<HierarchyCustomizer>();
            PuzzleBoxBehaviour[] puzzleBoxBehaviours = gameObject.GetComponents<PuzzleBoxBehaviour>();

            if (puzzleBoxBehaviours.Length == 0)
            {
                return;
            }

            float offset = 0;

            // Cinemachine also adds a texture to the hierarchy. There might be other components
            // that also do, but this is the one that is most likely to cause issues.
            if (gameObject.TryGetComponent(out Cinemachine.CinemachineBrain brain)) {
                offset += 20;
            };

            // We don't display the PuzzleBox icon if there is only a HierarchyCustomizer.
            bool hasPuzzleBoxComponent = false;
            foreach(PuzzleBoxBehaviour puzzleBoxBehaviour in puzzleBoxBehaviours)
            {
                if (puzzleBoxBehaviour != customizer)
                {
                    hasPuzzleBoxComponent = true;
                    break;
                }
            }

            // Adjust the color to fit the skin.
            Color oldColor = GUI.color;
            GUI.color = EditorGUIUtility.isProSkin ? PuzzleBox.EditorUtilities.lightColor : PuzzleBox.EditorUtilities.darkColor;


            // Draw the PuzzleBox icon if needed.
            if (hasPuzzleBoxComponent && PuzzleBox.EditorUtilities.icon != null)
            {
                Vector2 size = DrawTexture(PuzzleBox.EditorUtilities.icon, selectionRect, offset, Alignment.Right);
                offset += size.x;
            }

            // Now draw the other icons
            foreach (PuzzleBoxBehaviour puzzleBoxBehaviour in puzzleBoxBehaviours)
            {
                string iconName = puzzleBoxBehaviour.GetIcon();
                if (!string.IsNullOrEmpty(iconName))
                {
                    Texture icon = EditorUtilities.GetIconTexture(iconName);
                    if (icon != null)
                    {
                        Vector2 size = DrawTexture(icon, selectionRect, offset, Alignment.Right);
                        offset += size.x;
                    }
                }
            }

            // Then, add the icons added by the HierarchyCustomizer
            if (customizer)
            {
                if (customizer.icons != null)
                {
                    foreach (Texture icon in customizer.icons)
                    {
                        if (icon)
                        {
                            Vector2 size = DrawTexture(icon, selectionRect, offset, Alignment.Right);
                            offset += size.x;
                        }
                    }
                }
            }

            GUI.color = oldColor;
        }
    }
}

