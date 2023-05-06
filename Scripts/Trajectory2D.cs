using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    [RequireComponent(typeof(KinematicMotion2D))]
    public class Trajectory2D : MonoBehaviour
    {
        public Transform[] waypoints;
        public float speed = 1f;

        int currentPoint = 0;

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
                    currentPoint++;
                    if (currentPoint >= waypoints.Length)
                    {
                        currentPoint = 0;
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

        // Update is called once per frame
        void Update()
        {

        }
    }
} // namespace

