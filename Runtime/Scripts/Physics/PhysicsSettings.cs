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
    [PuzzleBox.HideInEnumeration]
    public class PhysicsSettings : PuzzleBoxBehaviour
    {
        public Vector3 gravity = new Vector3 (0, -9.8f, 0);

        public void SetGravity(float g)
        {
            SetGravity(new Vector3(0, g, 0));
        }

        public void SetGravity(Vector3 g)
        {
            gravity = g;
            Physics2D.gravity = (Vector2)gravity;
            Physics.gravity = gravity;
        }

        private void Awake()
        {
            SetGravity(gravity);
        }

        protected override void PerformFixedUpdate(float deltaSeconds)
        {
            SetGravity(gravity);
        }
    }
} // namespace

