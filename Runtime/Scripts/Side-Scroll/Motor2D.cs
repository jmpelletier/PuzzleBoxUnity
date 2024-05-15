
/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
     [AddComponentMenu("Puzzle Box/Side-Scroll/Motor 2D")]
    [RequireComponent(typeof(KinematicMotion2D))]
    public class Motor2D : PuzzleBoxBehaviour
    {
        public bool moveOnlyWhenGrounded = true;
        public bool moveInGroundDirection = true;
        public bool useAcceleration = true;
        public Vector2 velocity = Vector2.zero;
        public Vector2 acceleration = Vector2.zero;

        KinematicMotion2D motion2D;

        // Start is called before the first frame update
        void Start()
        {
            motion2D = GetComponent<KinematicMotion2D>();
        }

        protected override void PerformFixedUpdate(float deltaSeconds)
        {
            if (!moveOnlyWhenGrounded || motion2D.isGrounded)
            {
                float speed = velocity.magnitude;
                if (speed > 0.01f || useAcceleration)
                {
                    if (moveInGroundDirection)
                    {
                        Vector2 targetGroundVelocity = motion2D.groundRight * Vector2.Dot(motion2D.groundRight, velocity);
                        Vector2 groundVelocity = motion2D.groundRight * Vector2.Dot(motion2D.groundRight, motion2D.velocity);
                        if (Vector2.Dot(targetGroundVelocity, groundVelocity) < 0f || targetGroundVelocity.magnitude > groundVelocity.magnitude)
                        {
                            motion2D.velocity += targetGroundVelocity - groundVelocity;
                        }

                        // TODO: acceleration mode
                    }
                    else
                    {
                        if (useAcceleration)
                        {
                            motion2D.velocity += acceleration * Time.fixedDeltaTime;
                        }
                        else
                        {
                            motion2D.velocity = velocity;
                        }
                        
                    }
                    
                }
            }
        }
    }
}

