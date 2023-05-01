using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PuzzleBox
{
    [RequireComponent(typeof(KinematicMotion2D))]
    public class MoveBehavior : MonoBehaviour
    {
        [Header("ínè„")]
        public float walkSpeed = 3f;
        public float walkAcceleration = 10f;
        public float runSpeed = 10f;
        public float runAcceleration = 10f;
        public float breakingForce = 10f;
        public bool isRunning = false;

        [Header("ãÛíÜ")]
        public float airSpeed = 2f;
        public float airAcceleration = 10f;
        public float airBreakingForce = 10f;

        KinematicMotion2D motion2D;

        Vector2 motionInput;

        public void Move(Vector2 input)
        {
            motionInput = input;
        }

        void OnMove(InputValue val)
        {
            Move(val.Get<Vector2>());
        }

        void OnRun(InputValue val)
        {
            isRunning = val.isPressed;
        }

        // Start is called before the first frame update
        void Start()
        {
            motion2D = GetComponent<KinematicMotion2D>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (motion2D.isGrounded)
            {
                ApplyGroundMotion();
            }
            else
            {
                ApplyAirMotion();
            }
        }

        void ApplyAirMotion()
        {
            float horizontalVelocity = motion2D.velocity.x;
            float horizontalDirection = Mathf.Sign(horizontalVelocity);
            float groundHorizontalDirection = Mathf.Sign(motion2D.lastGroundVelocity.x);
            float airSpeedX = Mathf.Abs(horizontalVelocity);
            float speedLimit = airSpeed;

            if (Mathf.Abs(motionInput.x) > 0)
            {
                Vector2 velocityChange = Vector2.right * motionInput.x * airAcceleration * Time.fixedDeltaTime;
                motion2D.velocity += velocityChange;
            }
            else
            {
                float breakDirection = horizontalDirection * -1f;
                Vector2 velocityChange = breakDirection * Vector2.right * Mathf.Min(airBreakingForce * Time.fixedDeltaTime, Mathf.Abs(horizontalVelocity));
                motion2D.velocity += velocityChange;
            }

            if (horizontalDirection == groundHorizontalDirection)
            {
                speedLimit = Mathf.Max(Mathf.Abs(motion2D.lastGroundVelocity.x), speedLimit);
            }

            if (airSpeedX > speedLimit)
            {
                Vector2 velocityChange = Vector2.right * (airSpeedX - speedLimit) * -horizontalDirection;
                motion2D.velocity += velocityChange;
            }
        }

        void ApplyGroundMotion()
        {
            if (Mathf.Abs(motionInput.x) > 0)
            {
                float acceleration = isRunning ? runAcceleration : walkAcceleration;
                Vector2 velocityChange = motion2D.groundRight * motionInput.x * acceleration * Time.fixedDeltaTime;
                motion2D.velocity += velocityChange;
            }
            else
            {
                Vector2 groundVelocity = motion2D.groundRight * Vector2.Dot(motion2D.groundRight, motion2D.velocity);
                Vector2 breakDirection = groundVelocity.normalized * -1f;
                float groundSpeed = groundVelocity.magnitude;

                Vector2 velocityChange = breakDirection * Mathf.Min(breakingForce * Time.fixedDeltaTime, groundSpeed);
                motion2D.velocity += velocityChange;
            }

            Vector2 velocity = motion2D.groundRight * Vector2.Dot(motion2D.groundRight, motion2D.velocity);
            float speed = velocity.magnitude;
            float maxSpeed = isRunning ? runSpeed : walkSpeed;

            if (speed > maxSpeed)
            {
                Vector2 breakDirection = velocity.normalized * -1f;
                Vector2 velocityChange = breakDirection * (speed - maxSpeed);
                motion2D.velocity += velocityChange;
            }
        }
    }
}

