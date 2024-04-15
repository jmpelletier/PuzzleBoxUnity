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

namespace PuzzleBox
{
    public static class PuzzleBoxGizmos
    {
        static float connectionLineWidth = 2f;
        static Color outConnectionColor = new Color(0.11f, 0.47f, 0.98f);

        [DrawGizmo(GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Spawner target, GizmoType gizmoType)
        {
            Vector3 position = target.transform.position;

            Gizmos.color = Color.white;
            Gizmos.DrawIcon(position, $"PuzzleBox/{target.GetType().Name}Gizmo.png", true);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = outConnectionColor;
            style.alignment = TextAnchor.UpperCenter;
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

            if (!string.IsNullOrEmpty(displayText))
            {
                Handles.Label(position, displayText, style);
            }
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Switch target, GizmoType gizmoType)
        {
            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            foreach (var t in target.targets)
            {
                if (t != null)
                {
                    if (!EditorUtilities.DrawBezierConnection(target.transform.position, t.transform.position, selected ? connectionLineWidth * 3 : connectionLineWidth, outConnectionColor))
                    {
                        Handles.color = outConnectionColor;
                        Handles.DrawWireDisc(t.transform.position, -Camera.current.transform.forward, 0.1f, connectionLineWidth);
                    }
                }
            }

            Gizmos.color = Color.white;
            Gizmos.DrawIcon(position, $"PuzzleBox/{target.GetType().Name}Gizmo.png", true);
        }

        static void DrawActionDelegateConnections(Vector3 from, ActionDelegate[] actionDelegates, bool selected = false)
        {
            if (actionDelegates != null)
            {
                foreach(ActionDelegate actionDelegate in actionDelegates)
                {
                    if (actionDelegate != null)
                    {
                        if (!EditorUtilities.DrawBezierConnection(from, actionDelegate.transform.position, selected ? connectionLineWidth * 3 : connectionLineWidth, outConnectionColor))
                        {
                            Handles.color = outConnectionColor;
                            Handles.DrawWireDisc(actionDelegate.transform.position, -Camera.current.transform.forward, 0.1f, connectionLineWidth);
                        }
                    }
                }
            }
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Timer target, GizmoType gizmoType)
        {
            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawActionDelegateConnections(position, target.OnStart, selected);
            DrawActionDelegateConnections(position, target.OnUpdate, selected);
            DrawActionDelegateConnections(position, target.OnPause, selected);
            DrawActionDelegateConnections(position, target.OnEnd, selected);

            Gizmos.color = Color.white;
            Gizmos.DrawIcon(position, $"PuzzleBox/{target.GetType().Name}Gizmo.png", true);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = outConnectionColor;
            style.alignment = TextAnchor.UpperCenter;
            Handles.Label(position, $"{target.duration:0.##} s", style);
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Interval target, GizmoType gizmoType)
        {
            Vector3 position = target.transform.position;
            bool selected = Selection.activeGameObject == target.gameObject;

            DrawActionDelegateConnections(position, target.OnStart, selected);
            DrawActionDelegateConnections(position, target.OnUpdate, selected);
            DrawActionDelegateConnections(position, target.OnPause, selected);
            DrawActionDelegateConnections(position, target.OnTick, selected);

            Gizmos.color = Color.white;
            Gizmos.DrawIcon(position, $"PuzzleBox/{target.GetType().Name}Gizmo.png", true);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = outConnectionColor;
            style.alignment = TextAnchor.UpperCenter;
            Handles.Label(position, $"{target.interval:0.##} s", style);
        }

        [InitializeOnLoad]
        static class CopyGizmos
        {

            static CopyGizmos()
            {
                CopyPackageGizmos();
            }

            [MenuItem("Tools/Copy Package Gizmos")]
            static public void CopyPackageGizmos()
            {
                Debug.Log("Copying Gizmos...");
                string sourceDirectory = Path.Combine(Application.dataPath, "../Packages/com.jmpelletier.puzzlebox/Gizmos");
                string destinationDirectory = Path.Combine(Application.dataPath, "Gizmos/PuzzleBox");

                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                string[] files = Directory.GetFiles(sourceDirectory);

                foreach (string file in files)
                {
                    if (Path.GetExtension(file) == ".png" || Path.GetExtension(file) == ".jpg")
                    {
                        string destinationFile = Path.Combine(destinationDirectory, Path.GetFileName(file));
                        File.Copy(file, destinationFile, true);
                    }
                }
            }
        }
    }
}

