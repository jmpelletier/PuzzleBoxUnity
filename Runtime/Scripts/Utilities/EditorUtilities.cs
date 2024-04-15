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
    public static class EditorUtilities
    {
        public static Color lightColor = new Color(1, 1, 1);
        public static Color darkColor = new Color(0.4f, 0.4f, 0.4f);
        public static Color redColor = new Color(1, 0.15f, 0f);

        private static Texture _puzzleBoxLogoTexture = null;
        private static Dictionary<string, Texture> _iconTextures = new Dictionary<string, Texture>();

        private static string _iconsPath = "Packages/com.jmpelletier.puzzlebox/Runtime/Images/Icons/";
        private static string _logosPath = "Packages/com.jmpelletier.puzzlebox/Runtime/Images/Logos/";
        private static string _gizmosPath = "Packages/com.jmpelletier.puzzlebox/Gizmos/";

        public static Texture logo
        {
            get
            {
                if (_puzzleBoxLogoTexture == null)
                {
                    _puzzleBoxLogoTexture = AssetDatabase.LoadAssetAtPath<Texture>(_logosPath + "PuzzleBox_Logo_Small.png");
                }

                return _puzzleBoxLogoTexture;
            }
        }

        public static Texture GetIconTexture(string iconName)
        {
            if (_iconTextures.ContainsKey(iconName))
            {
                return _iconTextures[iconName];
            }

            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(_iconsPath + iconName + ".png");
            if (tex != null)
            {
                _iconTextures[iconName] = tex;
            }

            return tex;
        }

        public static void DrawIcon(string iconName, Rect rect)
        {
            Texture texture = GetIconTexture(iconName);
            if (texture)
            {
                Gizmos.DrawGUITexture(rect, texture);
            }
        }

        public static void DrawIcon(string iconName, Vector2 position)
        {
            Texture texture = GetIconTexture(iconName);
            if (texture)
            {
                Handles.Label(position, texture);
            }
        }

        private static Texture _puzzleBoxIconTexture = null;
        public static Texture icon
        {
            get
            {
                if (_puzzleBoxIconTexture == null)
                {
                    _puzzleBoxIconTexture = AssetDatabase.LoadAssetAtPath<Texture>(_iconsPath + "PuzzleBoxIconSmall.png");
                }

                return _puzzleBoxIconTexture;
            }
        }

        private static Vector3[] _trianglePoints = new Vector3[3];
        private static Vector3[] _rectanglePoints = new Vector3[4];

        public static void DrawArrowhead(Vector3 tipPosition, Vector3 basePosition, float halfWidth, Color color, Vector3 normal)
        {
            Vector3 d = tipPosition - basePosition;
            Vector3 direction = d.normalized;
            Vector3 cross = Vector3.Cross(direction, normal);

            _trianglePoints[0] = tipPosition;
            _trianglePoints[1] = basePosition + cross * halfWidth;
            _trianglePoints[2] = basePosition - cross * halfWidth;

            Handles.color = color;
            Handles.DrawAAConvexPolygon(_trianglePoints);
        }

        public static void DrawThickLine(Vector3 startPosition, Vector3 endPosition, float width, Color color, Vector3 normal)
        {
            float halfWidth = width * 0.5f;
            Vector3 d = endPosition - startPosition;
            Vector3 direction = d.normalized;
            Vector3 cross = Vector3.Cross(direction, normal);

            _rectanglePoints[0] = startPosition + cross * halfWidth;
            _rectanglePoints[1] = endPosition + cross * halfWidth;
            _rectanglePoints[2] = endPosition - cross * halfWidth;
            _rectanglePoints[3] = startPosition - cross * halfWidth;

            Handles.color = color;
            Handles.DrawAAConvexPolygon(_rectanglePoints);
        }

        public static void DrawArrowhead(Vector3 tipPosition, Vector3 basePosition, float halfWidth, Color color)
        {
            DrawArrowhead(tipPosition, basePosition, halfWidth, color, Vector3.forward);
        }

        public static void DrawArrow(Vector3 from, Vector3 to, float lineWidth, float arrowheadHalfWidth, float arrowheadLength, Color color, bool doubleEnded, Vector3 normal)
        {
            Vector3 direction = (to - from).normalized;
            Vector3 basePosition = to - direction * arrowheadLength;

            DrawArrowhead(to, basePosition, arrowheadHalfWidth, color, normal);

            if (doubleEnded)
            {
                Vector3 startBasePosition = from + direction * arrowheadLength;
                DrawThickLine(startBasePosition, basePosition, lineWidth, color, normal);
                DrawArrowhead(from, startBasePosition, arrowheadHalfWidth, color, normal);
            }
            else
            {
                DrawThickLine(from, basePosition, lineWidth, color, normal);
            }
        }

        public static void DrawArrow(Vector3 from, Vector3 to, float lineWidth, float arrowheadHalfWidth, float arrowheadLength, Color color, bool doubleEnded)
        {
            DrawArrow(from, to, lineWidth, arrowheadHalfWidth, arrowheadLength, color, doubleEnded, Vector3.forward);
        }

        public static void DrawArrow(Vector3 from, Vector3 to, float lineWidth, float arrowheadHalfWidth, float arrowheadLength, Color color)
        {
            DrawArrow(from, to, lineWidth, arrowheadHalfWidth, arrowheadLength, color, false, Vector3.forward);
        }

        public static void DrawArrow(Vector3 from, Vector3 to, float lineWidth, float arrowheadHalfWidth, float arrowheadLength, Color color, Vector3 normal)
        {
            DrawArrow(from, to, lineWidth, arrowheadHalfWidth, arrowheadLength, color, false, normal);
        }

        public static void DrawCollider(Collider2D coll, Color strokeColor, Color fillColor)
        {
            if (coll == null) return;

            if (coll is BoxCollider2D)
            {
                BoxCollider2D box = (BoxCollider2D)coll;

                Handles.DrawSolidRectangleWithOutline(new Rect(box.bounds.min, box.bounds.size), fillColor, strokeColor);
            }
            else if (coll is CircleCollider2D)
            {
                CircleCollider2D circle = (CircleCollider2D)coll;

                Handles.color = fillColor;
                Handles.DrawSolidDisc(circle.bounds.center, Vector3.back, circle.radius);

                Handles.color = strokeColor;
                Handles.DrawWireDisc(circle.bounds.center, Vector3.back, circle.radius);
            }
            else if (coll is PolygonCollider2D)
            {
                PolygonCollider2D polygon = (PolygonCollider2D)coll;

                Handles.color = fillColor;
                Vector3[] points = new Vector3[polygon.points.Length + 1];
                for (int i = 0; i < polygon.points.Length; i++)
                {
                    points[i] = polygon.transform.TransformPoint(polygon.points[i] + polygon.offset);
                }
                points[points.Length - 1] = points[0];

                Handles.DrawAAConvexPolygon(points);
                Handles.color = strokeColor;
                Handles.DrawAAPolyLine(points);
            }
            else
            {
                Handles.DrawSolidRectangleWithOutline(new Rect(coll.bounds.min, coll.bounds.size), fillColor, Color.clear);
            }
        }

        public static bool SceneIsIncludedInBuild(string sceneName)
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    if (scene.path.Contains($"/{sceneName}.unity"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

