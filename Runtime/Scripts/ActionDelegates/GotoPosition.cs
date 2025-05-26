/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using UnityEngine;

namespace PuzzleBox
{
    public class GotoPosition : ActionDelegate
    {
        public Transform position;
        public GameObject target;
        public Vector3 offset = Vector3.zero;
        public bool keepVelocity = true;

        // Start is called before the first frame update
        void Start()
        {

        }

        public override void Perform(GameObject sender)
        {
            PerformAction(() => {
                if (target != null && position != null)
                {
                    PuzzleBox.ObjectReference reference = target.GetComponent<PuzzleBox.ObjectReference>();
                    if (reference != null)
                    {
                        target = reference.referencedObject;
                        if (target == null)
                        {
                            return;
                        }
                    }

                    target.transform.position = position.position + offset;

                    if (!keepVelocity)
                    {
                        KinematicMotion2D motion2D = target.GetComponent<KinematicMotion2D>();
                        if (motion2D != null)
                        {
                            motion2D.velocity = Vector2.zero;
                            return;
                        }

                        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            rb.linearVelocity = Vector2.zero;
                            return;
                        }
                    }
                }
            });
        }
    }
}

