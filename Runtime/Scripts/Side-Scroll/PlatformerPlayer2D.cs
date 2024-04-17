/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;


namespace PuzzleBox
{
    public class PlatformerPlayer2D : KinematicMotion2D
    {
        [Header("移動")]
        //[Space]
        public float walkSpeed = 3f;
        public float walkAcceleration = 10f;
        public float runSpeed = 10f;
        public float runAcceleration = 10f;
        public float breakingForce = 10f;

        [Space]
        public float airSpeed = 2f;
        public float airAcceleration = 10f;
        public float airBreakingForce = 10f;

        [Space]
        [Min(0)]
        public float hangSpeed = 1f;
        public float hangGravityRatio = 0.1f;
        

        [Header("ジャンプ")]
        // ジャンプの最小の高さ。プレーヤーがジャンプボタンを一瞬で離した場合、
        // キャラクターはこの高さまで飛びます。
        [Min(0.1f)]
        public float minJumpHeight = 1f;

        // ジャンプの最高の高さです。プレーヤーがずっとジャンプボタンを押し続けると
        // この高さまで飛びます。
        [Min(0.1f)]
        public float maxJumpHeight = 3f;

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
        public int maxAirJumps = 1;

        // 通常のジャンプに比べてのダブルジャンプの高さ（比率）
        public float airJumpHeightRatio = 0.5f;

        // 落下などで地面から離れた直後でもジャンプを許します。ここでその時間のしきい値（秒）を指定します。
        public float fallJumpTimeLimit = 3f / 60f;

        [Space]
        public bool canWallJump = false;
        public float wallJumpHeightRatio = 1f;
        public float wallJumpHorizontalVelocity = 1f;

        [Space]
        public bool canJumpWhenGrabbing = true;
        public float grabJumpHeightRatio = 1f;
        public float grabJumpHorizontalVelocity = 1f;

        [Header("壁")]
        public Collider2D[] wallCheckSensors = new Collider2D[2];
        public float wallSlideGravityRatio = 0.5f;
        public float wallSlideMaxSpeed = 5f;

        [Space]
        public bool canGrabWall = false;
        public float maxWallGrabTime = 1f;

        [Space]
        public float wallClimbUpSpeed = 2f;
        public float wallClimbDownSpeed = 4f;
        public float climbOverEdgeJumpHeight = 1f;
        public float climbOverHorizontalVelocity = 2f;


        [Header("ダッシュ")]
        public bool canDash = false;

        public float dashSpeedSide = 20f;
        public float dashSpeedUp = 20f;
        public float dashSpeedDown = 0f;
        public float dashTime = 0.2f;

        [Curve(0, 0, 1f, 1f)]
        public AnimationCurve dashSpeedCurve = AnimationCurve.Linear(0, 1, 1, 1);

        [Tooltip("障害物に衝突した祭など、この速度を下回るとダッシュ状態が終わる。")]
        public float minDashSpeed = 0f;

        [Tooltip("ダッシュ開始後、ユーザー入力を無視する時間（秒）")]
        public float dashInputFreezeTime = 0.2f;

        public float dashCoolDownTime = 2f;

        [Tooltip("これが「オン」になっていると、ダッシュの方向が上下左右と斜め45°に制限される。「オフ」だとどの角度でもダッシュできる。")]
        public bool limitDashAngle = true;

        [Tooltip("ダッシュしている時の重力補正")]
        public float dashGravityRatio = 0f;


        public Action OnDestroyed;


        public bool isRunning { get; private set; }
        public bool isJumping { get; private set; }

        public bool isGrabbing { get; private set; }

        public bool isDashing { get; private set; }

        public Vector2 motionInput { get; private set; }

        private Vector2 rawMotionInput;

        public Vector2 facingDirection { get; private set; }

        public Vector2 inputDirection
        {
            get
            {
                if (motionInput.magnitude > SMALL_INPUT_THRESHOLD)
                {
                    return motionInput.normalized;
                }
                else
                {
                    return facingDirection;
                }
            }
        }

        public bool isTouchingWall { get; private set; }

        public float wallDirection { get; private set; }

        private bool acceptInput = true;


        protected const float SMALL_INPUT_THRESHOLD = 0.1f;

        private Utils.Timer inputFreezeTimer = new Utils.Timer();

        [HideInInspector]
        public Utils.Timer dashTimer = new Utils.Timer();
        [HideInInspector]
        public Utils.Timer dashCoolDownTimer = new Utils.Timer();
        private Vector2 dashDirection = Vector2.right;

        [HideInInspector]
        public Utils.Timer wallSlideWaitTimer = new Utils.Timer();
        [HideInInspector]
        public Utils.Timer wallGrabTimer = new Utils.Timer();

        private Utils.BufferedInput<bool> jumpInput = new Utils.BufferedInput<bool>(0.04f);


        protected Vector2 defaultFacingDirection = Vector2.right;


        public enum State
        {
            Walking,
            Running,
            Dashing,
            Jumping,
            WallJumping,
            Falling,
            WallSliding,
            ClimbingUp,
            ClimbingDown,
            ClimbingOver,
            Grabbing
        }

        //public State state { get; private set; }
        public State state;


        #region Unity Messages

        protected override void Awake()
        {
            base.Awake();

            dashTimer.OnStart += () => isDashing = true;
            dashTimer.OnEnd += () => isDashing = false;

            inputFreezeTimer.OnStart += () => { acceptInput = false; motionInput = Vector2.zero; };
            inputFreezeTimer.OnEnd += () => { acceptInput = true; motionInput = rawMotionInput; };

            // 壁感知のコライダの初期設定を行う
            foreach(Collider2D coll in wallCheckSensors)
            {
                if (coll != null)
                {
                    coll.isTrigger = true;
                }
            }
        }

        protected override void Start()
        {
            base.Start();

            normalGravityMultiplier = gravityMultiplier;
            facingDirection = defaultFacingDirection;
            wallDirection = 1;
            state = State.Walking;
            jumpInput.duration = jumpBufferTime;
        }

        void UpdateStateOnWall()
        {

            if (!isGrabbing)
            {
                state = State.Falling;
            }
            else if (!isTouchingWall)
            {
                state = State.Falling;
            }
            else if (wallGrabTimer.isFinished && !isGrounded)
            {
                state = State.WallSliding;
            }
            else if (motionInput.y > SMALL_INPUT_THRESHOLD && wallClimbUpSpeed > 0)
            {
                state = State.ClimbingUp;
            }
            else if (motionInput.y < -SMALL_INPUT_THRESHOLD && wallClimbDownSpeed > 0)
            {
                state = State.ClimbingDown;
            }
        }

        void TryGrabbingWall()
        {
            if (state != State.Grabbing && state != State.WallJumping && !wallGrabTimer.isFinished)
            {
                state = State.Grabbing;
                wallGrabTimer.Start(maxWallGrabTime);
            }
        }

        void UpdateStateOnGround()
        {
            if (isGrounded)
            {
                if (isTouchingWall && isGrabbing && canGrabWall)
                {
                    TryGrabbingWall();
                }
                else if (isRunning)
                {
                    state = State.Running;
                }
                else
                {
                    state = State.Walking;
                }
            }
        }

        void UpdateStateInAir()
        {
            if (!isGrounded)
            {
                if (isTouchingWall)
                {
                    if (isGrabbing && canGrabWall)
                    {
                        TryGrabbingWall();
                    }
                    else if (velocity.y < 0)
                    {
                        state = State.WallSliding;
                    }
                    else if (!isJumping)
                    {
                        state = State.Falling;
                    }
                }
                else if (isJumping)
                {
                    if (velocity.y < 0)
                    {
                        state = State.Falling;
                    }
                    else
                    {
                        state = State.Jumping;
                    }
                }
                else
                {
                    state = State.Falling;
                }
            }
        }

        void ClimbOverEdge()
        {
            float jumpVelocity = Mathf.Sqrt(-2f * Physics2D.gravity.y * normalGravityMultiplier * climbOverEdgeJumpHeight);
            velocity.y = jumpVelocity;
            velocity.x = climbOverHorizontalVelocity * wallDirection;
        }

        void UpdateState()
        {
            // タイマーの更新
            dashCoolDownTimer.Tick(Time.fixedDeltaTime);
            inputFreezeTimer.Tick(Time.fixedDeltaTime);

            if (isGrounded)
            {
                wallGrabTimer.Reset(maxWallGrabTime);
            }

            if (state == State.Grabbing || state == State.ClimbingUp || state == State.ClimbingDown)
            {
                if (!isGrounded)
                {
                    wallGrabTimer.active = true;
                    wallGrabTimer.Tick(Time.fixedDeltaTime);
                }
            }
            else
            {
                wallGrabTimer.active = false;
            }

            switch (state)
            {

                case State.Walking:
                case State.Running:
                    if (isGrounded)
                    {
                        UpdateStateOnGround();
                    }
                    else
                    {
                        UpdateStateInAir();
                    }
                    break;

                case State.Dashing:
                    
                    dashTimer.Tick(Time.fixedDeltaTime);

                    if (velocity.magnitude <= minDashSpeed)
                    {
                        dashTimer.Cancel();
                    }

                    if (dashTimer.isFinished)
                    {
                        if (isGrounded)
                        {
                            // Don't forget to set default state so we don't get stuck in dash
                            state = State.Walking;
                            UpdateStateOnGround();
                        }
                        else
                        {
                            state = State.Falling;
                            UpdateStateInAir();
                        }
                    }

                    break;

                case State.WallJumping:
                case State.Jumping:
                    if (isGrounded)
                    {
                        UpdateStateOnGround();
                    }
                    else
                    {
                        UpdateStateInAir();
                    }
                    break;

                case State.Falling:
                    if (isGrounded)
                    {
                        UpdateStateOnGround();
                    }
                    else
                    {
                        UpdateStateInAir();
                    }
                    break;

                case State.WallSliding:
                    if (isGrounded)
                    {
                        UpdateStateOnGround();
                    }
                    else
                    {
                        UpdateStateInAir();
                    }
                    break;

                case State.ClimbingUp:
                    if (!isTouchingWall)
                    {
                        ClimbOverEdge();
                        state = State.ClimbingOver;
                    }
                    else
                    {
                        UpdateStateOnWall();
                    }
                    break;

                case State.ClimbingDown:
                    UpdateStateOnWall();
                    break;

                case State.ClimbingOver:
                    if (isGrounded)
                    {
                        UpdateStateOnGround();
                    }
                    else if (velocity.y < 0)
                    {
                        UpdateStateInAir();
                    }
                    break;

                case State.Grabbing:
                    UpdateStateOnWall();
                    break;
            }
        }

        void UpdateFacingDirection()
        {
            if (acceptInput)
            {
                if (state == State.Grabbing || state == State.ClimbingUp || state == State.ClimbingDown)
                {
                    facingDirection = Vector2.right * wallDirection;
                }
                else
                {
                    if (motionInput.magnitude < SMALL_INPUT_THRESHOLD)
                    {
                        facingDirection = defaultFacingDirection;
                    }
                    else
                    {
                        facingDirection = motionInput.normalized;

                        defaultFacingDirection = motionInput.x < -SMALL_INPUT_THRESHOLD ? Vector2.left : (motionInput.x > SMALL_INPUT_THRESHOLD ? Vector2.right : defaultFacingDirection);
                    }
                }
                
                if (animationController != null)
                {
                    if (motionInput.magnitude > SMALL_INPUT_THRESHOLD)
                    {
                        animationController.SetFloat("FacingDirectionX", motionInput.normalized.x);
                        animationController.SetFloat("FacingDirectionY", motionInput.normalized.y);
                    }
                }
            }
            
        }

        protected override void FixedUpdate()
        {
            UpdateState();

            // 移動の処理

            if (isGrounded)
            {
                ApplyGroundMotion();
            }
            else
            {
                ApplyAirMotion();
            }

            base.FixedUpdate();

            UpdateWallTouchingState();

            if (state == State.WallSliding)
            {
                if (wallSlideWaitTimer.isFinished)
                {
                    if (velocity.y < -wallSlideMaxSpeed)
                    {
                        velocity.y = -wallSlideMaxSpeed;
                    }
                }
            }

            UpdateFacingDirection();

            // ジャンプバッファーの確認
            if (jumpInput.HasValue())
            {
                Jump(jumpInput.Get(), false);
            }
        }

        protected override void Landed(float speed)
        {
            animationController?.SetTrigger("Land");
            SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
        }

        protected override void Update()
        {
            if (animationController != null)
            {
                animationController.SetBool("IsGrounded", isGrounded);
                animationController.SetBool("IsRunning", state == State.Running);
                animationController.SetBool("IsDashing", state == State.Dashing);
                animationController.SetBool("IsClimbing", state == State.Grabbing || state == State.ClimbingDown || state == State.ClimbingUp);
                animationController.SetBool("IsJumping", state == State.Jumping);
                animationController.SetBool("IsWallSliding", state == State.WallSliding);

                animationController.SetFloat("VelocityX", velocity.x);
                animationController.SetFloat("VelocityY", velocity.y);
                animationController.SetFloat("Speed", velocity.magnitude);
                animationController.SetFloat("FacingDirectionX", facingDirection.x);
                animationController.SetFloat("FacingDirectionY", facingDirection.y);
                animationController.SetFloat("DashDirectionX", dashDirection.x);
                animationController.SetFloat("DashDirectionY", dashDirection.y);
                animationController.SetFloat("WallDirection", wallDirection);
            }

            base.Update();
        }

        GUIStyle guiStyle = new GUIStyle();

        // このメソッドを実装する事によって、Unityのシーンビューに独自の
        // 「ギズモ」を描写する事ができます。ここでは、ジャンプの高さを表す
        // 緑の線を描きます。
        private void OnDrawGizmosSelected()
        {
            if (isActiveAndEnabled)
            {
                Vector3 high = transform.position + Vector3.up * maxJumpHeight;
                Vector3 low = transform.position + Vector3.up * minJumpHeight;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(low + Vector3.left * 0.5f, low + Vector3.right * 0.5f);
                Gizmos.DrawLine(high + Vector3.left * 0.5f, high + Vector3.right * 0.5f);


                guiStyle.normal.textColor = Color.green;
                guiStyle.alignment = TextAnchor.MiddleLeft;
                Handles.Label(high + Vector3.right * 0.6f, "ジャンプ（最高）", guiStyle);
                Handles.Label(low + Vector3.right * 0.6f, "ジャンプ（最低）", guiStyle);

                foreach (Collider2D coll in wallCheckSensors)
                {
                    if (coll != null)
                    {
                        Vector3 lineDirection = Vector3.zero;
                        if (coll.bounds.center.x < transform.position.x)
                        {
                            lineDirection = Vector3.left;
                            guiStyle.alignment = TextAnchor.MiddleRight;                            
                        }
                        else
                        {
                            lineDirection = Vector3.right;
                            guiStyle.alignment = TextAnchor.MiddleLeft;
                        }

                        Gizmos.DrawLine(coll.bounds.center, coll.bounds.center + lineDirection * 0.5f);
                        Handles.Label(coll.bounds.center + lineDirection * 0.6f, "壁検知", guiStyle);
                    }
                }
            }
        }

        #endregion

        #region Player Input
        void OnMove(InputValue val)
        {
            Move(val.Get<Vector2>());
        }

        void OnRun(InputValue val)
        {
            Run(val.isPressed);
        }

        void OnGrabWall(InputValue val)
        {
            GrabWall(val.isPressed);
        }

        void OnDash()
        {
            Dash();
        }

        void SetUserInputEnabled(bool enabled)
        {
            if (animationController != null)
            {
                animationController.SetBool("InputEnabled", enabled);
            }
            else
            {
                PlayerInput playerInput = GetComponent<PlayerInput>();
                playerInput.enabled = enabled;
            }
        }
        
        #endregion

        #region Motion

        private Collider2D[] wallHits = new Collider2D[8];

        protected void UpdateWallTouchingState()
        {
            isTouchingWall = false;

            bool touchingRight = false;
            bool touchingLeft = false;  

            foreach (Collider2D coll in wallCheckSensors)
            {
                if (coll != null && coll.enabled)
                {
                    int count = coll.OverlapCollider(contactFilter, wallHits);
                    if (count > 0)
                    {
                        for(int i = 0; i < count; i++)
                        {
                            if (wallHits[i].gameObject != gameObject)
                            {
                                if (transform.InverseTransformPoint(coll.bounds.center).x < 0)
                                {
                                    touchingLeft = true;
                                }
                                else
                                {
                                    touchingRight = true;
                                }

                                break;
                            }
                        }
                    }
                }
            }

            isTouchingWall = touchingRight || touchingLeft;

            if (touchingRight && touchingLeft)
            {
                wallDirection = facingDirection.x < 0 ? -1 : 1;
            }
            else if (isTouchingWall)
            {
                wallDirection = touchingLeft ? -1 : 1;
            }
        }


        public void Run(bool state)
        {
            if (!acceptInput) return;

            isRunning = state;
        }

        public void GrabWall(bool grabbing)
        {
            if (!acceptInput) return;

            isGrabbing = grabbing;
        }

        public void Dash()
        {
            if (!acceptInput) return;

            if (canDash && dashCoolDownTimer.isFinished)
            {
                if (motionInput.magnitude > SMALL_INPUT_THRESHOLD)
                {
                    dashDirection = motionInput.normalized;
                }
                else
                {
                    dashDirection = facingDirection;
                }

                if (limitDashAngle)
                {
                    float angle = Mathf.Atan2(dashDirection.y, dashDirection.x);
                    angle = Mathf.Round(angle * 1.2732395447351626861510701069801f) * 0.78539816339744830961566084581988f;
                    dashDirection.x = Mathf.Cos(angle);
                    dashDirection.y = Mathf.Sin(angle);
                }

                velocity = dashDirection * CalculateDashSpeed(dashDirection);

                dashTimer.Start(dashTime);
                dashCoolDownTimer.Start(dashCoolDownTime);
                inputFreezeTimer.Start(dashInputFreezeTime);

                state = State.Dashing;

                SendMessage("DidDash", SendMessageOptions.DontRequireReceiver);
            }
        }

        public void Move(Vector2 input)
        {
            rawMotionInput = input;

            if (!acceptInput) return;

            motionInput = input;

            dashTimer.Cancel();
        }

        void ApplyWallMotion()
        {
            gravityMultiplier = 0;

            if (state == State.ClimbingUp)
            {
                velocity.y = motionInput.y * wallClimbUpSpeed;
            }

            else if (state == State.ClimbingDown)
            {
                velocity.y = motionInput.y * wallClimbDownSpeed;
            }

            else if (state == State.Grabbing)
            {
                velocity = Vector2.zero;
            }
        }

        void MoveInAir()
        {
            float horizontalVelocity = velocity.x;
            float horizontalDirection = Mathf.Sign(horizontalVelocity);
            float groundHorizontalDirection = Mathf.Sign(lastGroundVelocity.x);
            float airSpeedX = Mathf.Abs(horizontalVelocity);
            float speedLimit = airSpeed;

            
            // 空中の横移動
            if (Mathf.Abs(motionInput.x) > 0)
            {
                Vector2 velocityChange = Vector2.right * motionInput.x * airAcceleration * Time.fixedDeltaTime;
                velocity += velocityChange;
            }

            if (horizontalDirection == groundHorizontalDirection)
            {
                speedLimit = Mathf.Max(Mathf.Abs(lastGroundVelocity.x), speedLimit);
            }

            if (airSpeedX > speedLimit)
            {
                Vector2 velocityChange = Vector2.right * (airSpeedX - speedLimit) * -horizontalDirection;
                velocity += velocityChange;
            }
        }

        float CalculateDashSpeed(Vector2 direction)
        {
            float verticalSpeed = direction.y < 0 ? dashSpeedDown : dashSpeedUp;
            return Mathf.Abs(direction.x) * dashSpeedSide + Mathf.Abs(direction.y) * verticalSpeed;
        }

        Vector2 CalculateDashVelocity()
        {
            float phase = dashTimer.phase;
            float adjustment = 1f;
            if (dashSpeedCurve.keys.Length >= 2)
            {
                adjustment = dashSpeedCurve.Evaluate(phase);
            }
            return dashDirection * CalculateDashSpeed(dashDirection) * adjustment;
        }

        void ApplyAirMotion()
        {
            if (state == State.Dashing)
            {
                gravityMultiplier = dashGravityRatio;
                velocity = CalculateDashVelocity();
            }

            else if (state == State.Jumping)
            {
                gravityMultiplier = jumpGravityMultiplier;

                MoveInAir();
            }

            else if (state == State.WallJumping)
            {
                gravityMultiplier = jumpGravityMultiplier;

                MoveInAir();
            }

            else if (state == State.Falling)
            {
                if (velocity.y > 0)
                {
                    gravityMultiplier = breakGravityMultiplier;
                }
                else
                {
                    isJumping = false;
                    gravityMultiplier = normalGravityMultiplier;
                }

                MoveInAir();
            }

            else if (state == State.Grabbing || state == State.ClimbingUp || state == State.ClimbingDown)
            {
                ApplyWallMotion();
            }

            else if (state == State.ClimbingOver)
            {
                gravityMultiplier = normalGravityMultiplier;
                velocity.x = climbOverHorizontalVelocity * wallDirection;
            }

            else if (state == State.WallSliding)
            {
 
                if (wallSlideWaitTimer.isFinished)
                {
                    gravityMultiplier = wallSlideGravityRatio * normalGravityMultiplier;
                }
            }

            switch(state)
            {
                case State.Jumping:
                case State.WallJumping:
                case State.Falling:
                    if (Mathf.Abs(velocity.y) < hangSpeed)
                    {
                        gravityMultiplier *= hangGravityRatio;
                    }
                    break;
            }
        }

        void ApplyGroundMotion()
        {
            // 地面に立っているなら空中のジャンプ回数をリセットします。
            airJumps = 0;

            // 重力の加減は通常に戻します。
            gravityMultiplier = normalGravityMultiplier;

            if (state == State.Grabbing || state == State.ClimbingUp || state == State.ClimbingDown)
            {
                ApplyWallMotion();
            }

            else if (state == State.Walking || state == State.Running)
            {
                if (Mathf.Abs(motionInput.x) > 0)
                {
                    float acceleration = isRunning ? runAcceleration : walkAcceleration;
                    Vector2 velocityChange = groundRight * motionInput.x * acceleration * Time.fixedDeltaTime;
                    velocity += velocityChange;
                }
                else
                {
                    Vector2 breakDirection = velocity.normalized * -1f;
                    float groundSpeed = velocity.magnitude;
                    Vector2 velocityChange = breakDirection * Mathf.Min(breakingForce * Time.fixedDeltaTime, groundSpeed);
                    velocity += velocityChange;
                }

                Vector2 v = groundRight * Vector2.Dot(groundRight, velocity);
                float speed = velocity.magnitude;
                float maxSpeed = 0f;

                if (isRunning)
                {
                    maxSpeed = runSpeed;
                }
                else
                {
                    maxSpeed = walkSpeed;
                }

                if (speed > maxSpeed)
                {
                    Vector2 breakDirection = v.normalized * -1f;
                    Vector2 velocityChange = breakDirection * (speed - maxSpeed);
                    velocity += velocityChange;
                }
            }
            else if (state == State.Dashing)
            {
                velocity = CalculateDashVelocity();
            }
        }

        #endregion

        #region Jump

        // 連続ジャンプの回数
        int airJumps = 0;

        bool isAirJump = false;
        bool isWallJump = false;
        bool isGrabJump = false;

        // 通常の重力の度合い
        float normalGravityMultiplier = 1f;

        // ジャンプボタンを押している時と離している時の重力の調整。
        // ジャンプをした時にここの正しい値が計算されます。
        float jumpGravityMultiplier = 1f;
        float breakGravityMultiplier = 1f;

        // ジャンプできるかを確認します。
        bool CanJump()
        {
            isWallJump = false;
            isAirJump = false;
            isGrabJump = false;

            if (minJumpHeight <= 0 || maxJumpHeight <= 0)
            {
                return false;
            }

            switch(state)
            {
                case State.Walking:
                case State.Running:
                    return true;

                case State.Grabbing:
                case State.ClimbingUp:
                case State.ClimbingDown:
                    isGrabJump = canJumpWhenGrabbing;
                    return isGrabJump;
                
                case State.WallSliding:
                    isWallJump = canWallJump;
                    return canWallJump;

                case State.Dashing:
                    if (isGrounded)
                    {
                        return acceptInput;
                    }
                    else
                    {
                        goto case State.Falling;
                    }
                case State.Falling:
                    if (timeInAir <= fallJumpTimeLimit)
                    {
                        // 地面から離れた直後
                        return true;
                    }
                    else if (maxAirJumps > 0 && airJumps <= maxAirJumps)
                    {
                        // 空中ジャンプの上限に達していないのでジャンプできます。
                        isAirJump = true;
                        return true;
                    }
                    else if (velocity.y < 0)
                    {
                        // ここまで来たら空中で落下しています。もうすぐに着地するかも知れません。
                        // 指定の距離いないに地面を検出したらおまけのジャンプを許します。
                        RaycastHit2D groundHit;
                        return CheckForGround(velocity.normalized, jumpGroundCheckDistance, out groundHit);
                    }
                    else
                    {
                        return false;
                    }

                default:
                    return false;
            }
        }

        private Vector2 CalculateJumpVector(float angleOffsetDegrees, float jumpVelocity)
        {
            float theta = Mathf.PI * 0.5f + Mathf.Deg2Rad * angleOffsetDegrees;
            Vector2 jumpDir = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));

            Vector2 v = jumpDir;
            float vel = jumpVelocity;
            if (Mathf.Abs(v.y) > 0.0001f)
            {
                v.x *= vel / v.y;
                v.y = vel;
            }
            else
            {
                v.y = 0;
                v.x = vel;
            }

            return v;
        }

        // ジャンプを実行します。
        public void Jump(bool jumpState, bool bufferInput = true)
        {
            if (!acceptInput) return;

            if (jumpState) // ジャンプ開始
            {
                if (CanJump()) // ジャンプできるなら...
                {
                    float jumpVelocity = 0; // ジャンプの初期速度。これから計算します。

                    float maxSpeed = Mathf.Max(walkSpeed, runSpeed);
                    float speedRatio = Mathf.Abs(velocity.x) / maxSpeed;
                    float adjustedMaxJumpHeight = maxJumpHeight + speedRatio * jumpHeightSpeedBoost;

                    if (mode == JumpMode.Fast)
                    {
                        // ここは、斜方投射の計算です。通常の重力でmaxJumpHeightの高さに達するための初期速度を求めます。
                        jumpVelocity = Mathf.Sqrt(-2f * Physics2D.gravity.y * normalGravityMultiplier * adjustedMaxJumpHeight);
                        jumpGravityMultiplier = normalGravityMultiplier; // このモードでは、ジャンプの重力が通常重力になります。
                        breakGravityMultiplier = normalGravityMultiplier * (adjustedMaxJumpHeight / minJumpHeight); // ジャンプを止める重力の加減を計算します。

                    }
                    else if (mode == JumpMode.Slow)
                    {
                        // 上記と同様の計算ですが、条件を反転します。
                        jumpVelocity = Mathf.Sqrt(-2f * Physics2D.gravity.y * normalGravityMultiplier * minJumpHeight);
                        jumpGravityMultiplier = normalGravityMultiplier * (minJumpHeight / adjustedMaxJumpHeight);
                        breakGravityMultiplier = normalGravityMultiplier;
                    }

                    // ここは少し細かい調整です。KinematicMotion2Dの実装によって、ジャンプによる移動が始まる前に、重力によって
                    // 速度が下がってしまいます。１フレーム目の重力の影響をなくすために、ジャンプ速度を上げます。
                    jumpVelocity += -Physics2D.gravity.y * jumpGravityMultiplier * Time.fixedDeltaTime;

                    // 空中のジャンプの一つの実装です。ゲームのルールによって、
                    // ここの仕組みを変えることがあります。
                    if (isAirJump)
                    {
                        Vector2 v = CalculateJumpVector(jumpAngle * -Mathf.Sign(facingDirection.x), jumpVelocity * airJumpHeightRatio);
                       
                        velocity.x += v.x;
                        velocity.y = v.y;

                        state = State.Jumping;
                    }
                    else if (isWallJump)
                    {
                        Vector2 v = CalculateJumpVector(0, jumpVelocity * wallJumpHeightRatio);
                        
                        velocity.x = -wallDirection * wallJumpHorizontalVelocity;
                        velocity.y = v.y;

                        state = State.WallJumping;
                    }
                    else if (isGrabJump)
                    {
                        Vector2 v = CalculateJumpVector(0, jumpVelocity * grabJumpHeightRatio);

                        velocity.x = -wallDirection * grabJumpHorizontalVelocity;
                        velocity.y = v.y;

                        state = State.WallJumping;
                    }
                    else
                    {
                        Vector2 v = CalculateJumpVector(jumpAngle * -Mathf.Sign(facingDirection.x), jumpVelocity);

                        velocity.x += v.x;
                        velocity.y = v.y;

                        state = State.Jumping;
                    }


                    // 地面が動いていれば、その速度を自身の速度に足します。
                    velocity += groundVelocity;
                    lastGroundVelocity += groundVelocity;

                    // 重力調整を適用します。
                    gravityMultiplier = jumpGravityMultiplier;

                    airJumps++; // ジャンプの回数を増やします。
                    isJumping = true; // ジャンプしている事を記憶します。Dash

                    jumpInput.Reset();

                    animationController?.SetTrigger("Jump");

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
                gravityMultiplier = breakGravityMultiplier;
                isJumping = false;
            }
        }

        // これはUnityのInput Systemに呼び出されます。
        void OnJump(InputValue val)
        {
            Jump(val.isPressed, true);
        }
        #endregion


        bool AvoidPlatformEdge(Vector2 avoidDirection, float distanceX, float distanceY)
        {
            RaycastHit2D hit;
            Vector2 p = rb.position;
            bool canMove = Cast(avoidDirection, distanceX, out hit) == false;
            if (canMove)
            {
                rb.position += avoidDirection * distanceX;
                canMove = Cast(Vector2.up, distanceY, out hit) == false;
                if (canMove)
                {
                    rb.position += Vector2.up * distanceY;
                    Slide(avoidDirection * -distanceX);
                    positionAdjustment += Vector2.right * (rb.position.x - p.x);
                    return true;
                }
                else if (hit.distance > 0)
                {
                    rb.position += Vector2.up * hit.distance;
                    Slide(avoidDirection * -distanceX);
                    positionAdjustment += Vector2.right * (rb.position.x - p.x);
                    return true;
                }
                else
                {
                    rb.position = p;
                }
            }

            return false;
        }

        protected override float ProcessCollision(RaycastHit2D hit, Vector2 direction, float distanceRemaining)
        {
            float edgeSize = 0.5f;

            if (direction.y > 0)
            {
                Vector2 p = rb.position;
                Vector2 delta = direction * distanceRemaining;
                Vector2 avoidDirection = Vector2.right;

                bool didMove = AvoidPlatformEdge(Vector2.right, edgeSize, delta.y) || AvoidPlatformEdge(Vector2.left, edgeSize, delta.y);
                if (didMove)
                {
                    float moveDistance = (rb.position - p).magnitude;
                    return Mathf.Max(0f, distanceRemaining - moveDistance);
                }
            }
            return base.ProcessCollision(hit, direction, distanceRemaining);
        }

        protected override void ContactEnter(Contact contact)
        {
            base.ContactEnter(contact);

            if (state == State.Dashing)
            {
                if (IsWallNormal(contact.normal) && Vector2.Dot(contact.direction, velocity.normalized) > 0.707)
                {
                    dashTimer.Cancel();
                }
            }
        }

        private bool isKilled = false;

        public void Kill()
        {
            if (!isKilled)
            {
                //isKilled = true;
                if (animationController != null)
                {
                    animationController.SetTrigger("Die");
                }
                else
                {
                    DestroySelf();
                }

                SendMessage("WasKilled", SendMessageOptions.DontRequireReceiver);
            }
            
        }

        public void DestroySelf()
        {
            SendMessage("WasDestroyed", SendMessageOptions.DontRequireReceiver);
            OnDestroyed?.Invoke();
            Destroy(gameObject);
        }

        public override string GetIcon()
        {
            return "PlayerIcon";
        }
    }
} // namespace

