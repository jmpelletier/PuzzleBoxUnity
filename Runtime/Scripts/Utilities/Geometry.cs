/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */


using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    public static class Geometry
    {
        public const float EPSILON = 0.0005f;

        /**
         * Returns the overlap between two bounds. If there is no overlap,
         * will return a zero-size Bounds.
         */
        public static Bounds Overlap(Bounds b1, Bounds b2)
        {
            Bounds ret = new Bounds();
            ret.size = Vector3.zero;

            if (b1.Intersects(b2))
            {
                Vector3 min = new Vector3(
                    Mathf.Max(b1.min.x, b2.min.x),
                    Mathf.Max(b1.min.y, b2.min.y),
                    Mathf.Max(b1.min.z, b2.min.z));

                Vector3 max = new Vector3(
                    Mathf.Min(b1.max.x, b2.max.x),
                    Mathf.Min(b1.max.y, b2.max.y),
                    Mathf.Min(b1.max.z, b2.max.z));

                ret.min = min;
                ret.max = max;
            }

            return ret;
        }

        /**
         * Returns the distance from a ray to the edge of a bounds.
         * If the ray does not intersect the bounds, float.PositiveInfinity
         * is returned. 
         * Bounds.IntersectRay exists, but for some reason, the distance 
         * returns if a ray originates from inside the bounds is very
         * counter-intuitive. This method makes no such distinction, and will
         * return a positive value regardless of whether the ray originates from
         * inside the bounds or not.
         */
        public static float Intersect(Bounds bounds, Ray ray)
        {
            float distance;
            if (bounds.Contains(ray.origin))
            {
                // For some reason, when the ray originates from
                // inside the bounds, we need to use the *reverse*
                // direction...
                ray.direction *= -1f;
                if (bounds.IntersectRay(ray, out distance))
                {
                    return - distance;
                }
            }
            else if (bounds.IntersectRay(ray, out distance))
            {
                return distance;
            }

            return float.PositiveInfinity;
        }

        /**
         * Given two potentially overlapping bounds, move targetBounds in the given direction so that the two
         * bounds are separated by at least offset.
         */
        public static Bounds Separate(Bounds targetBounds, Bounds obstacleBounds, Vector3 direction, float offset)
        {
            Bounds ret = new Bounds(targetBounds.center, targetBounds.size);
            if (targetBounds.Intersects(obstacleBounds))
            {
                Ray ray = new Ray(targetBounds.center, direction * -1f);
                float distance = Intersect(targetBounds, ray);
                if (distance != float.PositiveInfinity)
                {
                    Vector3 checkPoint = targetBounds.center + ray.direction * distance;
                    ray.origin = checkPoint;
                    ray.direction = direction;

                    distance = Intersect(obstacleBounds, ray);
                    if (distance != float.PositiveInfinity)
                    {
                        ret.center += direction * (distance + offset);
                    }
                }
            }

            return ret;
        }

        /**
         * Check whether a point lies on a segment defined by segmentStart and segmentEnd.
         */
        public static bool PointIsOnLineSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
        {
            Vector2 c = segmentEnd - segmentStart;
            Vector2 a = point - segmentStart;
            Vector2 b = point - segmentEnd;

            return (a.magnitude + b.magnitude) - c.magnitude < EPSILON;
        }

        private static bool CheckBounds(float a1, float a2, float b1, float b2)
        {
            float min1 = Mathf.Min(a1, a2);
            float max1 = Mathf.Max(a1, a2);
            float min2 = Mathf.Min(b1, b2);
            float max2 = Mathf.Max(b1, b2);

            return !(min1 > max2 || max1 < min2);
        }

        private static bool CheckNumerator(float numerator, float denominator)
        {
            if (denominator > 0)
            {
                if (numerator < 0 || numerator >= denominator + EPSILON)
                {
                    return false;
                }
            }
            else
            {
                if (numerator > 0 || numerator <= denominator - EPSILON)
                {
                    return false;
                }
            }
            return true;
        }

        /**
         * This methods checks whether two segments, P and Q, defined by the points p1, p2 and q1, q2 intersect.
         * If they do, the Vector2 intersection will hold the position at which the points intersect.
         * If there is no intersection, intersection will be Vector2.zero.
         */
        public static bool SegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            // First check bounding boxes
            if (!CheckBounds(p1.x, p2.x, q1.x, q2.x) || !CheckBounds(p1.y, p2.y, q1.y, q2.y))
            {
                return false;
            }

            Vector2 vecA = p2 - p1;
            Vector2 vecB = q1 - q2;
            Vector2 vecC = p1 - q1;

            float denominator = vecA.y * vecB.x - vecA.x * vecB.y;

            if (denominator < EPSILON && denominator > -EPSILON)
            {
                return false; // Parallel
            }

            float alpha = vecB.y * vecC.x - vecB.x * vecC.y;
            if (!CheckNumerator(alpha, denominator))
            {
                // Segments don't cross
                return false;
            }

            float beta = vecA.x * vecC.y - vecA.y * vecC.x;
            if (!CheckNumerator(beta, denominator))
            {
                // Segments don't cross
                return false;
            }

            float num = alpha * vecA.x;
            intersection.x = p1.x + (num) / denominator;

            num = alpha * vecA.y;
            intersection.y = p1.y + (num) / denominator;

            return true;
        }

        /**
         * Return the distance between a point and the closest point on the line segment
         * defined by the points segmentStart and segmentEnd. 
         * If there is no line normal to the segment that goes through the point,
         * then the value returned is float.PositiveInfinity.
         */
        public static float DistanceFromPointToLineSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
        {
            Vector2 delta = segmentEnd - segmentStart;
            float segmentLengthSquared = delta.sqrMagnitude;

            if (Mathf.Approximately(0, segmentLengthSquared))
            {
                return Vector2.Distance(point, segmentStart);
            }

            float u = ((point.x - segmentStart.x) * (delta.x) + (point.y - segmentStart.y) * (delta.y)) / segmentLengthSquared;

            if (u > 0 && u <= 1)
            {
                Vector3 intersection = segmentStart + delta * u;
                return Vector2.Distance(intersection, point);
            }
            else return float.PositiveInfinity;
        }

        /**
         * Returns the signed area of a polygon defined by the ordered
         * vertices provided. See:
         * https://en.wikipedia.org/wiki/Signed_area
         */
        public static float SignedArea(Vector2[] vertices)
        {
            float area = 0.0f;
            if (vertices != null)
            {
                int count = vertices.Length;
                if (count >= 3)
                {
                    for (int i = 0, j = 1; i < count - 1; i++, j++)
                    {
                        area += vertices[i].x * vertices[j].y;
                        area -= vertices[i].y * vertices[j].x;
                    }

                    area += vertices[count - 1].x * vertices[0].y;
                    area -= vertices[count - 1].y * vertices[0].x;
                }
                
            }
            return area;
        }

        public static Vector2[] OffsetPath(Vector2[] vertices, float offset)
        {
            if (vertices == null || vertices.Length < 3)
            {
                return new Vector2[0];
            }

            int count = vertices.Length;
            int resultCount = count * 2;
            Vector2[] result = new Vector2[resultCount];

            // Find out the ordering
            float direction = -Mathf.Sign(SignedArea(vertices));

            // Make the segments
            int j = 0;
            for (int i = 0; i < count - 1; i++, j+=2)
            {
                result[j] = vertices[i];
                result[j+1] = vertices[i+1];
            }
            result[j] = vertices[count - 1];
            result[j + 1] = vertices[0];

            

            // Offset the segments
            for (j = 0; j < resultCount; j += 2)
            {
                Vector2 edgeDirection = (result[j + 1] - result[j]).normalized;
                Vector2 offsetDirection = new Vector2(-edgeDirection.y, edgeDirection.x) * direction;
                Vector2 vertexOffset = offsetDirection * offset;
                result[j] += vertexOffset - edgeDirection * offset;
                result[j + 1] += vertexOffset + edgeDirection * offset;
            }

            // Find intersections
            for (j = 0; j < resultCount; j += 2)
            {
                int k = (j + 2) % resultCount;
                Vector2 intersection;
                if (SegmentsIntersect(result[j], result[j + 1], result[k], result[k + 1], out intersection))
                {
                    result[j + 1] = result[k] = intersection;
                }
            }

            // Remove zero-length segments
            List<Vector2> filteredResults = new List<Vector2>
            {
                result[0]
            };

            for (int i = 1; i < resultCount - 1; i++)
            {
                float d = Vector2.Distance(result[i], result[i - 1]);
                if (d > EPSILON)
                {
                    filteredResults.Add(result[i]);
                }
            }

            if (Vector2.Distance(result[resultCount - 1], result[resultCount - 2]) > EPSILON &&
                Vector2.Distance(result[resultCount - 1], result[0]) > EPSILON)
            {
                filteredResults.Add(result[resultCount - 1]);
            }

            return filteredResults.ToArray();
        }
    }
}

