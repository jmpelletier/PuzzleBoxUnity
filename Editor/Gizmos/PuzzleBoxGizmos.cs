/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEngine.UIElements;

namespace PuzzleBox
{
    public static class PuzzleBoxGizmos
    {

        #region BASIC_SETTINGS

        const GizmoType defaultGizmoType = GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected | GizmoType.NotInSelectionHierarchy | GizmoType.InSelectionHierarchy;

        private const string _gizmosPath  = "Packages/com.jmpelletier.puzzlebox/Gizmos/";
        private const string _texturesPath = "Packages/com.jmpelletier.puzzlebox/Editor/Images/";

        public static Asset<Texture2D> bezierConnectionTexture = new PuzzleBox.Asset<Texture2D>(_texturesPath + "BezierConnectionTexture.png");

        #endregion // BASIC_SETTINGS

        #region STYLING

        /**
         * Styling: lines and arrows.
         */
        const float connectionLineWidth = 5f;
        const float selectedConnectionLineWidth = 10f;
        const float relationshipLineWidth = 0.01f;
        const float arrowLineWidth = 0.02f;

        /**
         * Styling: Colors
         */
        static public Color gizmoTextColor { get; private set; } = new Color(1f, 1f, 1f);
        static public Color gizmoConnectionColor { get; private set; } = new Color(0.7f, 0.7f, 0.7f);
        static public Color selectedGizmoConnectionColor { get; private set; } = new Color(1f, 1f, 1f);
        static public Color relationshipColor { get; private set; } = new Color(0, 0, 0, 0.65f);
        static public Color textLabelBackgroundColor { get; private set; } = new Color(0, 0, 0, 0.85f);

        /**
         * Styling: Spacing and sizing
         */
        const float gizmosVisibilityThreshold = 20f; // Do not draw gizmos that are further than this distance.
        const float minimumConnectionViewportDistance = 0.05f; // We use this to avoid drawing connections that are too short
        static public float gizmosWorldSpaceSize { get { return GizmoUtility.iconSize * 32f; } }
        static private float gizmosConnectionOffsetDistance { get { return gizmosWorldSpaceSize * 0.55f; } }
        static private Vector3 gizmosConnectionOffset { get { return Camera.current.transform.right * gizmosConnectionOffsetDistance; } }

        /**
         * Styling: Text styles
         */

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

        #endregion // STYLING

        #region STYLE_UTILITIES

        // Use this to compute the alpha value of a gizmo we want to fade out with distance
        static public float GetGizmosFadeFactor(Vector3 worldPosition)
        {
            float cameraDistance = Vector3.Distance(worldPosition, Camera.current.transform.position);
            return 1f - Mathf.Clamp01(cameraDistance / gizmosVisibilityThreshold);
        }

        static public Color FadeColor(Color color, float fadeFactor)
        {
            return new Color(color.r, color.g, color.g, color.a * fadeFactor);
        }

        static public Color FadeGizmoColor(Color color, Vector3 worldPosition)
        {
            return FadeColor(color, GetGizmosFadeFactor(worldPosition));
        }

        #endregion // STYLE_UTILITIES

        #region DRAWING_CONNECTIONS

        // Connected two points in world space with a bezier curve.
        public static void DrawBezierConnection(Vector3 from, Vector3 to, float lineWidth, Color color)
        {
            float viewportDistance = EditorUtilities.CalculateViewportDistance(from, to);
            if (viewportDistance > minimumConnectionViewportDistance)
            {
                Vector3 delta = to - from;
                float dx = Mathf.Max(1f, Mathf.Abs(delta.x));
                float dy = Mathf.Abs(delta.y);

                float tangentLength = 0.5f * dx * Mathf.Clamp01(dy * 10f);
                Vector3 tangent = new Vector3(tangentLength, 0, delta.z * 0.5f);
                Color c = Handles.color;
                Handles.color = color;
                Handles.DrawBezier(from, to, from + tangent, to - tangent, Color.white, bezierConnectionTexture.Get(), lineWidth);
                Handles.color = c;
            }
        }

        // Connect two objects with a straight line.
        static private void DrawRelationshipLine(Vector3 sourcePosition, Vector3 destinationPosition, bool isArrow = false)
        {
            if (sourcePosition != null && destinationPosition != null)
            {
                float colorFade = GetGizmosFadeFactor(sourcePosition);

                if (colorFade > 0)
                {
                    Vector3 delta = sourcePosition - destinationPosition;
                    Vector3 offset = gizmosConnectionOffsetDistance * delta.normalized;
                    EditorUtilities.DrawStraightLine(sourcePosition - offset, destinationPosition + offset, relationshipLineWidth, FadeColor(relationshipColor, colorFade), isArrow);
                }
            }
        }

        static private void DrawBezierConnection(Vector3 from, Vector3 to, bool selected, Color color)
        {
            DrawBezierConnection(from, to, selected ? selectedConnectionLineWidth : connectionLineWidth, color);
        }

        static private void DrawConnections(Vector3 position, IEnumerable<Transform> targetTransforms, bool selected, float verticalOffsetDistance = 0, string label = "")
        {
            Color c = Handles.color;
            float colorFade = selected ? 1f : GetGizmosFadeFactor(position);

            if (colorFade > 0f)
            {
                Handles.color = FadeColor(selected ? selectedGizmoConnectionColor : gizmoConnectionColor, colorFade);

                Vector3 horizontalOffset = gizmosConnectionOffset;
                Vector3 verticalOffset = Camera.current.transform.up * verticalOffsetDistance;
                position += horizontalOffset + verticalOffset;

                if (targetTransforms != null)
                {
                    foreach (Transform t in targetTransforms)
                    {
                        if (t != null)
                        {
                            DrawBezierConnection(position, t.position - horizontalOffset, selected, Handles.color);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(label))
                {
                    EditorUtilities.LabelWithBackground(position, label, textLabelBackgroundColor, gizmoConnectionTextStyle);
                }

                Handles.color = c;
            }
        }

        static private void DrawConnections(Vector3 position, IEnumerable<MonoBehaviour> targets, bool selected, float verticalOffset = 0f, string label = "")
        {
            if (targets != null)
            {
                DrawConnections(position, targets.Select(x => x == null ? null : x.transform), selected, verticalOffset, label);
            }
        }

        static private void DrawConnections(Vector3 position, IEnumerable<ActionDelegate.Target> targets, bool selected, float verticalOffset = 0f, string label = "")
        {
            if (targets != null)
            {
                DrawConnections(position, targets.Select(x => x.target == null ? null : x.target.transform), selected, verticalOffset, label);
            }
        }

        #endregion // DRAWING_CONNECTIONS

        #region DRAWING_ICONS

        static private void DrawIcon(MonoBehaviour target)
        {
            Gizmos.color = Color.white;
            float gizmoHeight = gizmosWorldSpaceSize;
            DrawIconForClass(target.transform.position, target.GetType());
        }

        static private bool DrawIconForClass(Vector3 position, System.Type type)
        {
            if (type != null && type != typeof(object))
            {
                string path = $"{_gizmosPath}{type.Name}Gizmo.png";
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

        static private void DrawBadge(MonoBehaviour target, string path)
        {
            // We need to offset the position of the icon to make sure it is drawn on top.
            Gizmos.DrawIcon(target.transform.position + Camera.current.transform.forward * -0.001f, path);
        }

        static private void DrawReferenceIcon(MonoBehaviour target)
        {
            const string path = _gizmosPath + "ReferenceBadgeGizmo.png";

            DrawBadge(target, path);
        }

        static private void DrawTimerIcon(MonoBehaviour target)
        {
            const string path = _gizmosPath + "DelayBadgeGizmo.png";

            DrawBadge(target, path);
        }

        #endregion // DRAWING_ICONS

        #region DRAWING_LABELS

        static private void DrawLabel(MonoBehaviour target, string text, int index = 0, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            int fontSize = gizmoTextStyle.fontSize;
            Vector3 textSize = gizmoTextStyle.CalcSize(new GUIContent(text));
            textSize.x *= 1.5f; // For some reason, the width returned is off by this factor...
            float lineHeight = EditorUtilities.ScreenDistanceToWorldDistance(textSize.y, target.transform.position);

            float labelOffset = gizmosWorldSpaceSize * 0.65f + lineHeight * index;
            Vector3 labelPosition = target.transform.position + -Camera.current.transform.up * labelOffset;
            EditorUtilities.LabelWithBackground(labelPosition, text, textLabelBackgroundColor, gizmoTextStyle);
        }

        #endregion

        #region INDIVIDUAL_GIZMOS

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Spawner target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;

            string displayText = "";
            if (target.prefabs != null)
            {
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
            }
           

            DrawLabel(target, displayText);
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Switch target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            bool selected = EditorUtilities.IsInSelectedHierarchy(target.gameObject);

            DrawConnections(target.transform.position, target.targets, selected);
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.ActionDelegate target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = EditorUtilities.IsInSelectedHierarchy(target.gameObject);

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
                    float offset = gizmosConnectionOffsetDistance;
                    foreach (GameObject arg in sendMessage.arguments)
                    {
                        if (arg != null)
                        {
                            DrawRelationshipLine(target.transform.position, arg.transform.position);
                        }
                    }
                }
            }
            else if (target is ClearGame)
            {
                DrawLabel(target, "Clear Game");
            }

            if (target.delay > 0)
            {
                DrawTimerIcon(target);
            }
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Timer target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = EditorUtilities.IsInSelectedHierarchy(target.gameObject);

            DrawConnections(position, target.OnStart, selected);
            DrawConnections(position, target.OnUpdate, selected);
            DrawConnections(position, target.OnPause, selected);
            DrawConnections(position, target.OnEnd, selected);

            DrawLabel(target, $"{target.duration:0.##} s");
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.ColliderActions target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = EditorUtilities.IsInSelectedHierarchy(target.gameObject);

            DrawConnections(position, target.triggerEnterActions, selected, gizmosWorldSpaceSize * 0.5f, "Enter");
            DrawConnections(position, target.triggerExitActions, selected, 0, "Exit");
            DrawConnections(position, target.collisionActions, selected, gizmosWorldSpaceSize * -0.5f, "Collision");

            if (target.otherObjectReference != null)
            {
                DrawRelationshipLine(target.otherObjectReference.transform.position, position, true);
            }
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Animate target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = EditorUtilities.IsInSelectedHierarchy(target.gameObject);

            if (target.mode == Animate.Mode.OneShot)
            {
                DrawConnections(position, target.OnStartPerforming, selected, gizmosWorldSpaceSize * 0.25f, "Start");
                DrawConnections(position, target.OnFinishedPerforming, selected, gizmosWorldSpaceSize * -0.25f, "Finished");
            }
            else
            {
                DrawConnections(position, target.OnTurnOn, selected, gizmosWorldSpaceSize * 0.5f, "Turning On");
                DrawConnections(position, target.OnTurningOff, selected, 0, "Turning Off");
                DrawConnections(position, target.OnTurnedOff, selected, gizmosWorldSpaceSize * -0.5f, "Off");
            }
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.Counter target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = EditorUtilities.IsInSelectedHierarchy(target.gameObject);

            DrawConnections(position, target.reachedMinimumActions, selected, gizmosWorldSpaceSize * 0.25f, "Reached Min.");
            DrawConnections(position, target.reachedMaximumActions, selected, gizmosWorldSpaceSize * -0.25f, "Reached Max.");

            if (target.referencedValue != null)
            {
                DrawRelationshipLine(position, target.referencedValue.transform.position);
            }
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.TargetObject target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = EditorUtilities.IsInSelectedHierarchy(target.gameObject);

            if (target.target != null)
            {
                DrawConnections(position, new Transform[] { target.target.transform }, selected);
            }
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.ParameterOverride target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            Vector3 position = target.transform.position;
            bool selected = EditorUtilities.IsInSelectedHierarchy(target.gameObject);

            if (target.target.behaviour != null)
            {
                DrawConnections(position, new Transform[] { target.target.behaviour.transform }, selected);
            }

            if (!string.IsNullOrWhiteSpace(target.fieldName))
            {
                DrawLabel(target, target.fieldName, 0);
                DrawLabel(target, target.value.ToString(), 1);
            }
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.BehaviourReference target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            DrawReferenceIcon(target);

            if (target.behaviour != null)
            {
                DrawIconForClass(target.transform.position, target.behaviour.GetClass());
            }
        }

        [DrawGizmo(defaultGizmoType)]
        static void DrawPuzzleBoxGizmo(PuzzleBox.ObjectReference target, GizmoType gizmoType)
        {
            if (target.hideGizmo) return;

            if (target.referencedBehaviours != null)
            {
                float offset = gizmosConnectionOffsetDistance;
                foreach (BehaviourReference reference in target.referencedBehaviours)
                {
                    if (reference != null)
                    {
                        DrawRelationshipLine(target.transform.position, reference.transform.position);
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

        #endregion // INDIVIDUAL_GIZMOS
    }
}

