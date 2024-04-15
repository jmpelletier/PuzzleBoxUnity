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
    [AddComponentMenu("Puzzle Box/Side-Scroll/Face Motion Input")]
    public class FaceMotionInput : PuzzleBoxBehaviour
    {
        public PlatformerPlayer2D platformerPlayer2D;

        public enum Mode
        {
            FacingDirection,
            InputDirection
        }

        public Mode mode;
        public bool flip = true;
        public bool rotate = false;
        public float minAngle = -90f;
        public float maxAngle = 90f;

        float scaleX = 1;

        // Start is called before the first frame update
        void Start()
        {
            if (platformerPlayer2D == null)
            {
                platformerPlayer2D = GetComponentInParent<PlatformerPlayer2D>();
            }
           

            scaleX = transform.localScale.x;
        }

        // Update is called once per frame
        void Update()
        {
            if (platformerPlayer2D != null)
            {
                if (rotate)
                {
                    float angle = 0;
                    switch (mode)
                    {
                        case Mode.FacingDirection:
                            angle = Mathf.Atan2(platformerPlayer2D.facingDirection.y, platformerPlayer2D.facingDirection.x) * Mathf.Rad2Deg;
                            break;
                        case Mode.InputDirection:
                            angle = Mathf.Atan2(platformerPlayer2D.inputDirection.y, platformerPlayer2D.inputDirection.x) * Mathf.Rad2Deg;
                            break;
                    }
                     
                    if (angle > 90f)
                    {
                        angle = 180f - Mathf.Clamp(180f - angle, minAngle, maxAngle);
                    }
                    else if (angle < -90f)
                    {
                        angle = -180f - Mathf.Clamp(-180f - angle, minAngle, maxAngle);
                    }
                    else
                    {
                        angle = Mathf.Clamp(angle, minAngle, maxAngle);
                    }
                    transform.localEulerAngles = new Vector3(0, 0, angle);
                }

                if (flip)
                {
                    Vector3 scale = transform.localScale;
                    scale.x = Mathf.Sign(platformerPlayer2D.facingDirection.x) * scaleX;
                    transform.localScale = scale;
                }
                
            }
        }
    }
} // namespace

