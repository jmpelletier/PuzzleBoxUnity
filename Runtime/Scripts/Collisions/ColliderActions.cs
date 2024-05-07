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
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace PuzzleBox
{
    [ExecuteAlways]
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
        public ObjectReference otherObjectReference;
        public bool useParentCollider = false;

        [Space]
        [Tooltip("一回反応したら無効にするか？")]
        public bool deactivateOnEnter = false;
        public bool deactivateOnExit = false;
        public bool deactivateOnCollision = false;

        [Header("Editor")]
        public Color strokeColor = new Color(0, 1, 0, 1);
        public Color fillColor = new Color(0, 1, 0, 0.1f);

        public int triggerCount = 0;

        private Collider2D fakeCollisionCollider = null;

        private class ColliderActionListener : MonoBehaviour
        {
            public ColliderActions parent;

            private void OnTriggerEnter2D(Collider2D collision)
            {
                parent?.OnTriggerEnter2D(collision);
            }

            private void OnTriggerExit2D(Collider2D collision)
            {
                parent?.OnTriggerExit2D(collision);
            }
        }

        private void Start()
        {
            triggerCount = 0;

            Collider2D coll = useParentCollider ? GetComponentInParent<Collider2D>() : GetComponent<Collider2D>();

            if (useParentCollider && Application.isPlaying)
            {
                if (coll != null && coll.gameObject != this.gameObject)
                {
                    ColliderActionListener listener = coll.gameObject.AddComponent<ColliderActionListener>();
                    listener.parent = this;
                }
            }

            if (Application.isPlaying && coll.isTrigger == false && coll != null)
            {
                // Fake contacts
                fakeCollisionCollider = CloneCollider(coll, coll.gameObject, 0.05f);
                fakeCollisionCollider.isTrigger = true;
            }
        }


        bool processCollisionEvent(Collider2D collision)
        {
            bool process = processTriggerEvent(collision);

            return process;
        }

        bool processTriggerEvent(Collider2D collision)
        {
            return enabled && !collision.isTrigger && (targetTag == "" || collision.tag == targetTag) && (layerMask.value & (1 << collision.gameObject.layer)) > 0;
        }

        bool IsValidContactPoint(Collider2D coll, Vector3 contact)
        {
            Vector3 center = coll.bounds.center;
            Vector3 delta = contact - center;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (contact.x > center.x && right)
                {
                    return true;
                }
                else if (contact.x < center.x && left)
                {
                    return true;
                }
            }
            else
            {
                if (contact.y > center.y && top)
                {
                    return true;
                }
                else if (contact.y < center.y && bottom)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (fakeCollisionCollider != null && processCollisionEvent(collision))
            {
                // Treat this as a collision
                Vector3 contact;
                if (GetContactPoint(fakeCollisionCollider, collision, out contact))
                {
                    if (IsValidContactPoint(fakeCollisionCollider, contact))
                    {
                        if (otherObjectReference != null)
                        {
                            otherObjectReference.referencedObject = collision.gameObject;
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

                return;
            }

            if (processTriggerEvent(collision))
            {
                triggerCount++;

                if (triggerCount == 1)
                {
                    if (otherObjectReference != null)
                    {
                        otherObjectReference.referencedObject = collision.gameObject;
                    }

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

                    if (otherObjectReference != null)
                    {
                        otherObjectReference.referencedObject = null;
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

        public override string GetIcon()
        {
            return "CollisionIcon";
        }

        public static bool GetContactPoint(Collider2D first, Collider2D second, out Vector3 point)
        {
            if (first != null && second != null)
            {
                if (first is BoxCollider2D)
                {
                    if (second is BoxCollider2D)
                    {
                        return GetContactPoint((BoxCollider2D)first, (BoxCollider2D)second, out point);
                    }
                    else if (second is CircleCollider2D)
                    {
                        return GetContactPoint((BoxCollider2D)first, (CircleCollider2D)second, out point);
                    }
                    else if (second is PolygonCollider2D)
                    {
                        return GetContactPoint((BoxCollider2D)first, (PolygonCollider2D)second, out point);
                    }
                }
                else if (first is CircleCollider2D)
                {
                    if (second is BoxCollider2D)
                    {
                        return GetContactPoint((BoxCollider2D)second, (CircleCollider2D)first, out point);
                    }
                    else if (second is CircleCollider2D)
                    {
                        return GetContactPoint((CircleCollider2D)first, (CircleCollider2D)second, out point);
                    }
                    else if (second is PolygonCollider2D)
                    {
                        return GetContactPoint((CircleCollider2D)first, (PolygonCollider2D)second, out point);
                    }
                }
                else if (first is PolygonCollider2D)
                {
                    if (second is BoxCollider2D)
                    {
                        return GetContactPoint((BoxCollider2D)second, (PolygonCollider2D)first, out point);
                    }
                    else if (second is CircleCollider2D)
                    {
                        return GetContactPoint((CircleCollider2D)second, (PolygonCollider2D)first, out point);
                    }
                    else if (second is PolygonCollider2D)
                    {
                        return GetContactPoint((PolygonCollider2D)second, (PolygonCollider2D)first, out point);
                    }
                }
            }

            point = Vector3.zero;
            return false;
        }

        public static bool GetContactPoint(BoxCollider2D first, BoxCollider2D second, out Vector3 point)
        {
            if (first != null && second != null)
            {
                Bounds boundsA = first.bounds;
                Bounds boundsB = second.bounds;

                float xMin = Mathf.Max(boundsA.min.x, boundsB.min.x);
                float yMin = Mathf.Max(boundsA.min.y, boundsB.min.y);
                float xMax = Mathf.Min(boundsA.max.x, boundsB.max.x);
                float yMax = Mathf.Min(boundsA.max.y, boundsB.max.y);

                Rect intersection = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
                point = intersection.center;
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        public static bool GetContactPoint(BoxCollider2D first, CircleCollider2D second, out Vector3 point)
        {
            if (first != null && second != null)
            {
                Vector3 center = second.bounds.center;
                Vector3 closest = first.bounds.ClosestPoint(center);
                point = closest;
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        public static bool GetContactPoint(BoxCollider2D first, PolygonCollider2D second, out Vector3 point)
        {
            throw new NotImplementedException();
        }

        public static bool GetContactPoint(CircleCollider2D first, CircleCollider2D second, out Vector3 point)
        {
            if (first != null && second != null)
            {
                Vector3 delta = second.bounds.center -  first.bounds.center;
                float d = delta.magnitude;
                point = first.bounds.center + delta.normalized * first.radius;
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        public static bool GetContactPoint(CircleCollider2D first, PolygonCollider2D second, out Vector3 point)
        {
            throw new NotImplementedException();
        }

        public static bool GetContactPoint(PolygonCollider2D first, PolygonCollider2D second, out Vector3 point)
        {
            throw new NotImplementedException();
        }

        public static Collider2D CloneCollider(Collider2D coll, GameObject gameObject, float sizeChange = 0)
        {
            Collider2D newColl = null;

            if (coll == null || gameObject == null)
            {
                return null;
            }

            if (coll is BoxCollider2D)
            {
                BoxCollider2D box = (BoxCollider2D)coll;
                BoxCollider2D newBox = gameObject.AddComponent<BoxCollider2D>();
                newBox.autoTiling = box.autoTiling;
                newBox.edgeRadius = box.edgeRadius;
                newBox.size = box.size + Vector2.one * sizeChange;
                newColl = newBox;
                
            }
            else if (coll is CircleCollider2D)
            {
                CircleCollider2D circle = (CircleCollider2D)coll;
                CircleCollider2D newCircle = gameObject.AddComponent<CircleCollider2D>();
                newCircle.radius = circle.radius + sizeChange;
                newColl = newCircle;
            }
            else if (coll is PolygonCollider2D)
            {
                PolygonCollider2D polygon = (PolygonCollider2D)coll;
                PolygonCollider2D newPolygon = gameObject.AddComponent<PolygonCollider2D>();
                newPolygon.autoTiling = polygon.autoTiling;
                newPolygon.pathCount = polygon.pathCount;
                newPolygon.points = (Vector2[])polygon.points.Clone();
                newPolygon.useDelaunayMesh = polygon.useDelaunayMesh;
                for (int i = 0; i < newPolygon.points.Length; i++)
                {
                    newPolygon.points[i] = newPolygon.points[i].normalized * (newPolygon.points[i].magnitude + sizeChange);
                }
                newColl = newPolygon;
            }

            if (newColl != null && coll != null)
            {
                newColl.callbackLayers = coll.callbackLayers;
                newColl.contactCaptureLayers = coll.contactCaptureLayers;
                newColl.excludeLayers = coll.excludeLayers;
                newColl.forceReceiveLayers = coll.forceReceiveLayers;
                newColl.forceSendLayers = coll.forceSendLayers;
                newColl.includeLayers = coll.includeLayers;
                newColl.isTrigger = coll.isTrigger;
                newColl.layerOverridePriority = coll.layerOverridePriority;
                newColl.offset = coll.offset;
                newColl.sharedMaterial = coll.sharedMaterial;
                newColl.usedByComposite = coll.usedByComposite;
                newColl.usedByEffector = coll.usedByEffector;
            }

            return newColl;
        }
    }
}

