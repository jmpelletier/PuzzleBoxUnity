/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;

namespace PuzzleBox
{
    [PuzzleBox.HideInEnumeration]
    public class PhysicsSettings : PuzzleBoxBehaviour
    {
        [Min(0)]
        [Tooltip("The absolute gravity force in m/s^2")]
        public float gravityForce = 9.8f;

        [Tooltip("The direction of gravity. By default, this points towards (0, -1, 0).")]
        public Vector3 gravityAngle = Vector3.zero;


        private ObservableVector3 gravity = new ObservableVector3();

        public void SetGravity(float force)
        {
            SetGravity(force, gravityAngle);
        }

        public void SetGravity(float force, Vector3 angle)
        {
            gravityForce = force;
            gravityAngle = angle;
            Quaternion rotation = Quaternion.Euler(angle);
            gravity.Set(rotation * Vector3.down * gravityForce);
            Physics.gravity = gravity;
        }

        public void SetGravity(Vector3 angle)
        {
            SetGravity(gravityForce, angle);
        }

        private void Awake()
        {
            SetGravity(gravityForce, gravityAngle);
        }

        protected override void PerformFixedUpdate(float deltaSeconds)
        {
            #if UNITY_EDITOR
            SetGravity(gravity);
            #endif
        }
    }
} // namespace

