/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

 
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace PuzzleBox
{
    public static class EditorUtilities
    {
        public static Color lightColor = new Color(1, 1, 1);
        public static Color darkColor = new Color(0.4f, 0.4f, 0.4f);
        public static Color redColor = new Color(1, 0.15f, 0f);

        private static Dictionary<string, Texture> _iconTextures = new Dictionary<string, Texture>();

        private static string _iconsPath = "Packages/com.jmpelletier.puzzlebox/Runtime/Images/Icons/";
        private static string _logosPath = "Packages/com.jmpelletier.puzzlebox/Runtime/Images/Logos/";
        private static string _texturesPath = "Packages/com.jmpelletier.puzzlebox/Editor/Images/";

        public class Asset<T> where T : UnityEngine.Object
        {
            public T _asset;
            public Asset(string path)
            {
                _asset = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            public T Get() { return _asset; }
        }

        public static Asset<Texture> logo = new Asset<Texture>(_logosPath + "PuzzleBox_Logo_Small.png");
        public static Asset<Texture2D> bezierConnectionTexture = new Asset<Texture2D>(_texturesPath + "BezierConnectionTexture.png");

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

            Color c = Handles.color;
            Handles.color = color;
            Handles.DrawAAConvexPolygon(_trianglePoints);
            Handles.color = c;
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

            Color c = Handles.color;
            Handles.color = color;
            Handles.DrawAAConvexPolygon(_rectanglePoints);
            Handles.color = c;
        }

        public static void DrawRectangle3D(Vector3 center, Vector2 size, Color color)
        {
            DrawRectangle3D(center, size, color, -Camera.current.transform.forward, Camera.current.transform.up);
        }

        public static void DrawRectangle3D(Vector3 center, Vector2 size, Color color, Vector3 normal, Vector3 up)
        {
            Vector3 right = Vector3.Cross(up, normal);
            _rectanglePoints[0] = center + right * size.x + up * size.y;
            _rectanglePoints[1] = center + right * size.x - up * size.y;
            _rectanglePoints[2] = center - right * size.x - up * size.y;
            _rectanglePoints[3] = center - right * size.x + up * size.y;

            Color c = Handles.color;
            Handles.color = color;
            Handles.DrawAAConvexPolygon(_rectanglePoints);
            Handles.color = c;
        }

        public static float ScreenDistanceToWorldDistance(float distance, Vector3 worldPosition)
        {
            Vector3 screenPosition = Camera.current.WorldToScreenPoint(worldPosition);
            screenPosition.x += distance;
            Vector3 offsetWorldPosition = Camera.current.ScreenToWorldPoint(screenPosition);
            return Vector3.Distance(offsetWorldPosition, worldPosition);
        }

        public static float WorldDistanceToScreenDistance(float distance, Vector3 worldPosition)
        {
            Vector3 screenPosition = Camera.current.WorldToScreenPoint(worldPosition);
            Vector3 offsetScreenPosition = Camera.current.WorldToScreenPoint(worldPosition + Vector3.up * distance);
            return Vector3.Distance(offsetScreenPosition, worldPosition);
        }

        public static void DrawScreenSpaceRectangle(Vector3 center, Vector2 size, float distance, Color color)
        {
            Vector3 halfSize = size * 0.5f;
            _rectanglePoints[0] = Camera.current.ScreenToWorldPoint(center - halfSize); // Bottom left
            _rectanglePoints[1] = Camera.current.ScreenToWorldPoint(center + new Vector3(halfSize.x, -halfSize.y, 0)); // Bottom right
            _rectanglePoints[2] = Camera.current.ScreenToWorldPoint(center + halfSize); // Top right
            _rectanglePoints[3] = Camera.current.ScreenToWorldPoint(center + new Vector3(-halfSize.x, halfSize.y, 0)); // Top left

            Color c = Handles.color;
            Handles.color = color;
            Handles.DrawAAConvexPolygon(_rectanglePoints);
            Handles.color = c;
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

        public static void DrawBezierConnection(Vector3 from, Vector3 to, float lineWidth, Color color)
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

        public static void LabelWithBackground(Vector3 worldPosition, string text, Color backgroundColor, GUIStyle textStyle)
        {
            if (!string.IsNullOrEmpty(text))
            {
                const float distanceThreshold = 10f;
                float cameraDistance = Vector3.Distance(worldPosition, Camera.current.transform.position);
                float fadeFactor = 1f - Mathf.Clamp01(cameraDistance / distanceThreshold);

                Vector3 textSize = textStyle.CalcSize(new GUIContent(text));
                textSize.x *= 1.5f; // For some reason, the width returned is off by this factor...

                Vector3 labelScreenPosition = Camera.current.WorldToScreenPoint(worldPosition);
                Vector3 rectPosition = labelScreenPosition;
                switch(textStyle.alignment)
                {
                    case TextAnchor.UpperLeft:
                        rectPosition.x += textSize.x * 0.5f - textStyle.padding.left;
                        rectPosition.y -= textSize.y * 0.5f - textStyle.padding.top;
                        break;
                    case TextAnchor.UpperCenter:
                        rectPosition.y += textSize.y * 0.5f - textStyle.padding.top;
                        break;
                    case TextAnchor.UpperRight:
                        rectPosition.x -= textSize.x * 0.5f - textStyle.padding.right;
                        rectPosition.y -= textSize.y * 0.5f - textStyle.padding.top;
                        break;
                    case TextAnchor.MiddleLeft:
                        rectPosition.x += textSize.x * 0.5f - textStyle.padding.left;
                        break;
                    case TextAnchor.MiddleCenter:
                        break;
                    case TextAnchor.MiddleRight:
                        rectPosition.x -= textSize.x * 0.5f - textStyle.padding.right;
                        break;
                    case TextAnchor.LowerLeft:
                        rectPosition.x += textSize.x * 0.5f - textStyle.padding.left;
                        rectPosition.y += textSize.y * 0.5f - textStyle.padding.bottom;
                        break;
                    case TextAnchor.LowerCenter:
                        rectPosition.y += textSize.y * 0.5f - textStyle.padding.bottom;
                        break;
                    case TextAnchor.LowerRight:
                        rectPosition.x -= textSize.x * 0.5f - textStyle.padding.right;
                        rectPosition.y += textSize.y * 0.5f - textStyle.padding.bottom;
                        break;
                }

                Color c = textStyle.normal.textColor;
                backgroundColor.a *= fadeFactor;
                textStyle.normal.textColor = new Color(c.r, c.g, c.b, fadeFactor);

                EditorUtilities.DrawScreenSpaceRectangle(rectPosition, textSize, cameraDistance, backgroundColor);
                Handles.Label(worldPosition, text, textStyle);
                textStyle.normal.textColor = c;
            }
        }

        public static void DrawTextToTexture(Texture2D texture, string text, GUIStyle style)
        {
            if (texture == null || string.IsNullOrEmpty(text))
            {
                return;
            }

            RenderTexture tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            RenderTexture previousRenderTexture = RenderTexture.active;
            RenderTexture.active = tmp;

            GL.Clear(true, true, Color.white);

            Graphics.SetRenderTarget(tmp);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, texture.width, texture.height, 0);

            GUIContent content = new GUIContent(text);
            Vector2 size = style.CalcSize(content);
            Rect rect = new Rect(0, 0, texture.width, texture.height);

            style.Draw(rect, content, false, false, false, false);

            GL.PopMatrix();

            texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            texture.Apply();

            RenderTexture.active = previousRenderTexture;
            RenderTexture.ReleaseTemporary(tmp);
        }

        public static bool DrawStraightConnection(Vector3 from, Vector3 to, float lineWidth, Color color, bool drawArrow = true, float minimumDistance = 0.25f, float offset = 0.25f)
        {
            Vector3 delta = to - from;

            if (delta.sqrMagnitude > minimumDistance * minimumDistance)
            {
                float arrowHeadLength = lineWidth * 2;
                Vector3 direction = delta.normalized;
                Vector3 positionOffset = direction * offset;
                from += positionOffset;
                to -= positionOffset;

                if (drawArrow)
                {
                    to -= direction * arrowHeadLength;
                }

                DrawThickLine(from, to, lineWidth, color, Vector3.forward);

                if (drawArrow)
                {
                    DrawArrowhead(to + direction * arrowHeadLength, to, arrowHeadLength * 0.5f, color);
                }
                
                return true;
            }
            return false;
        }

        public static void DrawCollider(Collider2D coll, Color strokeColor, Color fillColor)
        {
            if (coll == null) return;

            Color c = Handles.color;
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
            Handles.color = c;
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

