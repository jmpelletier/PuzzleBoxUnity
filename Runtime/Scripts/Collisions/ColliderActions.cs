/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */


using UnityEngine;

namespace PuzzleBox
{
    [ExecuteAlways]
    [RequireComponent(typeof(UniqueID))]
    public class ColliderActions : PersistentBehaviour
    {
        public enum TriggerMode
        {
            Any,
            All
        }

        [Space]
        public TriggerMode triggerMode = TriggerMode.Any;

        [Space]
        public ActionDelegate[] triggerEnterActions;
        public ActionDelegate[] triggerExitActions;
        public ActionDelegate[] collisionActions;

        [Space]
        public string targetTag = string.Empty;
        public string ignoreTag = string.Empty;
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

        [Space]
        public float skinDepth = 0.1f;

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
                fakeCollisionCollider = CloneCollider(coll, coll.gameObject, skinDepth);
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
            return enabled &&
                !collision.isTrigger &&
                (targetTag == "" || collision.tag == targetTag) &&
                (ignoreTag == "" || collision.tag != ignoreTag) &&
                (layerMask.value & (1 << collision.gameObject.layer)) > 0;
        }

        bool IsValidContactPoint(Collider2D coll, Vector3 contact)
        {
            if (coll is BoxCollider2D || coll is PolygonCollider2D)
            {
                float d_top = Mathf.Abs(contact.y - coll.bounds.max.y);
                float d_bottom = Mathf.Abs(contact.y - coll.bounds.min.y);
                float d_left = Mathf.Abs(contact.x - coll.bounds.min.x);
                float d_right = Mathf.Abs(contact.x - coll.bounds.max.x);

                float min = d_top;
                int i = 0;

                if (d_bottom < min) { min = d_bottom; i = 1; }
                if (d_left < min) { min = d_bottom; i = 2; }
                if (d_right < min) { min = d_bottom; i = 3; }

                switch(i)
                {
                    case 0:
                        return top;
                    case 1:
                        return bottom;
                    case 2:
                        return left;
                    case 3:
                        return right;
                }
            }
            else if (coll is CircleCollider2D)
            {
                Vector3 center = coll.bounds.center;
                Vector3 delta = contact - center;
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                {
                    if (delta.y > 0 && top) return true;
                    if (delta.y < 0 && bottom) return true;
                }
                else
                {
                    if (delta.x > 0 && right) return true;
                    if (delta.x < 0 && left) return true;
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

                if (triggerMode == TriggerMode.Any && triggerCount == 1 || triggerMode == TriggerMode.All)
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
                
                if (triggerMode == TriggerMode.Any && triggerCount == 0 || triggerMode == TriggerMode.All)
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

        private static bool LineSegmentsIntersect(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            float tNumerator = (bStart.x - aStart.x) * (bEnd.y - bStart.y) - (bStart.y - aStart.y) * (bEnd.x - bStart.x);
            float uNumerator = (aStart.y - aEnd.y) * (aStart.x - bStart.x) + (aEnd.x - aStart.x) * (aStart.y - bStart.y);
            float denominator = (bEnd.y - bStart.y) * (aEnd.x - aStart.x) - (bEnd.x - bStart.x) * (aEnd.y - aStart.y);

            if (Mathf.Approximately(denominator, 0.0f))
            {
                // Lines are parallel
                return false;
            }

            float t = tNumerator / denominator;
            float u = uNumerator / denominator;

            if ((t >= 0.0f && t <= 1.0f) && (u >= 0.0f && u <= 1.0f))
            {
                // Segments intersect
                intersection = aStart + t * (aEnd - aStart);
                return true;
            }

            return false;
        }

        private static bool LineSegmentIntersectsCircle(Vector2 aStart, Vector2 aEnd, Vector2 center, float radius, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            // Calculate the direction vector of the line segment
            Vector2 dir = aEnd - aStart;

            // Calculate the difference between the start of the line segment and the circle center
            Vector2 diff = aStart - center;

            // Calculate parameters for the quadratic equation
            float a = dir.sqrMagnitude;
            float b = 2 * Vector2.Dot(diff, dir);
            float c = diff.sqrMagnitude - radius * radius;

            // Calculate the discriminant of the quadratic equation
            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                // No intersection
                return false;
            }
            else
            {
                // Calculate the t values where the line intersects the circle
                float sqrtDiscriminant = Mathf.Sqrt(discriminant);
                float t1 = (-b - sqrtDiscriminant) / (2 * a);
                float t2 = (-b + sqrtDiscriminant) / (2 * a);

                if ((t1 >= 0 && t1 <= 1) || (t2 >= 0 && t2 <= 1))
                {
                    // At least one intersection point is within the line segment
                    // Calculate the intersection point
                    if (t1 >= 0 && t1 <= 1)
                    {
                        intersection = aStart + t1 * dir;
                    }
                    else
                    {
                        intersection = aStart + t2 * dir;
                    }
                    return true;
                }
                else
                {
                    // Both intersection points are outside the line segment
                    return false;
                }
            }
        }

        public static void TransformPath(Vector2[] path, Transform parent)
        {
            for (int i = 0; i < path.Length; i++)
            {
                path[i] = parent.TransformPoint(path[i]);
            }
        }


        public static bool GetContactPoint(BoxCollider2D first, PolygonCollider2D second, out Vector3 point)
        {
            if (first != null && second != null)
            {
                Vector2 topLeft = new Vector2(first.bounds.min.x, first.bounds.max.y);
                Vector2 topRight = first.bounds.max;
                Vector2 bottomRight = new Vector2(first.bounds.max.x, first.bounds.min.y);
                Vector2 bottomLeft = first.bounds.min;

                Vector2[] boxPath = new Vector2[] {
                    bottomRight, topRight, topLeft, bottomLeft
                };

                Vector2 intersect;
                for (int p = 0; p < second.pathCount; p++)
                {
                    Vector2[] path = second.GetPath(p);
                    TransformPath(path, second.transform);

                    if (IntersectPaths(boxPath, path, out intersect))
                    {
                        point = intersect;
                        return true;
                    }
                }
            }

            point = Vector3.zero;
            return false;
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
            if (first != null && second != null)
            {
                Vector2 intersect;
                for (int p = 0; p < second.pathCount; p++)
                {
                    Vector2[] path = second.GetPath(p);
                    TransformPath(path, second.transform);

                    int pathLength = path.Length;
                    for (int i = pathLength - 1; i >= 0; i--)
                    {
                        Vector2 p1 = path[i];
                        Vector2 p2 = i > 0 ? path[i - 1] : path[pathLength - 1];

                        if (LineSegmentIntersectsCircle(p1, p2, first.bounds.center, first.radius, out intersect))
                        {
                            point = intersect;
                            return true;
                        }

                    }
                }
            }

            point = Vector3.zero;
            return false;
        }

        public static bool IntersectPaths(Vector2[] path1, Vector2[] path2, out Vector2 intersect)
        {
            intersect = Vector2.zero;

            int path1Length = path1.Length;
            int path2Length = path2.Length;

            for (int i = path1Length - 1; i >= 0; i--)
            {
                Vector2 p1 = path1[i];
                Vector2 p2 = i > 0 ? path1[i - 1] : path1[path1Length - 1];

                for (int j = path2Length - 1; j >= 0; j--)
                {
                    Vector2 p3 = path2[j];
                    Vector2 p4 = j > 0 ? path2[j - 1] : path2[path2Length - 1];

                    if (LineSegmentsIntersect(p1, p2, p3, p4, out intersect))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool GetContactPoint(PolygonCollider2D first, PolygonCollider2D second, out Vector3 point)
        {
            if (first != null && second != null)
            {
                Vector2 intersect;

                for (int i = 0; i < first.pathCount; i++)
                {
                    Vector2[] path1 = first.GetPath(i);
                    TransformPath(path1, first.transform);

                    for (int j = 0; j < second.pathCount; j++)
                    {
                        Vector2[] path2 = second.GetPath(j);
                        TransformPath(path2, second.transform);

                        if (IntersectPaths(path1, path2, out intersect))
                        {
                            point = intersect;
                            return true;
                        }
                    }
                }
            }

            point = Vector3.zero;
            return false;
        }

        private static Vector2 PathPointNormal(Vector2 previous, Vector2 point, Vector2 next)
        {
            Vector2 center = (previous + point + next) * (1f/3f);
            Vector2 delta = point - center;
            return delta.normalized;
        }

        private static Vector2 ExpandPointAlongNormal(Vector2 previous, Vector2 point, Vector2 next, float distance)
        {
            Vector2 normal = PathPointNormal(previous, point, next);
            return point + normal * distance;
        }

        public static Vector2[] ExpandPath(Vector2[] path, float distance)
        {
            int count = path.Length;
            if (count > 2)
            {
                Vector2[] newPath = new Vector2[count * 2];
                Vector2 center = Vector2.zero;
                foreach(Vector2 p in path)
                {
                    center.x += p.x;
                    center.y += p.y;
                }
                center /= count;

                int j = 0;
                Vector2 p1;
                Vector2 p2;
                Vector2 halfPoint;
                Vector2 delta;
                for (int i = 0; i < count - 1; i++, j += 2)
                {
                    p1 = path[i];
                    p2 = path[i + 1];
                    halfPoint = (p1 + p2) * 0.5f;
                    delta = (halfPoint - center).normalized * distance;
                    newPath[j] = p1 + delta;
                    newPath[j + 1] = p2 + delta;
                }

                p1 = path[count - 1];
                p2 = path[0];
                halfPoint = (p1 + p2) * 0.5f;
                delta = (halfPoint - center).normalized * distance;
                newPath[j] = p1 + delta;
                newPath[j + 1] = p2 + delta;

                return newPath;
            }

            return path;
        }

        public static Collider2D CloneCollider(Collider2D coll, GameObject gameObject, float sizeChange)
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
                newPolygon.useDelaunayMesh = polygon.useDelaunayMesh;

                Vector2[] points = new Vector2[polygon.points.Length];

                Vector2 center = Vector2.zero;
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = polygon.points[i].normalized * (polygon.points[i].magnitude + sizeChange);
                    center.x += polygon.points[i].x;
                    center.y += polygon.points[i].y;
                }

                if (points.Length > 0)
                {
                    center.x /= points.Length;
                    center.y /= points.Length;
                }

                newPolygon.pathCount = polygon.pathCount;

                for (int i = 0; i < polygon.pathCount; i++)
                {
                    Vector2[] pathPoints = Geometry.OffsetPath(polygon.GetPath(i), sizeChange);
                    newPolygon.SetPath(i, pathPoints);
                }
                
                newColl = newPolygon;
            }
            else if (coll is CapsuleCollider2D)
            {
                CapsuleCollider2D capsule = (CapsuleCollider2D)coll;

                CapsuleCollider2D newCapsule = gameObject.AddComponent<CapsuleCollider2D>();
                Vector2 s = capsule.transform.InverseTransformVector(new Vector2(sizeChange, sizeChange));
                newCapsule.size = capsule.size + new Vector2(s.x * 2, s.y * 2);
                newColl = newCapsule;
            }
            else if (coll is EdgeCollider2D)
            {
                EdgeCollider2D edge = (EdgeCollider2D)coll;
                EdgeCollider2D newEdge = gameObject.AddComponent<EdgeCollider2D>();
                newEdge.points = edge.points;
                newEdge.edgeRadius = edge.edgeRadius + sizeChange;
                newEdge.useAdjacentEndPoint = edge.useAdjacentEndPoint;
                newEdge.useAdjacentStartPoint = edge.useAdjacentStartPoint;
                newColl = newEdge;
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
            }

            return newColl;
        }
    }
}

