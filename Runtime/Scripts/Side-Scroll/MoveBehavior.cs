using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PuzzleBox
{
     [AddComponentMenu("Puzzle Box/Side-Scroll/Move Behavior")]
    [RequireComponent(typeof(KinematicMotion2D))]
    public class MoveBehavior : MonoBehaviour
    {
        public bool freeze = false;

        [Header("地上")]
        public float walkSpeed = 3f;
        public float walkAcceleration = 10f;
        public float runSpeed = 10f;
        public float runAcceleration = 10f;
        public float breakingForce = 10f;
        public bool isRunning = false;

        [Header("空中")]
        public float airSpeed = 2f;
        public float airAcceleration = 10f;
        public float airBreakingForce = 10f;

        KinematicMotion2D motion2D;
        Animator animationController;

        float smallInputThreshold = 0.1f;

        public Vector2 motionInput { get; private set; }

        public void Move(Vector2 input)
        {
            motionInput = input;

            if (animationController != null)
            {
                if (input.magnitude > smallInputThreshold)
                {
                    animationController.SetFloat("FacingDirectionX", input.normalized.x);
                    animationController.SetFloat("FacingDirectionY", input.normalized.y);
                }
            }
        }

        void OnMove(InputValue val)
        {
            Move(val.Get<Vector2>());
        }

        void OnRun(InputValue val)
        {
            Run(val.isPressed);
        }

        public void Run(bool state)
        {
            isRunning = state;
        }


        // Start is called before the first frame update
        void Start()
        {
            motion2D = GetComponent<KinematicMotion2D>();
            animationController = GetComponent<Animator>();
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
        
        void Update()
        {
            if (animationController != null)
            {
                animationController.SetBool("IsRunning", isRunning);
            }
        }

        void ApplyAirMotion()
        {
            float horizontalVelocity = motion2D.velocity.x;
            float horizontalDirection = Mathf.Sign(horizontalVelocity);
            float groundHorizontalDirection = Mathf.Sign(motion2D.lastGroundVelocity.x);
            float airSpeedX = Mathf.Abs(horizontalVelocity);
            float speedLimit = freeze ? 0 : airSpeed;

            if (Mathf.Abs(motionInput.x) > 0)
            {
                Vector2 velocityChange = Vector2.right * motionInput.x * airAcceleration * Time.fixedDeltaTime;
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
                Vector2 breakDirection = motion2D.velocity.normalized * -1f;
                float groundSpeed = motion2D.velocity.magnitude;
                Vector2 velocityChange = breakDirection * Mathf.Min(breakingForce * Time.fixedDeltaTime, groundSpeed);
                motion2D.velocity += velocityChange;
            }

            Vector2 velocity = motion2D.groundRight * Vector2.Dot(motion2D.groundRight, motion2D.velocity);
            float speed = velocity.magnitude;
            float maxSpeed = freeze ? 0 : (isRunning ? runSpeed : walkSpeed);

            if (speed > maxSpeed)
            {
                Vector2 breakDirection = velocity.normalized * -1f;
                Vector2 velocityChange = breakDirection * (speed - maxSpeed);
                motion2D.velocity += velocityChange;
            }
        }

        public void Freeze(bool state)
        {
            freeze = state;
        }
    }
}