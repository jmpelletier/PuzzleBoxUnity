using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
     [AddComponentMenu("Puzzle Box/Side-Scroll/Trajectory 2D")]
    [RequireComponent(typeof(KinematicMotion2D))]
    public class Trajectory2D : MonoBehaviour
    {
        public enum Mode
        {
            Loop,
            Reverse
        }

        public Transform[] waypoints;
        public float speed = 1f;
        public Mode mode = Mode.Loop;

        int currentPoint = 0;
        int direction = 1;

        KinematicMotion2D motion2D;

        void FixedUpdate()
        {
            if (waypoints.Length > 0)
            {
                Vector2 target = waypoints[currentPoint].position;
                Vector2 delta =  target - motion2D.position;
                float distance = delta.magnitude;
                float travelDistance = speed * Time.fixedDeltaTime;
                float s = speed;
                if (distance <= travelDistance)
                {
                    s = distance / Time.fixedDeltaTime;
                    currentPoint += direction;
                    if (currentPoint >= waypoints.Length || currentPoint < 0)
                    {
                        if (mode == Mode.Loop)
                        {
                            currentPoint = 0;
                        }
                        else
                        {
                            direction *= -1;
                            currentPoint += 2 * direction;
                        }
                    }
                }

                motion2D.velocity = delta.normalized * s;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            motion2D = GetComponent<KinematicMotion2D>();
        }

        private void OnDrawGizmosSelected()
        {
            if (isActiveAndEnabled)
            {
                Gizmos.color = Color.green;

                Vector3 p = transform.position;
                foreach(Transform t in waypoints)
                {
                    if (t != null)
                    {
                        Gizmos.DrawLine(p, t.position);
                        p = t.position;
                    }
                }
            }
        }
    }
} // namespace

