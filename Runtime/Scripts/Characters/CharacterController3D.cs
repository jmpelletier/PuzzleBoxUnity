
/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using UnityEngine;

namespace PuzzleBox
{
    public class CharacterController3D : KinematicMotion3D
    {
        #region PUBLIC_PROPERTIES
        [Header("�ړ�")]

        public float walkSpeed = 3f;
        public float walkAcceleration = 10f;

        [PuzzleBox.Overridable]
        public float runSpeed = 10f;
        public float runAcceleration = 10f;
        public float breakingForce = 10f;

        [Space]
        [PuzzleBox.Overridable]
        public float airSpeed = 2f;
        public float airAcceleration = 10f;
        public float airBreakingForce = 10f;

        [Space]
        [Min(0)]
        public float hangSpeed = 1f;
        public float hangGravityRatio = 0.1f;

        [Space]
        [Tooltip("�v���[���[���͂̕����𔽓]�������ꍇ�A������̊Y�����鎲���u-1�v�ɂ���B")]
        public Vector2 movementInputScaling = Vector2.one;


        [Header("�W�����v")]

        [PuzzleBox.Overridable]
        public bool canJump = true;

        // �W�����v�̍ŏ��̍����B�v���[���[���W�����v�{�^������u�ŗ������ꍇ�A
        // �L�����N�^�[�͂��̍����܂Ŕ�т܂��B
        [Min(0.1f)]
        [PuzzleBox.Overridable]
        public float minJumpHeight = 1f;

        // �W�����v�̍ō��̍����ł��B�v���[���[�������ƃW�����v�{�^���������������
        // ���̍����܂Ŕ�т܂��B
        [Min(0.1f)]
        [PuzzleBox.Overridable]
        public float maxJumpHeight = 3f;

        [PuzzleBox.Overridable]
        public float jumpAngle = 0f;

        // �����ɂ��W�����v�����̒���
        public float jumpHeightSpeedBoost = 2f;



        // enum���`����ƃC���X�y�N�^�[�Ńh���b�v�_�E�����j���[��\�������鎖���ł��܂��B
        // �����́A�W�����v�̓����؂�ւ���悤�ɂ��Ă����܂��B
        // �W�����v�̍����𒲐�������@�͂���������܂����A�����ł͍ł����R�ł悭
        // �g������@���̗p���܂��B�W�����v���ɃL�����N�^�[�ɂ�����d�̗͂͂𒲐�
        // ���鎖�ɂ���āA�W�����v�̍�����ς��鎖���ł��܂��B
        // �����́uSlow�v��I�ԂƁA�W�����v�{�^���𗣂������ɁA�d�͂���߂܂��B
        // �uFast�v�ɂ���ƁA�t�Ƀ{�^���𗣂������ɁA�d�͂��������܂��B
        public enum JumpMode
        {
            Slow,
            Fast
        }

        public JumpMode mode = JumpMode.Fast;


        // �Q�[���̉��ʓx�𒲐����邽�߂ɁA�󒆂ł����n���O�Ȃ�W�����v���\��
        // ����H�v������܂��B�����ł͂��̋����̂������l��ݒ肵�܂��B
        public float jumpGroundCheckDistance = 0.2f;

        public float jumpBufferTime = 0.04f;

        // �_�u���W�����v�E�g���v���W�����v�������Ȃ炱���ŋ󒆂łł���W�����v�̉񐔂�
        // �w�肵�܂��B
        [PuzzleBox.Overridable]
        public int maxAirJumps = 1;

        // �ʏ�̃W�����v�ɔ�ׂẴ_�u���W�����v�̍����i�䗦�j
        [PuzzleBox.Overridable]
        public float airJumpHeightRatio = 0.5f;

        // �����ȂǂŒn�ʂ��痣�ꂽ����ł��W�����v�������܂��B�����ł��̎��Ԃ̂������l�i�b�j���w�肵�܂��B
        public float fallJumpTimeLimit = 3f / 60f;

        public enum State
        {
            OnGround,
            Jumping,
            Falling
        }

        //[NonSerialized]
        public State state = State.OnGround;

        public bool isJumping { get; private set; }
        public bool isRunning { get; private set; }

        #endregion

        #region PROTECTED_PROPERTIES

        #endregion

        #region PRIVATE_PROPERTIES

        // Buffered input for jumping
        private Utils.BufferedInput<bool> jumpInput = new Utils.BufferedInput<bool>(0.04f);

        // Used to disable input (for instance after dying).
        private bool acceptInput = true;

        private Vector2 motionInput = Vector2.zero;
        private Vector2 rawMotionInput = Vector2.zero;

        #endregion

        #region COMPONENTS

        Animator animationController;

        #endregion

        #region MONOBEHAVIOUR

        protected override void Start()
        {
            base.Start();

            animationController = GetComponent<Animator>();
        }

        protected override void PerformFixedUpdate(float deltaSeconds)
        {
            UpdateState();

            if (isGrounded)
            {
                ApplyGroundMotion();
            }
            else
            {
                ApplyAirMotion();
            }

            base.PerformFixedUpdate(deltaSeconds);

            UpdateFacingDirection();

            if (jumpInput.HasValue())
            {
                Jump(jumpInput.Get(), false);
            }

            Debug.DrawRay(transform.position, velocity.Get(), Color.red);
        }

        #endregion

        #region INPUT

        void OnJump(object val)
        {
            Jump(PuzzleBox.InputValue.IsPressed(val), true);
        }

        void OnMove(object val)
        {
            Move(PuzzleBox.InputValue.GetValue<Vector2>(val));
        }

        #endregion

        #region STATE_MACHINE
        private void UpdateState()
        {
            switch(state)
            {
                case State.OnGround:
                    if (!isGrounded)
                    {
                        state = State.Falling;
                    }
                    break;
                case State.Falling:
                    if (isJumping)
                    {
                        state = State.Jumping;
                    }
                    else if (isGrounded)
                    {
                        state = State.OnGround;
                    }
                    break;
                case State.Jumping:
                    if (isGrounded)
                    {
                        if (!isJumping)
                        {
                            state = State.OnGround;
                        }
                    }
                    else
                    {
                        if (!isJumping)
                        {
                            state = State.Falling;
                        }
                        else
                        {
                            float dot = Vector3.Dot(velocity, Physics.gravity);
                            if (dot > 0)
                            {
                                state = State.Falling;
                            }
                        }
                    }
                    break;
            }
        }

        #endregion

        #region MOTION

        public void Move(Vector2 input)
        {
            rawMotionInput = Vector2.Scale(input, movementInputScaling);

            if (!acceptInput) return;

            motionInput = rawMotionInput;
        }

        Vector3 BreakTowardsVelocity(Vector3 currentVelocity, Vector3 targetVelocity)
        {
            Vector3 velocityDelta = targetVelocity - currentVelocity;
            float velocityDifference = velocityDelta.magnitude;
            float increment = breakingForce * Time.fixedDeltaTime;

            if (increment >= velocityDifference)
            {
                return targetVelocity;
            }
            else
            {
                Vector3 breakDirection = Geometry.ProjectVectorOnPlane(velocityDelta.normalized, groundNormal);
                return currentVelocity + breakDirection * increment;
            }
        }

        void ApplyGroundMotion()
        {
            // �n�ʂɗ����Ă���Ȃ�󒆂̃W�����v�񐔂����Z�b�g���܂��B
            airJumps = 0;

            // �d�͂̉����͒ʏ�ɖ߂��܂��B
            gravityModifier = normalGravityMultiplier;

            Vector3 v = velocity.Get();

            Debug.DrawRay(transform.position, v, Color.red);

            if (state == State.OnGround)
            {
                // Check to see if there is motion input in this frame
                if (motionInput.magnitude > 0)
                {
                    // Project input vector on ground plane
                    Vector3 input3d = new Vector3(motionInput.x, 0, motionInput.y);
                    Vector3 inputGroundProjection = Geometry.ProjectVectorOnPlane(input3d, groundNormal);

                    // Get the correct acceleration value
                    float acceleration = isRunning ? runAcceleration : walkAcceleration;

                    // Get the velocity relative to external forces such as moving ground or wind
                    Vector3 relativeVelocity = v - externalVelocity;

                    // Since we are on the ground, project the velocity vector on the ground plane
                    Vector3 groundVelocity = Geometry.ProjectVectorOnPlane(relativeVelocity, groundNormal);

                    // Figure out how fast we are going
                    float speed = groundVelocity.magnitude;

                    // Get the correct speed limit
                    float maxSpeed = isRunning ? runSpeed : walkSpeed;

                    // Are we over the speed limit?
                    bool overSpeedLimit = speed > maxSpeed;

                    // Should we break? We only break if we are over the speed limit and 
                    // trying to accelerate in the direction of velocity (which would make us go faster).
                    bool shouldBreak = (overSpeedLimit && Vector3.Dot(inputGroundProjection, groundVelocity) > 0);

                    if (shouldBreak)
                    {
                        // Break towards the speed limit
                        Vector3 targetVelocity = groundVelocity.normalized * maxSpeed;
                        relativeVelocity = BreakTowardsVelocity(relativeVelocity, targetVelocity);
                        v = relativeVelocity + externalVelocity;
                    }
                    else
                    {
                        // Accelerate, but only up to the speed limit
                        Vector3 velocityChange = inputGroundProjection * acceleration * Time.fixedDeltaTime;
                        Vector3 targetVelocity = groundVelocity + velocityChange;
                        float targetSpeed = targetVelocity.magnitude;
                        if (targetSpeed > maxSpeed)
                        {
                            targetVelocity = targetVelocity.normalized * maxSpeed;
                        }
                        v = targetVelocity + externalVelocity;
                    }
                }
                else
                {
                    // No motion input, break
                    v = BreakTowardsVelocity(v, externalVelocity);
                }
            }

            velocity.Set(v);
        }

        void ApplyAirMotion()
        {

        }

        void UpdateFacingDirection()
        {

        }

        #endregion

        #region JUMP

        int airJumps = 0;

        bool isAirJump = false;
        float normalGravityMultiplier = 1f;
        float jumpGravityMultiplier = 1f;
        float breakGravityMultiplier = 1f;

        private bool CanJump()
        {
            isAirJump = false;

            if (!canJump || minJumpHeight <= 0 || maxJumpHeight <= 0)
            {
                return false;
            }

            switch(state)
            {
                 case State.OnGround: 
                    return true;
                case State.Jumping:
                case State.Falling:
                    if (timeInAir <= fallJumpTimeLimit)
                    {
                        // �n�ʂ��痣�ꂽ����
                        return true;
                    }
                    else if (maxAirJumps > 0 && airJumps < maxAirJumps)
                    {
                        // �󒆃W�����v�̏���ɒB���Ă��Ȃ��̂ŃW�����v�ł��܂��B
                        isAirJump = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }

            return true;
        }

        private Vector3 CalculateJumpVector(float jumpVelocity)
        {
            Vector3 jumpDir = -Physics.gravity.normalized;

            return jumpDir * jumpVelocity;
        }

        public void Jump(bool jumpState, bool bufferInput = true)
        {
            if (!acceptInput) return;

            if (jumpState) // Start jumping
            {
                // Check to see if we can jump...
                if (CanJump())
                {
                    float jumpSpeed = 0; // �W�����v�̏������x�B���ꂩ��v�Z���܂��B

                    float maxSpeed = Mathf.Max(walkSpeed, runSpeed);
                    Vector3 horizontalVelocity = Geometry.ProjectVectorOnPlane(velocity, groundNormal);
                    float horizontalSpeed = horizontalVelocity.magnitude;
                    float speedRatio = horizontalSpeed / maxSpeed;
                    float adjustedMaxJumpHeight = maxJumpHeight + speedRatio * jumpHeightSpeedBoost;
                    if (isAirJump)
                    {
                        adjustedMaxJumpHeight *= airJumpHeightRatio;
                    }

                    float gravityMagnitude = Physics.gravity.magnitude * gravityMultiplier;

                    if (mode == JumpMode.Fast)
                    {
                        // �����́A�Ε����˂̌v�Z�ł��B�ʏ�̏d�͂�maxJumpHeight�̍����ɒB���邽�߂̏������x�����߂܂��B
                        jumpSpeed = Mathf.Sqrt(2f * gravityMagnitude * normalGravityMultiplier * adjustedMaxJumpHeight);
                        jumpGravityMultiplier = normalGravityMultiplier; // ���̃��[�h�ł́A�W�����v�̏d�͂��ʏ�d�͂ɂȂ�܂��B
                        breakGravityMultiplier = normalGravityMultiplier * (adjustedMaxJumpHeight / minJumpHeight); // �W�����v���~�߂�d�͂̉������v�Z���܂��B

                    }
                    else if (mode == JumpMode.Slow)
                    {
                        // ��L�Ɠ��l�̌v�Z�ł����A�����𔽓]���܂��B
                        jumpSpeed = Mathf.Sqrt(2f * gravityMagnitude * normalGravityMultiplier * minJumpHeight);
                        jumpGravityMultiplier = normalGravityMultiplier * (minJumpHeight / adjustedMaxJumpHeight);
                        breakGravityMultiplier = normalGravityMultiplier;
                    }

                    // �����͏����ׂ��������ł��BKinematicMotion2D�̎����ɂ���āA�W�����v�ɂ��ړ����n�܂�O�ɁA�d�͂ɂ����
                    // ���x���������Ă��܂��܂��B�P�t���[���ڂ̏d�͂̉e�����Ȃ������߂ɁA�W�����v���x���グ�܂��B
                    jumpSpeed += gravityMagnitude * jumpGravityMultiplier * Time.fixedDeltaTime;

                    // Calculate the jump impulse
                    Vector3 jumpImpulse = CalculateJumpVector(jumpSpeed);

                    Vector3 horizontalVel = Geometry.ProjectVectorOnPlane(velocity, upDirection);

                    velocity.Set(jumpImpulse + horizontalVel);


                    state = State.Jumping;

                    // �d�͒�����K�p���܂��B
                    gravityModifier = jumpGravityMultiplier;

                    airJumps++; // �W�����v�̉񐔂𑝂₵�܂��B
                    isJumping = true; // �W�����v���Ă��鎖���L�����܂��BDash

                    jumpInput.Reset();

                    if (animationController != null)
                    {
                        animationController?.SetTrigger("Jump");
                    }
                    
                    SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);

                    // �n�ʂɗ����Ă��邩�ǂ����̔���������I�ɂ������܂��B
                    // �������Ȃ��ƁA�Ζʂ̏�ŃW�����v�������ɃW�����v����̈ړ�������
                    // ���ɂ���Ă��܂��܂��B
                    UpdateGroundedState();
                }
                else if (bufferInput)
                {
                    jumpInput.Set(jumpState, jumpBufferTime);
                }
            }
            else // �W�����v�I��
            {
                // �W�����v���I������̂ŁA�d�͂̉�����ς��܂��B
                gravityModifier = breakGravityMultiplier;
                isJumping = false;
            }
        }

        #endregion
    }
}

