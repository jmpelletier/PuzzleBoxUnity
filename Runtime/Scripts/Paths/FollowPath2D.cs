
/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace PuzzleBox
{
    [RequireComponent(typeof(KinematicMotion2D))]
    public class FollowPath2D : PuzzleBoxBehaviour
    {
        public enum Mode
        {
            Cycle,
            Return
        }

        public Mode mode = Mode.Cycle;
        public bool oneWay = false;
        public Waypoint[] waypoints;

        private int currentIndex = 0;
        private float minDistance = 0.05f;

        KinematicMotion2D motion2D;
        private int direction = 1;

        // Start is called before the first frame update
        void Start()
        {
            motion2D = GetComponent<KinematicMotion2D>();

            if (currentIndex < waypoints.Length)
            {
                transform.position = waypoints[currentIndex].position;
            }
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            motion2D.velocity = Vector2.zero;
        }

        private void NextIndex()
        {
            if (waypoints.Length <= 1)
            {
                return;
            }

            currentIndex += direction;

            if (currentIndex >= waypoints.Length) 
            { 
                switch(mode)
                {
                    case Mode.Cycle:
                        currentIndex = 0;
                        break;
                    case Mode.Return:
                        currentIndex -= 2;
                        direction = -1;
                        break;
                }

                if (oneWay)
                {
                    Toggle(false);
                }
            }
            else if (currentIndex < 0)
            {
                switch (mode)
                {
                    case Mode.Cycle:
                        currentIndex = waypoints.Length - 1;
                        break;
                    case Mode.Return:
                        currentIndex = 1;
                        direction = 1;
                        break;
                }

                if (oneWay)
                {
                    Toggle(false);
                }
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (waypoints.Length == 0)
            {
                return;
            }

            currentIndex = currentIndex % waypoints.Length;

            Waypoint currentWaypoint = waypoints[currentIndex];

            Vector3 delta = currentWaypoint.position - transform.position;
            float distance = delta.magnitude;
            if (distance < minDistance)
            {
                NextIndex();
                motion2D.velocity = Vector2.zero;
            }
            else
            {
                float speed = motion2D.velocity.magnitude;
                float breakingDistance = currentWaypoint.EstimateBreakingDistance(speed);

                float d = speed * Time.fixedDeltaTime;
                
                if (breakingDistance >= distance) 
                {
                    speed = Mathf.Max(0, speed - currentWaypoint.breaking * Time.fixedDeltaTime);

                    if (speed == 0)
                    {
                        NextIndex();
                    }
                }
                else if (speed < currentWaypoint.speed)
                {
                    if (float.IsNaN(currentWaypoint.speed) || float.IsInfinity(currentWaypoint.speed))
                    {
                        speed = speed + currentWaypoint.acceleration * Time.fixedDeltaTime;
                    }
                    else
                    {
                        speed = Mathf.Min(currentWaypoint.speed, speed + currentWaypoint.acceleration * Time.fixedDeltaTime);
                    }
                }

                if (d > distance)
                {
                    speed = distance / Time.fixedDeltaTime;
                }

                motion2D.velocity = delta.normalized * speed;
            }
        }

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

        private void OnDrawGizmos()
        {
            if (waypoints.Length == 0)
            {
                return;
            }

            int i = 0;
            while (i < waypoints.Length && waypoints[i] == null)
            {
                i++;
            }
            if (i == waypoints.Length)
            {
                return;
            }
            Vector3 p = waypoints[i].position;
            int firstIndex = i;


            if (mode == Mode.Cycle)
            {
                for (i = i + 1; i < waypoints.Length; i++)
                {
                    if (waypoints[i])
                    {
                        if (waypoints[i].gameObject == Selection.activeGameObject)
                        {
                            DrawSelectedArrow(p, waypoints[i]);
                        }
                        else
                        {
                            DrawArrow(p, waypoints[i]);
                        }

                        p = waypoints[i].position;
                    }
                }

                if (waypoints[firstIndex].gameObject == Selection.activeGameObject)
                {
                    DrawSelectedArrow(p, waypoints[firstIndex]);
                }
                else
                {
                    DrawArrow(p, waypoints[firstIndex]);
                }
            }
            else
            {
                Waypoint lastWaypoint = waypoints[firstIndex];
                for (; i < waypoints.Length; i++)
                {
                    if (waypoints[i])
                    {
                        if (waypoints[i].gameObject == Selection.activeGameObject)
                        {
                            DrawSelectedArrow(p, waypoints[i], false);
                            Waypoint nextWaypoint = null;
                            for (int j = i + 1; j <  waypoints.Length; j++)
                            {
                                if (waypoints[j])
                                {
                                    nextWaypoint = waypoints[j];
                                    break;
                                }
                            }

                            if (nextWaypoint)
                            {
                                DrawSelectedArrow(nextWaypoint.position, waypoints[i], false);
                            }
                        }
                        else if (!(i > 0 && lastWaypoint.gameObject == Selection.activeGameObject))
                        {
                            DrawArrow(p, waypoints[i], true);
                        }

                        p = waypoints[i].position;
                        lastWaypoint = waypoints[i];
                    }
                }
            }
        }
    }

}
