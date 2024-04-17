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

namespace PuzzleBox
{
    public static class PuzzleBoxGizmos
    {
        const float connectionLineWidth = 2f;

        public const string gizmosPath  = "Packages/com.jmpelletier.puzzlebox/Gizmos/";

        static public Color gizmoTextColor { get; private set; } = new Color(0.11f, 0.47f, 0.98f);
        static public Color gizmoConnectionColor { get; private set; } = gizmoTextColor;


        static private GUIStyle _gizmoTextStyle = GUIStyle.none;
        static public GUIStyle gizmoTextStyle
        {
            get
            {
                if (_gizmoTextStyle == GUIStyle.none)
                {
                    _gizmoTextStyle = new GUIStyle();
                    _gizmoTextStyle.normal.textColor = gizmoTextColor;
                    _gizmoTextStyle.alignment = TextAnchor.UpperCenter;
                }
                return _gizmoTextStyle;
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
                        if (!EditorUtilities.DrawBezierConnection(position, t.transform.position, selected ? connectionLineWidth * 3 : connectionLineWidth, gizmoConnectionColor))
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
            Gizmos.DrawIcon(target.transform.position, $"{gizmosPath}{target.GetType().Name}Gizmo.png", true);
        }

        static private void DrawLabel(MonoBehaviour target, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Handles.Label(target.transform.position + Vector3.down * 0.25f, text, gizmoTextStyle);
            }
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Spawner target, GizmoType gizmoType)
        {
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

        [DrawGizmo(GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Switch target, GizmoType gizmoType)
        {
            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawConnections(target.transform.position, target.targets, selected);
            DrawIcon(target);
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Timer target, GizmoType gizmoType)
        {
            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawConnections(position, target.OnStart, selected);
            DrawConnections(position, target.OnUpdate, selected);
            DrawConnections(position, target.OnPause, selected);
            DrawConnections(position, target.OnEnd, selected);

            DrawIcon(target);
            DrawLabel(target, $"{target.duration:0.##} s");
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Interval target, GizmoType gizmoType)
        {
            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawConnections(position, target.OnStart, selected);
            DrawConnections(position, target.OnUpdate, selected);
            DrawConnections(position, target.OnPause, selected);
            DrawConnections(position, target.OnTick, selected);

            DrawIcon(target);
            DrawLabel(target, $"{target.interval:0.##} s");
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Delay target, GizmoType gizmoType)
        {
            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawConnections(position, target.targets, selected);

            DrawIcon(target);
            DrawLabel(target, $"{target.delay:0.##} s");
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.ColliderActions target, GizmoType gizmoType)
        {
            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawConnections(position, target.triggerEnterActions, selected);
            DrawConnections(position, target.triggerExitActions, selected);
            DrawConnections(position, target.collisionActions, selected);

            DrawIcon(target);
        }
    }
}

