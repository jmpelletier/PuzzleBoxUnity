/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace PuzzleBox
{
    [RequireComponent(typeof(Rigidbody))]
    public class KinematicMotion3D : PuzzleBoxBehaviour
    {
        #region PUBLIC_PROPERTIES

        public bool simulatePhysics = true;

        [Header("Gravity")]
        public bool useGravity = true;
        public float gravityMultiplier = 1f;
        public float maxGroundDistance = 0.02f;
        public ObservableBool isGrounded = new ObservableBool();

        [HideInInspector]
        public float gravityModifier = 1f; // Temporary modifications (e.g. jump)

        [Header("Collisions")]
        public LayerMask collisionMask = ~0;
        public bool pushObjects = true;

        [Min(0f)]
        public float margin = 0.0000001f;

        [Min(0f)]
        public float maxGroundAngle = 45f;

        [Header("Speed")]
        [Min(0)]
        public float maxSpeedUp = 100f;

        [Min(0)]
        public float maxSpeedDown = 100f;

        [Min(0)]
        public float maxSpeedSide = 100f;

        public ObservableVector3 velocity = new ObservableVector3();

        #endregion

        #region COMPONENTS
        Rigidbody rb;
        public new Rigidbody rigidbody
        {
            get
            {
                if (rb == null) rb = GetComponent<Rigidbody>();
                return rb;
            }
        }

        #endregion

        #region PROTECTED_PROPERTIES

        protected const int MAX_COLLISIONS = 32;
        
        protected Collider[] collidedColliders = new Collider[MAX_COLLISIONS];

        protected Collider[] colliders;

        protected RaycastHit[] raycastHits = new RaycastHit[MAX_COLLISIONS];

        protected Vector3 groundNormal = Vector3.up;

        #endregion


        #region MONOBEHAVIOUR

        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;

            colliders = GetComponentsInChildren<Collider>().Where(c => !c.isTrigger).ToArray();
        }

        protected override void PerformFixedUpdate(float deltaSeconds)
        {
            if (!simulatePhysics) return;

            ResolveOverlaps();
            
            ApplyGravity(deltaSeconds);
            LimitVelocity();

            Vector3 newPosition = CalculateNewPosition(deltaSeconds);
            Vector3 delta = newPosition - rb.position;
            Vector3 actualVelocity = delta / deltaSeconds;

            MoveRigidbodyToPosition(newPosition);

            UpdateGroundedState();

            velocity.Set(actualVelocity);
        }

        #endregion

        #region UTILITIES

        protected Bounds GetBounds()
        {
            Bounds bounds = default;

            if (colliders.Length > 0)
            {
                bounds = colliders[0].bounds;

                for (int i = 1; i < colliders.Length; i++)
                {
                    bounds.Encapsulate(colliders[i].bounds);
                }
            }

            return bounds;
        }

        protected void ResolveOverlaps(int iterations = 0)
        {
            const int MAX_ITERATIONS = 2;
            if (iterations > MAX_ITERATIONS) return;

            Bounds bounds = GetBounds();
            int count = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, collidedColliders);
            for (int i = 0; i < count; i++)
            {
                if (collidedColliders[i].gameObject != gameObject && !collidedColliders[i].transform.IsChildOf(transform))
                {
                    foreach (Collider coll in colliders)
                    {
                        Vector3 direction;
                        float penetrationDistance;
                        bool collidersOverlap = Physics.ComputePenetration(coll, coll.transform.position, coll.transform.rotation,
                            collidedColliders[i], collidedColliders[i].transform.position, collidedColliders[i].transform.rotation, out direction, out penetrationDistance);
                        if (collidersOverlap)
                        {
                            Vector3 correction = direction * (penetrationDistance + margin);
                            MoveRigidbodyToPosition(rb.position + correction);
                            ResolveOverlaps(iterations + 1);
                            return;
                        }
                    }
                }
            }
        }

        bool IsGroundNormal(Vector3 normal)
        {
            float dot = Vector3.Dot(normal, Physics.gravity.normalized);
            float angle = Mathf.Abs(Mathf.Acos(-dot) * Mathf.Rad2Deg);
            return angle <= maxGroundAngle;
        }

        #endregion


        #region PHYSICS
        protected void ApplyGravity(float deltaSeconds)
        {
            if (useGravity && gravityMultiplier > 0)
            {
                Vector3 v = velocity;
                v += Physics.gravity * gravityMultiplier * gravityModifier * deltaSeconds;
                velocity.Set(v);
            }
        }

        protected void UpdateGroundedState()
        {
            RaycastHit hit;
            groundNormal = Physics.gravity.normalized * -1f;

            if (CastAllColliders(-groundNormal, maxGroundDistance, out hit, Vector3.zero))
            {
                if (IsGroundNormal(hit.normal))
                {
                    isGrounded.Set(true);
                    groundNormal = hit.normal;
                    return;
                }
            }

            isGrounded.Set(false);
        }

        private bool IsValidRaycastHit(RaycastHit hit)
        {
            return hit.collider.gameObject != gameObject && !hit.collider.transform.IsChildOf(transform);
        }

        private bool GetClosestHit(RaycastHit[] hits, int count, out RaycastHit hit)
        {
            int closestHit = -1;
            for (int i = 0; i < count; i++)
            {
                if (IsValidRaycastHit(hits[i]))
                {
                    if (closestHit == -1) closestHit = i;
                    else if (hits[i].distance < hits[closestHit].distance) closestHit = i;
                }
            }
            if (closestHit > -1)
            {
                hit = hits[closestHit];
                return true;
            }
            else
            {
                hit = default;
                return false;
            }
        }

        private bool CastAllColliders(Vector3 direction, float distance, out RaycastHit hit, Vector3 offset)
        {
            // I tried using Rigidbody.SweepTestAll, but this method fails for large deltas,
            // when there are more than one colliders attached. Sometimes, a collider that
            // is not the closest is returned instead. Here, we first check for the total bounds
            // and if there is a collision, we test individual colliders to find the actual point
            // of collision.

            // First we try a box cast with the bounds.
            Bounds bounds = GetBounds();
            bounds.center += offset;

            int collisionCount = Physics.BoxCastNonAlloc(bounds.center, bounds.extents, direction, raycastHits, transform.rotation, distance, collisionMask, QueryTriggerInteraction.Ignore);
            if (GetClosestHit(raycastHits, collisionCount, out hit))
            {
                // We hit something, check individual colliders
                float closest = Mathf.Infinity;
                RaycastHit h;
                foreach (Collider collider in colliders)
                {
                    if (CastCollider(collider, direction, distance, out h, offset))
                    {
                        if (hit.distance < closest)
                        {
                            closest = hit.distance;
                            hit = h;
                        }
                    }
                }

                if (closest < distance)
                {
                    // We actually hit something
                    return true;
                }

            }
            // We didn't hit anything
            hit = default;
            return false;
        }

        private bool CastCollider(Collider collider, Vector3 direction , float distance, out RaycastHit hit, Vector3 offset)
        {
            int collisionCount;

            if (collider is BoxCollider)
            {
                BoxCollider box = (BoxCollider)collider;
                Vector3 extents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);
                collisionCount = Physics.BoxCastNonAlloc(box.bounds.center + offset, extents, direction, raycastHits, box.transform.rotation, distance, collisionMask, QueryTriggerInteraction.Ignore);
            }
            else if (collider is SphereCollider)
            {
                SphereCollider sphere = (SphereCollider)collider;
                // This will not work if the scale isn't the same in all directions
                Vector3 globalScale = sphere.transform.lossyScale;
                float radius = sphere.radius * Mathf.Min(globalScale.x, Mathf.Min(globalScale.y, globalScale.z));
                collisionCount = Physics.SphereCastNonAlloc(sphere.bounds.center + offset, radius, direction , raycastHits, distance, collisionMask, QueryTriggerInteraction.Ignore);
            }
            else if (collider is CapsuleCollider)
            {
                CapsuleCollider capsule = (CapsuleCollider)collider;
                Vector3 globalScale = capsule.transform.lossyScale;
                Vector3 p1, p2;
                
                float radius = capsule.radius;
                float halfHeight = Mathf.Max(0, capsule.height - 2 * radius) * 0.5f;

                switch (capsule.direction)
                {
                    case 0: // x
                        p1 = capsule.transform.TransformPoint(capsule.center + Vector3.right * halfHeight);
                        p2 = capsule.transform.TransformPoint(capsule.center + Vector3.left * halfHeight);
                        radius *= globalScale.x;
                        break;
                    case 1: // y
                        p1 = capsule.transform.TransformPoint(capsule.center + Vector3.up * halfHeight);
                        p2 = capsule.transform.TransformPoint(capsule.center + Vector3.down * halfHeight);
                        radius *= globalScale.y;
                        break;
                    default: // z
                        p1 = capsule.transform.TransformPoint(capsule.center + Vector3.forward * halfHeight);
                        p2 = capsule.transform.TransformPoint(capsule.center + Vector3.back * halfHeight);
                        radius *= globalScale.z;
                        break;
                }

                collisionCount = Physics.CapsuleCastNonAlloc(p1 + offset, p2 + offset, radius, direction, raycastHits, distance, collisionMask, QueryTriggerInteraction.Ignore);
                
            }
            else
            {
                collisionCount = Physics.BoxCastNonAlloc(collider.bounds.center + offset, collider.bounds.extents, direction, raycastHits, Quaternion.identity, distance, collisionMask, QueryTriggerInteraction.Ignore);
            }

            return GetClosestHit(raycastHits, collisionCount, out hit);
        }

        protected Vector3 Slide(Vector3 position, Vector3 delta, int iterations = 0)
        {
            const int MAX_ITERATIONS = 2;

            if (iterations > MAX_ITERATIONS)
            {
                return position;
            }

            float distance = delta.magnitude;

            if (distance > 0)
            {
                Vector3 direction = delta.normalized;
                Vector3 offset = position - rb.position;

                RaycastHit hit;
                if (CastAllColliders(direction, distance, out hit, offset))
                {
                    // We actually hit something
                    if (pushObjects && hit.rigidbody != null && !hit.rigidbody.isKinematic)
                    {
                        Vector3 deltav = velocity - hit.rigidbody.velocity;
                        hit.rigidbody.AddForceAtPosition(deltav * rb.mass, hit.point, ForceMode.Impulse);
                    }

                    float distanceToCollision = hit.distance - margin;
                    Vector3 newPosition = position + direction * distanceToCollision;
                    float distanceRemaining = distance - distanceToCollision;

                    Vector3 groundPlaneNormal = -Physics.gravity.normalized;
                    Vector3 d = direction * distanceRemaining;
                    Vector3 groundPlaneProjection = Geometry.ProjectVectorOnPlane(d, groundPlaneNormal);

                    // If we hit the ground, try to slide further, however we only do this if we 
                    // are not grounded yet. This is to avoid sliding down slopes when we land.
                    if (IsGroundNormal(hit.normal))
                    {
                        Vector3 newDelta = Geometry.ProjectVectorOnPlane(groundPlaneProjection, hit.normal);

                        return Slide(newPosition, newDelta);
                    }
                    else
                    {
                        Vector3 horizontalComponent = d - groundPlaneProjection;
                        Vector3 slidePosition = Slide(newPosition, horizontalComponent);

                        return slidePosition;
                    }
                }
            }

            // No hit, just update position
            return position + delta;
        }

        protected Vector3 CalculateNewPosition(float deltaSeconds)
        {
            Vector3 position = rb.position;

            Vector3 delta = velocity.Get() * deltaSeconds;
            
            return Slide(position, delta);
        }

        protected void LimitVelocity()
        {
            Vector3 v = velocity;
            float xSpeed = Mathf.Abs(v.x);
            float zSpeed = Mathf.Abs(v.z);
            if (xSpeed > maxSpeedSide) v.x = maxSpeedSide * Mathf.Sign(v.x);
            if (zSpeed > maxSpeedSide) v.z = maxSpeedSide * Mathf.Sign(v.z);
            if (v.y > maxSpeedUp) v.y = maxSpeedUp;
            if (v.y < -maxSpeedDown) v.y = -maxSpeedDown;

            velocity.Set(v);
        }

        protected void MoveRigidbodyToPosition(Vector3 newPosition)
        {
            //rb.position = newPosition;
            rb.MovePosition(newPosition);
        }

        #endregion
    }
}

