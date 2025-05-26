/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

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
        public float margin = 0.002f;

        [Min(0f)]
        public float maxGroundAngle = 45f;

        [Min(0f)]
        public float maxCeilingAngle = 45f;

        [Min(0f)]
        public float maxStepHeight = 0.1f;

        [Min(0)]
        public int maxIterations = 2;

        [Header("Speed")]

        [Min(0)]
        public float smallDistanceThreshold = 0.00001f;

        public ObservableVector3 velocity = new ObservableVector3();

        [HideInInspector]
        public Vector3 externalVelocity = Vector3.zero;

        public float timeInAir { get; private set; }

        public Vector3 groundNormal = Vector3.up;

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

        protected enum SurfaceType
        {
            Ground,
            Wall,
            Ceiling
        }

        protected const int MAX_COLLISIONS = 32;
        
        protected Collider[] collidedColliders = new Collider[MAX_COLLISIONS];

        protected Collider[] colliders;

        protected RaycastHit[] raycastHits = new RaycastHit[MAX_COLLISIONS];

        protected Vector3 upDirection = Vector3.up;
        float groundThreshold = 0f;
        float ceilingThreshold = 0f;

        #endregion

        #region PRIVATE_PROPERTIES

        // Threshold to deal with precision issues
        const float SMALL_VALUE_THRESHOLD = 0.0001f;

        // The distance traveled in a single frame.
        private float distanceTraveled = 0f;

        // The last direction of movement (when sliding)
        private Vector3 movementDirection = Vector3.forward;

        #endregion

        #region MONOBEHAVIOUR

        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;

            FindColliders();
        }

        private void FindColliders()
        {
            colliders = GetComponentsInChildren<Collider>().Where(c => !c.isTrigger).ToArray();
        }

        protected override void PerformFixedUpdate(float deltaSeconds)
        {
            // Update the up direction (we assume gravity can change).
            upDirection = Physics.gravity.normalized * -1f;
            groundThreshold = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            ceilingThreshold = -Mathf.Cos(maxCeilingAngle * Mathf.Deg2Rad);

            if (!simulatePhysics) return;

            ApplyForces(deltaSeconds);

            //ResolveOverlaps();
            
            //ApplyGravity(deltaSeconds);
            //LimitVelocity();

            //Vector3 newPosition = CalculateNewPosition(deltaSeconds);
            //Vector3 delta = newPosition - rb.position;

            //Vector3 actualVelocity = delta.normalized * distanceTraveled / deltaSeconds;
            //Vector3 actualVelocity = delta / deltaSeconds;
            //Vector3 actualVelocity = movementDirection * distanceTraveled / deltaSeconds;

            //MoveRigidbodyToPosition(newPosition);

            UpdateGroundedState();

            if (!isGrounded)
            {
                timeInAir += deltaSeconds;
            }
            else
            {
                timeInAir = 0f;
            }

            //velocity.Set(actualVelocity);
        }

        #endregion

        #region UTILITIES

        public Bounds GetBounds()
        {
            Bounds bounds = default;
            if (colliders == null || colliders.Length == 0)
            {
                FindColliders();
            }

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

        public Vector3 position
        {
            get
            {
                if (rb == null)
                {
                    rb = GetComponent<Rigidbody>();
                }
                return rb.position;
            }
        }

        protected void ResolveOverlaps(int iterations = 0)
        {
            if (iterations > maxIterations) return;

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
            float dot = Vector3.Dot(normal, upDirection);
            float angle = Mathf.Abs(Mathf.Acos(dot) * Mathf.Rad2Deg);
            return angle <= maxGroundAngle;
        }

        bool IsCeilingNormal(Vector3 normal)
        {
            float dot = Vector3.Dot(normal, upDirection);
            float angle = Mathf.Abs(Mathf.Acos(-dot) * Mathf.Rad2Deg);
            return angle <= maxCeilingAngle;
        }

        #endregion

        #region PHYSICS

        const int MaximumForceCount = 8;

        public interface IForce
        {
            public Vector3 Update(float deltaSeconds);
            public void SetFinalVelocity(Vector3 velocity);
        }

        private class GravityForce : IForce
        {
            Vector3 velocity;

            public Vector3 Update(float deltaSeconds)
            {
                velocity += Physics.gravity * deltaSeconds;
                return velocity;
            }

            public void SetFinalVelocity(Vector3 v)
            {
                // Gravity always points in the same direction
                velocity = Physics.gravity.normalized * v.magnitude;
            }

            public bool Slide()
            {
                return false;
            }

            public bool Step()
            {
                return false;
            }
        }

        private IForce[] _forces = new IForce[MaximumForceCount];
        private GravityForce _gravity = new GravityForce();

        private Vector3 ApplyForce(Vector3 position, IForce force, float deltaSeconds, float modifier, bool isGravity)
        {
            Vector3 v = force.Update(deltaSeconds) * modifier;
            Vector3 delta = v * deltaSeconds;
            Vector3 newPosition = Slide(position, delta, maxStepHeight, isGravity, 0);
            delta = newPosition - position;
            force.SetFinalVelocity(delta / deltaSeconds);
            return newPosition;
        }

        private void ApplyForces(float deltaSeconds)
        {
            Vector3 position = rb.position;

            // Apply gravity first
            if (useGravity)
            {
                position = ApplyForce(position, _gravity, deltaSeconds, gravityModifier * gravityMultiplier, true);
            }

            // Apply the other forces
            for (int i = 0; i < MaximumForceCount; i++)
            {
                if (_forces[i] != null)
                {
                    position = ApplyForce(position, _forces[i], deltaSeconds, 1, false);
                }
                else
                {
                    break;
                }
            }

            MoveRigidbodyToPosition(position);
        }

        public IForce AddForce(IForce force)
        {
            for (int i = 0; i < MaximumForceCount; i++)
            {
                if (_forces[i] == null)
                {
                    _forces[i] = force;
                    return force;
                }
            }
            return null;
        }

        public void RemoveForce(IForce force)
        {
            int i = 0;
            for (; i < MaximumForceCount; i++)
            {
                if (_forces[i] == force)
                {
                    _forces[i] = null;

                    while(i < MaximumForceCount - 2)
                    {
                        _forces[i] = _forces[i + 1];
                        i++;
                    }
                }
            }
        }

        protected SurfaceType GetSurfaceType(Vector3 normal)
        {
            float dot = Vector3.Dot(normal, upDirection);
            if (dot > groundThreshold) return SurfaceType.Ground;
            else if (dot < ceilingThreshold) return SurfaceType.Ceiling;
            else return SurfaceType.Wall;
        }

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
            groundNormal = upDirection;

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

        public bool CastAllColliders(Vector3 direction, float distance, out RaycastHit hit, Vector3 offset)
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
                float radius = sphere.radius * Mathf.Max(globalScale.x, Mathf.Max(globalScale.y, globalScale.z));
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

        private bool AvoidSteps(ref Vector3 position, ref Vector3 motionRemaining, float stepHeight, int iterations)
        {
            if (iterations < maxIterations)
            {
                Vector3 offset = position - rb.position;
                Vector3 newPosition = position;
                float distance = stepHeight;

                // Move up
                RaycastHit hit;
                if (CastAllColliders(upDirection, stepHeight + margin, out hit, offset))
                {
                    // We hit something, don't try to move further
                    return true;
                }

                newPosition += upDirection * distance;
                offset = newPosition - rb.position;

                // Try to move.
                // Here we could be tempted to slide into position. However, if we do so, this we cause
                // us to climb up walls. Furthermore, to avoid stepping up a wall, we must cast far enough
                // to make sure we do hit any slanted wall.
                float castDistance = stepHeight * Mathf.Tan(maxGroundAngle * Mathf.Deg2Rad);
                Vector3 horizontalMotion = PuzzleBox.Geometry.ProjectVectorOnPlane(motionRemaining, upDirection);
                
                float horizontalDistance = horizontalMotion.magnitude;
                float d = Mathf.Max(castDistance, horizontalDistance);

                if (d > 0 && CastAllColliders(horizontalMotion.normalized, castDistance, out hit, offset))
                {
                    // We hit something, don't try to move further
                    return true;
                }

                Vector3 verticalMotion = motionRemaining - horizontalMotion;
                float verticalDistance = verticalMotion.magnitude;
                if (verticalDistance > 0 && CastAllColliders(verticalMotion.normalized, verticalDistance, out hit, offset))
                {
                    // We hit something, don't try to move further
                    return true;
                }

                newPosition += motionRemaining;

                // Move back down
                offset = newPosition - rb.position;
                distance = stepHeight;
                if (CastAllColliders(-upDirection, distance, out hit, offset))
                {
                    distance = hit.distance;
                }

                position = newPosition - upDirection * distance;
                motionRemaining = Vector3.zero;
            }

            return false;
        }


        public Vector3 Slide(Vector3 position, Vector3 delta, float stepHeight, bool isGravity, int iterations = 0)
        {
            if (iterations > maxIterations)
            {
                return position;
            }

            float distance = delta.magnitude;

            if (distance > smallDistanceThreshold)
            {
                Vector3 direction = delta.normalized;
                Vector3 offset = position - rb.position;

                RaycastHit hit;
                if (CastAllColliders(direction, distance + margin, out hit, offset))
                {
                    float distanceToCollision = hit.distance - margin;

                    if (distanceToCollision <= distance)
                    {
                        // We actually hit something
                        if (pushObjects && hit.rigidbody != null && !hit.rigidbody.isKinematic)
                        {
                            Vector3 velocityChange = velocity - hit.rigidbody.linearVelocity;
                            hit.rigidbody.AddForceAtPosition(velocityChange * rb.mass, hit.point, ForceMode.Impulse);
                        }

                        Vector3 collisionDelta = direction * distanceToCollision;
                        Vector3 newPosition = position + collisionDelta;
                        float distanceRemaining = distance - distanceToCollision;
                        Vector3 motionRemaining = direction * distanceRemaining;

                        distanceTraveled += distanceToCollision;

                        SurfaceType surfaceType = GetSurfaceType(hit.normal);

                        if (surfaceType == SurfaceType.Ground)
                        {
                            // If this is gravity, this is as far as we go
                            if (isGravity)
                            {
                                return newPosition;
                            }
                            else
                            {
                                return Slide(newPosition, motionRemaining, 0, false, iterations + 1);
                            }
                        }
                        else if (surfaceType == SurfaceType.Ceiling)
                        {
                            return Slide(newPosition, motionRemaining, 0, false, iterations + 1);
                        }
                        else
                        {
                            // Try to step over
                            if (AvoidSteps(ref newPosition, ref motionRemaining, maxStepHeight, iterations + 1))
                            {
                                // We hit something and couldn't over. Proceed with slide.
                                // We only slide vertically downwards
                                bool isDownwardMotion = Vector3.Dot(motionRemaining, upDirection) < -SMALL_VALUE_THRESHOLD;

                                if (isDownwardMotion)
                                {
                                    // We slide normally
                                    Vector3 projection = Geometry.ProjectVectorOnPlane(motionRemaining, hit.normal);
                                    return Slide(newPosition, projection, 0, isGravity, iterations + 1);
                                }
                                else
                                {
                                    // We don't move up the slope
                                    Vector3 projection = Geometry.ProjectVectorOnPlane(motionRemaining, hit.normal);
                                    Vector3 horizontalMotion = Geometry.ProjectVectorOnPlane(projection, upDirection);
                                    return Slide(newPosition, horizontalMotion, 0, isGravity, iterations + 1);
                                }
                            }
                            else
                            {
                                // We avoided steps, just update position
                                return newPosition;
                            }
                        }
                    }
                }
            }

            // No hit, just update position
            distanceTraveled = distance;
            return position + delta;
        }

        protected void MoveRigidbodyToPosition(Vector3 newPosition)
        {
            rb.MovePosition(newPosition);
        }

        protected bool IsFalling
        {
            get
            {
                if (!isGrounded)
                {
                    return Vector3.Dot(velocity, upDirection) < 0;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion
    }
}

