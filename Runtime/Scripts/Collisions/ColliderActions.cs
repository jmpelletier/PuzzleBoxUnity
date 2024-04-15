/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleBox
{

    [AddComponentMenu("Puzzle Box/Collider Actions")]
    [RequireComponent(typeof(UniqueID))]
    public class ColliderActions : PersistentBehaviour
    {
        [Space]
        public ActionDelegate[] triggerEnterActions;
        public ActionDelegate[] triggerExitActions;
        public ActionDelegate[] collisionActions;

        [Space]
        public string targetTag = "";
        public LayerMask layerMask = ~0;

        [Header("Collision Direction")]
        public bool top = true;
        public bool right = true;
        public bool bottom = true;
        public bool left = true;

        [Space]
        [Tooltip("一回反応したら無効にするか？")]
        public bool deactivateOnEnter = false;
        public bool deactivateOnExit = false;
        public bool deactivateOnCollision = false;

        [Header("Editor")]
        public Color strokeColor = new Color(0, 1, 0, 1);
        public Color fillColor = new Color(0, 1, 0, 0.1f);

        public int triggerCount = 0;

        private void Start()
        {
            triggerCount = 0;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (enabled && !collision.collider.isTrigger && (targetTag == "" || collision.collider.tag == targetTag) && (layerMask.value & (1 << collision.gameObject.layer)) > 0)
            {
                bool validContact = false;
                foreach(ContactPoint2D contactPoint in collision.contacts)
                {
                    if (contactPoint.normal.x < 0 && right)
                    {
                        validContact = true;
                        break;
                    }
                    if (contactPoint.normal.x > 0 && left)
                    {
                        validContact = true;
                        break;
                    }
                    if (contactPoint.normal.y < 0 && top)
                    {
                        validContact = true;
                        break;
                    }
                    if (contactPoint.normal.x > 0 && bottom)
                    {
                        validContact = true;
                        break;
                    }
                }
                if (!validContact)
                {
                    return;
                }

                foreach (ActionDelegate action in collisionActions)
                {
                    if (action != null)
                    {
                        action.Perform(gameObject);
                        action.Perform(gameObject, collision.gameObject);
                    }
                }
            }
        }

        bool processTriggerEvent(Collider2D collision)
        {
            return enabled && !collision.isTrigger && (targetTag == "" || collision.tag == targetTag) && (layerMask.value & (1 << collision.gameObject.layer)) > 0;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            
            if (processTriggerEvent(collision))
            {
                triggerCount++;

                if (triggerCount == 1)
                {
                    foreach (ActionDelegate action in triggerEnterActions)
                    {
                        if (action != null)
                        {
                            action.Perform(gameObject);
                            action.Perform(gameObject, collision.gameObject);
                            action.Perform(gameObject, true);
                        }
                    }

                    if (deactivateOnEnter)
                    {
                        enabled = false;
                        SaveState();
                    }
                }  
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (processTriggerEvent(collision))
            {
                triggerCount--;
                
                if (triggerCount == 0)
                {
                    foreach (ActionDelegate action in triggerExitActions)
                    {
                        if (action != null)
                        {
                            action.Perform(gameObject);
                            action.Perform(gameObject, false);
                        }
                    }

                    if (deactivateOnEnter)
                    {
                        enabled = false;
                        SaveState();
                    }
                }
            }
        }

        GUIStyle guiStyle = new GUIStyle();

        private void OnDrawGizmos()
        {
            Collider2D coll = GetComponent<Collider2D>();

            EditorUtilities.DrawCollider(coll, strokeColor, fillColor);
        }
    }
}

