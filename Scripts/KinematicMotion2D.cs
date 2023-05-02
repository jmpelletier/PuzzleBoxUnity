    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    /**
     * このクラスは本来、Rigibody2Dが行うような処理を代わりに担当します。
     * プラットフォームゲームなど、より正確な動かいが必要な時に使います。
     */
    [RequireComponent(typeof(Rigidbody2D))]
    public class KinematicMotion2D : MonoBehaviour
    {
        [Header("重力")] // インスペクターに見出しを表紙する
        public bool useGravity = true; // 重力の影響を受けるか？
        public float gravityMultiplier = 1f; // 重力の力の調整


        [Header("衝突判定")]
        // Rigidbody2Dを使ってプレーヤーキャラクターなどを実装すると、
        // キャラクターがコライダに引っかかったり、壁などに食い込んだりする
        // 不具合がよく発生してしまいます。一つの対策として、衝突・接触する
        // コライダと少しだけ隙間を開けます。「margin」でその隙間の大きさを
        // 調整します。
        public float margin = 0.02f;

        // 地面の最大の傾斜。これ以上急な坂は壁として認識します。
        public float maxGroundAngleDegrees = 45;
        // 天井の最大の角度。地面と同様に壁との識別に使います。
        public float maxCeilingAngleDegrees = 45;

        // オブジェクトが地面に立っているかどうかを判定するために、
        // 以下のパラメータで指定する距離まで、下に地面に該当する
        // コライダがないかを確認します。この値がmarginより小さいと
        // 地面が検出できなくなります。
        public float groundCheckDistance = 0.03f;

        // 一部のオブジェクトと衝突したくない（すり抜けたい）場合、
        // ここで指定します。
        public LayerMask collisionMask = ~0; // 「~0」はここで「全て」に解釈されます。

        // このパラメータは少し上級者向けで従来なら変える必要がありません。
        // 何かと衝突したら、移動方向を変えて移動を続けてみます。
        // （例えば、斜面に衝突したら、止まるのではなく、斜面の上に移動を続けます。）
        // この値はこの処理を繰り返す最大の回数です。
        public int maxIterations = 1;

        // 斜面に立っている時に微かに滑る事があります。これを止めるために、
        // このしきい値より小さな動きを無視します。これを高く設定してしまうと、
        // 完全に動けなくなるので微小の値にしましょう。
        public float minSlideDistance = 0.002f;

       
        [Header("速度")]
        // 方向と関係なく、絶対に超えられない速度。
        public float maxSpeed = 100f;

        [HideInInspector] // インスペクターで隠す。
        public Vector2 velocity; // 移動の速度。基本的に他のコンポーネントがコードで変えます。

        [HideInInspector] // インスペクターで隠す。
        public Vector2 lastGroundVelocity; // 地面に最後に接触していた時の速度。

        // オブジェクトが地面に立っているかどうか。
        // この値はC#の便利な機能「アクセサ」を使って、クラスの外部から変えられないようにしています。
        public bool isGrounded { get; private set; }

        // 地面の法線（地面に立っていない時は真上を指します。）
        public Vector2 groundNormal { get; private set; }

        // C#のアクセサは計算によってプロパティを返す事ができます。
        // ここは、地面の法線に対して、「右方向」を返します。つまり、
        // 地面にそって右を移動した場合の移動方向です。
        public Vector2 groundRight
        {
            get
            {
                return new Vector2(groundNormal.y, -groundNormal.x);
            }
        }

        // スクリプトで使うコンポーネントへの参照です。
        Rigidbody2D rb;

        // このコンポーネントは必須ではないですが、あれば使います。
        Animator animationController;

        // 衝突判定に必要な変数。
        // 効率をよくするために、メソッドの外で宣言しておきます。
        RaycastHit2D[] hits = new RaycastHit2D[8];
        ContactFilter2D contactFilter = new ContactFilter2D();

        // このメソッドはKinematicMotion2Dの肝心な処理を行います。
        // 「delta」パラメータで指定した距離までオブジェクトを移動します。
        // もし、途中で何かと衝突したら、衝突した面の向きによって、
        // 面に沿って移動を続けます。つまり、面に沿って「スライド」します。
        // 「iteration」パラメータは一回の移動でこれまでに何回スライドしたかを
        // 指定します。
        void Slide(Vector2 delta, int iterations = 0)
        {
            // まず、移動の方向と距離を抽出します。
            Vector2 direction = delta.normalized;
            float distance = delta.magnitude;

            // 小さすぎる動きを無視します。
            if (distance < minSlideDistance)
            {
                return;
            }

            // 独自のメソッドで指定の場所まで移動したら何かに衝突するかを確かめます。
            RaycastHit2D hit; // 衝突があった場合、詳細がここで記憶されます。
            bool collided = Cast(direction, distance, out hit); // 衝突があったかが返されます。

            if (collided) // 衝突しました...
            {
                float contactDistance = hit.distance; // 衝突した位置までの距離

                // 一応、衝突せずに移動できるところまで移動します。
                rb.position += direction * contactDistance;

                // スライドを試す回数がまだ残っているか？
                if (iterations < maxIterations)
                {
                    // スライドができるのは地面と天井だけです。
                    if (IsGroundNormal(hit.normal) || IsCeilingNormal(hit.normal))
                    {
                        // スライドできるようですので、まだ残っている移動距離を計算します。
                        float distanceRemaining = distance - contactDistance;

                        // 衝突した面に対する「右方向」
                        Vector2 right = new Vector2(hit.normal.y, -hit.normal.x);

                        // スライドするのは、横方向のみ。力学敵におかしいですが、落下からの着地した時に、
                        // 滑りを止めるための処理です。
                        Vector2 slideDelta = new Vector2(direction.x * distanceRemaining, 0);

                        // 数学でベクトルを習っていないと以下の行が少し難解ですが、
                        // 「ベクトル射影」を使って、目指す移動（slideDelta）を接触面の方向（right）に
                        // 修正した場合、どれくらいの移動（projection）になるかを計算します。
                        // ベクトル射影とその計算で行う「ベクトル内積（Dot Product）」はゲームでよく使う
                        // 演算ですので、覚えておくといいです。
                        Vector2 projection = right * Vector2.Dot(right, slideDelta);

                        // ここは、今度、プログラミング初心者にとって特に理解が難しいテクニックを使います。
                        // あるメソッドが自身を呼び出すという「再帰的メソッド」または「再帰的関数」です。
                        // ここまでは、移動中に何かと衝突して、動けることろまで動きました。方向を変えて、
                        // 移動を続けたいですが、その途中でまた何かと衝突するかも知れません。
                        // その処理を行うために、もう一度最初からSlideを実行します。メソッドを再帰的に
                        // 呼び出すと無限に繰り返す危険があるので、必ず繰り返した回数を記憶して、ここみたいに
                        // 一定の回数以上繰り返さないチェックを入れます。
                        Slide(projection, iterations + 1);
                    }
                }
            }
            else
            {
                // 衝突がなかったので、オブジェクトの位置を変えます。
                // オブジェクトの位置を変える時に、通常の物理演算に悪影響がないように
                // transformではなく、Rigidbody2Dコンポーネント経由で動かします。
                rb.position += delta;
            }
        }

        // このコンポーネントでもう一つの重要なメソッドです。移動方向（direction）と距離（distance）に
        // 移動した場合、何かに衝突するかを確認します。衝突があったかどうかを返します。衝突があった場合、
        // hitにその詳細が記憶されます。
        bool Cast(Vector2 direction, float distance, out RaycastHit2D hit)
        {
            distance += margin; // 移動距離に隙間を足します。
            bool collided = false; // 衝突したか？初期値はfalseに。

            hit = new RaycastHit2D(); // 衝突がなかった時にhitは初期のままにします。
            contactFilter.layerMask = collisionMask; // 衝突するとしないUnityのレイヤーを準備します。

            // Rigidbody2Dの「Cast」メソッドで衝突判定を行います。「Cast」とは、直訳すると釣りの専門用語で
            // 「竿で仕掛けを飛ばす」という意味です。オブジェクトについているコライダが空間で指定と方向と距離で
            // 移動すれば、何に当たるかを計算する処理です。ゲームプログラミングの基本的な演算の一つです。
            int hitCount = rb.Cast(direction, contactFilter, hits, distance);

            // hitCountを当たったコライダの回数です。
            if (hitCount > 0) // 何かと衝突した...
            {
                // 衝突したコライダを一つずつ確認して、最も近い接触点を探します。
                for (int i = 0; i < hitCount; i++)
                {
                    if (hits[i].distance < distance)
                    {
                        // 今まで確認した接触の中で最も近いですので、詳細を覚えておきます。
                        distance = hits[i].distance; // 衝突までの移動距離
                        hit = hits[i]; // 衝突の詳細情報を記憶します。
                        hit.distance -= margin; // 移動距離からマージンを引いて、衝突からオブジェクトを離します。
                        collided = true; // 衝突したことを記憶します。
                    }
                }
            }

            return collided; // 衝突したかどうかを返します。
        }

        // このメソッドで渡された法線を持つ面が地面に該当するかどうかを返します。
        public bool IsGroundNormal(Vector2 normal)
        {
            // 法線と真上を指すベクトルの角度を計算して、しきい値より低いか返します。
            return Vector2.Angle(Vector2.up, normal) < maxGroundAngleDegrees;
        }

        // このメソッドはスライドできる天井かどうかを返します。
        public bool IsCeilingNormal(Vector2 normal)
        {
            // 法線と真上を指すベクトルの角度を計算して、しきい値より低いか返します。
            return Vector2.Angle(Vector2.down, normal) < maxCeilingAngleDegrees;
        }

        // 初期の処理
        void Start()
        {
            rb = GetComponent<Rigidbody2D>(); // Rigidbody2Dコンポーネントへの参照を取得
            animationController = GetComponent<Animator>();

            // Rigidbody2Dの種類を「キネマティック」にします。そうすると、物理エンジンではなく、
            // ここのスクリプトがオブジェクトを動かします。
            rb.bodyType = RigidbodyType2D.Kinematic;

            // このスクリプトは重力が真下へ働く前提で作られています。しかし、プロジェクト設定で、
            // どの方向にも重力を設定する事ができます。もし、このスクリプトと互換性のない設定が
            // 検出されたらエラーをコンソールに出力します。
            if (Physics2D.gravity.x != 0f || Physics2D.gravity.y > 0f)
            {
                Debug.LogError("重力が真下へ向かっていないとこのコンポーネントは正しく動作しません。プロジェクト設定を確認してください。");
            }
        }

        // RigidbodyまたはRigidbody2Dを使うと移動をUpdateではなく、等間隔で実行される
        // FixedUpdateで行わなければなりません。
        void FixedUpdate()
        {
            if (useGravity) // 重力の影響を受けるか？
            {
                // 重力の方向へ加速します。「Time.fixedDeltaTime」は
                // 前回FixedUpdateが実行された時から経過した時間を記憶しています。
                velocity += Physics2D.gravity * Time.fixedDeltaTime * gravityMultiplier;
            }

            // 最大速度を超えていないかを確認します。
            if (velocity.magnitude > maxSpeed)
            {
                // スピード違反しているようで、最大速度に減速します。
                velocity = velocity.normalized * maxSpeed;
            }

            // この１フレームで移動する距離を計算します。
            Vector2 motion = velocity * Time.fixedDeltaTime;

            // 動きを滑らかにして、爽快な操作感を実現するための工夫として、横に移動してから
            // 縦に移動します。ここでいう「横」と「縦」は絶対の方向ではなく、地面の向きに
            // 対する方向です。地面に立っていなければ、真上と真横になります。
            // ここもベクトル射影が登場します。
            Vector2 horizontalMotion = groundRight * Vector2.Dot(groundRight, motion);
            Vector2 verticalMotion = groundNormal * Vector2.Dot(groundNormal, motion);

            // 移動する前の位置を記憶しておきます。
            Vector2 startPosition = rb.position;

            // 横、そして縦に移動します。
            Slide(horizontalMotion);
            Slide(verticalMotion);

            // 衝突とスライドをしたかも知れません。実際の移動を計算します。
            Vector2 actualMotion = rb.position - startPosition;
            
            // 実際の移動から実際の速度を計算します。
            velocity = actualMotion / Time.fixedDeltaTime;

            // 地面に立っているかどうかの状態を更新します。
            UpdateGroundedState();

            if (isGrounded)
            {
                // 地面に立っている時の速度を記憶しておきます。プレーヤーの空中移動の計算に必要な値です。
                lastGroundVelocity = velocity;
            }
        }

        // オブジェクトを動かす処理は物理演算に影響が出る可能性があるので、FixedUpdateで行います。
        // しかし、アニメーションの更新はUpdateと同期しているので、アニメーター関連の処理はここで
        // 行います。
        void Update()
        {
            // アニメーターコンポーネントがなければ何もしません。
            if (animationController != null)
            {
                // アニメーターに「IsGrounded」、「VelocityX」と「VelocityY」がなければ、
                // 警告が出ます。必ず、アニメーターの設定で追加しましょう。
                animationController.SetBool("IsGrounded", isGrounded);
                animationController.SetFloat("VelocityX", velocity.x);
                animationController.SetFloat("VelocityY", velocity.y);
            }
        }

        // 地面を検出します。指定の方向と距離に地面に当たるコライダを見つければ、trueを返します。
        // 衝突の詳細はhitに記憶されます。
        public bool CheckForGround(Vector2 direction, float distance, out RaycastHit2D hit)
        {
            hit = new RaycastHit2D(); // デフォルトに初期化する

            // Rigidbody2Dにキャストしてもらいます。
            int hitCount = rb.Cast(direction, contactFilter, hits, distance);
            for (int i = 0; i < hitCount; i++)
            {
                // 重力の方向と衝突した面の法線を比較します。
                if (IsGroundNormal(hits[i].normal))
                {
                    // 地面を見つけたので詳細を記憶します。
                    hit = hits[i];
                    return true; // これ以上地面を探す必要がありません。
                }
            }

            // ここまできたら地面は検出できませんでした。
            return false;
        }

        // このメソッドはオブジェクトが地面に立っているかどうか、と地面の向きの状態を更新します。
        void UpdateGroundedState()
        {
            RaycastHit2D groundHit;
            isGrounded = CheckForGround(Vector2.down, groundCheckDistance, out groundHit);
            
            if (isGrounded)
            {
                groundNormal = groundHit.normal;
            }
            else
            {
                groundNormal = Vector2.up;

            }
        }
    }
} // namespace

