/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;

namespace PuzzleBox
{
    public class Motor : PuzzleBoxBehaviour
    {
        public Vector3 velocity;
        public float acceleration = 20f;

        KinematicMotion2D motion2D;
        KinematicMotion3D motion3D;

        private class Force : KinematicMotion3D.IForce
        {
            public Vector3 velocity;
            public Vector3 targetVelocity;
            public float acceleration;

            public void SetFinalVelocity(Vector3 v)
            {
                velocity = v;
            }

            public Vector3 Update(float deltaSeconds)
            {
                float speedChange = deltaSeconds * acceleration;
                Vector3 targetDelta = targetVelocity - velocity;
                float speedDifference = targetDelta.magnitude;
                if (speedDifference >= speedChange)
                {
                    velocity += targetDelta.normalized * speedChange;
                }
                else
                {
                    velocity = targetVelocity;
                }
                return velocity;
            }
        }

        private Force _force = new Force();

        protected override void PerformFixedUpdate(float deltaSeconds)
        {
            _force.targetVelocity = velocity;
            _force.acceleration = acceleration;
        }

        private void OnEnable()
        {
            motion2D = GetComponent<KinematicMotion2D>();
            motion3D = GetComponent<KinematicMotion3D>();

            _force.targetVelocity = velocity;
            _force.acceleration = acceleration;

            if (motion3D != null)
            {
                motion3D.AddForce(_force);
            }
        }

        private void OnDisable()
        {
            if (motion3D != null)
            {
                motion3D.RemoveForce(_force);
            }
        }
    }
}

