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
    public class Rotate2D : PuzzleBoxBehaviour
    {
        public float degreesPerSecond = 45f;
        public float acceleration = 10f;
        public float breakingForce = 10f;

        public bool moving;

        public float speed = 0f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (moving)
            {
                speed = PuzzleBox.MathUtilities.EaseTowards(speed, degreesPerSecond, acceleration, Time.fixedDeltaTime);
            }
            else
            {
                speed = PuzzleBox.MathUtilities.EaseTowards(speed, 0, breakingForce, Time.fixedDeltaTime);
            }

            transform.Rotate(Vector3.forward, speed * Time.fixedDeltaTime);
        }

        public override void Toggle()
        {
            moving = !moving;
        }

        public override void Toggle(bool value)
        {
            moving = value;
        }
    }

}
