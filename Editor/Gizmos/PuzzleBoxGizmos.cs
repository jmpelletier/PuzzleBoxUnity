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
        const float relationshipLineWidth = 0.01f;
        const float arrowLineWidth = 0.02f;
        const GizmoType defaultGizmoType = GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected | GizmoType.NotInSelectionHierarchy | GizmoType.InSelectionHierarchy;

        public const string gizmosPath  = "Packages/com.jmpelletier.puzzlebox/Gizmos/";

        static public Color gizmoTextColor { get; private set; } = new Color(1f, 1f, 1f);
        static public Color gizmoConnectionColor { get; private set; } = new Color(0.7f, 0.7f, 0.7f);
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

        static private GUIStyle _gizmoConnectionTextStyle = GUIStyle.none;
        static public GUIStyle gizmoConnectionTextStyle
        {
            get
            {
                if (_gizmoConnectionTextStyle == GUIStyle.none)
                {
                    _gizmoConnectionTextStyle = new GUIStyle();
                    _gizmoConnectionTextStyle.normal.textColor = gizmoTextColor;
                    _gizmoConnectionTextStyle.alignment = TextAnchor.LowerLeft;
                    _gizmoConnectionTextStyle.fontSize = 9;
                    _gizmoConnectionTextStyle.padding = new RectOffset(5, 5, 5, 5);
                }
                return _gizmoConnectionTextStyle;
            }
        }

        static private float GetGizmoWorldSpaceHeight()
        {
            return GizmoUtility.iconSize * 32f;
        }

        static private float GetGizmoConnectionOffset()
        {
            return GetGizmoWorldSpaceHeight() * 0.5f;
        }

        static private void DrawBezierConnection(Vector3 from, Vector3 to, bool selected)
        {
            EditorUtilities.DrawBezierConnection(from, to, selected ? connectionLineWidth * 3 : connectionLineWidth, gizmoConnectionColor);
        }

        static private void DrawConnections(Vector3 position, IEnumerable<ActionDelegate.Target> targets, bool selected, float verticalOffset = 0f, string label = "")
        {
            Color c = Handles.color;
            Handles.color = gizmoConnectionColor;

            Vector3 horizontalOffset = Camera.current.transform.right * GetGizmoConnectionOffset();
            position += Camera.current.transform.up * verticalOffset;
            position += horizontalOffset;

            if (targets != null)
            {
                foreach (var t in targets)
                {
                    if (t.target != null && t.behaviour != null)
                    {
                        DrawBezierConnection(position, t.target.transform.position + horizontalOffset, selected);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(label))
            {
                EditorUtilities.LabelWithBackground(position, label, textLabelBackgroundColor, gizmoConnectionTextStyle);
            }

            Handles.color = c;
        }

        static private void DrawConnections(Vector3 position, IEnumerable<MonoBehaviour> targets, bool selected, float verticalOffset = 0f, string label = "")
        {
            Color c = Handles.color;
            Handles.color = gizmoConnectionColor;

            Vector3 horizontalOffset = Camera.current.transform.right * GetGizmoConnectionOffset();
            position += Camera.current.transform.up * verticalOffset;
            position += horizontalOffset;

            if (targets != null)
            {
                foreach (var t in targets)
                {
                    if (t != null)
                    {
                        DrawBezierConnection(position, t.transform.position - horizontalOffset, selected);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(label))
            {
                EditorUtilities.LabelWithBackground(position, label, textLabelBackgroundColor, gizmoConnectionTextStyle);
            }

            Handles.color = c;
        }

        static private void DrawIcon(MonoBehaviour target)
        {
            Gizmos.color = Color.white;
            float gizmoHeight = GetGizmoWorldSpaceHeight();
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

        static private void DrawReferenceIcon(MonoBehaviour target)
        {
            const string path = gizmosPath + "ReferenceGizmo.png";
            Gizmos.DrawIcon(target.transform.position, path);
        }

        static private void DrawLabel(MonoBehaviour target, string text, TextAnchor anchor = TextAnchor.MiddleCenter, int index = 0)
        {
            int fontSize = gizmoTextStyle.fontSize;
            Vector3 textSize = gizmoTextStyle.CalcSize(new GUIContent(text));
            textSize.x *= 1.5f; // For some reason, the width returned is off by this factor...
            float lineHeight = EditorUtilities.ScreenDistanceToWorldDistance(textSize.y, target.transform.position);

            float labelOffset = GetGizmoWorldSpaceHeight() * 0.65f + lineHeight * index;
            Vector3 labelPosition = target.transform.position + -Camera.current.transform.up * labelOffset;
            EditorUtilities.LabelWithBackground(labelPosition, text, textLabelBackgroundColor, gizmoTextStyle);
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Spawner target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;

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
        }


        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.ActionDelegate target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            if (target is SendMessage)
            {
                SendMessage sendMessage = (SendMessage)target;
                DrawLabel(target, sendMessage.message);
                if (sendMessage.targets != null && sendMessage.targets.Length > 0)
                {
                    DrawConnections(position, sendMessage.targets.Select(x => x.behaviour), selected);
                }
                if (sendMessage.arguments != null && sendMessage.arguments.Length > 0)
                {
                    float offset = GetGizmoConnectionOffset();
                    foreach (GameObject arg in sendMessage.arguments)
                    {
                        if (arg != null)
                        {
                            EditorUtilities.DrawStraightConnection(arg.transform.position, position, relationshipLineWidth, relationshipColor, false, 0, offset);
                        }
                    }
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

            DrawLabel(target, $"{target.interval:0.##} s");
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Delay target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawConnections(position, target.targets, selected);

            DrawLabel(target, $"{target.delay:0.##} s");
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.ColliderActions target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawConnections(position, target.triggerEnterActions, selected, GetGizmoWorldSpaceHeight() * 0.5f, "Enter");
            DrawConnections(position, target.triggerExitActions, selected, 0, "Exit");
            DrawConnections(position, target.collisionActions, selected, GetGizmoWorldSpaceHeight() * -0.5f, "Collision");


            if (target.otherObjectReference != null)
            {
                EditorUtilities.DrawStraightConnection(target.otherObjectReference.transform.position, position, arrowLineWidth, relationshipColor, true, 0, GetGizmoWorldSpaceHeight() * 0.5f);
            }
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.BehaviourReference target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            if (target.behaviour != null)
            {
                DrawIconForClass(target.transform.position, target.behaviour.GetClass());
            }

            DrawReferenceIcon(target);
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.ObjectReference target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            if (target.referencedBehaviours != null)
            {
                float offset = GetGizmoConnectionOffset();
                foreach(BehaviourReference reference in target.referencedBehaviours)
                {
                    if (reference != null)
                    {
                        EditorUtilities.DrawStraightConnection(reference.transform.position, target.transform.position, relationshipLineWidth, relationshipColor, false, 0, offset);
                    }
                }
            }

            DrawReferenceIcon(target);
        }


        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.PuzzleBoxBehaviour target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            DrawIcon(target);
        }
    }
}

