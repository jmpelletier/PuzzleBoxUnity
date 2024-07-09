
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
        [Header("移動")]

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
        [Tooltip("プレーヤー入力の方向を反転したい場合、こちらの該当する軸を「-1」にする。")]
        public Vector2 movementInputScaling = Vector2.one;


        [Header("ジャンプ")]

        [PuzzleBox.Overridable]
        public bool canJump = true;

        // ジャンプの最小の高さ。プレーヤーがジャンプボタンを一瞬で離した場合、
        // キャラクターはこの高さまで飛びます。
        [Min(0.1f)]
        [PuzzleBox.Overridable]
        public float minJumpHeight = 1f;

        // ジャンプの最高の高さです。プレーヤーがずっとジャンプボタンを押し続けると
        // この高さまで飛びます。
        [Min(0.1f)]
        [PuzzleBox.Overridable]
        public float maxJumpHeight = 3f;

        [PuzzleBox.Overridable]
        public float jumpAngle = 0f;

        // 助走によるジャンプ高さの調整
        public float jumpHeightSpeedBoost = 2f;



        // enumを定義するとインスペクターでドロップダウンメニューを表示させる事ができます。
        // ここは、ジャンプの動作を切り替えるようにしておきます。
        // ジャンプの高さを調整する方法はいくつかありますが、ここでは最も自然でよく
        // 使われる方法を採用します。ジャンプ中にキャラクターにかかる重力の力を調整
        // する事によって、ジャンプの高さを変える事ができます。
        // ここの「Slow」を選ぶと、ジャンプボタンを離した時に、重力を弱めます。
        // 「Fast」にすると、逆にボタンを離した時に、重力を強くします。
        public enum JumpMode
        {
            Slow,
            Fast
        }

        public JumpMode mode = JumpMode.Fast;


        // ゲームの何位度を調整するために、空中でも着地寸前ならジャンプを可能に
        // する工夫があります。ここではその距離のしきい値を設定します。
        public float jumpGroundCheckDistance = 0.2f;

        public float jumpBufferTime = 0.04f;

        // ダブルジャンプ・トリプルジャンプを許すならここで空中でできるジャンプの回数を
        // 指定します。
        [PuzzleBox.Overridable]
        public int maxAirJumps = 1;

        // 通常のジャンプに比べてのダブルジャンプの高さ（比率）
        [PuzzleBox.Overridable]
        public float airJumpHeightRatio = 0.5f;

        // 落下などで地面から離れた直後でもジャンプを許します。ここでその時間のしきい値（秒）を指定します。
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
            // 地面に立っているなら空中のジャンプ回数をリセットします。
            airJumps = 0;

            // 重力の加減は通常に戻します。
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
                        // 地面から離れた直後
                        return true;
                    }
                    else if (maxAirJumps > 0 && airJumps < maxAirJumps)
                    {
                        // 空中ジャンプの上限に達していないのでジャンプできます。
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
                    float jumpSpeed = 0; // ジャンプの初期速度。これから計算します。

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
                        // ここは、斜方投射の計算です。通常の重力でmaxJumpHeightの高さに達するための初期速度を求めます。
                        jumpSpeed = Mathf.Sqrt(2f * gravityMagnitude * normalGravityMultiplier * adjustedMaxJumpHeight);
                        jumpGravityMultiplier = normalGravityMultiplier; // このモードでは、ジャンプの重力が通常重力になります。
                        breakGravityMultiplier = normalGravityMultiplier * (adjustedMaxJumpHeight / minJumpHeight); // ジャンプを止める重力の加減を計算します。

                    }
                    else if (mode == JumpMode.Slow)
                    {
                        // 上記と同様の計算ですが、条件を反転します。
                        jumpSpeed = Mathf.Sqrt(2f * gravityMagnitude * normalGravityMultiplier * minJumpHeight);
                        jumpGravityMultiplier = normalGravityMultiplier * (minJumpHeight / adjustedMaxJumpHeight);
                        breakGravityMultiplier = normalGravityMultiplier;
                    }

                    // ここは少し細かい調整です。KinematicMotion2Dの実装によって、ジャンプによる移動が始まる前に、重力によって
                    // 速度が下がってしまいます。１フレーム目の重力の影響をなくすために、ジャンプ速度を上げます。
                    jumpSpeed += gravityMagnitude * jumpGravityMultiplier * Time.fixedDeltaTime;

                    // Calculate the jump impulse
                    Vector3 jumpImpulse = CalculateJumpVector(jumpSpeed);

                    Vector3 horizontalVel = Geometry.ProjectVectorOnPlane(velocity, upDirection);

                    velocity.Set(jumpImpulse + horizontalVel);


                    state = State.Jumping;

                    // 重力調整を適用します。
                    gravityModifier = jumpGravityMultiplier;

                    airJumps++; // ジャンプの回数を増やします。
                    isJumping = true; // ジャンプしている事を記憶します。Dash

                    jumpInput.Reset();

                    if (animationController != null)
                    {
                        animationController?.SetTrigger("Jump");
                    }
                    
                    SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);

                    // 地面に立っているかどうかの判定を強制的にし直します。
                    // そうしないと、斜面の上でジャンプした時にジャンプ直後の移動方向が
                    // 横にそれてしまいます。
                    UpdateGroundedState();
                }
                else if (bufferInput)
                {
                    jumpInput.Set(jumpState, jumpBufferTime);
                }
            }
            else // ジャンプ終了
            {
                // ジャンプが終わったので、重力の加減を変えます。
                gravityModifier = breakGravityMultiplier;
                isJumping = false;
            }
        }

        #endregion
    }
}

