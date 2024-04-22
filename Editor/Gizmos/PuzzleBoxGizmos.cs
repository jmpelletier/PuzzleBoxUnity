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
using static Codice.Client.BaseCommands.Import.Commit;
using System.Reflection;
using static Cinemachine.CinemachineBlendDefinition;
using UnityEngine.UIElements;
using System.Linq;

namespace PuzzleBox
{
    public static class PuzzleBoxGizmos
    {
        const float connectionLineWidth = 5f;
        const GizmoType defaultGizmoType = GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected | GizmoType.NotInSelectionHierarchy | GizmoType.InSelectionHierarchy;

        public const string gizmosPath  = "Packages/com.jmpelletier.puzzlebox/Gizmos/";

        static public Color gizmoTextColor { get; private set; } = new Color(1f, 1f, 1f);
        static public Color gizmoConnectionColor { get; private set; } = new Color(0.11f, 0.47f, 0.98f);
        static public Color relationshipColor { get; private set; } = new Color(0, 0, 0, 0.65f);
        static public Color textLabelBackgroundColor { get; private set; } = new Color(0, 0, 0, 0.85f);


        static private GUIStyle _gizmoTextStyle = GUIStyle.none;
        static public GUIStyle gizmoTextStyle
        {
            get
            {
                if (_gizmoTextStyle == GUIStyle.none)
                {
                    _gizmoTextStyle = new GUIStyle();
                    _gizmoTextStyle.normal.textColor = gizmoTextColor;
                    _gizmoTextStyle.alignment = TextAnchor.MiddleCenter;
                    _gizmoTextStyle.fontSize = 12;
                    _gizmoTextStyle.padding = new RectOffset(5, 5, 10, 8);
                }
                return _gizmoTextStyle;
            }
        }

        static private void DrawConnections(Vector3 position, IEnumerable<ActionDelegate.Target> targets, bool selected)
        {
            if (targets != null)
            {
                foreach (var t in targets)
                {
                    if (t.target != null && t.behaviour != null)
                    {
                        if (!EditorUtilities.DrawBezierConnection(position, t.target.transform.position, selected ? connectionLineWidth * 3 : connectionLineWidth, gizmoConnectionColor, 0.25f, GizmoUtility.iconSize * 18f))
                        {
                            Handles.color = gizmoConnectionColor;
                            Handles.DrawWireDisc(t.target.transform.position, -Camera.current.transform.forward, 0.1f, connectionLineWidth);
                        }
                    }
                }
            }
        }

        static private void DrawConnections(Vector3 position, IEnumerable<MonoBehaviour> targets, bool selected)
        {
            if (targets != null)
            {
                foreach (var t in targets)
                {
                    if (t != null)
                    {
                        if (!EditorUtilities.DrawBezierConnection(position, t.transform.position, selected ? connectionLineWidth * 3 : connectionLineWidth, gizmoConnectionColor, 0.25f, GizmoUtility.iconSize * 18f))
                        {
                            Handles.color = gizmoConnectionColor;
                            Handles.DrawWireDisc(t.transform.position, -Camera.current.transform.forward, 0.1f, connectionLineWidth);
                        }
                    }
                }
            }
        }

        static private void DrawIcon(MonoBehaviour target)
        {
            Gizmos.color = Color.white;
            DrawIconForClass(target.transform.position, target.GetType());
        }

        static private bool DrawIconForClass(Vector3 position, System.Type type)
        {
            if (type != null && type != typeof(object))
            {
                string path = $"{gizmosPath}{type.Name}Gizmo.png";
                if (File.Exists(path))
                {
                    Gizmos.DrawIcon(position, path, true);
                    return true;
                }
                else
                {
                    return DrawIconForClass(position, type.BaseType);
                }
            }
            return false;
        }

        static private void DrawLabel(MonoBehaviour target, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                const float distanceThreshold = 20f;
                float cameraDistance = Vector3.Distance(target.transform.position, Camera.current.transform.position);
                float fadeFactor = 1f - Mathf.Clamp01(cameraDistance / distanceThreshold);

                float labelOffset = GizmoUtility.iconSize * 20f;
                Vector3 labelPosition = target.transform.position + Vector3.down * labelOffset;
                Vector3 labelScreenPosition = Camera.current.WorldToScreenPoint(labelPosition);
                
                Vector3 textSize = gizmoTextStyle.CalcSize(new GUIContent(text));
                textSize.x *= 1.5f; // For some reason, the width returned is off by this factor...

                Color c = gizmoTextStyle.normal.textColor;
                Color backgroundColor = new Color(textLabelBackgroundColor.r, textLabelBackgroundColor.g, textLabelBackgroundColor.b, textLabelBackgroundColor.a * fadeFactor);
                gizmoTextStyle.normal.textColor = new Color(c.r, c.g, c.b, fadeFactor);

                EditorUtilities.DrawScreenSpaceRectangle(labelScreenPosition, textSize, cameraDistance, backgroundColor);
                Handles.Label(labelPosition, text, gizmoTextStyle);
                gizmoTextStyle.normal.textColor = c;
            }
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Spawner target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;

            DrawIcon(target);

            string displayText = "";
            foreach (GameObject prefab in target.prefabs)
            {
                if (prefab != null)
                {
                    if (!string.IsNullOrEmpty(displayText))
                    {
                        displayText += "\n";
                    }
                    displayText += prefab.name;
                }
            }

            DrawLabel(target, displayText);
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Switch target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            bool selected = Selection.activeGameObject == target.gameObject;

            DrawConnections(target.transform.position, target.targets, selected);
            DrawIcon(target);
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Waypoint target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawIcon(target);
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.ActionDelegate target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawIcon(target);

            if (target is SendMessage)
            {
                SendMessage sendMessage = (SendMessage)target;
                DrawLabel(target, sendMessage.message);
                if (sendMessage.targets != null && sendMessage.targets.Length > 0)
                {
                    DrawConnections(position, sendMessage.targets.Select(x => x.behaviour), selected);
                }
            }
            else if (target is ClearGame)
            {
                DrawLabel(target, "Clear Game");
            }
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Timer target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawConnections(position, target.OnStart, selected);
            DrawConnections(position, target.OnUpdate, selected);
            DrawConnections(position, target.OnPause, selected);
            DrawConnections(position, target.OnEnd, selected);

            DrawIcon(target);
            DrawLabel(target, $"{target.duration:0.##} s");
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Interval target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawConnections(position, target.OnStart, selected);
            DrawConnections(position, target.OnUpdate, selected);
            DrawConnections(position, target.OnPause, selected);
            DrawConnections(position, target.OnTick, selected);

            DrawIcon(target);
            DrawLabel(target, $"{target.interval:0.##} s");
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Delay target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawConnections(position, target.targets, selected);

            DrawIcon(target);
            DrawLabel(target, $"{target.delay:0.##} s");
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.ColliderActions target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawConnections(position, target.triggerEnterActions, selected);
            DrawConnections(position, target.triggerExitActions, selected);
            DrawConnections(position, target.collisionActions, selected);

            if (target.otherObjectReference != null)
            {
                EditorUtilities.DrawStraightConnection(target.otherObjectReference.transform.position, position, 0.05f, relationshipColor);
            }

            DrawIcon(target);
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.ObjectReference target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            DrawIcon(target);
        }
    }
}

