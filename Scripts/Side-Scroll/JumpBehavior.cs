using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PuzzleBox
{
    /**
     * このクラスはプレーヤーキャラクターなどの「ジャンプ」を実装します。
     * UnityのInput Systemからの入力、または外部スクリプトから制御できます。
     */
    [RequireComponent(typeof(KinematicMotion2D))]
    public class JumpBehavior : MonoBehaviour
    {
        // ジャンプの最小の高さ。プレーヤーがジャンプボタンを一瞬で離した場合、
        // キャラクターはこの高さまで飛びます。
        public float minJumpHeight = 1f;

        // ジャンプの最高の高さです。プレーヤーがずっとジャンプボタンを押し続けると
        // この高さまで飛びます。
        public float maxJumpHeight = 3f;

        public Vector2 jumpDirection = Vector2.up;

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
        public float groundCheckDistance = 0.2f;

        // ダブルジャンプ・トリプルジャンプを許すならここで空中でできるジャンプの回数を
        // 指定します。
        public int maxAirJumps = 1;

        // 落下などで地面から離れた直後でもジャンプを許します。ここでその時間のしきい値（秒）を指定します。
        public float fallJumpTimeLimit = 3f / 60f;

        // ジャンプをしているかどうか
        public bool isJumping { get; private set; }
        
        // 連続ジャンプの回数
        int airJumps = 0;

        // 通常の重力の度合い
        float normalGravityMultiplier = 1f;

        // ジャンプボタンを押している時と離している時の重力の調整。
        // ジャンプをした時にここの正しい値が計算されます。
        float jumpGravityMultiplier = 1f;
        float breakGravityMultiplier = 1f;

        // 必要なコンポーネントへの参照
        KinematicMotion2D motion2D;

        // このコンポーネントは必須ではないですが、あれば使います。
        Animator animationController;

        // Start is called before the first frame update
        void Start()
        {
            // 必要な参照を取得
            motion2D = GetComponent<KinematicMotion2D>();
            animationController = GetComponent<Animator>();

            // 初期の重力の設定を記憶
            normalGravityMultiplier = motion2D.gravityMultiplier;
        }

        // ジャンプできるかを確認します。
        bool CanJump()
        {
            if (minJumpHeight <= 0 || maxJumpHeight <= 0)
            {
                return false;
            }

            if (motion2D.isGrounded)
            {
                // 地面に立っているならジャンプできます。
                return true;
            }

            if (airJumps < maxAirJumps)
            {
                // 空中ジャンプの上限に達していないのでジャンプできます。
                return true;
            }

            if (motion2D.timeInAir <= fallJumpTimeLimit)
            {
                // 地面から離れた直後
                return true;
            }

            if (motion2D.velocity.y > 0)
            {
                // 空中で上昇しているのでジャンプできません。
                return false;
            }


            // ここまで来たら空中で落下しています。もうすぐに着地するかも知れません。
            // 指定の距離いないに地面を検出したらおまけのジャンプを許します。
            RaycastHit2D groundHit;
            return motion2D.CheckForGround(motion2D.velocity.normalized, groundCheckDistance, out groundHit);
        }

        // ジャンプを実行します。
        public void Jump(bool state)
        {
            if (state) // ジャンプ開始
            {
                if (CanJump()) // ジャンプできるなら...
                {
                    float jumpVelocity = 0; // ジャンプの初期速度。これから計算します。

                    if (mode == JumpMode.Fast)
                    {
                        // ここは、斜方投射の計算です。通常の重力でmaxJumpHeightの高さに達するための初期速度を求めます。
                        jumpVelocity = Mathf.Sqrt(-2f * Physics2D.gravity.y * normalGravityMultiplier * maxJumpHeight);
                        jumpGravityMultiplier = normalGravityMultiplier; // このモードでは、ジャンプの重力が通常重力になります。
                        breakGravityMultiplier = normalGravityMultiplier * (maxJumpHeight / minJumpHeight); // ジャンプを止める重力の加減を計算します。
                    }
                    else if (mode == JumpMode.Slow)
                    {
                        // 上記と同様の計算ですが、条件を反転します。
                        jumpVelocity = Mathf.Sqrt(-2f * Physics2D.gravity.y * normalGravityMultiplier * minJumpHeight);
                        jumpGravityMultiplier = normalGravityMultiplier * (minJumpHeight / maxJumpHeight);
                        breakGravityMultiplier = normalGravityMultiplier;
                    }

                    // ここは少し細かい調整です。KinematicMotion2Dの実装によって、ジャンプによる移動が始まる前に、重力によって
                    // 速度が下がってしまいます。１フレーム目の重力の影響をなくすために、ジャンプ速度を上げます。
                    jumpVelocity += -Physics2D.gravity.y * jumpGravityMultiplier * Time.fixedDeltaTime;

                    Vector2 jumpDir = jumpDirection.normalized;
                    jumpDir *= (1f / jumpDir.y);

                    // 空中のジャンプの一つの実装です。ゲームのルールによって、
                    // ここの仕組みを変えることがあります。
                    if (motion2D.isGrounded)
                    {
                        motion2D.velocity += jumpDir * jumpVelocity;
                    }
                    else
                    {
                        if (motion2D.velocity.y < 0f)
                        {
                            // 落下しているなら、落下速度と関係なく、縦の速度をジャンプ速度にします。
                            Vector2 v = motion2D.velocity;
                            Vector2 delta = jumpDir * jumpVelocity;
                            v.x += delta.x;
                            v.y = delta.y;
                        }
                        else
                        {
                            // 落下していなければ、ジャンプ速度を現在の縦の速度に足します。
                            motion2D.velocity += jumpDir * jumpVelocity;
                        }
                    }

                    

                    // 地面が動いていれば、その速度を自身の速度に足します。
                    motion2D.velocity += motion2D.groundVelocity;
                    motion2D.lastGroundVelocity += motion2D.groundVelocity;

                    // 重力調整を適用します。
                    motion2D.gravityMultiplier = jumpGravityMultiplier;
                    
                    airJumps++; // ジャンプの回数を増やします。
                    isJumping = true; // ジャンプしている事を記憶します。

                    // 地面に立っているかどうかの判定を強制的にし直します。
                    // そうしないと、斜面の上でジャンプした時にジャンプ直後の移動方向が
                    // 横にそれてしまいます。
                    motion2D.UpdateGroundedState();

                    // アニメーターコンポーネントがあれば、ジャンプした事を通知します。
                    // 「Jump」パラメータがアニメーターに追加されていないと警告が出ます。
                    // 必ず追加しておきましょう。
                    if (animationController != null)
                    {
                        animationController.SetTrigger("Jump");
                    }
                }
            }
            else // ジャンプ終了
            {
                // ジャンプが終わったので、重力の加減を変えます。
                motion2D.gravityMultiplier = breakGravityMultiplier;
                isJumping = false;
            }
        }

        // これはUnityのInput Systemに呼び出されます。
        void OnJump(InputValue val)
        {
            Jump(val.isPressed);
        }

        // KinematicMotion2Dスクリプトに説明がある通り、オブジェクトを動かす処理は
        // FixedUpdateで行います。
        void FixedUpdate()
        {
            if (motion2D.isGrounded)
            {
                // 地面に立っているなら空中のジャンプ回数をリセットします。
                airJumps = 0;

                // 重力の加減は通常に戻します。
                motion2D.gravityMultiplier = normalGravityMultiplier;                                                
            }
            else
            {
                // プレーヤーがジャンプボタンを押し続けても、落下しているならジャンプを止めます。
                if (motion2D.velocity.y <= 0 && isJumping)
                {
                    Jump(false);
                }
            }
        }

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
            }
        }
    }
} // namespace

