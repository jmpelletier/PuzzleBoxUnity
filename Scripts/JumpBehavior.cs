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
        // enumを定義するとインスペクターでドロップダウンメニューを表示させる事ができます。
        // ここは、ジャンプのアルゴリズムを切り替えるようにしておきます。
        // ジャンプの高さを調整する方法はいくつかありますが、ここでは２つを実装します。
        // １つ目の方法（GravityMultiplier）では、プレーヤーがジャンプボタンを押している間に
        // 重力の力を弱めます。2つ目の方法（BreakForce）では、ジャンプボタンを離すと上昇が止まるまで、
        // 縦に減速させる力を適用します。BreakForce法では、力を強くする事によってより正確に
        // ジャンプの高さを調整する事ができます。
        public enum JumpMode
        {
            GravityMultiplier,
            BreakForce
        }

        public JumpMode mode = JumpMode.GravityMultiplier;

        public float jumpSpeed = 5f; // ジャンプの初期速度

        // ジャンプの高さを調整する仕組みは色々あります。このスクリプトで
        // 実装しているのは、有名なプラットフォームゲームで採用された
        // アルゴリズムです。プレーヤーがジャンプボタンを押している間に
        // 重力の力を弱める事によって、ボタンを長く押すほどジャンプが高く
        // なります。
        public float jumpGravityMultiplier = 0.7f;

        // 「BreakForce」モードでジャンプを止める力の強さ
        public float breakingForce = 200f;

        // ゲームの何位度を調整するために、空中でも着地寸前ならジャンプを可能に
        // する工夫があります。ここではその距離のしきい値を設定します。
        public float groundCheckDistance = 0.2f;

        // ダブルジャンプ・トリプルジャンプを許すならここで空中でできるジャンプの回数を
        // 指定します。
        public int maxAirJumps = 1;

        // ジャンプをしているかどうか
        public bool isJumping { get; private set; }
        
        // 連続ジャンプの回数
        int airJumps = 0;

        // 通常の重力の度合い
        float normalGravityMultiplier = 1f;

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
                    // 空中のジャンプの一つの実装です。ゲームのルールによって、
                    // ここの仕組みを変えることがあります。
                    if (motion2D.velocity.y <= 0f)
                    {
                        // 落下しているなら、落下速度と関係なく、縦の速度をジャンプ速度にします。
                        motion2D.velocity.y = jumpSpeed;
                    }
                    else
                    {
                        // 落下していなければ、ジャンプ速度を現在の縦の速度に足します。
                        motion2D.velocity.y += jumpSpeed;
                    }

                    if (mode == JumpMode.GravityMultiplier)
                    {
                        // 重力の力を弱めます。
                        motion2D.gravityMultiplier = jumpGravityMultiplier * normalGravityMultiplier;
                    }
                    
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
                if (mode == JumpMode.GravityMultiplier)
                {
                    // ジャンプが終了したので、通常の重力に戻します。
                    motion2D.gravityMultiplier = normalGravityMultiplier;
                }
                    
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
            }
            else
            {
                // プレーヤーがジャンプボタンを押し続けても、落下しているならジャンプを止めます。
                if (motion2D.velocity.y < 0 && isJumping)
                {
                    Jump(false);
                }

                if (mode == JumpMode.BreakForce)
                {
                    // 上昇していて、プレーヤーがジャンプボタンを押していなければ、
                    // ジャンプを止める力を適用します。
                    if (motion2D.velocity.y > 0 && !isJumping)
                    {
                        // 必要以上に強い力を加えません。
                        float force = Mathf.Min(motion2D.velocity.y, breakingForce * Time.fixedDeltaTime);

                        // 上昇を止める力を適用します。
                        motion2D.velocity.y -= force;
                    }
                }
            }
        }


    }
} // namespace

