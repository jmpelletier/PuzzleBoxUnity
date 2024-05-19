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
    public class WindArea : PuzzleBoxBehaviour
    {
        [Space]
        public float angle = 0f;
        public float speed = 10f;

        [Space]
        public string targetTag = string.Empty;
        public string ignoreTag = string.Empty;
        public LayerMask layerMask = ~0;

        bool processTriggerEvent(Collider2D collision)
        {
            return enabled &&
                !collision.isTrigger &&
                (targetTag == "" || collision.tag == targetTag) &&
                (ignoreTag == "" || collision.tag != ignoreTag) &&
                (layerMask.value & (1 << collision.gameObject.layer)) > 0;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        private void AddWindForce(PuzzleBoxBehaviour target)
        {
            KinematicMotion2D motion = target as KinematicMotion2D;
            if (motion != null)
            {
                float theta = Mathf.Deg2Rad * angle;
                float distance = speed * Time.fixedDeltaTime;
                Vector2 vec = new Vector2(Mathf.Cos(theta) * distance, Mathf.Sin(theta) * distance);

                motion.MoveBy(vec);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (processTriggerEvent(collision))
            {
                KinematicMotion2D motion = collision.GetComponent<KinematicMotion2D>();
                if (motion != null)
                {
                    motion.OnPostFixedUpdateActions += AddWindForce;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (processTriggerEvent(collision))
            {
                KinematicMotion2D motion = collision.GetComponent<KinematicMotion2D>();
                if (motion != null)
                {
                    motion.OnPostFixedUpdateActions -= AddWindForce;
                }
            }
        }


        public override string GetIcon()
        {
            return "CollisionIcon";
        }
    }
}

