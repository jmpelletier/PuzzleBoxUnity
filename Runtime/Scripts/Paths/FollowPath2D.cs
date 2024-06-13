
/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System;
using System.Collections;
using System.Collections.Generic;
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
        public bool moving = true;
        public bool oneWay = false;
        public Waypoint[] waypoints;

        private int currentIndex = 0;
        private float minDistance = 0.05f;

        KinematicMotion2D motion2D;
        private int direction = 1;
        private bool stopAtNextWaypoint = false;

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

        [PuzzleBox.Action]
        public void Move()
        {
            moving = true;
            stopAtNextWaypoint = false;
        }

        [PuzzleBox.Action]
        public void Stop()
        {
            moving = false;
            stopAtNextWaypoint = false;
        }

        [PuzzleBox.Action]
        public void StopNext()
        {
            stopAtNextWaypoint = true;
        }

        private void NextIndex()
        {
            if (stopAtNextWaypoint)
            {
                moving = false;
                stopAtNextWaypoint = false;
            }

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
        protected override void PerformFixedUpdate(float deltaSeconds)
        {
            if (waypoints.Length == 0 || !moving)
            {
                motion2D.velocity = Vector2.zero;
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

        public override string GetIcon()
        {
            return "FollowPathIcon";
        }
    }

}
