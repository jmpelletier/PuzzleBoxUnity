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
    [CustomEditor(typeof(PuzzleBox.FollowPath2D))]
    public class FollowPath2DEditor : PuzzleBoxBehaviourEditor
    {
        void VisualizeSpeed(Vector3 from, Waypoint toWaypoint, float offset, Color color)
        {
            const float arrowheadHalfWidth = 0.05f;
            const float arrowheadLength = 0.1f;


            Vector3 to = toWaypoint.position;

            const float dt = 1f / 10f;
            float speed = 0f;
            Vector3 delta = to - from;
            Vector3 n = Vector3.Cross(delta, Vector3.forward);

            Vector3 direction = delta.normalized;
            float distance = delta.magnitude;
            float d = 0;

            while (distance > arrowheadLength)
            {
                Vector3 basePosition = from + direction * d + n * offset;
                Vector3 tipPosition = basePosition + direction * arrowheadLength;

                EditorUtilities.DrawArrowhead(tipPosition, basePosition, arrowheadHalfWidth, color);

                float breakingDistance = toWaypoint.EstimateBreakingDistance(speed);
                if (breakingDistance >= distance)
                {
                    speed = Mathf.Max(0, speed - toWaypoint.breaking * dt);
                }
                else
                {
                    speed += dt * toWaypoint.acceleration;
                    speed = Mathf.Min(toWaypoint.speed, speed);
                }

                if (speed < 0.001f)
                {
                    break;
                }

                distance -= speed * dt;
                d += speed * dt;
            }
        }

        void DrawSelectedArrow(Vector3 from, Waypoint toWaypoint, bool twoWay = false)
        {
            const float lineWidth = 0.02f;
            const float arrowheadHalfWidth = 0.05f;
            const float arrowheadLength = 0.1f;
            Color color = new Color(0.93f, 0.66f, 0.19f);

            EditorUtilities.DrawArrow(from, toWaypoint.position, lineWidth, arrowheadHalfWidth, arrowheadLength, color, twoWay);

            VisualizeSpeed(from, toWaypoint, 0, color);
        }

        void DrawArrow(Vector3 from, Waypoint toWaypoint, bool twoWay = false)
        {
            const float lineWidth = 0.01f;
            const float arrowheadHalfWidth = 0.05f;
            const float arrowheadLength = 0.1f;
            Color color = new Color(0.96f, 0.9f, 0.25f);

            EditorUtilities.DrawArrow(from, toWaypoint.position, lineWidth, arrowheadHalfWidth, arrowheadLength, color, twoWay);
        }

        private void OnSceneGUI()
        {
            FollowPath2D followPath2D = (FollowPath2D)target;

            if (followPath2D.waypoints.Length == 0)
            {
                return;
            }

            int i = 0;
            while (i < followPath2D.waypoints.Length && followPath2D.waypoints[i] == null)
            {
                i++;
            }
            if (i == followPath2D.waypoints.Length)
            {
                return;
            }
            Vector3 p = followPath2D.waypoints[i].position;
            int firstIndex = i;


            if (followPath2D.mode == FollowPath2D.Mode.Cycle)
            {
                for (i = i + 1; i < followPath2D.waypoints.Length; i++)
                {
                    if (followPath2D.waypoints[i])
                    {
                        if (followPath2D.waypoints[i].gameObject == Selection.activeGameObject)
                        {
                            DrawSelectedArrow(p, followPath2D.waypoints[i]);
                        }
                        else
                        {
                            DrawArrow(p, followPath2D.waypoints[i]);
                        }

                        p = followPath2D.waypoints[i].position;
                    }
                }

                if (followPath2D.waypoints[firstIndex].gameObject == Selection.activeGameObject)
                {
                    DrawSelectedArrow(p, followPath2D.waypoints[firstIndex]);
                }
                else
                {
                    DrawArrow(p, followPath2D.waypoints[firstIndex]);
                }
            }
            else
            {
                Waypoint lastWaypoint = followPath2D.waypoints[firstIndex];
                for (; i < followPath2D.waypoints.Length; i++)
                {
                    if (followPath2D.waypoints[i])
                    {
                        if (followPath2D.waypoints[i].gameObject == Selection.activeGameObject)
                        {
                            DrawSelectedArrow(p, followPath2D.waypoints[i], false);
                            Waypoint nextWaypoint = null;
                            for (int j = i + 1; j < followPath2D.waypoints.Length; j++)
                            {
                                if (followPath2D.waypoints[j])
                                {
                                    nextWaypoint = followPath2D.waypoints[j];
                                    break;
                                }
                            }

                            if (nextWaypoint)
                            {
                                DrawSelectedArrow(nextWaypoint.position, followPath2D.waypoints[i], false);
                            }
                        }
                        else if (!(i > 0 && lastWaypoint.gameObject == Selection.activeGameObject))
                        {
                            DrawArrow(p, followPath2D.waypoints[i], true);
                        }

                        p = followPath2D.waypoints[i].position;
                        lastWaypoint = followPath2D.waypoints[i];
                    }
                }
            }
        }
    }
}
